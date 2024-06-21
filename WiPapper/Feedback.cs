using System.Net;
using System.Net.Mail;

namespace WiPapper
{
    public static class Feedback
    {
        public static void SendEmail(string name, string subject, string body)
        {
            var fromAddress = new MailAddress("wipapperfeedback@mail.ru");
            var toAddress = new MailAddress("wipapperfeedback@gmail.com");

            var smtp = new SmtpClient
            {
                Host = "smtp.mail.ru",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, "XfTjGGiCjyrkEPnUnCW6")
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = $"Обратная связь от {name} - {subject}",
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
