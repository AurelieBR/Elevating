using Elevating.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Elevating.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<GoalAction> GoalActions => Set<GoalAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}