namespace Api8.Services;

public class AllowedIpService : IAllowedIpService
{
    private readonly List<string> _allowedIps = new () { "127.0.0.1", "192.168.0.1" };

    public bool IsAllowedIp(string ip)
    {
        return _allowedIps.Contains(ip);
    }
}