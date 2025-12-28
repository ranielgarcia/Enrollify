using NJsonSchema.Generation;
using System.Reflection;
using Vogen;

namespace Enrollify.WebAPI.Plumbing;

/// <summary>
/// NSwag schema processor that maps Vogen value objects to their underlying primitive types in OpenAPI/Swagger
/// </summary>
public class VogenNSwagSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var type = context.Type;

        // Check if this type is a Vogen value object by looking for the ValueObjectAttribute
        var valueObjectAttribute = type.GetCustomAttribute<ValueObjectAttribute>();
        
        if (valueObjectAttribute != null)
        {
            // Get the underlying type from the ValueObject attribute
            var underlyingType = GetUnderlyingType(type);
            
            if (underlyingType != null)
            {
                // Replace the schema with the underlying type's schema
                var underlyingSchema = context.Generator.Generate(underlyingType, context.Resolver);
                
                // Copy properties from underlying schema to current schema
                context.Schema.Type = underlyingSchema.Type;
                context.Schema.Format = underlyingSchema.Format;
                context.Schema.Pattern = underlyingSchema.Pattern;
                context.Schema.MinLength = underlyingSchema.MinLength;
                context.Schema.MaxLength = underlyingSchema.MaxLength;
                context.Schema.Minimum = underlyingSchema.Minimum;
                context.Schema.Maximum = underlyingSchema.Maximum;
                context.Schema.ExclusiveMinimum = underlyingSchema.ExclusiveMinimum;
                context.Schema.ExclusiveMaximum = underlyingSchema.ExclusiveMaximum;
                
                // Remove the value property wrapper if it exists
                context.Schema.Properties.Clear();
                
                // Optionally add a description to indicate this was a value object
                if (string.IsNullOrEmpty(context.Schema.Description))
                {
                    context.Schema.Description = $"Value object wrapping {underlyingType.Name}";
                }
            }
        }
    }

    private Type? GetUnderlyingType(Type valueObjectType)
    {
        // Vogen value objects have a Value property with the underlying type
        var valueProperty = valueObjectType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        return valueProperty?.PropertyType;
    }
}
