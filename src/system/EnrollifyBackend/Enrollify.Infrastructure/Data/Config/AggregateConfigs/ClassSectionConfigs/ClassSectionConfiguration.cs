using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionConfigs;

public class ClassSectionConfiguration : IEntityTypeConfiguration<ClassSection>
{
    public void Configure (EntityTypeBuilder<ClassSection> builder)
    {
        builder.ToTable("ClassSections");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Ignore(x => x.FullName);

        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.YearLevel).IsRequired();

        builder.HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.Curriculum)
            .WithMany()
            .HasForeignKey(e => e.CurriculumId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.AcademicTerm)
            .WithMany()
            .HasForeignKey(e => e.AcademicTermId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.Adviser)
            .WithMany()
            .HasForeignKey(e => e.AdviserId)
            .IsRequired(false)
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
