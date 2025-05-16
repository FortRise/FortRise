using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;

namespace FortLauncher;

internal abstract class Patcher
{
    public abstract PatcherScope Scope { get; }

    public void StartPatching(ModuleDefinition module)
    {
        if (Scope.HasFlag(PatcherScope.Assembly))
        {
            PatchAssembly(module);
        }

        if (Scope.HasFlag(PatcherScope.Type))
        {
            foreach (var type in module.Types)
            {
                PatchingType(type);
            }
        }
    }

    public void PatchingType(TypeDefinition type)
    {
        PatchType(type);
        if (Scope.HasFlag(PatcherScope.Property))
        {
            foreach (var property in type.Properties)
            {
                PatchProperty(property);
            }
        }

        if (Scope.HasFlag(PatcherScope.Field))
        {
            foreach (var field in type.Fields)
            {
                PatchField(field);
            }
        }

        if (Scope.HasFlag(PatcherScope.Method))
        {
            foreach (var method in type.GetMethods())
            {
                PatchMethod(method);
            }
        }

        if (Scope.HasFlag(PatcherScope.NestedType))
        {
            foreach (var ntype in type.NestedTypes)
            {
                PatchNestedType(ntype);
                PatchingType(ntype);
            }
        }
    }


    public virtual void PatchAssembly(ModuleDefinition module) { }
    public virtual void PatchType(TypeDefinition type) {}
    public virtual void PatchNestedType(TypeDefinition type) {}
    public virtual void PatchField(FieldDefinition field) {}
    public virtual void PatchProperty(PropertyDefinition property) {}
    public virtual void PatchMethod(MethodDefinition method) {}
}
