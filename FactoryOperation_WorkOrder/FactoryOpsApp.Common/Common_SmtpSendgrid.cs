using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Common
{
    public class Common_SmtpSendgrid
    {
        //private readonly EmailConfiguration _emailConfig;
        ////---------------------------------------SMTP-Email-OTP-SEND-------------------------------
        //private MimeMessage CreateEmail(MessageSmtp msg)
        //{
        //    var email = new MimeMessage();
        //    email.From.Add(new MailboxAddress("email", _emailConfig.From));
        //    email.To.AddRange(msg.To);
        //    email.Subject = msg.Subject;
        //    email.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = msg.Body };

        //    return email;
        //}
        //private void Send(MimeMessage message)
        //{
        //    using var Client = new SmtpClient();
        //    try
        //    {
        //        Client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
        //        Client.AuthenticationMechanisms.Remove("XOAUTH2");
        //        Client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
        //        Client.Send(message);
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {

        //        Client.Disconnect(true);
        //        Client.Dispose();
        //    }

        //}
        //public void SendEmail(MessageSmtp send)
        //{
        //    var EmailMessage = CreateEmail(send);
        //    Send(EmailMessage);

        //}
    }
}
