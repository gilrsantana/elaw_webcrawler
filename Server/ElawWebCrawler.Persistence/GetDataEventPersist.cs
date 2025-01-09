using ElawWebCrawler.Data;
using ElawWebCrawler.Domain.Entities;
using ElawWebCrawler.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElawWebCrawler.Persistence;

internal class GetDataEventPersist(WebCrawlerContext context) : IGetDataEventPersist
{
    public void Add(GetDataEvent entity)
    {
        context.Add(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<GetDataEvent>> GetAllAsync()
    {
        return await context.GetDataEvents.ToListAsync();
    }

    public async Task<GetDataEvent?> GetByIdAsync(string id)
    {
        return await context.GetDataEvents.FirstOrDefaultAsync(x => x.Id == id);
    }
}