using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Infrastructure.Data.Config.AggregateConfigs.ClassSectionConfigs;

public class ClassSectionEnrollmentEligibilityValidationMessageConfiguration
  : IEntityTypeConfiguration<ClassSectionEnrollmentEligibilityValidationMessage>
{
  public void Configure(EntityTypeBuilder<ClassSectionEnrollmentEligibilityValidationMessage> builder)
  {
    builder.ToTable("ClassSectionEnrollmentEligibilityValidationMessages");

    builder.HasKey(e => e.Id);
    builder.Property(e => e.Id)
      .UseIdentityColumn()
      .IsRequired();

    builder.Property(e => e.ClassSectionId)
      .IsRequired();

    builder.Property(e => e.Code)
      .HasMaxLength(50)
      .IsRequired();

    builder.Property(e => e.Message)
      .HasMaxLength(255)
      .IsRequired();

    builder.Property(e => e.ComputedAt)
      .IsRequired();

    builder.HasOne<ClassSection>()
      .WithMany()
      .HasForeignKey(e => e.ClassSectionId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}


