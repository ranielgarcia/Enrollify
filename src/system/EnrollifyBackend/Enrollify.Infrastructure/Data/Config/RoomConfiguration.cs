using Enrollify.Core.RoomAggregate;
using Enrollify.Core.RoomTypeAggregate;

namespace Enrollify.Infrastructure.Data.Config;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.Property(e => e.Id)
          .HasConversion(
            id => id.Value,
            value => RoomId.From(value))
          .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
          .HasVogenConversion()
          .HasMaxLength(RoomName.MaxLength)
          .HasColumnType($"VARCHAR({RoomName.MaxLength})")
          .IsRequired();

        builder.Property(e => e.StudentCapacity)
          .IsRequired();


        // Add check constraint for positive capacity
        builder.ToTable(t => t.HasCheckConstraint("CHK_Rooms_StudentCapacity_Positive", "[StudentCapacity] > 0"));


        builder.Property(e => e.RoomTypeId)
          .HasConversion(
            id => id.Value,
            value => RoomTypeId.From(value))
          .HasColumnName("RoomTypeId")
          .IsRequired();

        // Audit fields
        builder.Property(e => e.CreatedAt)
          .HasColumnType("DATETIME2")
          .HasDefaultValueSql("SYSDATETIME()")
          .IsRequired();

        builder.Property(e => e.CreatedBy)
          .IsRequired(false);

        builder.Property(e => e.UpdatedAt)
          .HasColumnType("DATETIME2")
          .IsRequired(false);

        builder.Property(e => e.UpdatedBy)
          .IsRequired(false);

        builder.Property(e => e.DeletedAt)
          .HasColumnType("DATETIME2")
          .IsRequired(false);

        builder.Property(e => e.DeletedBy)
          .IsRequired(false);

        builder.Property(e => e.IsActive)
          .HasDefaultValue(true)
          .IsRequired();

        // Don't expose navigation property publicly
        builder.HasOne<RoomType>()
          .WithMany()
          .HasForeignKey(r => r.RoomTypeId)
          .HasConstraintName("FK_Rooms_RoomType")
          .OnDelete(DeleteBehavior.Restrict);

        // Foreign key relationships to Users (if User entity exists)
        // Uncomment when User entity is available
        /*
        builder.HasOne<User>()
          .WithMany()
          .HasForeignKey(e => e.CreatedBy)
          .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<User>()
          .WithMany()
          .HasForeignKey(e => e.UpdatedBy)
          .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<User>()
          .WithMany()
          .HasForeignKey(e => e.DeletedBy)
          .OnDelete(DeleteBehavior.NoAction);
        */
    }
}