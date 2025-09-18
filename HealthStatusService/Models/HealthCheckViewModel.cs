using Data.Entities;
using System.Collections.Generic;

namespace HealthStatusService.Models
{
    public class HealthCheckViewModel
    {
        public List<HealthCheck> HealthChecks { get; set; }
        public double AvailabilityPercent { get; set; }
        public int Total { get; set; }
        public int Ok { get; set; }
        public int NotOk { get; set; }
    }
}