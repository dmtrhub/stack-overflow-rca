using Data.Repositories;
using HealthStatusService.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HealthStatusService.Controllers
{
    public class HealthStatusController : Controller
    {
        // GET: HealthStatus
        private readonly HealthCheckRepository _repo;

        public HealthStatusController()
        {
            string conn = System.Configuration.ConfigurationManager.AppSettings["DataConnectionString"];
            _repo = new HealthCheckRepository(conn);
        }

        public async Task<ActionResult> Index()
        {
            var healthChecks = await _repo.GetRecentHealthChecksAsync("StackOverflowService", TimeSpan.FromHours(3));

            int total = healthChecks.Count;
            int ok = healthChecks.Count(hc => hc.Status == "OK");
            int notOk = healthChecks.Count(hc => hc.Status == "NOT_OK");

            var chartData = new { labels = new[] { "OK", "NOT_OK" }, data = new[] { ok, notOk } };
            ViewBag.ChartData = chartData;

            double availabilityPercent = total == 0 ? 0 : (ok * 100.0 / total);

            var model = new HealthCheckViewModel
            {
                HealthChecks = healthChecks.OrderBy(hc => hc.TimeStamp).ToList(),
                Total = total,
                Ok = ok,
                NotOk = notOk,
                AvailabilityPercent = availabilityPercent
            };

            return View(model);
        }
    }
}