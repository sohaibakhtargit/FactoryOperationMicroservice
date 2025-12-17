using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AnnouncementTemplateCreateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }

        [Required]
        public AnnouncementType Type { get; set; } 

        [Required, StringLength(255)]
        public string TitleTemplate { get; set; }

        [Required]
        public string MessageTemplate { get; set; }

        [Required]
        public List<string> DefaultChannels { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class AnnouncementTemplateUpdateDto
    {
        public int TemplateId { get; set; }
        [StringLength(100)]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public AnnouncementType? Type { get; set; }
        [StringLength(255)]
        public string? TitleTemplate { get; set; }
        public string? MessageTemplate { get; set; }
        public List<string>? DefaultChannels { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class TemplatePreviewDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public List<string> Channels { get; set; }
    }
}
