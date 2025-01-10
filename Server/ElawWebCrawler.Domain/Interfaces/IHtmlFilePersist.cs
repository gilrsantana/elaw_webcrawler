using ElawWebCrawler.Domain.Entities;

namespace ElawWebCrawler.Domain.Interfaces;

public interface IHtmlFilePersist
{
    void Add(HtmlFile entity);
    Task<bool> SaveChangesAsync();
    Task<IEnumerable<HtmlFile>> GetAllAsync();
    Task<IEnumerable<HtmlFile>> GetByRequestKeyAsync(string requestKey);
    
    Task<HtmlFile?> GetByIdAsync(string id);
}