using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CLubLaRibera_Web.Controllers
{
    public class Utilidades
    {
        private string mailOrigen = "desarrollos.lopezgaston@gmail.com";
        private string clave = "50110392";

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
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential(mailOrigen, clave);
            smtp.Send(mail);

            smtp.Dispose();
        }
    }
}
