# ASP.NET Core 8 開發實戰：部署維運篇

### **透過 IConfiguration 直接取得組態設定**

```json
// appsettings.json
{
    "AppSettings": {
        "SomeKey": "SomeValue"
    },
    "ConnectionStrings": {
        "DefaultConnection": "Server=.;Database=MyDatabase;Trusted_Connection=True;"
    }
}
```

```csharp
// 在建構式中注入 IConfiguration
public WeatherForecastController(ILogger<WeatherForecastController> logger,
        IConfiguration configuration)

// 取得組態設定
_config.GetValue<string>("AppSettings:SomeKey")

// 取得連線字串可以直接呼叫內部函數
_config.GetConnectionString("DefaultConnection")
```


### **正規強型別取得組態設定方式**

定義組態設定類別，命名最好以 Options 結尾

```csharp
public class AppSettingsOptions
{
    public const string SectionName = "AppSettings";
    public required string SomeKey { get; set; }
    public required string SmtpIp { get; set; }
    public int SmtpPort { get; set; } = 255;
}
```

在 `Program.cs` 中註冊組態設定類別並綁定組態設定

```csharp
builder.Services.Configure<AppSettingsOptions>(builder.Configuration.GetSection(AppSettingsOptions.SectionName));
```

在需要使用的地方注入組態設定類別

```csharp
public WeatherForecastController(ILogger<WeatherForecastController> logger, IOptions<AppSettingsOptions> appSettings)
```

透過 .Value 取得組態設定值

```csharp
_appSettings.Value.SmtpIp + ":" + _appSettings.Value.SmtpPort
```

### **使用環境變數覆蓋 appSettings.json**

```bash
export AppSettings__SmtpIp=smtp.outlook.com
export AppSettings__SmtpPort=785
./Api8        
```

### **使用指令參數覆蓋環境變數和 appSettings.json**

```bash
./Api8 --AppSettings:SmtpIp=smtp.haha.com --AppSettings:SmtpPort=777
```

