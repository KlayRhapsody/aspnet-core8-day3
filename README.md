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

### **使用 Process 環境變數**

在 Linux 下使用 process 環境變數只能透過 script 的方式

```bash
#!/bin/sh

echo $TMP

export TMP=123
sh ./script/process-env.sh # 123
TMP=234 sh ./script/process-env.sh # 234
```

### **環境變數**

* 系統環境變數，ex. /etc/profile
* 使用者環境變數，.profile
* Session 環境變數，Terminal
* Process 環境變數，Linux

### **使用 Configuration 相關注意事項**

* 幫定強型別物件綁定組態設定後，盡量不要將綁定後的物件註冊為單例，若注入的類別只有讀取還不會發生問題，但若有修改設定值的需求，則會影響到其他使用該物件的類別或是 Thread Safety 的問題
* 不用特別將 IConfiguration 物件加入 DI 容器也可注入組態物件

### **設定提供者的套用順序（依序套用設定**

* `AddJsonFile("appsettings.json",
  optional: true, reloadOnChange: true)`

* `AddJsonFile($"appsettings.{env.EnvironmentName}.json",
  optional: true, reloadOnChange: true)`

* `AddUserSecrets(appAssembly, optional: true)`
  - 這個設定預設只有在「開發環境」(Development) 才會套用
  - 透過 dotnet publish 發行的網站預設為 Production 環境

* `AddEnvironmentVariables()`

* `AddCommandLine(args)` // 優先權最高（會覆寫先前設定）


### **CI 作法**

* 在跑 CI 時，產生 appsettings.CI.json，並設定 CI 環境的設定值
* 在跑 CI 時，可以透過環境變數設定，例如：`export ASPNETCORE_ENVIRONMENT=CI`


### **`Configure<TOptions>` 和 `AddOptions<TOptions>().Bind(...)` 差異**

| **特性**                  | **Configure<TOptions>**                                           | **AddOptions<TOptions>().Bind(...)**                         |
|---------------------------|-------------------------------------------------------------------|-------------------------------------------------------------|
| **綁定方式**              | 自動綁定 `IConfiguration`。                                       | 手動綁定，需要呼叫 `Bind`。                                 |
| **使用便捷性**            | 更簡潔，適合單純綁定場景。                                         | 更靈活，適合需要額外處理（如驗證或後期配置）的場景。         |
| **支持驗證**              | 不直接支持驗證。                                                 | 可以鏈式呼叫 `.Validate(...)` 來進行驗證。                 |
| **用途**                  | 快速綁定常見的配置類型。                                          | 適用於需要定制行為的配置場景。                              |
| **結果（服務注入）**      | 添加 `IOptions<T>` 和 `IOptionsMonitor<T>`。                      | 同樣會添加 `IOptions<T>` 和 `IOptionsMonitor<T>`。         |

<br>
比較兩個場景

| **功能**                        | **`Configure<TOptions>`**                              | **`AddOptions<TOptions>().Bind(...)`**               |
|----------------------------------|-------------------------------------------------------|------------------------------------------------------|
| **簡單配置綁定**                | 是                                                    | 是                                                   |
| **驗證規則**                    | 否                                                    | 是（`Validate` 方法）                                |
| **後期處理（例如設置預設值）** | 否                                                    | 是（`PostConfigure` 方法）                           |
| **適用場景**                    | 配置很簡單，無需驗證或後期處理                         | 需要驗證或需要進一步的自定義行為                     |


### **驗證資料欄位屬性**

將類別屬性上加上對應的驗證屬性

```csharp
public class AppSettingsOptions
{
    public const string SectionName = "AppSettings";
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string SomeKey { get; set; }
    
    [Required]
    [RegularExpression(@"^\d{1,3}(\.\d{1,3}){3}$", ErrorMessage = "Invalid IP address format.")]
    public required string SmtpIp { get; set; }
    
    [Range(1, 65535, ErrorMessage = "Port number must be between 1 and 65535.")]
    public int SmtpPort { get; set; }
}
```

在 `Program.cs` 中註冊驗證，當發送請求時才會檢查是否符合驗證屬性

```csharp
builder.Services.AddOptions<AppSettingsOptions>()
    .Bind(builder.Configuration.GetSection(AppSettingsOptions.SectionName))
    .ValidateDataAnnotations();
```

啟動時就檢查是否符合驗證屬性，若不符合則會拋出例外，並中斷啟動

```csharp
builder.Services.AddOptions<AppSettingsOptions>()
    .Bind(builder.Configuration.GetSection(AppSettingsOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```