using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.RoomTypeConfigs;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomTypes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .HasVogenConversion()
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.Name)
          .IsRequired();

        builder.HasIndex(e => e.Name)
          .IsUnique();

        // Audit fields
        builder.ConfigureAuditFields();

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