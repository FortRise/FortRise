using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using MonoMod;
using TowerFall;

namespace FortRise;

// https://github.com/EverestAPI/Everest/blob/dev/Celeste.Mod.mm/Mod/Everest/Everest.Relinker.cs

public static partial class RiseCore 
{

    /// <summary>
    /// A TowerFall checksum executable.
    /// </summary>
    public static string GameChecksum { get; internal set; }

    public static class Relinker 
    {
        private static bool temporaryASM;

        internal readonly static Dictionary<string, ModuleDefinition> StaticRelinkModuleCache = new Dictionary<string, ModuleDefinition>() {
            { "MonoMod", ModuleDefinition.ReadModule(typeof(MonoModder).Assembly.Location, new ReaderParameters(ReadingMode.Immediate)) },
            { "TowerFall", ModuleDefinition.ReadModule(typeof(TFGame).Assembly.Location, new ReaderParameters(ReadingMode.Immediate)) }
        };
        private static Dictionary<string, ModuleDefinition> sharedRelinkModuleMap;
        public static Dictionary<string, ModuleDefinition> SharedRelinkModuleMap
        {
            get 
            {
                if (sharedRelinkModuleMap != null)
                    return sharedRelinkModuleMap;
                
                sharedRelinkModuleMap = new Dictionary<string, ModuleDefinition>();
                string[] entries = Directory.GetFiles(GameRootPath);
                for (int i = 0; i < entries.Length; i++) 
                {
                    var path = entries[i];
                    var name = Path.GetFileName(path);
                    var nn = name.Substring(0, Math.Max(0, name.Length - 4));

                    if (name.EndsWith(".mm.dll")) 
                    {
                        if (name.StartsWith("TowerFall.")) 
                        {
                            sharedRelinkModuleMap[nn] = StaticRelinkModuleCache["TowerFall"];
                            Logger.Info($"[Relinker] Relinking {name}");
                        }
                        else 
                        {
                            Logger.Warning($"[Relinker] Found unknown {name}");
                            int dot = name.IndexOf('.');
                            if (dot < 0)
                                continue;
                            string nameRelinkedNeutral = name.Substring(0, dot);
                            string nameRelinked = nameRelinkedNeutral + ".dll";
                            string pathRelinked = Path.Combine(Path.GetDirectoryName(path), nameRelinked);
                            if (!File.Exists(pathRelinked))
                                continue;
                            if (!StaticRelinkModuleCache.TryGetValue(nameRelinkedNeutral, out ModuleDefinition relinked)) {
                                relinked = ModuleDefinition.ReadModule(pathRelinked, new ReaderParameters(ReadingMode.Immediate));
                                StaticRelinkModuleCache[nameRelinkedNeutral] = relinked;
                            }
                            Logger.Info($"[Relinker] Remapped to {nameRelinked}");
                            sharedRelinkModuleMap[nn] = relinked;
                        }
                    }
                }
                return sharedRelinkModuleMap;
            }
        }


        private static Dictionary<string, object> sharedRelinkMap;
        public static Dictionary<string, object> SharedRelinkMap 
        {
            get 
            {
                if (sharedRelinkMap != null)
                    return sharedRelinkMap;
                
                sharedRelinkMap = new Dictionary<string, object>();
                AssemblyName[] asmRefs = typeof(TFGame).Assembly.GetReferencedAssemblies();

                for (int i = 0; i < asmRefs.Length; i++) 
                {
                    var asmRef = asmRefs[i];

                    if (!asmRef.FullName.ToLowerInvariant().Contains("fna") &&
                        !asmRef.FullName.ToLowerInvariant().Contains("xna") &&
                        !asmRef.FullName.ToLowerInvariant().Contains("monogame"))
                            continue;
                    
                    Logger.Info($"[Relinker] Relinking {asmRef.Name}");

                    var asm = Assembly.Load(asmRef);
                    var module = ModuleDefinition.ReadModule(asm.Location, new ReaderParameters(ReadingMode.Immediate));
                    SharedRelinkModuleMap[asmRef.FullName] = SharedRelinkModuleMap[asmRef.Name] = module;
                    Type[] types = asm.GetExportedTypes();
                    for (int k = 0; k < types.Length; k++) 
                    {
                        var type = types[k];
                        var typeDef = module.GetType(type.FullName) ?? module.GetType(type.FullName.Replace('+', '/'));
                        if (typeDef == null)
                            continue;
                        SharedRelinkMap[typeDef.FullName] = typeDef;
                    }
                }

                return sharedRelinkMap;
            }
        }

