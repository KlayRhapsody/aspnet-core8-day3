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


### **`IOptionsSnapshot<TOptions>`**
- **特性**：
  - 針對 **每個請求（Request）** 都會重新計算一次選項值。
  - 是一個 **有限範圍（Scoped）服務**，只能用在像 HTTP 請求這樣的暫時性環境中。
  - 在請求期間，**值是固定的，不會變動**。
  - 如果配置提供者（如 `IConfiguration`）支持動態更新，**選項值會在新的請求開始時更新**。

- **優點**：
  - 適合在每個請求中需要一致的配置值的場景（例如：Web API 的請求）。
  - 支持在應用程式運行期間讀取更新的設定值。

- **缺點**：
  - 因為每個請求都要重新計算選項值，可能導致效能損失，尤其是在高流量環境下。


### **`IOptionsMonitor<TOptions>`**
- **特性**：
  - 是一個 **單例（Singleton）服務**，所有地方共享同一個選項值。
  - **隨時可以取得最新的選項值**，特別適合用於單例服務的場景。
  - 當配置更新時，會立即反映新的值，並可設置回調函式來處理配置變更。

- **優點**：
  - 效能較好，因為選項值是全局共享的。
  - 適合需要即時響應配置變更的場景，例如背景服務。

- **缺點**：
  - 在請求處理期間，如果配置變更，可能導致獲取的值不一致。


### **`IOptionsSnapshot<TOptions>` 和 `IOptionsMonitor<TOptions>`兩者的主要差異**
| **特性**                 | **`IOptionsMonitor<TOptions>`**               | **`IOptionsSnapshot<TOptions>`**          |
|--------------------------|-----------------------------------------------|-------------------------------------------|
| **服務範圍**             | Singleton（全域共享）                        | Scoped（每個請求範圍內）                  |
| **值更新頻率**           | 即時更新                                     | 每個請求重新計算一次                      |
| **適合場景**             | 單例服務、背景工作（需要最新值）              | 每個請求需要一致的值（例如 Web API 請求） |
| **效能**                 | 效能較好，因為值是共用的                     | 效能較低，因為每次請求都重新計算          |


### **使用 `IValidateOptions<TOptions>` 與 `IValidatableObject` 差異**

| **特性**                          | **`AppSettingsValidation`**                                | **`IValidatableObject`**                            |
|-----------------------------------|-----------------------------------------------------------|----------------------------------------------------|
| **責任分離**                      | 驗證邏輯與配置類分離，符合單一責任原則                     | 驗證邏輯與配置類耦合，不符合單一責任原則           |
| **靈活性**                        | 支援多個驗證器，便於擴展                                   | 所有驗證邏輯必須集中在配置類內                     |
| **依賴注入支持**                  | 可以訪問其他依賴服務（透過建構函數注入到驗證類別中）       | 無法直接訪問依賴服務，只能使用配置類的上下文       |
| **測試**                          | 驗證邏輯獨立，易於單元測試                                 | 驗證邏輯內嵌，測試可能需要模擬整個配置類           |
| **使用場景**                      | 適合複雜驗證邏輯或需要多種驗證器的場景                     | 適合簡單的驗證邏輯，且驗證與配置類密切相關的場景   |
| **與 Data Annotations 的整合性** | 不直接整合，但可以與 `ValidateDataAnnotations()` 並存      | 與 Data Annotations 無縫整合                       |


### **Log 改為使用 Json 格式輸出**

```csharp
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Indented = true,
    };
    options.UseUtcTimestamp = true;
});
```

Log 輸出結果, 透過 `Scopes` 屬性可以看到相關的資訊

```json
{
  "Timestamp": "2025-01-09 09:10:43",
  "EventId": 1814918596,
  "LogLevel": "Critical",
  "Category": "WFCon",
  "Message": "Critical log",
  "State": {
    "Message": "Critical log",
    "{OriginalFormat}": "Critical log"
  },
  "Scopes": [
    {
      "Message": "SpanId:68f7724439bccd99, TraceId:c5738a15eeea4a0d230c031c5cc5abc4, ParentId:0000000000000000",
      "SpanId": "68f7724439bccd99",
      "TraceId": "c5738a15eeea4a0d230c031c5cc5abc4",
      "ParentId": "0000000000000000"
    },
    {
      "Message": "ConnectionId:0HN9GHLVEH8HL",
      "ConnectionId": "0HN9GHLVEH8HL"
    },
    {
      "Message": "RequestPath:/weatherforecast/ RequestId:0HN9GHLVEH8HL:00000001",
      "RequestId": "0HN9GHLVEH8HL:00000001",
      "RequestPath": "/weatherforecast/"
    },
    {
      "Message": "Api8.Controllers.WeatherForecastController.Get (Api8)",
      "ActionId": "9f6eed45-4b37-4283-9b31-ef14e02cd6ad",
      "ActionName": "Api8.Controllers.WeatherForecastController.Get (Api8)"
    }
  ]
}
```


### **自定義 Log Category**

