using ElawWebCrawler.Domain.Entities;

namespace ElawWebCrawler.Domain.Interfaces;

public interface IGetDataEventPersist
{
    void Add(GetDataEvent entity);
    Task<bool> SaveChangesAsync();
    Task<IEnumerable<GetDataEvent>> GetAllAsync();
    Task<GetDataEvent?> GetByIdAsync(string id);
}