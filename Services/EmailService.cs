using System.Net.Mail;
using BugscapeMVC.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace BugscapeMVC.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            string emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email")!;
            string displayName = _mailSettings.DisplayName ?? Environment.GetEnvironmentVariable("DisplayName")!;

            MimeMessage email = new()
            {
                Sender = new MailboxAddress(displayName, emailSender),
                Subject = subject,
                
            };

            email.To.Add(MailboxAddress.Parse(emailTo));

            BodyBuilder emailBody = new()
            {
                HtmlBody = htmlMessage
            };

            email.Body = emailBody.ToMessageBody();

            using SmtpClient smtpClient = new();

            try
            { 
                string host = _mailSettings.Host ?? Environment.GetEnvironmentVariable("MailHost")!;
                int port = _mailSettings.Port != 0 ? _mailSettings.Port : int.Parse(Environment.GetEnvironmentVariable("MailPort")!);
                string password = _mailSettings.Password ?? Environment.GetEnvironmentVariable("MailPassword")!;

                smtpClient.Connect(host, port, SecureSocketOptions.StartTls);
                smtpClient.Authenticate(emailSender, password);

                await smtpClient.SendAsync(email);

                smtpClient.Disconnect(true);
            }
            catch (Exception)
            {             
                throw;
            }
        }
    }
}