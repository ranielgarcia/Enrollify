using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.AcademicYearConfigs;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("AcademicYears");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
          .UseIdentityColumn()
          .IsRequired();

        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.EndDate).IsRequired();

        builder.Ignore(e => e.StartYear);
        builder.Ignore(e => e.EndYear);
        builder.Ignore(e => e.AcademicYearTitle);
        builder.Ignore(e => e.AcademicYearSlug);

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
        builder.Navigation(c => c.AcademicTerms)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<AcademicTerm>(at => at.AcademicTerms, atb =>
        {
            atb.ToTable("AcademicTerms");

            atb.WithOwner().HasForeignKey(e => e.AcademicYearId);

            atb.HasKey(at => at.Id);
            atb.Property(at => at.Id)
                .UseIdentityColumn()
                .IsRequired();


            atb.Property(e => e.TermNumber).IsRequired();
            atb.Ignore(e => e.TermName);
            atb.Property(e => e.AcademicYearId).IsRequired();


            // Audit fields
            atb.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
            atb.Property(a => a.CreatedBy).HasColumnName("CreatedBy");
            atb.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
            atb.Property(a => a.UpdatedBy).HasColumnName("UpdatedBy");
            atb.Property(a => a.DeletedAt).HasColumnName("DeletedAt");
            atb.Property(a => a.DeletedBy).HasColumnName("DeletedBy");
            atb.Property(a => a.IsActive).HasColumnName("IsActive");

            // Foreign key relationships for audit fields
            atb.HasOne(e => e.CreatedByUser)
              .WithMany()
              .HasForeignKey(e => e.CreatedBy)
              .OnDelete(DeleteBehavior.NoAction);
            atb.HasOne(e => e.UpdatedByUser)
              .WithMany()
              .HasForeignKey(e => e.UpdatedBy)
              .OnDelete(DeleteBehavior.NoAction);
            atb.HasOne(e => e.DeletedByUser)
              .WithMany()
              .HasForeignKey(e => e.DeletedBy)
              .OnDelete(DeleteBehavior.NoAction);
        });


    }
}
