using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddSystemMetricDto
    {
        public int ActiveTenants { get; set; }
        public int ActiveUsers { get; set; }
        public long TotalStorage { get; set; }
        public int OpenWorkOrders { get; set; }
        public int ErrorCount { get; set; }
    }

    public class GetSystemMetricDto : AddSystemMetricDto
    {
        public int MetricId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

