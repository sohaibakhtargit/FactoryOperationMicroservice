using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class MaintenanceScheduleOccurrence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OccurrenceId { get; set; }

        public int TenantId { get; set; }

        [ForeignKey(nameof(MaintenanceSchedule))]
        public int ScheduleId { get; set; }

        public DateTime OccurrenceDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FrequencyType FrequencyType { get; set; }

        public int FrequencyValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public MaintenanceSchedule MaintenanceSchedule { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        // Soft Delete and Active Flags
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;

    }
}
