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

        builder.Property(e => e.RoomTypeId)
          .HasColumnName("RoomTypeId")
          .IsRequired();

        // Audit fields
        builder.ConfigureAuditFields();

        // Don't expose navigation property publicly
        builder.HasOne<RoomType>()
          .WithMany()
          .HasForeignKey(r => r.RoomTypeId);

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