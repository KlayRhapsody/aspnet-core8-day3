namespace Api8.Models;

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