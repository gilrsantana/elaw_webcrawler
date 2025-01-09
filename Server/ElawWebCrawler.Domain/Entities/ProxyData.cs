namespace ElawWebCrawler.Domain.Entities;

public class ProxyData
{
    public string IpAddress { get; private set; }
    public string Port { get; private set; }
    public string Country { get; private set; }
    public string Protocol { get; private set; }
    
    public ProxyData(string ipAddress, string port, string country, string protocol)
    {
        IpAddress = ipAddress;
        Port = port;
        Country = country;
        Protocol = protocol;
    }
}