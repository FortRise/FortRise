using System;
using System.Collections.Generic;
using System.IO;
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
    public static string GameRootPath { get; internal set; }
    public static string GameChecksum { get; internal set; }
    public static class Relinker 
    {
        private static bool temporaryASM;
        private static bool runtimeRulesParsed;
        private static ModuleMetadata currentMetaRelinking;
        public static List<Assembly> RelinkedAssemblies = new();
        private static Dictionary<string, ModuleDefinition> relinkedModules = new ();

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
                            Logger.Info($"Relinking {name}");
                        }
                        else 
                        {
                            Logger.Log($"Found unknown {name}", Logger.LogLevel.Warning);
                            int dot = name.IndexOf(".");
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
                            Logger.Log($"Remapped to {nameRelinked}", Logger.LogLevel.Info);
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
                    
                    Logger.Info($"Relinking {asmRef.Name}");

                    var asm = Assembly.Load(asmRef);
                    var module = ModuleDefinition.ReadModule(asm.Location, new ReaderParameters(ReadingMode.Immediate));
                    SharedRelinkModuleMap[asmRef.FullName] = SharedRelinkModuleMap[asmRef.Name] = module;
                    Type[] types = asm.GetExportedTypes();
                    for (int k = 0; k < types.Length; k++) 
                    {
                        var type = types[i];
                        var typeDef = module.GetType(type.FullName) ?? module.GetType(type.FullName.Replace('+', '/'));
                        if (typeDef == null)
                            continue;
                        SharedRelinkMap[typeDef.FullName] = typeDef;
                    }
                }

                return sharedRelinkMap;
            }
        }

        private static MonoModder modder;
        public static MonoModder Modder {
            get {
                if (modder != null)
                    return modder;

                modder = new MonoModder() {
                    CleanupEnabled = false,
                    RelinkModuleMap = SharedRelinkModuleMap,
                    RelinkMap = SharedRelinkMap,
                    DependencyDirs = {
                        GameRootPath
                    },
                    ReaderParameters = {
                        SymbolReaderProvider = new RelinkerSymbolReaderProvider()
                    }
                };

                return modder;
            }
            set {
                modder = value;
            }
        }

        public static Assembly GetRelinkedAssembly(
            ModuleMetadata meta, string pathToAssembly, Stream stream) 
        {
            var assemblyDirectories = Path.GetDirectoryName(pathToAssembly).Replace("\\", "/").Split('/');
            var lastDirectory = assemblyDirectories[assemblyDirectories.Length - 1];
            string asmName = Path.GetFileNameWithoutExtension(pathToAssembly);
            ModuleDefinition module = null;
            asmName = asmName.Replace(" ", "_");

            var dirPath = Path.Combine(GameRootPath, "Mods", "_RelinkerCache");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            var cachedPath = Path.Combine(dirPath, $"{lastDirectory}.{asmName}.dll");
            var cachedChecksumPath = cachedPath.Substring(0, cachedPath.Length - 4) + ".sum";

            var checksums = new string[2];
            checksums[0] = GameChecksum;
            checksums[1] = RiseCore.GetChecksum(ref stream).ToHexadecimalString();

            if (File.Exists(cachedPath) && File.Exists(cachedChecksumPath) && 
                ChecksumsEqual(checksums, File.ReadAllLines(cachedChecksumPath))) 
            {
                Logger.Info($"Loading cached assembly for {meta} - {asmName}");

                var mod = ModuleDefinition.ReadModule(cachedPath);
                try 
                {
                    var asm = Assembly.LoadFrom(cachedPath);
                    RelinkedAssemblies.Add(asm);

                    if (!relinkedModules.ContainsKey(mod.Assembly.Name.Name)) 
                    {
                        relinkedModules.Add(mod.Assembly.Name.Name, mod);
                        mod = null;
                    }
                    else 
                    {
                        Logger.Log($"Encountered a module name conflict loading cached assembly {meta} - {asmName} - {module.Assembly.Name}", Logger.LogLevel.Warning);
                    }
                    return asm;
                }
                catch (Exception e) 
                {
                    Logger.Error($"Failed loading {meta} - {asmName}");
                    Logger.Error(e.ToString());
                    return null;
                }
                finally 
                {
                    mod?.Dispose();
                }
            }

            var dependencyResolver = GenerateModDependencyResolver(meta);

            AssemblyResolveEventHandler resolver = (s, r) => 
            {
                var dep = dependencyResolver(Modder, Modder.Module, r.Name, r.FullName);
                if (dep != null) 
                {
                    return dep.Assembly;
                }
                
                if (r.FullName.ToLowerInvariant().Contains("fna") || r.FullName.ToLowerInvariant().Contains("xna")) 
                {
                    var asmRefs = typeof(TFGame).Assembly.GetReferencedAssemblies();
                    for (int i = 0; i < asmRefs.Length; i++) 
                    {
                        var asmRef = asmRefs[i];
                        if (!asmRef.FullName.ToLowerInvariant().Contains("xna") &&
                            !asmRef.FullName.ToLowerInvariant().Contains("fna") &&
                            !asmRef.FullName.ToLowerInvariant().Contains("monogame"))
                                continue;
                            
                        return ((DefaultAssemblyResolver)modder.AssemblyResolver).Resolve(AssemblyNameReference.Parse(asmRef.FullName));
                    }
                }
                return null;
            };

            try 
            {
                currentMetaRelinking = meta;
                MonoModder modder = Modder;
                modder.Input = stream;
                modder.OutputPath = cachedPath;
                modder.MissingDependencyResolver = dependencyResolver;

                var metaPath = meta.DLL.Substring(0, meta.DLL.Length - 4) + ".pdb";
                modder.ReaderParameters.SymbolStream = OpenSymbol(metaPath);
                modder.ReaderParameters.ReadSymbols = modder.ReaderParameters.SymbolStream != null;

                ((DefaultAssemblyResolver)modder.AssemblyResolver).ResolveFailure += resolver;

                if (modder.ReaderParameters.SymbolReaderProvider != null && modder.ReaderParameters.SymbolReaderProvider is RelinkerSymbolReaderProvider) 
                {
                    ((RelinkerSymbolReaderProvider)modder.ReaderParameters.SymbolReaderProvider).Format = DebugSymbolFormat.PDB;
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

                if (modder.ReaderParameters.SymbolReaderProvider != null && modder.ReaderParameters.SymbolReaderProvider is RelinkerSymbolReaderProvider) 
                {
                    ((RelinkerSymbolReaderProvider)modder.ReaderParameters.SymbolReaderProvider).Format = DebugSymbolFormat.Auto;
                }


                modder.MapDependencies();

                ModuleDefinition rulesDef = null;
                if (!runtimeRulesParsed) 
                {
                    runtimeRulesParsed = true;

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
                    if (File.Exists(rulesPath)) 
                    {
                        rulesDef = ModuleDefinition.ReadModule(rulesPath, new ReaderParameters(ReadingMode.Immediate));
                    }
                }


                if (rulesDef != null) 
                {
                    modder.MapDependencies(rulesDef);
                    modder.ParseRules(rulesDef);
                    rulesDef.Dispose();
                }
                modder.ParseRules(modder.Module);
                modder.AutoPatch();
                ISymbolWriterProvider symbolWriterProvider = modder.WriterParameters.SymbolWriterProvider;

                Retry:
                try 
                {
                    modder.WriterParameters.SymbolWriterProvider = symbolWriterProvider;
                    modder.WriterParameters.WriteSymbols = true;
                    modder.Write();
                }
                catch 
                {
                    try 
                    {
                        modder.WriterParameters.SymbolWriterProvider = null;
                        modder.WriterParameters.WriteSymbols = false;
                        modder.Write();
                    }
                    catch when (!temporaryASM) 
                    {
                        temporaryASM = true;
                        long stamp2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        dirPath = Path.Combine(Path.GetTempPath(), $"FortRise.{Path.GetFileNameWithoutExtension(dirPath)}.{stamp2}.dll");
                        modder.Module.Name += "." + stamp2;
                        modder.Module.Assembly.Name.Name += "." + stamp2;
                        modder.OutputPath =  dirPath;
                        goto Retry;
                    }
                }
                finally 
                {
                    modder.WriterParameters.SymbolWriterProvider = symbolWriterProvider;
                }


                module = modder.Module;
            }
            catch (Exception e) 
            {
                Logger.Error($"Failed Relinking {meta} - {asmName}");
                Logger.Error(e.ToString());
            }
            finally 
            {
                ((DefaultAssemblyResolver)Modder.AssemblyResolver).ResolveFailure -= resolver;
                Modder.ReaderParameters.SymbolStream?.Dispose();
                if (module != Modder.Module)
                    Modder.Module?.Dispose();
                Modder.Module = null;

                Modder.ClearCaches(moduleSpecific: true);
                // Modder.Dispose();
                // Modder = null;
            }

            try 
            {
                if (File.Exists(cachedChecksumPath))
                    File.Delete(cachedChecksumPath);
                
                if (!temporaryASM) 
                {
                    File.WriteAllLines(cachedChecksumPath, checksums);
                }

                if (Modder != null) 
                {
                    foreach(AssemblyNameReference aref in module.AssemblyReferences) 
                    {
                        if (Modder.DependencyCache.ContainsKey(aref.FullName))
                            Logger.Info($"dep. {module.Name} -> (({aref.FullName}), ({aref.Name})) found");
                        else
                            Logger.Info($"dep. {module.Name} -> (({aref.FullName}), ({aref.Name})) NOT FOUND");
                    }
                }

                var asm = Assembly.LoadFrom(cachedPath);
                RelinkedAssemblies.Add(asm);
                if (!relinkedModules.ContainsKey(module.Assembly.Name.Name)) 
                {
                    relinkedModules.Add(module.Assembly.Name.Name, module);
                    module = null;
                }
                else 
                {
                    Logger.Log($"Encountered a module name conflict loading cached assembly {meta} - {asmName} - {module.Assembly.Name}", Logger.LogLevel.Warning);
                }
                return asm;
            }
            catch (Exception e) 
            {
                Logger.Error($"Failed Loading {meta} - {asmName}");
                Logger.Error(e.ToString());
                return null;
            }
            finally 
            {
                module?.Dispose();
            }
        }


        private static MissingDependencyResolver GenerateModDependencyResolver(ModuleMetadata meta) 
        {
            if (!string.IsNullOrEmpty(Path.GetDirectoryName(meta.DLL))) 
            {
                return (mod, main, name, fullname) => 
                {
                    if (relinkedModules.TryGetValue(name, out ModuleDefinition def)) 
                    {
                        return def;
                    }

                    string path = name + ".dll";
                    if (!string.IsNullOrEmpty(meta.DLL))
                        path = Path.Combine(Path.GetDirectoryName(meta.DLL), path);
                    if (!File.Exists(path))
                        path = Path.Combine(meta.PathDirectory, path);
                    if (File.Exists(path)) 
                    {
                        return ModuleDefinition.ReadModule(path, mod.GenReaderParameters(false, path));
                    }
                    
                    Logger.Log($"Couldn't find the dependency {main.Name} -> {fullname}, ({name})");
                    return null;
                };
            }
            return null;
        }

        private static Stream OpenSymbol(string symbolPath) 
        {
            if (!string.IsNullOrEmpty(symbolPath) && File.Exists(symbolPath)) 
            {
                return File.OpenRead(symbolPath);
            }
            return null;
        }

        public static bool ChecksumsEqual(string[] a, string[] b) {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++) 
            {
                if (a[i].Trim() != b[i].Trim())
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