using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class MaintenanceScheduleOccurrenceDto
    {
        public int ScheduleId { get; set; }
        public int OccurrenceId { get; set; }
        public FrequencyType FrequencyType { get; set; }
        public int FrequencyValue { get; set; }
        public DateTime OccurrenceDate { get; set; }
        public string ScheduleName { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public string LocationFilter { get; set; }
        public int ActiveWorkOrders { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
