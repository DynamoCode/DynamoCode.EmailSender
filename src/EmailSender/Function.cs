using System;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Configuration;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EmailSender
{
    public class Function
    {
        private static IConfigurationRoot Configuration;
        static Function()
        {
            var builder = new ConfigurationBuilder()
                .AddSystemsManager("/DynamoCode.EmailSender");
            Configuration = builder.Build();
        }

        private SmtpSettings GetSettings()
        {
            return new SmtpSettings
            {
                Server = Configuration["SmtpSettings:Server"],
                Port = Convert.ToInt32(Configuration["SmtpSettings:Port"]),
                Username = Configuration["SmtpSettings:Username"],
                Password = Configuration["SmtpSettings:Password"],
                EnableSsl = Convert.ToBoolean(Configuration["SmtpSettings:EnableSsl"])
            };
        }

        private void LogDebug(string text)
        {
            Console.WriteLine($"[DEBUG] {text}");
        }

        public string HandleSQSEvent(SQSEvent sqsEvent, ILambdaContext context)
        {
            var debug = Convert.ToBoolean(Environment.GetEnvironmentVariable("DEBUG"));

            Console.WriteLine($"Beginning to process {sqsEvent.Records.Count} records...");

            foreach (var record in sqsEvent.Records)
            {
                try
                {
                    if (debug)
                    {
                        LogDebug($"Message ID: {record.MessageId}");
                        LogDebug($"Event Source: {record.EventSource}");
                    }

                    var msg = JsonSerializer.Deserialize<EmailMessage>(record.Body);

                    if (debug)
                    {
                        LogDebug($"From: {msg.FromEmail} \"{msg.FromName}\"");
                        LogDebug($"To: {msg.ToEmail}");
                        LogDebug($"Subject: {msg.Subject}");
                        LogDebug($"Body: {msg.Body}");
                    }

                    var smtpSettings = GetSettings();

                    if (debug)
                    {
                        LogDebug($"Server: {smtpSettings.Server}");
                        LogDebug($"Port: {smtpSettings.Port}");
                        LogDebug($"Username: {smtpSettings.Username}");
                        LogDebug($"Password: {smtpSettings.Password}");
                        LogDebug($"EnableSsl: {smtpSettings.EnableSsl}");
                    }


                    using (var message = new MailMessage())
                    {
                        message.To.Add(new MailAddress(msg.ToEmail));
                        message.From = new MailAddress(msg.FromEmail, msg.FromName);
                        message.Subject = msg.Subject;
                        message.Body = msg.Body;
                        message.IsBodyHtml = true;

                        using (var client = new SmtpClient(smtpSettings.Server))
                        {
                            client.Port = smtpSettings.Port;
                            client.UseDefaultCredentials = false;
                            client.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);
                            client.EnableSsl = smtpSettings.EnableSsl;
                            client.Send(message);
                        }
                    }
                    Console.WriteLine($"Email sent successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while sending an email");
                    //if (debug)
                    {
                        LogDebug(ex.Message);
                        LogDebug(ex.StackTrace);
                    }
                }
            }

            Console.WriteLine("Processing complete.");

            return $"Processed {sqsEvent.Records.Count} records.";
        }
    }
}
