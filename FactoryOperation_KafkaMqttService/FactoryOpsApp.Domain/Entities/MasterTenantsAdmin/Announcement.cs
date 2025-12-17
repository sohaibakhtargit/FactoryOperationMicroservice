using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Type { get; set; } = "info";

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime? ScheduledTime { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "draft";

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
      

        [ForeignKey("AnnouncementTemplates")]
        public int? TemplateId { get; set; }
        public AnnouncementTemplate? Template { get; set; }
        public virtual ICollection<AnnouncementChannel> Channels { get; set; }
        public virtual ICollection<AnnouncementTenant> Tenants { get; set; }
    }

}
