using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using Limilabs.Client.SMTP;
using Limilabs.Cryptography;
using Limilabs.Mail;
using Limilabs.Mail.Tools;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GsuTez
{
    class Email
    {
        public static void Main(string[] args)
        {
            Email emails = new Email();

            //Display all maill  with Imap Protocol
            emails.DisplayMail_Imap();
            //emails.DisplayMail_Pop3();
            //emails.SendMail_Smtp();
            
            Console.ReadLine();
        }

        public void DisplayMail_Imap()
        {

           


            using (Imap imap = new Imap())
            {
                imap.ConnectSSL("imap.gmail.com");
                imap.UseBestLogin("mailgsu2@gmail.com", "kadir55555");
                imap.SelectInbox();
                List<long> uids = imap.Search(Flag.All);
                Console.WriteLine("Email Sayiyis:" + uids.Count);

                for (int i = uids.Count-1; i < uids.Count; i++)
                {
                    IMail email = new MailBuilder().CreateFromEml(imap.GetMessageByUID(i));

                    Console.WriteLine("Subject : " + email.Subject);
                    Console.WriteLine("From : " + email.From);
                    Console.WriteLine("To : " + email.To);
                    
                    //Email - Html
                    Console.WriteLine(email.GetBodyAsHtml().ToString());
                    //Email HTML  to plain text converter

                    Console.WriteLine("******HTML---->TEXT******");
                    Console.WriteLine(email.GetTextFromHtml());

                    BounceMail(email);
                    if (email.IsDKIMSigned)
                    {
                        bool isValid = email.CheckDKIMSignature();
                        if (isValid)
                            Console.WriteLine("True");
                        else
                            Console.WriteLine("false");


                    }

                }
                imap.Close();
            }
        }

        public void DisplayMail_Pop3()
        {
            using (Pop3 pop3 = new Pop3())
            {
                pop3.ConnectSSL("pop.gmail.com");
                pop3.UseBestLogin("mailgsu2@gmail.com", "kadir55555");

                List<string> uids = pop3.GetAll();
                Console.WriteLine("Uids : " + uids.Count);

                foreach (string uid in uids)
                {
                    IMail email = new MailBuilder().CreateFromEml(pop3.GetMessageByUID(uid));

                    Console.WriteLine("Subject : " + email.Subject);
                    Console.WriteLine("From : " + email.From);
                    Console.WriteLine("To : " + email.To);
                }

                pop3.Close();
            }
        }

        public void SendMail_Smtp()
        {
            RSACryptoServiceProvider rsa = new PemReader().ReadPrivateKeyFromFile(@"C:\Users\Kadirroi\Desktop\Openssl\dkim_private.key");

            IMail email = Limilabs.Mail.Fluent.Mail.Text("text")
                   .From("mailgsu2@gmail.com")
                   .To("kadir@kadir.mahmutcavd")
                   .Subject("subject")
                   .DKIMSign(rsa, "alpha", "gmail.com")
                   .Create();

            using (Smtp smtp = new Smtp())
            {
                smtp.Connect("smtp.gmail.com");  // or ConnectSSL for SSL
                smtp.UseBestLogin("mailgsu2@gmail.com", "kadir55555");
                smtp.SendMessage(email);
                Console.WriteLine("Send Mail");
                smtp.Close();
            }


          

        }

        public void BounceMail(IMail email)
        {

            
            Bounce bounce = new Bounce();
            BounceResult result = bounce.Examine(email);

            
            //Email Gönderimi başarılımı başarısızmı olduğu  kontrol edilmektedir.
            if (result.IsDeliveryFailure)
            {
                Console.WriteLine(result.Recipient);

                Console.WriteLine(result.Action);    // DSNAction.Failed or DSNAction.Delayed

                Console.WriteLine(result.Reason);
                Console.WriteLine(result.Status);
            }
            else
                Console.WriteLine("Email sent Succesful");


        }
    }
}