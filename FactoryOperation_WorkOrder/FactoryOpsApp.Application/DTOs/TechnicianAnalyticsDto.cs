using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class TechnicianAnalyticsDto
    {
        public string? TechnicianName { get; set; }
        public string? Skills { get; set; }
        public string? Status { get; set; }
        public double Utilization { get; set; }
        public int ActiveWorkOrders { get; set; }
    }

    public class LaborAnalyticsDto
    {
        public int TotalTechnicians { get; set; }
        public int AvailableTechnicians { get; set; }
        public double AvgUtilization { get; set; }
        public int ActiveWorkOrders { get; set; }
        public double ResourceEfficiency { get; set; }

        public List<TechnicianAnalyticsDto>? TechnicianDetails { get; set; }
    }
}
