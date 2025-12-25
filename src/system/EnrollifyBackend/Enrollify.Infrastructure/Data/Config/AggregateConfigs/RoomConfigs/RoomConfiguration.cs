using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoomConfigs;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .ValueGeneratedOnAdd()
          .IsRequired();

        builder.Property(e => e.RoomNumber).IsRequired();
        builder.Property(e => e.Capacity).IsRequired();
        builder.Property(e => e.RoomTypeId).IsRequired();
        builder.Property(e => e.BuildingId).IsRequired();
        builder.Property(e => e.CollegeId).IsRequired();

        // Don't expose navigation property publicly
        builder.HasOne<RoomType>()
          .WithMany()
          .HasForeignKey(r => r.RoomTypeId)
          .OnDelete(DeleteBehavior.NoAction);

        builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
        builder.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
        builder.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
        builder.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
        builder.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
        builder.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
        builder.Property(a => a.IsActive).HasColumnName("IsActive");

        // Foreign key relationships for audit fields
        builder.HasOne(e => e.CreatedByUser)
          .WithMany()
          .HasForeignKey(e => e.CreatedBy)
          .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.UpdatedByUser)
          .WithMany()
          .HasForeignKey(e => e.UpdatedBy)
          .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.DeletedByUser)
          .WithMany()
          .HasForeignKey(e => e.DeletedBy)
          .OnDelete(DeleteBehavior.NoAction);

    }
}