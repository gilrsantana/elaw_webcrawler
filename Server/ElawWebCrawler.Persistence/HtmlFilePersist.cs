using ElawWebCrawler.Data;
using ElawWebCrawler.Domain.Entities;
using ElawWebCrawler.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElawWebCrawler.Persistence;

public class HtmlFilePersist(WebCrawlerContext context) : IHtmlFilePersist
{
    public void Add(HtmlFile entity)
    {
        context.Add(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<HtmlFile>> GetAllAsync()
    {
        return await context.HtmlFiles.ToListAsync();
    }

    public async Task<HtmlFile?> GetByIdAsync(string id)
    {
        return await context.HtmlFiles.FirstOrDefaultAsync(x => x.Id == id);
    }
}