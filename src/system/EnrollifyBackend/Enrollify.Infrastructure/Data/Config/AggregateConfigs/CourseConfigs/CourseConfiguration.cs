using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CourseConfigs;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(d => d.Id);
        builder.Property(e => e.Id)
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(d => d.Code).IsRequired();
        builder.Property(d => d.Name).IsRequired();
        builder.Property(d => d.DurationYears).IsRequired();
        builder.Property(d => d.Description).IsRequired();
        builder.Property(d => d.CollegeId).IsRequired();

        builder.HasOne(d => d.College)
               .WithMany()
               .HasForeignKey(d => d.CollegeId)
               .OnDelete(DeleteBehavior.NoAction);

        // Audit fields
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
