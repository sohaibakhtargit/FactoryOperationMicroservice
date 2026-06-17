using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FactoryOpsApp.Application.DTOs
{
    public class MonitoringHealthDto
    {

        public int ActiveTenants { get; set; }
        public int TotalUsers { get; set; }
        public double SystemLoad { get; set; }
        public int OpenIssues { get; set; }
        public int TotalSupportTickets { get; set; }
        public int OpenSupportTickets { get; set; }

        public double FailedLoginRate { get; set; }
        public int UnusualActivityCount { get; set; }
        public int RateLimitViolations { get; set; }


        public int P50Latency { get; set; }  
        public int P95Latency { get; set; }
        public int P99Latency { get; set; }
        public double TimeoutRate { get; set; }  

        public double CacheHitRate { get; set; } // percentage
        public double MemoryUsage { get; set; } // percentage
        public double DiskUsage { get; set; } // percentage
        public double NetworkLatency { get; set; } // milliseconds
    }
}