```csharp
public WeatherForecastController(ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IOptionsSnapshot<AppSettingsOptions> appSettings)
{

}

var logger = _loggerFactory.CreateLogger(CategoryName);

// 使用 EventId 來識別不同的 Log
logger.LogInformation(eventId, "✅ Information log: {SmtpIp}, {SmtpPort}", 
            _appSettings.Value.SmtpIp,
            _appSettings.Value.SmtpPort);
```


### **透過 BeginScope 來將部分結構化日誌加入 Scope**

可在 Middleware 中透過 `BeginScope` 來加入 Scope

```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["RequestType"] = context.Request.Method,
    ["RequestQueryString"] = context.Request.QueryString
}))
{
    await _next(context);
}
```

Log 輸出結果

```json
{
  "Message": "System.Collections.Generic.Dictionary\u00602[System.String,System.Object]",
  "RequestType": "GET",
  "RequestQueryString": "?city=Hanoi\u0026date=2021-09-01"
}
```


### **透過 `appsettings.json` 設定 Serilog Log**

以下設定有額外使用 Serilog.Expressions 套件
將 WriteTo 設定為 Console 和 File，並設定格式化輸出

注意事項：
* File 中無法在 appsettings.json 中設定 encoding 屬性（需透過程式碼設定）
* File 設定需拿掉色碼設定

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft.AspNetCore.Mvc": "Warning",
      "Microsoft.AspNetCore.Routing": "Warning",
      "Microsoft.AspNetCore.Hosting": "Warning"
    }
  },
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "formatter": {
          "type": "Serilog.Templates.ExpressionTemplate, Serilog.Expressions",
          "template": "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
          "theme": "Serilog.Templates.Themes.TemplateTheme::Code, Serilog.Expressions"
        }
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "Log/log-.txt",
        "formatter": {
          "type": "Serilog.Templates.ExpressionTemplate, Serilog.Expressions",
          "template": "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}"
        },
        "rollingInterval": "Day",
        "buffered": true,
        "flushToDiskInterval": "00:00:01"
      }
    }
  ]
}
```

兩階段初始化 (Two-stage Initialization)
* 第一階段：先啟動一個簡單的 Serilog Logger（Bootstrap Logger），捕捉應用程式啟動過程的錯誤。
* 第二階段：ASP.NET Core 啟動完成後，切換成完整設定的 Logger

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
```


### **`ReadFrom.Services(services)` 的用途**

* 整合 DI：讓 Serilog 自動載入註冊在 DI 容器的服務（如 Enricher、Sink、Filter）。
* 動態設定：支援動態切換日誌層級（如 LoggingLevelSwitch）。
* 擴充性強：可使用 DI 提供的服務來擴充日誌功能。


### **在 Serilog 中使用自定義 EnrichDiagnosticContext**

安裝套件

```bash
dotnet add package Serilog.Formatting.Compact
```

在每一個請求中的 Middleware 中加入自定義屬性

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestPath}";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContent) =>
    {
        diagnosticContext.Set("RequestHost", httpContent.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContent.Request.Scheme);
        diagnosticContext.Set("RequestProtocol", httpContent.Request.Protocol);
    };
});
```

改用 Serilog.Formatting.Compact 來記錄 JSON 格式的 Log，並設定 FromLogContext

```json
{
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "Log/log-.txt",
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact",
        "rollingInterval": "Day",
        "buffered": true,
        "flushToDiskInterval": "00:00:01"
      }
    }
  ],
  "Enrich": [ "FromLogContext" ]
}
```

輸出範例

```json
{
  "@t":"2025-01-13T07:09:24.7792740Z",
  "@mt":"Handled {RequestPath}",
  "@tr":"c99acacc3e79f892dcfe344ff1cf53ef",
  "@sp":"94c51da91394fe9f",
  "RequestHost":"localhost:5200",
  "RequestScheme":"http",
  "RequestProtocol":"HTTP/1.1",
  "RequestMethod":"GET",
  "RequestPath":"/weatherforecast/",
  "StatusCode":200,
  "Elapsed":40.321083,
  "SourceContext":"Serilog.AspNetCore.RequestLoggingMiddleware",
  "RequestId":"0HN9JK4R1PHVF:00000001",
  "ConnectionId":"0HN9JK4R1PHVF"
}
```


### **ASP.NET Core Web Host 常見的環境變數**

* `ASPNETCORE_ENVIRONMENT`
* `ASPNETCORE_URLS`
* `ASPNETCORE_Kestrel__Certificates__Default__Path`
* `ASPNETCORE_Kestrel__Certificates__Default__Password`
* `ASPNETCORE_CONTENTROOT`
* `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`
* `ASPNETCORE_HOSTINGSTARTUPEXCLUDEASSEMBLIES`
* `ASPNETCORE_PREVENTHOSTINGSTARTUP`
* `ASPNETCORE_HTTP_PORTS`
* `ASPNETCORE_SHUTDOWNTIMEOUTSECONDS`
* `ASPNETCORE_WEBROOT`
* `ASPNETCORE_TEMP`
* `ASPNETCORE_FORWARDEDHEADERS_ENABLED`