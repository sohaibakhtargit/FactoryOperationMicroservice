using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class SystemMetric
    {
        [Key]
        public int MetricId { get; set; } 

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int ActiveTenants { get; set; }

        public int ActiveUsers { get; set; }

        public long TotalStorage { get; set; }

        
      //  public int OpenWorkOrders { get; set; }

        public int ErrorCount { get; set; }
    }
}

