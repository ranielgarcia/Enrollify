using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionSubjectOfferingConfigs;

public class ClassSectionSubjectOfferingConfiguration : IEntityTypeConfiguration<ClassSectionSubjectOffering>
{
    public void Configure(EntityTypeBuilder<ClassSectionSubjectOffering> builder)
    {
        builder.ToTable("ClassSectionSubjectOffering");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(e => e.SubjectId)
            .IsRequired();

        builder.Property(e => e.SubjectUnitsOverride)
            .HasColumnType("decimal(3,1)");

        builder.Property(e => e.TeacherId)
            .IsRequired(false);

        builder.Property(e => e.ClassSectionId)
            .IsRequired();

        builder.Property(e => e.RoomId)
            .IsRequired(false);

        builder.Property(e => e.DaysPerWeek)
            .IsRequired().HasDefaultValue(1);

        builder.Property(e => e.HoursPerDay)
            .HasColumnType("decimal(3,1)")
            .IsRequired().HasDefaultValue(1);

        builder.Property(e => e.MaxNumberOfStudents);

        builder.Property(e => e.CurriculumSubjectId)
            .IsRequired();

        builder.Property(e => e.SnapshotSubjectCode)
            .HasMaxLength(SubjectCode.MaxLength)
            .IsRequired();

        builder.Property(e => e.SnapshotSubjectTitle)
            .IsRequired();

        builder.Property(e => e.SnapshotUnits)
            .HasColumnType("decimal(3,1)");

        builder.Property(e => e.SnapshotIsElective)
            .IsRequired();

        builder.Property(e => e.SnapshotElectiveGroupName)
            .IsRequired(false);

        // Navigation to Subject
        builder.HasOne(e => e.Subject)
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.NoAction);

        // Navigation to Teacher
        builder.HasOne(e => e.Teacher)
            .WithMany()
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);

        // Navigation to ClassSection
        builder.HasOne<Core.Aggregates.ClassSectionAggregate.ClassSection>()
            .WithMany()
            .HasForeignKey(e => e.ClassSectionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Navigation to Room
        builder.HasOne(e => e.Room)
            .WithMany()
            .HasForeignKey(e => e.RoomId)
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

        // EF Core can't add/remove items via a read-only collection,
        // so you tell EF to use the backing field instead of the property
        // This is mainly about materialization and change-tracking without requiring a public setter or a mutable collection property.
        builder.Navigation(c => c.ClassSchedules)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Configure ClassSchedules as owned collection
        builder.OwnsMany<ClassSchedule>(c => c.ClassSchedules, cs =>
        {
            cs.ToTable("ClassSchedules");

            cs.WithOwner().HasForeignKey(e => e.ClassSectionSubjectOfferingId);

            cs.HasKey(e => e.Id);
            cs.Property(e => e.Id)
                .UseIdentityColumn()
                .IsRequired();

            cs.Property(e => e.ClassSectionSubjectOfferingId)
                .IsRequired();

            cs.Property(e => e.DayOfWeek)
                .HasConversion(
                    v => v.Value,
                    v => Core.Constants.DayOfWeekEnum.FromValue(v))
                .HasColumnType("char(3)")
                .IsRequired();

            cs.Property(e => e.StartTime)
                .IsRequired();

            cs.Property(e => e.EndTime)
                .IsRequired();

            // Audit fields for ClassSchedules
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
        });
    }
}
