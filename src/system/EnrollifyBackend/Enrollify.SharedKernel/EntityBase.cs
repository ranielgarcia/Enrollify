namespace Enrollify.SharedKernel;

/// <summary>
/// A base class for DDD Entities. Includes support for domain events dispatched post-persistence.
/// If you prefer GUID Ids, change it here.
/// If you need to support both GUID and int IDs, change to EntityBase&lt;TId&gt; and use TId as the type for Id.
/// </summary>
public abstract class EntityBase : HasDomainEventsBase
{
  public int Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public int? CreatedBy { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public int? UpdatedBy { get; set; }
  public DateTime? DeletedAt { get; set; }
  public int? DeletedBy { get; set; }
  public bool IsActive { get; set; } = true;
}

public abstract class EntityBase<TId> : HasDomainEventsBase
  where TId : struct, IEquatable<TId>
{
  public TId Id { get; set; } = default!;
  public DateTime CreatedAt { get; set; }
  public int? CreatedBy { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public int? UpdatedBy { get; set; }
  public DateTime? DeletedAt { get; set; }
  public int? DeletedBy { get; set; }
  public bool IsActive { get; set; } = true;
}

/// <summary>
/// For use with Vogen or similar tools for generating code for 
/// strongly typed Ids.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TId"></typeparam>
public abstract class EntityBase<T, TId> : HasDomainEventsBase
  where T : EntityBase<T, TId>
{
  public TId Id { get; set; } = default!;
  public DateTime CreatedAt { get; set; }
  public int? CreatedBy { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public int? UpdatedBy { get; set; }
  public DateTime? DeletedAt { get; set; }
  public int? DeletedBy { get; set; }
  public bool IsActive { get; set; } = true;
}

