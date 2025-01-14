



Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
    // .WriteTo.File(
    //     path: "log/log.txt",
    //     rollingInterval: RollingInterval.Day,
    //     outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
    //     buffered: true,
    //     flushToDiskInterval: TimeSpan.FromSeconds(1) // 預設 2 秒
    // )
    // .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configure<TOptions>：推薦作為默認選擇，特別是需要 DI 支持或監控配置變更的場景。
    // builder.Services.Configure<AppSettingsOptions>(
    //     builder.Configuration.GetSection(AppSettingsOptions.SectionName));

    // Get<T>：適合單次、靜態使用場景，不需要 DI 或變更監控。
    // var appSettingsOptions = builder.Configuration.GetSection(AppSettingsOptions.SectionName)
    //     .Get<AppSettingsOptions>();

    // Bind：適合已有實例的情況，且需手動控制綁定的生命周期。
    // builder.Configuration.GetSection(AppSettingsOptions.SectionName).Bind(appSettingsOptions);

    // builder.Configuration.Sources.Clear();
    // var externalConfigPath = Path.Combine(Directory.GetCurrentDirectory(), 
    //     builder.Configuration.GetValue<string>("ExternalAppSettings")!);

    // builder.Configuration.AddJsonFile(
    //     path: externalConfigPath, 
    //     optional: true, 
    //     reloadOnChange: true);

    builder.Services.AddOptions<AppSettingsOptions>()
        .Bind(builder.Configuration.GetSection(AppSettingsOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.PostConfigure<AppSettingsOptions>(options =>
    {
        options.SomeKey += "Ahaha";
    });

    builder.Services.AddSingleton<IValidateOptions<AppSettingsOptions>, AppSettingsValidation>();
    builder.Services.AddSingleton<IAllowedIpService, AllowedIpService>();

    // Log Provider
    // builder.Logging.ClearProviders();

    // builder.Logging.AddSimpleConsole(options =>
    // {
    //     options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    //     options.ColorBehavior = LoggerColorBehavior.Enabled;
    // });

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

    var sinkOptions = new MSSqlServerSinkOptions
    {
        TableName = "LogEvents",
        AutoCreateSqlTable = true,          // 若資料表不存在，則自動建表
        BatchPostingLimit = 50,             // 每批寫入幾筆（預設 50）
        BatchPeriod = TimeSpan.FromSeconds(5),  // 多少秒後強制寫入一次
    };

    // builder.Logging.AddSerilog();
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.MSSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
            sinkOptions: sinkOptions));
        
    var app = builder.Build();

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

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.UseLogRequestType();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


