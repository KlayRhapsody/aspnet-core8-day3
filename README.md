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

### **將 AppSettings 設定調整為多階層子屬性**

調整 Ip 和 Port 為 Smtp 的子屬性

```json
{
    "AppSettings": {
        "Smtp": {
            "Ip": "smtp.gmail.com",
            "Port": 587
        }
    }
}
```

對應調整 AppSettingsOptions 類別

```csharp
public class AppSettingsOptions
{
    public const string SectionName = "AppSettings";
    public required string SomeKey { get; set; }
    public required SmtpOptions Smtp { get; set; }
}

public class SmtpOptions
{
    public required string Ip { get; set; }
    public int Port { get; set; }
}
```

調整呼叫方式

```csharp
_appSettings.Value.Smtp.Ip + ":" + _appSettings.Value.Smtp.Port
```

### **新增自定義組態提供者**

在方案目錄下新增 `./ExternalConfig/appsettings.json`

```json
{
  "AppSettings": {
    "SomeKey": "SomeValue",
    "Smtp": {
      "Ip": "smtp.external.com",
      "Port": 8888
    }
  }
}
```

在 `Program.cs` 中註冊自定義組態提供者

```csharp
// 加入當前專案目錄在結合設定檔中設定的相對目錄位置
var externalConfigPath = Path.Combine(Directory.GetCurrentDirectory(), 
    builder.Configuration.GetValue<string>("ExternalAppSettings")!);

// 加入外部設定檔，並開啟自動重新載入和可選設定
// 若設定檔不存在則不會拋出例外
builder.Configuration.AddJsonFile(
    path: externalConfigPath, 
    optional: true, 
    reloadOnChange: true);
```
