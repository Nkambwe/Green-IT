namespace GreenIT.Domain.Entities {
    public class Department {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<ITAsset> ITAssets { get; set; }
            = new List<ITAsset>();

        public ICollection<AssetAssignment> AssetAssignments { get; set; }
            = new List<AssetAssignment>();

        public ICollection<EnergyAudit> EnergyAudits { get; set; }
            = new List<EnergyAudit>();

        public ICollection<PrintingRecord> PrintingRecords { get; set; }
            = new List<PrintingRecord>();

        public ICollection<SustainabilityAction> SustainabilityActions { get; set; }
            = new List<SustainabilityAction>();
    }

}
