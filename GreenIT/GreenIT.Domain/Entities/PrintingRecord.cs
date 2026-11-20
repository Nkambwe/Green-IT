namespace GreenIT.Domain.Entities {
    public class PrintingRecord {
        public int Id { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public int Year { get; set; }
        public int Month { get; set; }

        public int TotalPages { get; set; }
        public int DuplexPages { get; set; }
        public int ColourPages { get; set; }
        public int PrintJobs { get; set; }

        public string? Source { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }

}
