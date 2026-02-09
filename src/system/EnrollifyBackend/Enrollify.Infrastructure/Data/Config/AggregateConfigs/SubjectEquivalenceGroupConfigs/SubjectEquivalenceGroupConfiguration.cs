using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.SubjectEquivalenceGroupConfigs;

public class SubjectEquivalenceGroupConfiguration : IEntityTypeConfiguration<SubjectEquivalenceGroup>
{
    public void Configure(EntityTypeBuilder<SubjectEquivalenceGroup> builder)
    {
        builder.ToTable("SubjectEquivalenceGroups");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.Name).IsRequired();


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
        builder.Navigation(c => c.SubjectEquivalences)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Configure SubjectEquivalences as an owned collection
        builder.OwnsMany<SubjectEquivalence>(g => g.SubjectEquivalences, se =>
        {
            se.ToTable("SubjectEquivalences");

            se.WithOwner().HasForeignKey(e => e.EquivalenceGroupId);

            se.HasKey(e => e.Id);
            se.Property(e => e.Id)
                .UseIdentityColumn()
                .IsRequired();
            se.Property(e => e.SubjectId).IsRequired();

            // Navigation to Subject
            se.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.NoAction);

            // Audit fields for CurriculumSubjects
            se.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
            se.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
            se.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
            se.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
            se.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
            se.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
            se.Property(a => a.IsActive).HasColumnName("IsActive");
            // Foreign key relationships for audit fields
            se.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            se.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            se.HasOne(e => e.DeletedByUser)
                .WithMany()
                .HasForeignKey(e => e.DeletedBy)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
