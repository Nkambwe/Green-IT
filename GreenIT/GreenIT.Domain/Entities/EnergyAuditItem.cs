namespace GreenIT.Domain.Entities {
    public class EnergyAuditItem {
        public int Id { get; set; }

        public int EnergyAuditId { get; set; }
        public EnergyAudit EnergyAudit { get; set; } = null!;

        public int? ITAssetId { get; set; }
        public ITAsset? ITAsset { get; set; }

        public bool IsSleepModeEnabled { get; set; }
        public bool IsAutomaticShutdownEnabled { get; set; }
        public bool IsMonitorPowerSavingEnabled { get; set; }

        public string? Observation { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }

}