        private static ModuleDefinition runtimeRulesModule;

        public static Assembly LoadModAssembly(ModuleMetadata meta, string asmDLL, Stream stream) 
        {
            return Relink(meta, Path.GetFileNameWithoutExtension(asmDLL), stream);
        }

        public static Assembly Relink(
            ModuleMetadata meta, ReadOnlySpan<char> name, Stream stream) 
        {
            ModuleDefinition module = null;
            Span<char> asmName = stackalloc char[name.Length];
            name.Replace(asmName, ' ', '_');

            var dirPath = Path.Combine(GameRootPath, "Mods", "_RelinkerCache");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var cachedPath = Path.Combine(dirPath, $"{asmName}.{meta.Name}.dll");
            var cachedChecksumPath = string.Concat(cachedPath.AsSpan(0, cachedPath.Length - 4), ".sum");

            var checksums = new string[2];
            checksums[0] = GameChecksum;
            checksums[1] = RiseCore.GetChecksum(ref stream).ToHexadecimalString();
            

            if (File.Exists(cachedPath) && File.Exists(cachedChecksumPath) && 
                ChecksumsEqual(checksums, File.ReadAllLines(cachedChecksumPath))) 
            {
                Logger.Info($"[Relinker] Loading cached assembly for {meta} - {asmName}");

                try 
                {
                    return meta.AssemblyLoadContext.LoadRelinkedAssembly(cachedPath);
                }
                catch (Exception e) 
                {
                    Logger.Error($"[Relinker] Failed loading {meta} - {asmName}");
                    Logger.Error(e.ToString());
                    return null;
                }
            }

            MonoModder modder = new MonoModder() {
                CleanupEnabled = false,
                RelinkModuleMap = SharedRelinkModuleMap,
                RelinkMap = SharedRelinkMap,
                MissingDependencyThrow = false,
                DependencyDirs = {
                    GameRootPath
                },
                AssemblyResolver = meta.AssemblyLoadContext,
                ReaderParameters = {
                    SymbolReaderProvider = new RelinkerSymbolReaderProvider()
                }
            };

            try 
            {
                modder.Input = stream;
                modder.OutputPath = cachedPath;

                var metaPath = string.Concat(meta.DLL.AsSpan(0, meta.DLL.Length - 4), ".pdb");
                modder.ReaderParameters.SymbolStream = OpenSymbol(meta, metaPath);
                modder.ReaderParameters.ReadSymbols = modder.ReaderParameters.SymbolStream != null;

                if (modder.ReaderParameters.SymbolReaderProvider != null && modder.ReaderParameters.SymbolReaderProvider is RelinkerSymbolReaderProvider provider) 
                {
                    provider.Format = DebugSymbolFormat.PDB;
                }

                try 
                {
                    modder.ReaderParameters.ReadSymbols = true;
                    modder.Read();
                }
                catch 
                {
                    modder.ReaderParameters.SymbolStream?.Dispose();
                    modder.ReaderParameters.SymbolStream = null;
                    modder.ReaderParameters.ReadSymbols = false;
                    stream.Seek(0, SeekOrigin.Begin);
                    modder.Read();
                }

                if (modder.ReaderParameters.SymbolReaderProvider != null && modder.ReaderParameters.SymbolReaderProvider is RelinkerSymbolReaderProvider provider1) 
                {
                    provider1.Format = DebugSymbolFormat.Auto;
                }

                modder.MapDependencies();
                if (runtimeRulesModule == null) 
                {
                    string rulesPath = Path.Combine(
                        Path.GetDirectoryName(typeof(TFGame).Assembly.Location),
                        Path.GetFileNameWithoutExtension(typeof(TFGame).Assembly.Location) + ".FortRise.mm.dll");
                    
                    if (!File.Exists(rulesPath)) 
                    {
                        rulesPath = Path.Combine(
                            Path.GetDirectoryName(typeof(TFGame).Assembly.Location),
                            "TowerFall.FortRise.mm.dll"
                        );
                    }
                    if (!File.Exists(rulesPath))
                        throw new InvalidOperationException($"[Relinker] Couldn't find runtime rules .FortRise.mm.dll");

                    if (File.Exists(rulesPath)) 
                    {
                        runtimeRulesModule = ModuleDefinition.ReadModule(rulesPath, new ReaderParameters(ReadingMode.Immediate));
                    }
                }

                modder.MapDependencies(runtimeRulesModule);
                var runtimeRulesType = runtimeRulesModule.GetType("MonoMod.MonoModRules");
                modder.ParseRules(runtimeRulesModule);
                if (runtimeRulesType != null)
                    runtimeRulesModule.Types.Add(runtimeRulesType);

                modder.ParseRules(modder.Module);
                modder.AutoPatch();
                ISymbolWriterProvider symbolWriterProvider = modder.WriterParameters.SymbolWriterProvider;

                Retry:
                try 
                {
                    Logger.Info("[Relinker] Try writing the module with symbols");
                    modder.WriterParameters.SymbolWriterProvider = symbolWriterProvider;
                    modder.WriterParameters.WriteSymbols = true;
                    using (var fs = File.Create(cachedPath))
                    {
                        modder.Write(fs);
                    }
                }
                catch 
                {
                    try 
                    {
                        Logger.Info("[Relinker] Writing the module without symbols");
                        modder.WriterParameters.SymbolWriterProvider = null;
                        modder.WriterParameters.WriteSymbols = false;
                        using (var fs = File.Create(cachedPath))
                        {
                            modder.Write(fs);
                        }
                    }
                    catch when (!temporaryASM) 
                    {
                        Logger.Error($"[Relinker] {cachedPath} is currently in used.");
                        temporaryASM = true;
                        long stamp2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        cachedPath = Path.Combine(Path.GetTempPath(), $"FortRise.{asmName}.{Path.GetFileNameWithoutExtension(dirPath)}.{stamp2}.dll");
                        Logger.Info($"[Relinker] Moving to {cachedPath}");
                        modder.Module.Name += "." + stamp2;
                        modder.Module.Assembly.Name.Name += "." + stamp2;
                        modder.OutputPath = cachedPath;
                        goto Retry;
                    }
                }

                module = modder.Module;
            }
            catch (Exception e) 
            {
                Logger.Error($"[Relinker] Failed Relinking {meta} - {asmName}");
                Logger.Error(e.ToString());
            }
            finally 
            {
                modder.ReaderParameters.SymbolStream?.Dispose();
                if (module != modder.Module)
                    modder.Module?.Dispose();
                modder.Module = null;
            }

            try 
            {
                if (File.Exists(cachedChecksumPath))
                    File.Delete(cachedChecksumPath);
                
                if (!temporaryASM) 
                {
                    File.WriteAllLines(cachedChecksumPath, checksums);
                }

                return meta.AssemblyLoadContext.LoadRelinkedAssembly(cachedPath);
            }
            catch (Exception e) 
            {
                Logger.Error($"[Relinker] Failed Loading {meta} - {asmName}");
                Logger.Error(e.ToString());
                return null;
            }
        }

