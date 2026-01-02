using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.SubjectConfigs;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.Code).IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Units).IsRequired();
        builder.Property(e => e.Description).IsRequired();

        builder.HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.PreferRoomType)
            .WithMany()
            .HasForeignKey(e => e.PreferRoomTypeId)
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

        builder.OwnsMany<SubjectPrerequisite>(r => r.SubjectPrerequisites, p =>
        {
            p.ToTable("SubjectPrerequisites");

            p.WithOwner().HasForeignKey(e => e.SourceSubjectId);
            p.HasKey(e => new { e.SourceSubjectId, e.PrerequisiteSubjectId });

            p.Property(e => e.PrerequisiteSubjectId).IsRequired();


            p.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
            p.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
            p.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
            p.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
            p.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
            p.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
            p.Property(a => a.IsActive).HasColumnName("IsActive");

            // Foreign key relationships for audit fields
            p.HasOne(e => e.CreatedByUser)
              .WithMany()
              .HasForeignKey(e => e.CreatedBy)
              .OnDelete(DeleteBehavior.NoAction);

            p.HasOne(e => e.UpdatedByUser)
              .WithMany()
              .HasForeignKey(e => e.UpdatedBy)
              .OnDelete(DeleteBehavior.NoAction);

            p.HasOne(e => e.DeletedByUser)
              .WithMany()
              .HasForeignKey(e => e.DeletedBy)
              .OnDelete(DeleteBehavior.NoAction);
        });


    }
}
