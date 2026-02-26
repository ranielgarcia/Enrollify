using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Enrollify.Analyzers;

/// <summary>
/// Reports an error when an <c>Include</c> call loads a collection of <c>IAuditable</c>
/// entities without a <c>.Where(x =&gt; x.IsActive)</c> filter.
///
/// EF Core does not support global query filters on owned entity types, so every
/// specification that includes an owned <c>IAuditable</c> collection must apply
/// the soft-delete filter explicitly via a filtered include.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SoftDeleteFilterAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ENRL001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Missing soft-delete filter on Include of IAuditable collection",
        messageFormat: "Include of '{0}' is missing a .Where(x => x.IsActive) filter — owned IAuditable collections are not covered by the global query filter",
        category: "Enrollify.Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: "https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager#filtered-include");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Match:  .Include(x => x.Collection)
        // Skip:   .Include(x => x.Collection.Where(...))
        if (!TryGetIncludeMethodName(invocation, out var methodName))
            return;

        if (methodName != "Include")
            return;

        if (invocation.ArgumentList.Arguments.Count != 1)
            return;

        var argument = invocation.ArgumentList.Arguments[0].Expression;

        // Unwrap lambda: x => <body>
        ExpressionSyntax? lambdaBody = argument switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Body as ExpressionSyntax,
            ParenthesizedLambdaExpressionSyntax paren => paren.Body as ExpressionSyntax,
            _ => null
        };

        if (lambdaBody == null)
            return;

        // If the body is an invocation (e.g. x.Collection.Where(...)), a filter is already present.
        if (lambdaBody is InvocationExpressionSyntax)
            return;

        // The body must be a member access: x.Collection
        if (lambdaBody is not MemberAccessExpressionSyntax memberAccess)
            return;

        // Resolve the type of the collection member
        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess, context.CancellationToken);
        var collectionType = typeInfo.Type;
        if (collectionType == null)
            return;

        // Extract the element type from the generic collection
        var elementType = GetCollectionElementType(collectionType);
        if (elementType == null)
            return;

        // Check whether the element type implements IAuditable
        var iAuditableSymbol = context.Compilation.GetTypeByMetadataName("Enrollify.Core.IAuditable");
        if (iAuditableSymbol == null)
            return;

        if (!ImplementsInterface(elementType, iAuditableSymbol))
            return;

        var diagnostic = Diagnostic.Create(
            Rule,
            memberAccess.Name.GetLocation(),
            memberAccess.Name.Identifier.Text);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool TryGetIncludeMethodName(
        InvocationExpressionSyntax invocation,
        out string methodName)
    {
        methodName = string.Empty;

        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            methodName = memberAccess.Name.Identifier.Text;
            return true;
        }

        return false;
    }

    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
    {
        // Direct generic type: IReadOnlyCollection<T>, List<T>, etc.
        if (type is INamedTypeSymbol { IsGenericType: true } namedType
            && namedType.TypeArguments.Length == 1)
        {
            return namedType.TypeArguments[0];
        }

        // Fall back to IEnumerable<T> interface
        foreach (var iface in type.AllInterfaces)
        {
            if (iface.IsGenericType
                && iface.MetadataName == "IEnumerable`1"
                && iface.TypeArguments.Length == 1)
            {
                return iface.TypeArguments[0];
            }
        }

        return null;
    }

    private static bool ImplementsInterface(ITypeSymbol type, ITypeSymbol interfaceSymbol)
    {
        if (SymbolEqualityComparer.Default.Equals(type, interfaceSymbol))
            return true;

        return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceSymbol));
    }
}