        public static Assembly FakeRelink(ModuleMetadata meta, ReadOnlySpan<char> name, Stream stream)
        {
            Span<char> asmName = stackalloc char[name.Length];
            name.Replace(asmName, ' ', '_');

            var dirPath = Path.Combine(GameRootPath, "Mods", "_RelinkerCache");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var cachedPath = Path.Combine(dirPath, $"{asmName}.{meta.Name}.dll");
            var cachedChecksumPath = string.Concat(cachedPath.AsSpan(0, cachedPath.Length - 4), ".sum");

            var checksums = new string[2];
            checksums[0] = GameChecksum;
            checksums[1] = RiseCore.GetChecksum(ref stream).ToHexadecimalString();
            

            if (File.Exists(cachedPath) && File.Exists(cachedChecksumPath) && 
                ChecksumsEqual(checksums, File.ReadAllLines(cachedChecksumPath))) 
            {
                Logger.Info($"[Fake Relinker] Loading cached assembly for {meta} - {asmName}");

                try 
                {
                    return meta.AssemblyLoadContext.LoadRelinkedAssembly(cachedPath);
                }
                catch (Exception e) 
                {
                    Logger.Error($"[Fake Relinker] Failed loading {meta} - {asmName}");
                    Logger.Error(e.ToString());
                    return null;
                }
            }

            using (var fs = File.Create(cachedPath))
            {
                stream.CopyTo(fs);
            }

            try 
            {
                if (File.Exists(cachedChecksumPath))
                    File.Delete(cachedChecksumPath);
                
                if (!temporaryASM) 
                {
                    File.WriteAllLines(cachedChecksumPath, checksums);
                }

                return meta.AssemblyLoadContext.LoadRelinkedAssembly(cachedPath);
            }
            catch (Exception e) 
            {
                Logger.Error($"[Fake Relinker] Failed Loading {meta} - {asmName}");
                Logger.Error(e.ToString());
                return null;
            }
        }

