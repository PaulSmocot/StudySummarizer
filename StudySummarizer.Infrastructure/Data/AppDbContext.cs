using Microsoft.EntityFrameworkCore;
using StudySummarizer.Domain.Entities;

namespace StudySummarizer.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Summary> Summaries => Set<Summary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Summary)
            .WithOne(s => s.Document)
            .HasForeignKey<Summary>(s => s.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
