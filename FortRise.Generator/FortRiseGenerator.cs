#pragma warning disable RSEXPERIMENTAL002
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace FortRise.Generator;

[Generator]
internal class FortRiseGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => InterceptorPredicate(node),
                static (ctx, ct) =>
                {
                    return InterceptorTransform(ctx, ct);
                }
            )
            .Where(x => x is not null);


        var compilation = context.CompilationProvider.Combine(provider.Collect());
        context.RegisterSourceOutput(compilation,
            static (ctx, source) => ExecuteInterceptors(ctx, source.Right!, source.Left));
    }

    private static void ExecuteInterceptors(
        SourceProductionContext context,
        ImmutableArray<CandidateInvocation> toIntercept,
        Compilation compilation)
    {
        var sb = new StringBuilder();
        int i = 0;

        // Add the InterceptsLocationAttribute to the generated file,
        // along with the start of the interceptor
        sb.Append("""
                    #nullable enable
                    namespace System.Runtime.CompilerServices
                    {
                        [global::System.Diagnostics.Conditional("DEBUG")]
                        [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
                        sealed file class InterceptsLocationAttribute : global::System.Attribute
                        {
                            public InterceptsLocationAttribute(int version, string data)
                            {
                                _ = version;
                                _ = data;
                            }
                        }
                    }
                    
                    namespace FortRise.GeneratedAccess
                    {
                        static file class PrivateAccessGenerated
                        {
                    """);

        foreach (var invocation in toIntercept)
        {
            var location = invocation.Location;
            int version = location.Version; // 1
            string data = location.Data; // e.g. yxKJBEhzkHdnMhHGENjk8qgBAABQcm9ncmFtLmNz
            string displayLocation = location.GetDisplayLocation(); // e.g. Program.cs(19,32)

            var model = compilation.GetSemanticModel(invocation.TypeArgumentsList.SyntaxTree);

            string accessName = invocation.Arguments[0].ToFullString();
            string typeName = model.GetTypeInfo(invocation.TypeArgumentsList.Arguments[0]).Type!.ToDisplayString();
            string? retType = null;
            if (invocation.TypeArgumentsList.Arguments.Count == 2)
            {
                retType = model.GetTypeInfo(invocation.TypeArgumentsList.Arguments[1]).Type!.ToDisplayString();
            }

            string type = invocation.InvocationType.Name;

            sb.AppendLine(
            $"""        

                    [global::System.Runtime.CompilerServices.UnsafeAccessor(
                        global::System.Runtime.CompilerServices.UnsafeAccessorKind.{type},
                        Name = {accessName}
                    )]
            """);

            switch (invocation.InvocationType)
            {
                case FieldInvocationType field:
                    sb.AppendLine(
                    $"""
                            public static extern ref {retType} Field{i}({typeName} instance);
                    """
                    );

                    sb.AppendLine(
                    $"""
                            [global::System.Runtime.CompilerServices.InterceptsLocation({version}, "{data}")] // {displayLocation}
                    """
                    );

                    sb.AppendLine($$"""
                            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                            public static FastPrivateAccess<TReturn> Field{{i}}_Intercept<TBase, TReturn>(string name, TBase? instance) 
                            {
                                unsafe 
                                {
                                    ref var res = ref Field{{i}}((instance as {{typeName}})!);
                                    return new FastPrivateAccess<TReturn>(System.Runtime.CompilerServices.Unsafe.AsPointer<{{retType}}>(ref res));
                                }
                            }
                    """);
                    break;
                case MethodInvocationType method:
                    var tmodel = compilation.GetSemanticModel(invocation.ArgumentList.SyntaxTree);
                    var expr = method.Types.Expression;

                    if (expr is ImplicitArrayCreationExpressionSyntax implicitA)
                    {
                        var initializer = implicitA.Initializer;

                        foreach (var elm in initializer.Expressions)
                        {

                        }
                    }
                    else if (expr is ArrayCreationExpressionSyntax arrayA)
                    {
                        var initializer = arrayA.Initializer;
                        if (initializer is not null)
                        {
                            foreach (var elm in initializer.Expressions)
                            {

                            }
                        }

                    }
                    else if (expr is CollectionExpressionSyntax coll)
                    {
                        foreach (var elm in coll.Elements)
                        {
                        }
                    }


                    // if (t.HasValue)
                    // {
                    //     throw new Exception(t.Value.ToString());
                    // }

                    sb.AppendLine(
                    $"""
                            public static extern {retType ?? "void"} Method{i}({typeName} instance);
                    """
                    );

                    sb.AppendLine(
                    $"""
                            [global::System.Runtime.CompilerServices.InterceptsLocation({version}, "{data}")] // {displayLocation}
                    """
                    );

                    if (retType != null)
                    {
                        sb.AppendLine($$"""
                                public static FastPrivateAccess<TReturn> Method{{i}}_Intercept<TBase, TReturn>(string name, TBase? instance) 
                                {
                                    unsafe 
                                    {
                                        ref var res = ref Field{{i}}((instance as {{typeName}})!);
                                        return new FastPrivateAccess<TReturn>(System.Runtime.CompilerServices.Unsafe.AsPointer<{{retType}}>(ref res));
                                    }
                                }
                        """);
                    }
                    else
                    {
                        sb.AppendLine($$"""
                                public static void Method{{i}}_Intercept<TBase>(string name, TBase? instance, Type[] arguments) 
                                {
                                    Method{{i}}((instance as {{typeName}})!);
                                }
                        """);
                    }

                    break;
            }


            i += 1;
        }

        sb.AppendLine("""
            }
        }
        """);

        // Add the source to the compilation
        string contents = sb.ToString();
        string filename = $"FortRise_Private_Interception.g.cs";
        context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
        sb.Clear();
    }

    private static bool InterceptorPredicate(SyntaxNode node) =>
        node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.ValueText: "Field" or "Method"
            }
        };

    private static CandidateInvocation? InterceptorTransform(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        // Is this an instance method invocation? (we know it must be due to the predicate check, but play it safe)
        if (ctx.Node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: { } } } invocation
            // Get the semantic definition of the method invocation
            && ctx.SemanticModel.GetOperation(ctx.Node, ct) is IInvocationOperation targetOperation
            // This is the main check - is the method a ToString invocation on System.Enum.ToString()?
            && targetOperation.TargetMethod is
            {
                Name: "Field" or "Method",
                ContainingType:
                {
                    Name: "Private",
                    ContainingNamespace:
                    {
                        Name: "FortRise",
                        ContainingNamespace.IsGlobalNamespace: true
                    }
                }
            } method)
        {
            if (ctx.SemanticModel.GetInterceptableLocation(invocation) is { } location)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax expressionSyntax)
                {
                    if (expressionSyntax.Name is GenericNameSyntax genericName)
                    {
                        // Return the location details and the full type details
                        return new CandidateInvocation(
                            location,
                            invocation.ArgumentList.Arguments,
                            invocation.ArgumentList,
                            genericName.TypeArgumentList,
                            method.Name switch
                            {
                                "Field" => new FieldInvocationType(),
                                "Method" => new MethodInvocationType() { Types = invocation.ArgumentList.Arguments[2] },
                                _ => throw new NotSupportedException()
                            }
                        );
                    }
                }
            }
        }

        return null;
    }

    public record CandidateInvocation(
        InterceptableLocation Location,
        SeparatedSyntaxList<ArgumentSyntax> Arguments,
        ArgumentListSyntax ArgumentList,
        TypeArgumentListSyntax TypeArgumentsList,
        BaseInvocationType InvocationType
    );
}

internal abstract class BaseInvocationType
{
    public abstract string Name { get; }
}
internal sealed class FieldInvocationType : BaseInvocationType
{
    public override string Name => "Field";
}

internal sealed class MethodInvocationType : BaseInvocationType
{
    public override string Name => "Method";
    public required ArgumentSyntax Types;
}
