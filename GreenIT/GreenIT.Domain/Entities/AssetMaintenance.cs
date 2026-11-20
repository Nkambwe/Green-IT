namespace GreenIT.Domain.Entities {
    public class AssetMaintenance {
        public int Id { get; set; }

        public int ITAssetId { get; set; }
        public ITAsset ITAsset { get; set; } = null!;

        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime MaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }

        public decimal? MaintenanceCost { get; set; }

        public string? ServiceProvider { get; set; }
        public string? PerformedBy { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }

}
