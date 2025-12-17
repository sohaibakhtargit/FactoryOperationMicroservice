using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    [Table("AnnouncementTemplates")]
    public class AnnouncementTemplate
    {
        [Key]
        [Column("TemplateId")]
        public int TemplateId { get; set; }

        [Required]
        [Column("Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Required]
        [Column("Type")]
        public AnnouncementType Type { get; set; } 

        [Required]
        [Column("TitleTemplate")]
        [MaxLength(255)]
        public string TitleTemplate { get; set; }

        [Required]
        [Column("MessageTemplate")]
        public string MessageTemplate { get; set; }

        [Required]
        [Column("DefaultChannels", TypeName = "jsonb")]
        public string DefaultChannels { get; set; } = "[\"email\"]";

        [Column("IsSystem")]
        public bool IsSystem { get; set; } = false;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Required]
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("CreatedBy")]
        public int? CreatedBy { get; set; }

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [Column("UpdatedBy")]
        public int? UpdatedBy { get; set; }
    }

    public enum AnnouncementType
    {
        info,
        warning,
        critical
    }
}
