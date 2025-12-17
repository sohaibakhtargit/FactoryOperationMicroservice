using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class FeedbackMetric
    {
        public int MetricId { get; set; }
        public int? TenantId { get; set; }

        public decimal AvgRating { get; set; }
        public decimal SatisfactionRate { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

}
