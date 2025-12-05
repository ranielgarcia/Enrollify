using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enrollify.Infrastructure.Generator;

/// <summary>
/// Generates Dapper type handlers for SmartEnum types (including SmartFlagEnum) at compile time using source generators.
/// </summary>
/// <remarks>This generator scans the compilation for types that inherit from SmartEnum or SmartFlagEnum and
/// produces Dapper type handler classes for each detected enum. It also generates a registration class to
/// simplify the process of registering all generated type handlers with Dapper. Use this generator to enable seamless
/// mapping of SmartEnum types in Dapper queries and commands.</remarks>
[Generator]
public class DapperSmartEnumTypeHandlerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get the compilation
        var compilation = context.CompilationProvider;

        // Generate type handlers from compilation (includes referenced assemblies)
        context.RegisterSourceOutput(compilation, static (spc, compilation) =>
        {
            var smartEnumTypes = GetAllSmartEnumTypes(compilation);

            foreach (var type in smartEnumTypes)
            {
                var handlerName = $"{type.SimpleName}DapperTypeHandler";
                string source = GenerateTypeHandler(handlerName, type.FullName, type.ValueType, type.Namespace, type.IsSmartFlagEnum);
                spc.AddSource($"{handlerName}.g.cs", source);
            }

            // Generate registration class
            if (smartEnumTypes.Count > 0)
            {
                string registrationSource = GenerateRegistrationClass(smartEnumTypes);
                spc.AddSource("SmartEnumDapperTypeHandlerRegistration.g.cs", registrationSource);
            }
        });
    }

    private static List<SmartEnumTypeInfo> GetAllSmartEnumTypes(Compilation compilation)
    {
        var smartEnumTypes = new List<SmartEnumTypeInfo>();

        // Scan all types in the compilation (including referenced assemblies)
        var allTypes = GetAllTypes(compilation.GlobalNamespace);

        foreach (var type in allTypes)
        {
            if (type.TypeKind != TypeKind.Class || type.IsAbstract)
                continue;

            var smartEnumInfo = GetSmartEnumInfo(type);
            if (smartEnumInfo != null)
            {
                smartEnumTypes.Add(smartEnumInfo);
            }
        }

        return smartEnumTypes;
    }

    private static SmartEnumTypeInfo? GetSmartEnumInfo(INamedTypeSymbol type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType)
            {
                var genericTypeDef = baseType.ConstructedFrom;
                var baseTypeName = genericTypeDef.ToDisplayString();

                // Check for SmartEnum<T> or SmartEnum<T, TValue>
                if (baseTypeName.StartsWith("Ardalis.SmartEnum.SmartEnum<"))
                {
                    var valueType = baseType.TypeArguments.Length > 1 
                        ? baseType.TypeArguments[1].ToDisplayString() 
                        : "int";

                    return new SmartEnumTypeInfo
                    {
                        FullName = type.ToDisplayString(),
                        SimpleName = type.Name,
                        ValueType = valueType,
                        Namespace = type.ContainingNamespace.ToDisplayString(),
                        IsSmartFlagEnum = false
                    };
                }

                // Check for SmartFlagEnum<T>
                if (baseTypeName.StartsWith("Ardalis.SmartEnum.SmartFlagEnum<"))
                {
                    return new SmartEnumTypeInfo
                    {
                        FullName = type.ToDisplayString(),
                        SimpleName = type.Name,
                        ValueType = "int",
                        Namespace = type.ContainingNamespace.ToDisplayString(),
                        IsSmartFlagEnum = true
                    };
                }
            }
            baseType = baseType.BaseType;
        }

        return null;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;

            // Recursively get nested types
            foreach (var nestedType in GetNestedTypes(type))
            {
                yield return nestedType;
            }
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(childNamespace))
            {
                yield return type;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nestedType in type.GetTypeMembers())
        {
            yield return nestedType;

            foreach (var nested in GetNestedTypes(nestedType))
            {
                yield return nested;
            }
        }
    }

    private static string GenerateTypeHandler(string handlerName, string enumType, string valueType, string enumNamespace, bool isSmartFlagEnum)
    {
        var parseMethod = isSmartFlagEnum
            ? $"return System.Linq.Enumerable.Single({enumType}.FromValue(({valueType})value));"
            : $"return {enumType}.FromValue(({valueType})value);";

        return $@"// <auto-generated/>
using System.Data;
using Dapper;
using {enumNamespace};

namespace Enrollify.Infrastructure.Data.Dapper.Generated
{{
    /// <summary>
    /// Dapper type handler for {enumType}
    /// </summary>
    public sealed class {handlerName} : SqlMapper.TypeHandler<{enumType}>
    {{
        public override void SetValue(IDbDataParameter parameter, {enumType} value)
        {{
            parameter.Value = value.Value;
        }}

        public override {enumType} Parse(object value)
        {{
            {parseMethod}
        }}
    }}
}}";
    }

    private static string GenerateRegistrationClass(IEnumerable<SmartEnumTypeInfo> smartEnumTypes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("using Dapper;");
        sb.AppendLine();
        
        // Add distinct namespaces
        var namespaces = smartEnumTypes.Select(t => t.Namespace).Distinct().OrderBy(ns => ns);
        foreach (var ns in namespaces)
        {
            sb.AppendLine($"using {ns};");
        }
        
        sb.AppendLine();
        sb.AppendLine("namespace Enrollify.Infrastructure.Data.Dapper.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Auto-generated registration for SmartEnum Dapper type handlers");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static class SmartEnumDapperTypeHandlerRegistration");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Registers all SmartEnum type handlers with Dapper");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public static void RegisterTypeHandlers()");
        sb.AppendLine("        {");
        
        foreach (var type in smartEnumTypes.OrderBy(t => t.FullName))
        {
            sb.AppendLine($"            SqlMapper.AddTypeHandler(new {type.SimpleName}DapperTypeHandler());");
        }
        
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    class SmartEnumTypeInfo
    {
        public string FullName { get; set; } = string.Empty;
        public string SimpleName { get; set; } = string.Empty;
        public string ValueType { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public bool IsSmartFlagEnum { get; set; }
    }
}
