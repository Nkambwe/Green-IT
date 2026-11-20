using GreenIT.Domain.Enums;

namespace GreenIT.Domain.Entities {
    public class ITAsset {
        public int Id { get; set; }

        public string AssetTag { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }

        public string Name { get; set; } = string.Empty;
        public AssetCategory Category { get; set; }

        public string? Manufacturer { get; set; }
        public string? Model { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public string? Location { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public decimal? PurchaseCost { get; set; }

        public AssetCondition Condition { get; set; }
        public AssetStatus Status { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<AssetAssignment> Assignments { get; set; }
            = new List<AssetAssignment>();

        public ICollection<AssetMaintenance> MaintenanceRecords { get; set; }
            = new List<AssetMaintenance>();

        public ICollection<AssetLifecycleEvent> LifecycleEvents { get; set; }
            = new List<AssetLifecycleEvent>();

        public ICollection<EnergyAuditItem> EnergyAuditItems { get; set; }
            = new List<EnergyAuditItem>();
    }

}
