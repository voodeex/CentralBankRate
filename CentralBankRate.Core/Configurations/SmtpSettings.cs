namespace CentralBankRate.Core.Configurations;

public class SmtpSettings
{
    public string Host { get; init; } = "";
    public int Port { get; init; } = 587;
    
    public bool UseSslOnConnect { get; init; }

    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
}