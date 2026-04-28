using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.TeacherConfigs;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.FirstName).IsRequired();
        builder.Property(e => e.MiddleName).IsRequired();
        builder.Property(e => e.LastName).IsRequired();

        builder.Property(e => e.TeacherIdentifier).IsRequired();
        builder.Property(e => e.Email).IsRequired();
        builder.Property(e => e.PhoneNumber).IsRequired();

        builder.Property(e => e.AcademicTitle).IsRequired(false);
        builder.Property(e => e.Qualification).IsRequired(false);
        builder.Property(e => e.Specialization).IsRequired(false);
        builder.Property(e => e.OfficeLocation).IsRequired(false);
        builder.Property(e => e.OfficeHours).IsRequired(false);
        builder.Property(e => e.Biography).IsRequired(false);

        // Owned type for Photo
        builder.OwnsOne(e => e.Photo, photo =>
        {
            photo.Property(p => p.Filename).HasColumnName("PhotoFilename").IsRequired(false);
            photo.Property(p => p.ContentType).HasColumnName("PhotoContentType").IsRequired(false);
        });

        // Navigation to Department
        builder.HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
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
