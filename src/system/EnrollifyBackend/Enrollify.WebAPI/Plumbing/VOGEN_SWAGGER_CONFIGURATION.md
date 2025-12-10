# Vogen Configuration for Swagger/OpenAPI with FastEndpoints

## Problem
The Swagger/OpenAPI schema was showing Vogen value objects as complex objects with a `value` property instead of their underlying primitive types.

### Example of the Problem:
```json
"EnrollifyCoreAggregatesRoleAggregateRoleName": {
  "type": "object",
  "additionalProperties": false,
  "properties": {
    "value": {
      "type": "string"
    }
  }
}
```

### Desired Output:
```json
"EnrollifyCoreAggregatesRoleAggregateRoleName": {
  "type": "string"
}
```

## Solution

### 1. Created Assembly-Level Vogen Configuration
**File:** `Enrollify.Core\VogenConfiguration.cs`

```csharp
using Vogen;

// Configure Vogen defaults for the entire Enrollify.Core assembly
[assembly: VogenDefaults(
    staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon | StaticAbstractsGeneration.InstanceMethodsAndProperties,
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateOpenApiMappingExtensionMethod)]
```

**Note:** The `openApiSchemaCustomizations` setting is included for completeness, but it only works with Swashbuckle (Microsoft.OpenApi), not NSwag which FastEndpoints uses.

### 2. Removed Duplicate VogenDefaults from RoomTypeId.cs
Moved the assembly attribute from `RoomTypeId.cs` to the dedicated `VogenConfiguration.cs` file to follow best practices (assembly attributes should be in their own file).

### 3. Created Custom NSwag Schema Processor
**File:** `Enrollify.WebAPI\Plumbing\VogenNSwagSchemaProcessor.cs`

Since FastEndpoints uses **NSwag** (not Swashbuckle), we created a custom `ISchemaProcessor` that:
- Detects Vogen value objects by checking for the `ValueObjectAttribute`
- Extracts the underlying primitive type from the `Value` property
- Replaces the complex object schema with the simple primitive type schema

```csharp
public class VogenNSwagSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var type = context.Type;
        var valueObjectAttribute = type.GetCustomAttribute<ValueObjectAttribute>();
        
        if (valueObjectAttribute != null)
        {
            var underlyingType = GetUnderlyingType(type);
            
            if (underlyingType != null)
            {
                var underlyingSchema = context.Generator.Generate(underlyingType, context.Resolver);
                
                // Copy primitive type properties to the value object schema
                context.Schema.Type = underlyingSchema.Type;
                context.Schema.Format = underlyingSchema.Format;
                // ... etc
                
                // Remove the "value" property wrapper
                context.Schema.Properties.Clear();
            }
        }
    }
}
```

### 4. Registered the Schema Processor in FastEndpoints
**File:** `Enrollify.WebAPI\Plumbing\FastEndpointsRegistration.cs`

```csharp
services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Laundro API";
            s.Version = "v1";

            // Map Vogen value objects to their underlying primitive types in OpenAPI
            s.SchemaSettings.SchemaProcessors.Add(new VogenNSwagSchemaProcessor());
        };
    });
```

## How It Works

1. When FastEndpoints generates the OpenAPI/Swagger document, it processes each type through registered schema processors
2. Our `VogenNSwagSchemaProcessor` checks if a type is a Vogen value object
3. If it is, it replaces the complex object schema with the schema of the underlying primitive type
4. The resulting OpenAPI document now shows value objects as their underlying types (string, int, etc.)

## Testing

To verify the fix:

1. Run the application
2. Navigate to the Swagger UI (typically at `/swagger`)
3. Check the schema definitions for Vogen value objects like:
   - `RoomTypeId` should show as `integer` (underlying type: int)
   - `RoleName` should show as `string` (underlying type: string)
   - `RoleDescription` should show as `string` (underlying type: string)
   - `UserEmail` should show as `string` (underlying type: string)

## Why This Approach?

- **Swashbuckle vs NSwag:** Vogen's built-in OpenAPI support is designed for Swashbuckle, but FastEndpoints uses NSwag
- **Custom Processor:** By implementing `ISchemaProcessor`, we hook into NSwag's schema generation pipeline
- **Type Detection:** We use reflection to detect Vogen value objects and extract their underlying types
- **Clean Schema:** The resulting OpenAPI schema is clean and shows primitives, making the API easier to consume

## References

- [Vogen OpenAPI Documentation](https://stevedunn.github.io/Vogen/use-in-swagger.html)
- FastEndpoints uses NSwag for OpenAPI generation
- NSwag Schema Processors: https://github.com/RicoSuter/NJsonSchema/wiki/Schema-Processors
