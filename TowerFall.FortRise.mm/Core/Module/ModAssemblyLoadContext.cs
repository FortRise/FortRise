using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using Mono.Cecil;

namespace FortRise;

internal sealed class ModAssemblyLoadContext : AssemblyLoadContext, IAssemblyResolver
{
    public ModuleMetadata Metadata { get; private set; }
    public static readonly string UnmanagedFolders;
    public const string Unmanaged = "Unmanaged";
    private string directoryDll;
    private bool isDisposed;
    private static Dictionary<string, AssemblyDefinition> loadAsm = new Dictionary<string, AssemblyDefinition>();
    internal readonly Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
    internal readonly Dictionary<string, ModuleDefinition> LoadedModules = new Dictionary<string, ModuleDefinition>();
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
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            UnmanagedFolders = "osx-x64";
        }
    }


    internal ModAssemblyLoadContext(ModuleMetadata metadata) : base(metadata.Name, true)
    {
        Metadata = metadata;

        if (!string.IsNullOrEmpty(metadata.PathDirectory))
        {
            directoryDll = Path.GetDirectoryName(Path.Combine(metadata.PathDirectory, metadata.DLL)).Replace('\\', '/');
        }
        else 
        {
            directoryDll = Path.GetDirectoryName(Path.Combine(Path.GetFileNameWithoutExtension(metadata.PathZip), metadata.DLL)).Replace('\\', '/');
        }
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        Assembly asm;
        if ((asm = LoadModAssembly(Path.Combine(directoryDll, $"{assemblyName.Name}.dll"))) != null) 
        {
            return asm;
        }

        // load from launcher instead
        return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
    }

    public Assembly LoadModAssembly(string path)
    {
        lock (syncRoot)
        {
            ref var asm = ref CollectionsMarshal.GetValueRefOrAddDefault(LoadedAssemblies, path, out bool exists);
            if (exists)
            {
                return asm;
            }

            string asmDLL = Path.GetFileNameWithoutExtension(path);

            if (!string.IsNullOrEmpty(Metadata.PathDirectory))
            {
                if (File.Exists(path))
                {
                    using (var asmFS = File.OpenRead(path))
                    {
                        
                        asm = RiseCore.Relinker.LoadModAssembly(Metadata, asmDLL, asmFS);
                        if (asm == null) 
                        {
                            // let's try our best to load the dependency
                            asmFS.Seek(0, SeekOrigin.Begin);
                            asm = RiseCore.Relinker.FakeRelink(Metadata, Path.GetFileNameWithoutExtension(asmDLL), asmFS);
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(Metadata.PathZip))
            {
                using (var asmZip = ZipFile.OpenRead(Metadata.PathZip))
                {
                    string entryPath = path.Replace('\\', '/');
                    ZipArchiveEntry entry = asmZip.GetEntry(entryPath);

                    if (entry != null)
                    {
                        using (var dllStream = entry.ExtractStream())
                        {
                            asm = RiseCore.Relinker.LoadModAssembly(Metadata, asmDLL, dllStream);
                            if (asm == null)
                            {
                                // let's try our best to load the dependency
                                dllStream.Seek(0, SeekOrigin.Begin);
                                asm = RiseCore.Relinker.FakeRelink(Metadata, Path.GetFileNameWithoutExtension(asmDLL), dllStream);
                            }
                        }
                    }
                }
            }
            else 
            {
                throw new UnreachableException();
            }

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
                    using (var symFS = File.OpenRead(symPath))
                    {
                        asm = LoadFromStream(asmFS, symFS);
                    }
                }
                else 
                {
                    asm = LoadFromStream(asmFS);
                }
            }
            string asmName = asm.GetName().Name;
            if (!LoadedModules.TryAdd(asmName, mod))
            {
                Logger.Warning($"[Relinker] Encountered a module name conflict loading cached assembly {Metadata} - {mod.Assembly.Name}");
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
        ref var asm = ref CollectionsMarshal.GetValueRefOrAddDefault(loadAsm, name.Name, out bool exists);
        if (exists)
        {
            return asm;
        }

        // try to load the assembly relative to mod
        if (LoadModAssembly(Path.Combine(directoryDll, $"{name.Name}.dll")) != null) 
        {
            asm = LoadedModules[name.Name].Assembly;
            return asm;
        }

        // try to load the launcher assembly instead
        var globalAsmRef = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(name.Name));
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
        string libName = name;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            libName = $"{name}.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            libName = $"lib{name}.so";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            libName = $"lib{name}.dylib";
        }

        if (!string.IsNullOrEmpty(Metadata.PathDirectory))
        {
            if (NativeLibrary.TryLoad(Path.Combine(directoryDll, Unmanaged, UnmanagedFolders, libName), out IntPtr handle))
            {
                return handle;
            }
        }
        else if (!string.IsNullOrEmpty(Metadata.PathZip))
        {
            string extractionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods", "_RelinkerCache", Unmanaged, UnmanagedFolders, Metadata.Name);

            string metaHash = Metadata.Hash.ToHexadecimalString();

            if (Directory.Exists(extractionPath) && !File.Exists(extractionPath + ".sum") || File.ReadAllText(extractionPath + ".sum") != metaHash)
            {
                Directory.Delete(extractionPath, true);
            }


            if (!Directory.Exists(extractionPath))
            {
                string unmanagedPath = Path.Combine(Unmanaged, UnmanagedFolders);
                using (ZipArchive zip = ZipFile.OpenRead(Metadata.PathZip))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (!entry.FullName.StartsWith(unmanagedPath) || entry.FullName.EndsWith('/'))
                        {
                            continue;
                        }

                        if (!Directory.Exists(extractionPath))
                        {
                            Directory.CreateDirectory(extractionPath);
                        }

                        using (Stream input = entry.Open())
                        {
                            using (Stream output = File.Create(Path.Combine(extractionPath, entry.Name))) 
                            {
                                input.CopyTo(output);
                            }
                        }
                    }
                }

                File.WriteAllText(extractionPath + ".sum", metaHash);
            }

            if (NativeLibrary.TryLoad(Path.Combine(extractionPath, libName), out IntPtr handle))
            {
                return handle;
            }
        }
        else 
        {
            throw new UnreachableException();
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
        System.GC.SuppressFinalize(this);
    }
}