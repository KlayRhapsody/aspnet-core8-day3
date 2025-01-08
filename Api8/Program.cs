using Api8.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AppSettingsOptions>(
    builder.Configuration.GetSection(AppSettingsOptions.SectionName));

var externalConfigPath = Path.Combine(Directory.GetCurrentDirectory(), 
    builder.Configuration.GetValue<string>("ExternalAppSettings")!);
builder.Configuration.AddJsonFile(
    path: externalConfigPath, 
    optional: true, 
    reloadOnChange: true);

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
