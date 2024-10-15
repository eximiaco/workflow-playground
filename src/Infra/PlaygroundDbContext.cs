
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WorkflowPlayground.Domain;

namespace WorkflowPlayground.Infra;

public class PlaygroundDbContext(DbContextOptions<PlaygroundDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}