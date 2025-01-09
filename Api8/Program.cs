
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
builder.Logging.ClearProviders();

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    options.ColorBehavior = LoggerColorBehavior.Enabled;
});

// builder.Logging.AddJsonConsole(options =>
// {
//     options.IncludeScopes = true;
//     options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
//     options.JsonWriterOptions = new JsonWriterOptions
//     {
//         Indented = true,
//     };
//     options.UseUtcTimestamp = true;
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
