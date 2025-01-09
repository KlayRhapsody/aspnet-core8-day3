namespace Api8.Services;

public interface IAllowedIpService
{
    bool IsAllowedIp(string ip);
}
