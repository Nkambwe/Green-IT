
using System.ComponentModel.DataAnnotations;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Maintenance {
    public class EditModel : PageModel {
        private readonly GreenITDbContext _db;

        public EditModel(GreenITDbContext db) {
            _db = db;
        }

        public int RecordId { get; set; }

        public string AssetTag { get; set; } = string.Empty;

        public string AssetName { get; set; } = string.Empty;

        [BindProperty]
        public MaintenanceInput Input { get; set; } = new();

        public class MaintenanceInput {
            [Required]
            [StringLength(100)]
            public string MaintenanceType { get; set; } = string.Empty;

            [Required]
            [StringLength(1000)]
            public string Description { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Date)]
            public DateTime MaintenanceDate { get; set; }

            [DataType(DataType.Date)]
            public DateTime? NextMaintenanceDate { get; set; }

            [Range(0, 999999999)]
            public decimal? MaintenanceCost { get; set; }

            [StringLength(200)]
            public string? ServiceProvider { get; set; }

            [StringLength(200)]
            public string? PerformedBy { get; set; }

            [StringLength(1000)]
            public string? Notes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id) {
            var record = await _db.AssetMaintenanceRecords
                .AsNoTracking()
                .Include(x => x.ITAsset)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
                return NotFound();

            LoadRecord(record);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id) {
            var record = await _db.AssetMaintenanceRecords
                .Include(x => x.ITAsset)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
                return NotFound();

            LoadAssetInformation(record);

            if (Input.NextMaintenanceDate.HasValue &&
                Input.NextMaintenanceDate.Value.Date <
                Input.MaintenanceDate.Date) {
                ModelState.AddModelError(
                    "Input.NextMaintenanceDate",
                    "Next maintenance date cannot be earlier than the maintenance date.");
            }

            if (!ModelState.IsValid)
                return Page();

            record.MaintenanceType = Input.MaintenanceType.Trim();
            record.Description = Input.Description.Trim();
            record.MaintenanceDate = Input.MaintenanceDate.Date;
            record.NextMaintenanceDate = Input.NextMaintenanceDate?.Date;
            record.MaintenanceCost = Input.MaintenanceCost;
            record.ServiceProvider = Clean(Input.ServiceProvider);
            record.PerformedBy = Clean(Input.PerformedBy);
            record.Notes = Clean(Input.Notes);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Maintenance record updated successfully.";

            return RedirectToPage("./Details", new { id });
        }

        private void LoadRecord(
            GreenIT.Domain.Entities.AssetMaintenance record) {
            LoadAssetInformation(record);

            Input = new MaintenanceInput
            {
                MaintenanceType = record.MaintenanceType,
                Description = record.Description,
                MaintenanceDate = record.MaintenanceDate,
                NextMaintenanceDate = record.NextMaintenanceDate,
                MaintenanceCost = record.MaintenanceCost,
                ServiceProvider = record.ServiceProvider,
                PerformedBy = record.PerformedBy,
                Notes = record.Notes
            };
        }

        private void LoadAssetInformation(
            GreenIT.Domain.Entities.AssetMaintenance record) {
            RecordId = record.Id;
            AssetTag = record.ITAsset.AssetTag;
            AssetName = record.ITAsset.Name;
        }

        private static string? Clean(string? value) {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
