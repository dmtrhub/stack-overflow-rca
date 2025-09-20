using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.Services;
using Microsoft.Azure; 
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;         
using Microsoft.WindowsAzure.Storage.Queue;   
using StackOverflowService_WebRole.AzureStorage;
using StackOverflowService_WebRole.Repositories;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);

        private CloudQueue _queue;

        private AnswerRepository _answerRepo;
        private QuestionRepository _questionRepo;
        private NotificationLogRepository _logRepo;
        private IEmailService _emailService;

        public override bool OnStart()
        {
            _answerRepo = new AnswerRepository();
            _questionRepo = new QuestionRepository();
            _logRepo = new NotificationLogRepository();

            string host = CloudConfigurationManager.GetSetting("SmtpHost");
            int port = int.Parse(CloudConfigurationManager.GetSetting("SmtpPort"));
            string user = CloudConfigurationManager.GetSetting("SmtpUser");
            string pass = CloudConfigurationManager.GetSetting("SmtpPass");
            string from = CloudConfigurationManager.GetSetting("FromAddress");
            _emailService = new EmailService(host, port, user, pass, from);

            string connectionString = CloudConfigurationManager.GetSetting("DataConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            _queue = queueClient.GetQueueReference("notifications");
            _queue.CreateIfNotExists();

            StartHealthMonitoringEndpoint();

            return base.OnStart();
        }

        public override void Run()
        {
            Trace.TraceInformation("NotificationService is running...");
            try
            {
                this.RunAsync(_cancellationTokenSource.Token).Wait();
            }
            finally
            {
                _runCompleteEvent.Set();
            }
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Primanje poruke korišćenjem CloudQueue
                CloudQueueMessage receivedMessage = await _queue.GetMessageAsync(token);

                if (receivedMessage != null)
                {
                    try
                    {
                        string answerId = receivedMessage.AsString;
                        Trace.TraceInformation($"Obrada poruke za odgovor ID: {answerId}");

                        // 1. Dohvati najbolji odgovor
                        var bestAnswer = await _answerRepo.GetAnswerByIdAsync(answerId);
                        if (bestAnswer == null) throw new Exception($"Odgovor sa ID={answerId} nije pronađen.");

                        // 2. Na osnovu odgovora, pronađi pitanje
                        string questionId = bestAnswer.PartitionKey.Split('_')[1];
                        var question = await _questionRepo.GetQuestionByIdAsync(questionId);
                        if (question == null) throw new Exception($"Pitanje za odgovor ID={answerId} nije pronađeno.");

                        // 3. Pronađi sve korisnike koji su odgovorili
                        var allAnswersForQuestion = await _answerRepo.GetAnswersByQuestionIdAsync(question.RowKey);
                        var userEmailsToNotify = allAnswersForQuestion
                                                    .Select(a => a.AnsweredByEmail)
                                                    .Distinct()
                                                    .ToList();

                        // 4. Slanje emailova
                        string subject = $"Pitanje '{question.Title}' je zatvoreno";
                        string body = $"Pitanje je uspešno zatvoreno.<br/><br/>" +
                                      $"<b>Autor finalnog odgovora:</b> {bestAnswer.AnsweredByEmail}<br/>" +
                                      $"<b>Tekst odgovora:</b><br/>" +
                                      $"<p style='border-left: 2px solid #ccc; padding-left: 10px; margin-left: 5px;'>{bestAnswer.Description}</p>";

                        foreach (var email in userEmailsToNotify)
                        {
                            await _emailService.SendEmailAsync(email, subject, body);
                        }

                        // 5. Persistiranje informacija
                        var log = new NotificationLogTableEntity(answerId) { SentEmailsCount = userEmailsToNotify.Count };
                        await _logRepo.SaveLogAsync(log);

                        Trace.TraceInformation($"Uspešno poslato {userEmailsToNotify.Count} notifikacija za odgovor ID: {answerId}");

                        // Brisanje poruke iz reda. Prosleđuje se ceo objekat poruke.
                        await _queue.DeleteMessageAsync(receivedMessage, token);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Greška pri obradi poruke: {ex.Message}. Poruka će biti vidljiva za ponovnu obradu.");
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
            }
        }

        private void StartHealthMonitoringEndpoint()
        {
            try
            {
                var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HealthCheckEndpoint"];
                var listener = new HttpListener();
                listener.Prefixes.Add($"{endpoint.Protocol}://*:{endpoint.IPEndpoint.Port}/health-monitoring/");
                listener.Start();

                Task.Run(async () =>
                {
                    while (true)
                    {
                        HttpListenerContext context = await listener.GetContextAsync();
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Neuspešno pokretanje health monitoring endpointa: {ex.Message}");
            }
        }

        public override void OnStop()
        {
            _cancellationTokenSource.Cancel();
            _runCompleteEvent.WaitOne();
            base.OnStop();
        }
    }
}

