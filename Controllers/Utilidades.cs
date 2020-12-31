using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CLubLaRibera_Web.Controllers
{
    public class Utilidades
    {
        private string mailOrigen = "gastonlopez5@gmail.com";

        public void EnciarCorreo (string mailDestino, string asunto, string mensaje)
        {
            MailMessage mail = new MailMessage();
            
            mail.IsBodyHtml = true;
            mail.From = new MailAddress(mailOrigen);
            mail.To.Add(mailDestino);
            mail.Subject = asunto;
            mail.Body = mensaje;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 25;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = new System.Net.NetworkCredential("gastonlopez5@gmail.com", "50110392");
            smtp.Send(mail);

            smtp.Dispose();
        }
    }
}
