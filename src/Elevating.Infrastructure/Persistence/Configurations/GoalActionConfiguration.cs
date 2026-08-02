using Elevating.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elevating.Infrastructure.Persistence.Configurations;

public sealed class GoalActionConfiguration
    : IEntityTypeConfiguration<GoalAction>
{
    public void Configure(
        EntityTypeBuilder<GoalAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("GoalActions");

        builder.HasKey(action => action.Id);

        builder.Property(action => action.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(action => action.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(action => action.Position)
            .IsRequired();

        builder.Property(action => action.CreatedDate)
            .IsRequired();

        builder.Property(action => action.UpdatedDate)
            .IsRequired();

        builder.HasOne(action => action.Goal)
            .WithMany(goal => goal.Actions)
            .HasForeignKey(action => action.GoalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(action => action.GoalId);

        builder.HasIndex(action => new
        {
            action.GoalId,
            action.Position
        });
    }
}