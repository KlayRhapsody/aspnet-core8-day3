using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(HostingStartupLibrary.MyHostingStartup))]

namespace HostingStartupLibrary;

public class MyHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
           var settings = new Dictionary<string, string>
           {
                {"InjectSetting:AppName", "YouCantFindMe"},
                {"InjectSetting:AppVersion", "1.0"},
                {"InjectSetting:AppDescription", "This is a test app."}
           };

           config.AddInMemoryCollection(settings!); 
        });

        // builder.ConfigureServices(services =>
        // {
        //     services.AddScoped<IMyService, MyService>();
        // });
    }
}

public interface IMyService
{
    string GetMessage();
}

public class MyService : IMyService
{
    private readonly IConfiguration _config;

    public MyService(IConfiguration config)
    {
        _config = config;
    }

    public string GetMessage()
    {
        // return "Hello from HostingStartup Library!";
        return $"AppName: {_config["InjectSetting:AppName"]}, " +
               $"AppVersion: {_config["InjectSetting:AppVersion"]}, " +
               $"AppDescription: {_config["InjectSetting:AppDescription"]}";
    }
}