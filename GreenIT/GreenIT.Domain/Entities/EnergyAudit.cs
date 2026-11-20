using GreenIT.Domain.Enums;

namespace GreenIT.Domain.Entities {
    public class EnergyAudit {
        public int Id { get; set; }

        public string AuditReference { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public DateTime AuditDate { get; set; }

        public EnergyAuditStatus Status { get; set; }

        public string? AuditorUserId { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<EnergyAuditItem> Items { get; set; }
            = new List<EnergyAuditItem>();
    }

}
