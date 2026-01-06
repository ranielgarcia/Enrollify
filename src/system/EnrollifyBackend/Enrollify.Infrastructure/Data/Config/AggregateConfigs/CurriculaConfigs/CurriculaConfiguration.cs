using Enrollify.Core.Aggregates.CurriculaAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.CurriculaConfigs;

public class CurriculaConfiguration : IEntityTypeConfiguration<Curricula>
{
    public void Configure(EntityTypeBuilder<Curricula> builder)
    {
        builder.ToTable("Curricula");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(e => e.CourseId)
            .IsRequired();

        builder.Property(e => e.EffectiveYear)
            .IsRequired();

        builder.Property(e => e.Version)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.StatusId)
            .HasColumnName("StatusId")
            .HasConversion(
                v => v.Value,
                v => Core.Constants.CurriculaStatusEnum.FromValue(v))
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.ApprovedDate);

        // Navigation to Course
        builder.HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.NoAction);

        // EF Core can't add/remove items via a read-only collection,
        // so you tell EF to use the backing field instead of the property
        // This is mainly about materialization and change-tracking without requiring a public setter or a mutable collection property.
        builder.Navigation(c => c.CurriculumSubjects)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

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

        // Configure CurriculumSubjects as owned collection
        builder.OwnsMany<CurriculumSubject>(c => c.CurriculumSubjects, cs =>
        {
            cs.ToTable("CurriculumSubjects");

            cs.WithOwner().HasForeignKey(e => e.CurriculumId);

            cs.HasKey(e => e.Id);
            cs.Property(e => e.Id)
                .UseIdentityColumn()
                .IsRequired();

            cs.Property(e => e.SubjectId)
                .IsRequired();

            cs.Property(e => e.YearLevel)
                .IsRequired();

            cs.Property(e => e.TermNumber)
                .IsRequired();

            cs.Property(e => e.IsElective)
                .IsRequired();

            cs.Property(e => e.ElectiveGroupName)
                .HasMaxLength(50);

            // Navigation to Subject
            cs.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.NoAction);

            // Audit fields for CurriculumSubjects
            cs.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
            cs.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
            cs.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
            cs.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
            cs.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
            cs.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
            cs.Property(a => a.IsActive).HasColumnName("IsActive");

            // Foreign key relationships for audit fields
            cs.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            cs.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            cs.HasOne(e => e.DeletedByUser)
                .WithMany()
                .HasForeignKey(e => e.DeletedBy)
                .OnDelete(DeleteBehavior.NoAction);

                // Configure CurriculumSubjectPrerequisites as nested owned collection
                cs.OwnsMany<CurriculumSubjectPrerequisite>(s => s.Prerequisites, csp =>
                {
                    csp.ToTable("CurriculumSubjectPrerequisites");

                    csp.WithOwner().HasForeignKey(e => e.CurriculumSubjectId);

                    csp.HasKey(e => new { e.CurriculumSubjectId, e.PrerequisiteCurriculumSubjectId });

                    csp.Property(e => e.PrerequisiteCurriculumSubjectId)
                        .IsRequired();

                    csp.Property(e => e.MinimumGrade)
                        .HasColumnType("decimal(3,2)");

                    // Audit fields for CurriculumSubjectPrerequisites
                    csp.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
                    csp.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
                    csp.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
                    csp.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
                    csp.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
                    csp.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
                    csp.Property(a => a.IsActive).HasColumnName("IsActive");

                    // Foreign key relationships for audit fields
                    csp.HasOne(e => e.CreatedByUser)
                        .WithMany()
                        .HasForeignKey(e => e.CreatedBy)
                        .OnDelete(DeleteBehavior.NoAction);

                    csp.HasOne(e => e.UpdatedByUser)
                        .WithMany()
                        .HasForeignKey(e => e.UpdatedBy)
                        .OnDelete(DeleteBehavior.NoAction);

                    csp.HasOne(e => e.DeletedByUser)
                        .WithMany()
                        .HasForeignKey(e => e.DeletedBy)
                        .OnDelete(DeleteBehavior.NoAction);
                });

                // Configure navigation property access mode after OwnsMany is defined
                cs.Navigation(s => s.Prerequisites)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });
    }
}
