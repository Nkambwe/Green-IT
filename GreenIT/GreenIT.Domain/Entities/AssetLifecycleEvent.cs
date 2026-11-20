using GreenIT.Domain.Enums;

namespace GreenIT.Domain.Entities {
    public class AssetLifecycleEvent {
        public int Id { get; set; }

        public int ITAssetId { get; set; }
        public ITAsset ITAsset { get; set; } = null!;

        public AssetLifecycleEventType EventType { get; set; }

        public DateTime EventDate { get; set; }

        public int? FromDepartmentId { get; set; }
        public int? ToDepartmentId { get; set; }

        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public string? RecordedByUserId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }

}
