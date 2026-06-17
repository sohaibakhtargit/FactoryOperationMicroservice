using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("NotificationsMapping")]
    public class NotificationsMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationMappingId { get; set; }

        [Required]
        public int MasterNotificationId { get; set; }

        [ForeignKey(nameof(MasterNotificationId))]
        public MasterNotification? MasterNotification { get; set; }

        public int? WorkOrderId { get; set; }
        public int? ServiceRequestId { get; set; }

        public int TenantId { get; set; } 

        public int? TargetUsersId { get; set; }
        public int? IncomingNotifications { get; set; }
        public int? OutgoingNotifications { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsReadByTenant { get; set; } = false;

        public int? CreatedBy { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }
    }
}
