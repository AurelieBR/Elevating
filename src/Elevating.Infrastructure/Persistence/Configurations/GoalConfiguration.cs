using Elevating.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elevating.Infrastructure.Persistence.Configurations;

public sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Goals");

        builder.HasKey(goal => goal.Id);

        builder.Property(goal => goal.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(goal => goal.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(goal => goal.Description)
            .HasMaxLength(2000);

        builder.Property(goal => goal.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(goal => goal.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(goal => goal.TargetDate);

        builder.Property(goal => goal.CreatedDate)
            .IsRequired();

        builder.Property(goal => goal.UpdatedDate)
            .IsRequired();

        builder.HasIndex(goal => goal.Status);

        builder.HasIndex(goal => goal.Category);
    }
}