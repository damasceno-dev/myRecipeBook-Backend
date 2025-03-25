using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MyRecipeBook.Domain.Interfaces.Email;
using System.Security.Authentication;

namespace MyRecipeBook.Infrastructure.Services;

public class EmailUserResetPasswordCode : ISendUserResetPasswordCode
{
    private const string GmailSmtpServer = "smtp.gmail.com";
    private const int SmtpPort = 587;
    private string? Gmail { get; set; }
    private string? Name { get; set; }
    private string? Password { get; set; }

    public EmailUserResetPasswordCode()
    {
        // Use the same env file loading logic as InfraDependencyInjectionExtension
        var envFilePath = File.Exists("Infrastructure.env")
            ? "Infrastructure.env" // Path for publishing environment
            : "../MyRecipeBook.Infrastructure/Infrastructure.env"; // Path for development environment

        DotNetEnv.Env.Load(envFilePath);
        
        var gmailConfig = Environment.GetEnvironmentVariable("GMAIL_APP_CONFIG");

        if (string.IsNullOrWhiteSpace(gmailConfig))
            throw new ArgumentException("Invalid GMAIL configuration string");
        
        var configParts = gmailConfig.Split(';')
            .Select(part => part.Split('='))
            .ToDictionary(kv => kv[0], kv => kv[1]);

        Gmail = configParts.GetValueOrDefault("Gmail");
        Name = configParts.GetValueOrDefault("Name");
        Password = configParts.GetValueOrDefault("Password");

        if (string.IsNullOrWhiteSpace(Gmail) || string.IsNullOrWhiteSpace(Name) ||string.IsNullOrWhiteSpace(Password))
            throw new ArgumentException("Invalid GMAIL connection string values");
    }
    
    public async Task Send(string userEmail, string code)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(Name, Gmail));
        email.To.Add(new MailboxAddress("", userEmail));
        email.Subject = ResourceEmailPasswordReset.EMAIL_TITLE;
        
        email.Body = new TextPart("plain")
        {
            Text = $"{ResourceEmailPasswordReset.EMAIL_BODY} {code}"
        };

        using var smtpClient = new SmtpClient();
        try
        {
            // SecureSocketOptions is an enum in MailKit, not a class with properties
            // Use the SslProtocols directly when connecting
            smtpClient.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

            await smtpClient.ConnectAsync(GmailSmtpServer, SmtpPort, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(Gmail, Password);
            await smtpClient.SendAsync(email);
        }
        catch (SmtpCommandException smtpEx)
        {
            Console.WriteLine($@"SMTP command error: {smtpEx.Message}");
            Console.WriteLine($@"Status code: {smtpEx.StatusCode}");
            throw new InvalidOperationException($"SMTP command error: {smtpEx.Message}", smtpEx);
        }
        catch (SmtpProtocolException protocolEx)
        {
            Console.WriteLine($@"SMTP protocol error: {protocolEx.Message}");
            throw new InvalidOperationException($"SMTP protocol error: {protocolEx.Message}", protocolEx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error sending reset password email: {ex.Message}");
            throw new InvalidOperationException($"Error sending reset password email: {ex.Message}", ex);
        }
        finally
        {
            if (smtpClient.IsConnected)
                await smtpClient.DisconnectAsync(true);
        }
    }
}