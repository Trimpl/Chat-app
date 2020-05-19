using MimeKit;
using Microsoft.Extensions.Logging;
using System;
using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace Chat.Services
{
    public class EmailService
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<EmailService> logger;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            this.logger = logger;
            Configuration = configuration;
        }
        public void SendEmailCustom(string email, string url)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Confirm registration", "pahinash@gmail.com"));
                message.To.Add(new MailboxAddress(email));
                message.Subject = "Confirm registration"; 
                message.Body = new BodyBuilder() { HtmlBody = "<div style=\"color: green;\">Confirm registration <a href=" + '"' + url + '"' + ">here</a> and enjoy</div>" }.ToMessageBody();

                using (MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
                {
                    IConfigurationSection EmailAuthSection =
                        Configuration.GetSection("Email:MailKit");
                    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls); 
                    client.Authenticate(EmailAuthSection["Login"], EmailAuthSection["Password"]); 
                    client.Send(message);
                    client.Disconnect(true);
                    logger.LogInformation("Register confirmation email has been sent successfully");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.GetBaseException().Message);
            }
        }
    }
}