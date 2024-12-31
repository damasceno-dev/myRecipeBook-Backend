using MimeKit;
using MailKit.Net.Smtp;
using MyRecipeBook.Domain.Interfaces.Email;

namespace MyRecipeBook.Infrastructure.Services;

public class EmailUserResetPasswordCode : ISendUserResetPasswordCode
{
    private const string GmailSmtpServer = "smtp.gmail.com";
    private const int SmtpPort = 587; // Use 465 for SSL
    private string? Gmail { get; set; }
    private string? Name { get; set; }
    private string? Password { get; set; }

    public EmailUserResetPasswordCode()
    {
        DotNetEnv.Env.Load("../MyRecipeBook.Infrastructure/.env");        
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
            await smtpClient.ConnectAsync(GmailSmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(Gmail, Password);
            await smtpClient.SendAsync(email);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error sending google reset email password: {ex.Message}");
        }
        finally
        {
            await smtpClient.DisconnectAsync(true);
        }
    }
}