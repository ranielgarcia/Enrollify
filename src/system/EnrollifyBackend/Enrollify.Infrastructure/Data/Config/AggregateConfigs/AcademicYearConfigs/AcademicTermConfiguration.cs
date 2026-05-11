using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.AcademicYearConfigs;

public class AcademicTermConfiguration : IEntityTypeConfiguration<AcademicTerm>
{
    public void Configure(EntityTypeBuilder<AcademicTerm> builder)
    {
        builder.ToTable("AcademicTerms");

        builder.HasKey(at => at.Id);
        builder.Property(at => at.Id)
            .UseIdentityColumn()
            .IsRequired();

        builder.Property(e => e.TermNumber).IsRequired();
        builder.Ignore(e => e.TermName);
        builder.Property(e => e.AcademicYearId).IsRequired();

        // Foreign key relationship to AcademicYear
        builder.HasOne<AcademicYear>()
            .WithMany(ay => ay.AcademicTerms)
            .HasForeignKey(at => at.AcademicYearId)
            .OnDelete(DeleteBehavior.Cascade);

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
