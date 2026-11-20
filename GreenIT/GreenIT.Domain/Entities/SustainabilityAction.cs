using GreenIT.Domain.Enums;

namespace GreenIT.Domain.Entities {
    public class SustainabilityAction {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public string? AssignedToUserId { get; set; }

        public SustainabilityPriority Priority { get; set; }
        public SustainabilityActionStatus Status { get; set; }

        public int ProgressPercentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

}
