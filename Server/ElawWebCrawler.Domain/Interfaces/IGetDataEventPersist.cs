using ElawWebCrawler.Domain.Entities;

namespace ElawWebCrawler.Domain.Interfaces;

public interface IGetDataEventPersist
{
    void Add(GetDataEvent entity);
    Task<bool> SaveChangesAsync();
    Task<IEnumerable<GetDataEvent>> GetAllAsync();
    Task<IEnumerable<GetDataEvent>> GetByRequestKeyAsync(string requestKey);
    Task<GetDataEvent?> GetByIdAsync(string id);
}