using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddTrainingModuleDto
    {

        public int TrainingModuleId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TrainingType? ModuleType { get; set; }
        public DifficultyLevel? Difficulty { get; set; }
        public int? DurationMinutes { get; set; }
        public TrainingCategory? Category { get; set; }
        public string? Prerequisites { get; set; }
        //public string LearningObjectives { get; set; }
        //public int[] CompletedUserIds { get; set; }
        //public int[] EnrolledUserIds { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class GetTrainingModuleDto
    {
        public int TenantId { get; set; }
        public int TrainingModuleId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TrainingType? ModuleType { get; set; }
        public DifficultyLevel? Difficulty { get; set; }
        public int? DurationMinutes { get; set; }
        public TrainingCategory? Category { get; set; }
        public string? Prerequisites { get; set; }
        public bool? IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
