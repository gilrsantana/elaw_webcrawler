using ElawWebCrawler.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElawWebCrawler.Data;

public class WebCrawlerContext : DbContext
{
    public DbSet<GetDataEvent> GetDataEvents { get; set; }
    
    public WebCrawlerContext(DbContextOptions<WebCrawlerContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GetDataEvent>(e =>
        {
            e.ToTable("GetDataEvents");
            e.HasKey(x => x.Id)
                .HasName("PK_GetDataEvents");
            e.Property(x => x.Id)
                .HasColumnName("Id")
                .HasColumnType("varchar")
                .HasMaxLength(36)
                .IsRequired();
            e.Property(x => x.StartTime)
                .HasColumnType("datetime")
                .IsRequired();
            e.Property(x => x.EndTime)
                .HasColumnType("datetime")
                .IsRequired();
            e.Property(x => x.PagesCount)
                .HasColumnType("int")
                .IsRequired();
            e.Property(x => x.RowsCount)
                .HasColumnType("int")
                .IsRequired();
            e.Property(x => x.JsonFile)
                .HasColumnType("varchar(max)")
                .IsRequired();
        });
    }
}