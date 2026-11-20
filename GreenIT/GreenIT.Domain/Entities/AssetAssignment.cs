namespace GreenIT.Domain.Entities {
    public class AssetAssignment {
        public int Id { get; set; }

        public int ITAssetId { get; set; }
        public ITAsset ITAsset { get; set; } = null!;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public string? AssignedToUserId { get; set; }

        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }

}
