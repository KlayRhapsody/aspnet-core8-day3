namespace Api8.Models;

public class AppSettingsOptions
{
    public const string SectionName = "AppSettings";
    public required string SomeKey { get; set; }
    public required string SmtpIp { get; set; }
    public int SmtpPort { get; set; } = 255;
}