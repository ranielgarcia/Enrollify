using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Enrollify.Analyzers;

/// <summary>
/// Enforces DDD encapsulation rules on aggregate root entities:
/// <list type="bullet">
///   <item>ENRL002 – Every property declared on an aggregate root must have a private setter.</item>
///   <item>ENRL003 – Every aggregate root must declare a private parameterless constructor (required by EF Core).</item>
/// </list>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AggregateRootEncapsulationAnalyzer : DiagnosticAnalyzer
{
    public const string PrivateSetterDiagnosticId = "ENRL002";
    public const string PrivateConstructorDiagnosticId = "ENRL003";

    private static readonly DiagnosticDescriptor PrivateSetterRule = new(
        id: PrivateSetterDiagnosticId,
        title: "Aggregate root property must have a private setter",
        messageFormat: "Property '{0}' on aggregate root '{1}' must have a private setter",
        category: "Enrollify.DDD",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PrivateConstructorRule = new(
        id: PrivateConstructorDiagnosticId,
        title: "Aggregate root must have a private parameterless constructor",
        messageFormat: "Aggregate root '{0}' must have a private parameterless constructor",
        category: "Enrollify.DDD",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(PrivateSetterRule, PrivateConstructorRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        if (namedType.TypeKind != TypeKind.Class || namedType.IsAbstract)
            return;

        var iAggregateRoot = context.Compilation.GetTypeByMetadataName("Enrollify.SharedKernel.IAggregateRoot");
        if (iAggregateRoot == null)
            return;

        if (!namedType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iAggregateRoot)))
            return;

        CheckProperties(context, namedType);
        CheckPrivateParameterlessConstructor(context, namedType);
    }

    private static void CheckProperties(SymbolAnalysisContext context, INamedTypeSymbol namedType)
    {
        var properties = namedType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && !p.IsImplicitlyDeclared);

        foreach (var property in properties)
        {
            if (property.SetMethod == null)
                continue;

            if (property.SetMethod.DeclaredAccessibility != Accessibility.Private)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    PrivateSetterRule,
                    property.Locations.FirstOrDefault(),
                    property.Name,
                    namedType.Name));
            }
        }
    }

    private static void CheckPrivateParameterlessConstructor(SymbolAnalysisContext context, INamedTypeSymbol namedType)
    {
        var hasPrivateParameterless = namedType.Constructors
            .Any(c => !c.IsStatic
                      && c.Parameters.Length == 0
                      && c.DeclaredAccessibility == Accessibility.Private);

        if (!hasPrivateParameterless)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                PrivateConstructorRule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name));
        }
    }
}
