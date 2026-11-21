
using System.ComponentModel.DataAnnotations;
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Maintenance {
    public class CreateModel : PageModel {
        private readonly GreenITDbContext _db;

        public CreateModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty]
        public MaintenanceInput Input { get; set; } = new();

        public List<SelectListItem> Assets { get; set; } = new();

        public class MaintenanceInput {
            [Range(1, int.MaxValue, ErrorMessage = "Select an asset.")]
            public int ITAssetId { get; set; }

            [Required]
            [StringLength(100)]
            public string MaintenanceType { get; set; } = string.Empty;

            [Required]
            [StringLength(1000)]
            public string Description { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Date)]
            public DateTime MaintenanceDate { get; set; } = DateTime.Today;

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

        public async Task OnGetAsync() {
            await LoadAssetsAsync();
        }

        public async Task<IActionResult> OnPostAsync() {
            await LoadAssetsAsync();

            var assetExists = await _db.ITAssets
                .AnyAsync(x => x.Id == Input.ITAssetId);

            if (!assetExists) {
                ModelState.AddModelError(
                    "Input.ITAssetId",
                    "Select a valid asset.");
            }

            if (Input.NextMaintenanceDate.HasValue &&
                Input.NextMaintenanceDate.Value.Date < Input.MaintenanceDate.Date) {
                ModelState.AddModelError(
                    "Input.NextMaintenanceDate",
                    "Next maintenance date cannot be earlier than the maintenance date.");
            }

            if (!ModelState.IsValid)
                return Page();

            var maintenance = new AssetMaintenance
            {
                ITAssetId = Input.ITAssetId,
                MaintenanceType = Input.MaintenanceType.Trim(),
                Description = Input.Description.Trim(),
                MaintenanceDate = Input.MaintenanceDate.Date,
                NextMaintenanceDate = Input.NextMaintenanceDate?.Date,
                MaintenanceCost = Input.MaintenanceCost,
                ServiceProvider = Clean(Input.ServiceProvider),
                PerformedBy = Clean(Input.PerformedBy),
                Notes = Clean(Input.Notes),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.AssetMaintenanceRecords.Add(maintenance);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Maintenance record created successfully.";

            return RedirectToPage("./Index");
        }

        private async Task LoadAssetsAsync() {
            Assets = await _db.ITAssets
                .AsNoTracking()
                .Where(x => x.Status != AssetStatus.Disposed)
                .OrderBy(x => x.AssetTag)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.AssetTag + " - " + x.Name
                })
                .ToListAsync();
        }

        private static string? Clean(string? value) {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