        private static Stream OpenSymbol(ModuleMetadata metadata, string name) 
        {
            if (!string.IsNullOrEmpty(metadata.PathZip)) 
            {
                using var zipFile = ZipFile.OpenRead(metadata.PathZip);
                foreach (var entry in zipFile.Entries) 
                {
                    if (!name.Contains(entry.FullName))
                        continue;
                    return entry.ExtractStream();
                }
            }
            if (!string.IsNullOrEmpty(metadata.PathDirectory)) 
            {
                var pdbPath = Path.Combine(metadata.PathDirectory, name);
                if (File.Exists(pdbPath)) 
                {
                    return File.OpenRead(pdbPath);
                }
            }
            return null;
        }

        public static bool ChecksumsEqual(string[] a, string[] b) {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++) 
            {
                var left = a[i].AsSpan().Trim();
                var right = b[i].AsSpan().Trim();
                if (!left.SequenceEqual(right))
                    return false;
            }
            return true;
        }
    }
}

public class RelinkerSymbolReaderProvider : ISymbolReaderProvider {

    public DebugSymbolFormat Format;

    public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream) {
        switch (Format) {
            case DebugSymbolFormat.MDB:
                return new MdbReaderProvider().GetSymbolReader(module, symbolStream);

            case DebugSymbolFormat.PDB:
                if (IsPortablePdb(symbolStream))
                    return new PortablePdbReaderProvider().GetSymbolReader(module, symbolStream);
                return new NativePdbReaderProvider().GetSymbolReader(module, symbolStream);

            default:
                return null;
        }
    }

    public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName) {
        return null;
    }

    public static bool IsPortablePdb(Stream stream) {
        long start = stream.Position;
        if (stream.Length - start < 4)
            return false;
        try {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
                return reader.ReadUInt32() == 0x424a5342;
        } finally {
            stream.Seek(start, SeekOrigin.Begin);
        }
    }

}