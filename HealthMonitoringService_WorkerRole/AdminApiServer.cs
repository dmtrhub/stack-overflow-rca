using Data.Repositories;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace HealthMonitoringService
{
    public class AdminApiServer
    {
        private readonly HttpListener _listener;
        private readonly AlertEmailRepository _alertRepo;

        public AdminApiServer(string prefix, AlertEmailRepository alertRepo)
        {
            _alertRepo = alertRepo;
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix); // npr. http://localhost:5003/
        }

        public void Start()
        {
            _listener.Start();
            Task.Run(() => ListenLoop());
        }

        private async Task ListenLoop()
        {
            while (true)
            {
                var context = await _listener.GetContextAsync();
                var req = context.Request;
                var resp = context.Response;
                string responseString = "";

                if (req.Url.AbsolutePath == "/emails" && req.HttpMethod == "GET")
                {
                    var emails = await _alertRepo.GetAllEmailsAsync();
                    responseString = new JavaScriptSerializer().Serialize(emails);
                }
                else if (req.Url.AbsolutePath == "/emails" && req.HttpMethod == "POST")
                {
                    string body;
                    using (var reader = new System.IO.StreamReader(req.InputStream, req.ContentEncoding))
                        body = await reader.ReadToEndAsync();

                    var data = new JavaScriptSerializer().Deserialize<dynamic>(body);
                    string email = data["email"];
                    await _alertRepo.AddEmailAsync(email);
                    responseString = "OK";
                }
                else if (req.Url.AbsolutePath == "/emails" && req.HttpMethod == "DELETE")
                {
                    string body;
                    using (var reader = new System.IO.StreamReader(req.InputStream, req.ContentEncoding))
                        body = await reader.ReadToEndAsync();

                    var data = new JavaScriptSerializer().Deserialize<dynamic>(body);
                    string email = data["email"];
                    await _alertRepo.RemoveEmailAsync(email);
                    responseString = "OK";
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                resp.ContentLength64 = buffer.Length;
                await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                resp.Close();
            }
        }
    }
}