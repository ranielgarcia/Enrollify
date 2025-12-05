# Enrollify Infrastructure Vogen Value Object Dapper Type Handler Source Generator

This project contains a source generator that automatically creates Dapper type handlers for Vogen value objects used in the Enrollify application. By leveraging this generator, developers can streamline the integration of Vogen value objects with Dapper, reducing boilerplate code and potential errors.

Instead of manually creating Dapper type handlers for each Vogen value object, this source generator scans the codebase for Vogen value objects and generates the necessary type handlers automatically.

## Without This Source Generator

```csharp
using Vogen;

[ValueObject<Guid>(conversions: Conversions.DapperTypeHandler)]
public partial struct CustomerId { }
```

Vogen auto-generates this for you:

```csharp
public sealed class DapperTypeHandler : SqlMapper.TypeHandler<CustomerId>
{
    public override CustomerId Parse(object value) => CustomerId.From((Guid)value);
    public override void SetValue(IDbDataParameter parameter, CustomerId value) => parameter.Value = value.Value;
}
```

Then you have to manually add the following for each value object

```csharp
SqlMapper.AddTypeHandler(new CustomerId.DapperTypeHandler());
```

Since we are following DDD and Clean Architecture in this system, we can't add (conversions: Conversions.DapperTypeHandler) on each value object
Core project don't need to know the details of the infrastructure

## With This Source Generator

With this source generator, it generates handler for each value object with ValueObjectAttribute 

e.g. 

```csharp
using Vogen;

[ValueObject<Guid>]
public partial struct CustomerId { }
```

This generate the following code


```csharp
using Dapper;
using MyProject.Domain;

public class CustomerIdHandler : SqlMapper.TypeHandler<CustomerId>
{
    public override void SetValue(IDbDataParameter parameter, CustomerId value)
        => parameter.Value = value.Value;

    public override CustomerId Parse(object value)
        => CustomerId.From((Guid)value);
}
```