using Enrollify.Core.RoomTypeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enrollify.Infrastructure.Data.Config;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomTypes");

        builder.Property(e => e.Id)
          .HasConversion(
            id => id.Value,
            value => RoomTypeId.From(value))
          .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
          .HasVogenConversion()
          .HasMaxLength(RoomTypeName.MaxLength)
          .HasColumnType($"VARCHAR({RoomTypeName.MaxLength})")
          .IsRequired();

        builder.HasIndex(e => e.Name)
          .IsUnique();

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

        // Foreign key relationships (if User entity exists)
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