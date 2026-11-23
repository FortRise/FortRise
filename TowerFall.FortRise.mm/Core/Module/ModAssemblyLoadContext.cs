using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.Extensions.Logging;
using Mono.Cecil;

namespace FortRise;

internal sealed class ModAssemblyLoadContext : AssemblyLoadContext, IAssemblyResolver
{
    public ModuleMetadata Metadata { get; private set; }
    public static readonly string UnmanagedFolders;
    public const string Unmanaged = "Unmanaged";
    private bool isDisposed;

    private readonly static Dictionary<string, AssemblyDefinition> loadAsm = [];
    private readonly Dictionary<string, IntPtr> cachedNativeLibraries = [];
    internal readonly Dictionary<string, Assembly> LoadedAssemblies = [];
    internal readonly Dictionary<string, ModuleDefinition> LoadedModules = [];
    private static readonly Lock syncRoot = new Lock();

    static ModAssemblyLoadContext()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            UnmanagedFolders = "win-x64";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            UnmanagedFolders = "linux-x64";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            UnmanagedFolders = "osx-x64";
        }
    }


    internal ModAssemblyLoadContext(ModuleMetadata metadata) : base(metadata.Name, true)
    {
        Metadata = metadata;
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var mod = RiseCore.ModuleManager.GetMod(Metadata.Name);

        Assembly asm = null;
        if (mod.OwnedResources.TryGetValue($"{assemblyName.Name}.dll", out var dll))
        {
            asm = LoadModAssembly(dll); 
        }

        if (asm != null) 
        {
            return asm;
        }

        // load from launcher instead
        return Default.LoadFromAssemblyName(assemblyName);
    }

    public Assembly LoadModAssembly(IResourceInfo path)
    {
        lock (syncRoot)
        {
            ref var asm = ref CollectionsMarshal.GetValueRefOrAddDefault(
                LoadedAssemblies, 
                path.RootPath, 
                out bool exists);

            if (exists)
            {
                return asm;
            }

            using var asmFS = path.Stream;
            asm = Resolver.LoadModAssembly(path.Resource, path.Name, asmFS);

            if (Unsafe.IsNullRef(ref asm))
            {
                return null;
            }

            return asm;
        }
    }

    internal Assembly LoadRelinkedAssembly(string path)
    {
        ModuleDefinition mod = null;
        try
        {
            mod = ModuleDefinition.ReadModule(path);

            string symPath = Path.ChangeExtension(path, "pdb");

            Assembly asm;
            using (var asmFS = File.OpenRead(path))
            {
                if (File.Exists(symPath))
                {
                    using var symFS = File.OpenRead(symPath);
                    asm = LoadFromStream(asmFS, symFS);
                }
                else 
                {
                    asm = LoadFromStream(asmFS);
                }
            }
            string asmName = asm.GetName().Name;
            if (!LoadedModules.TryAdd(asmName, mod))
            {
                RiseCore.logger.LogWarning(
                    "Encountered module name conflict loading cached assembly {metadata} - {asm}",
                    Metadata,
                    mod.Assembly.Name
                );
            }

            return asm;
        }
        catch 
        {
            mod?.Dispose();
            throw;
        }
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name)
    {
        ref var asm = ref CollectionsMarshal.GetValueRefOrAddDefault(
            loadAsm, 
            name.Name, 
            out bool exists
        );

        if (exists)
        {
            return asm;
        }

        var mod = RiseCore.ModuleManager.GetMod(Metadata.Name);


        // try to load the assembly relative to mod
        if (mod.OwnedResources.TryGetValue($"{name.Name}.dll", out var dll) 
            && LoadModAssembly(dll) != null)
        {
            asm = LoadedModules[name.Name].Assembly;
            return asm;
        }

        // try to load the launcher assembly instead
        var globalAsmRef = Default.LoadFromAssemblyName(
            new AssemblyName(name.Name));

        if (globalAsmRef == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(globalAsmRef.Location))
        {
            asm = ModuleDefinition.ReadModule(globalAsmRef.Location).Assembly;
        }

        return asm;
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
        return Resolve(name);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        IntPtr handle = LoadUnmanaged(unmanagedDllName);

        return handle;
    }

    private IntPtr LoadUnmanaged(string name)
    {
        // TODO: fallback lib with no lib
        string libName = name switch 
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => $"{name}.dll",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => $"lib{name}.so",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => $"lib{name}.dylib",
            _ => name
        };


        var mod = RiseCore.ModuleManager.GetMod(Metadata.Name);

        string extractionPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "Mods", 
            "_RelinkerCache", 
            Metadata.Name
        );

        string filepath = Path.Combine(extractionPath, "Unmanaged", UnmanagedFolders, libName);
        string filepathSum = Path.Combine(extractionPath, "Unmanaged", UnmanagedFolders, libName + ".sum");
        string file = Path.GetFileName(filepath);

        ref var nativeLibrary = ref CollectionsMarshal.GetValueRefOrAddDefault(cachedNativeLibraries, file, out bool exists);

        if (exists)
        {
            return nativeLibrary;
        }

        if (!Directory.Exists(extractionPath))
        {
            Directory.CreateDirectory(extractionPath);
        }

        ReadOnlySpan<char> metaHash = Metadata.Hash.ToHexadecimalString();

        // checks if the cache exists and has a same checksum
        if (Directory.Exists(extractionPath) && File.Exists(filepath) && File.Exists(filepathSum))
        {
            RiseCore.logger.LogInformation(
                "[ModAssemblyLoadContext] Loading cached native dll for Metadata: {metadata} - {file}", 
                Metadata, file);
            if (metaHash.SequenceEqual(File.ReadAllText(filepathSum)) && 
                NativeLibrary.TryLoad(filepath, out IntPtr h))
            {
                return nativeLibrary = h;
            }
        }

        string unmanagedPath = Path.Combine(Unmanaged, UnmanagedFolders)
            .Replace('\\', '/');
        string secondUnmanagedPath = Path.Combine(Unmanaged, UnmanagedFolders, "native")
            .Replace('\\', '/');

        foreach (var entry in mod.OwnedResources)
        {
            if (!(entry.Key.StartsWith(unmanagedPath) || 
                entry.Key.StartsWith(secondUnmanagedPath)))
            {
                continue;
            }
            if (entry.Value.ResourceType 
                == typeof(ResourceTypeFolder))
            {
                continue;
            }

            var outFile = Path.Combine(extractionPath, entry.Key);
            var outDir = Path.GetDirectoryName(outFile);

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            using Stream input = entry.Value.Stream;
            using Stream output = File.Create(outFile);

            input.CopyTo(output);
        }

        File.WriteAllText(filepathSum, metaHash);

        if (NativeLibrary.TryLoad(filepath, out IntPtr handle))
        {
            return nativeLibrary = handle;
        }
        
        return IntPtr.Zero;
    }


    private void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                LoadedAssemblies.Clear();

                foreach (var module in LoadedModules.Values)
                {
                    module.Dispose();
                }
                LoadedModules.Clear();
            }
            isDisposed = true;
        }
    }

    ~ModAssemblyLoadContext()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
