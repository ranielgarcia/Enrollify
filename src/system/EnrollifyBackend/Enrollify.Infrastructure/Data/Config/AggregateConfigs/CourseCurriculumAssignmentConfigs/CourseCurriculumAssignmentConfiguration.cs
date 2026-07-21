using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CourseCurriculumAssignmentConfigs;

public class CourseCurriculumAssignmentConfiguration : IEntityTypeConfiguration<CourseCurriculumAssignment>
{
    public void Configure(EntityTypeBuilder<CourseCurriculumAssignment> builder)
    {
        builder.ToTable("CourseCurriculumAssignments");
        builder.HasKey(d => d.Id);
        builder.Property(e => e.Id)
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(d => d.CourseId).IsRequired();
        builder.Property(d => d.EntryAcademicYearId).IsRequired();
        builder.Property(d => d.CurriculumId).IsRequired();
        builder.Property(d => d.IsLocked).IsRequired(true).HasDefaultValue(false);
        builder.Property(d => d.LockRemarks).HasMaxLength(500);

        builder.HasOne(d => d.Course)
               .WithMany()
               .HasForeignKey(d => d.CourseId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.EntryAcademicYear)
               .WithMany()
               .HasForeignKey(d => d.EntryAcademicYearId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.Curriculum)
               .WithMany()
               .HasForeignKey(d => d.CurriculumId)
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
