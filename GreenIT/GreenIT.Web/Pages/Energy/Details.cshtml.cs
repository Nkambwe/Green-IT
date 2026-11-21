
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Energy {
    public class DetailsModel : PageModel {
        private readonly GreenITDbContext _db;

        public DetailsModel(GreenITDbContext db) {
            _db = db;
        }

        public EnergyAudit Audit { get; set; } = null!;

        public List<WorkstationViewModel> Workstations { get; set; } = new();

        [BindProperty]
        public ChecklistInput Input { get; set; } = new();

        public class ChecklistInput {
            public int AuditId { get; set; }

            public List<WorkstationInput> Items { get; set; } = new();
        }

        public class WorkstationInput {
            public int ITAssetId { get; set; }

            public bool IsSleepModeEnabled { get; set; }

            public bool IsAutomaticShutdownEnabled { get; set; }

            public bool IsMonitorPowerSavingEnabled { get; set; }

            public string? Observation { get; set; }
        }

        public class WorkstationViewModel {
            public int ITAssetId { get; set; }
            public string AssetTag { get; set; } = string.Empty;
            public string AssetName { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string? Location { get; set; }

            public bool IsSleepModeEnabled { get; set; }
            public bool IsAutomaticShutdownEnabled { get; set; }
            public bool IsMonitorPowerSavingEnabled { get; set; }

            public string? Observation { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id) {
            var audit = await _db.EnergyAudits
                .AsNoTracking()
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (audit == null)
                return NotFound();

            Audit = audit;

            var assets = await _db.ITAssets
                .AsNoTracking()
                .Where(x =>
                    x.DepartmentId == audit.DepartmentId &&
                    x.Status == AssetStatus.InUse &&
                    (x.Category == AssetCategory.Laptop ||
                     x.Category == AssetCategory.Desktop))
                .OrderBy(x => x.AssetTag)
                .ToListAsync();

            var existingItems = await _db.EnergyAuditItems
                .AsNoTracking()
                .Where(x => x.EnergyAuditId == id && x.ITAssetId != null)
                .ToListAsync();

            var itemLookup = existingItems
                .GroupBy(x => x.ITAssetId!.Value)
                .ToDictionary(x => x.Key, x => x.First());

            Workstations = assets.Select(asset =>
            {
                itemLookup.TryGetValue(asset.Id, out var item);

                return new WorkstationViewModel
                {
                    ITAssetId = asset.Id,
                    AssetTag = asset.AssetTag,
                    AssetName = asset.Name,
                    Category = asset.Category.ToString(),
                    Location = asset.Location,

                    IsSleepModeEnabled = item?.IsSleepModeEnabled ?? false,
                    IsAutomaticShutdownEnabled = item?.IsAutomaticShutdownEnabled ?? false,
                    IsMonitorPowerSavingEnabled = item?.IsMonitorPowerSavingEnabled ?? false,

                    Observation = item?.Observation
                };
            }).ToList();

            Input.AuditId = id;

            Input.Items = Workstations.Select(x => new WorkstationInput
            {
                ITAssetId = x.ITAssetId,
                IsSleepModeEnabled = x.IsSleepModeEnabled,
                IsAutomaticShutdownEnabled = x.IsAutomaticShutdownEnabled,
                IsMonitorPowerSavingEnabled = x.IsMonitorPowerSavingEnabled,
                Observation = x.Observation
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveObservationsAsync() {
            var audit = await _db.EnergyAudits
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == Input.AuditId);

            if (audit == null)
                return NotFound();

            if (audit.Status != EnergyAuditStatus.Draft) {
                TempData["ErrorMessage"] =
                    "Observations can only be edited while the audit is in Draft status.";

                return RedirectToPage("./Details", new { id = audit.Id });
            }

            var submittedIds = Input.Items
                .Select(x => x.ITAssetId)
                .Distinct()
                .ToList();

            var validAssetIds = await _db.ITAssets
                .Where(x =>
                    submittedIds.Contains(x.Id) &&
                    x.DepartmentId == audit.DepartmentId &&
                    x.Status == AssetStatus.InUse &&
                    (x.Category == AssetCategory.Laptop ||
                     x.Category == AssetCategory.Desktop))
                .Select(x => x.Id)
                .ToListAsync();

            if (validAssetIds.Count != submittedIds.Count) {
                TempData["ErrorMessage"] =
                    "One or more selected workstations are invalid. Please reload the audit.";

                return RedirectToPage("./Details", new { id = audit.Id });
            }

            var existingItems = await _db.EnergyAuditItems
                .Where(x => x.EnergyAuditId == audit.Id)
                .ToListAsync();

            foreach (var input in Input.Items) {
                var item = existingItems.FirstOrDefault(x =>
                    x.ITAssetId == input.ITAssetId);

                if (item == null) {
                    item = new EnergyAuditItem
                    {
                        EnergyAuditId = audit.Id,
                        ITAssetId = input.ITAssetId,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    _db.EnergyAuditItems.Add(item);
                }

                item.IsSleepModeEnabled = input.IsSleepModeEnabled;
                item.IsAutomaticShutdownEnabled = input.IsAutomaticShutdownEnabled;
                item.IsMonitorPowerSavingEnabled = input.IsMonitorPowerSavingEnabled;
                item.Observation = Clean(input.Observation);
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Workstation observations saved successfully.";

            return RedirectToPage("./Details", new { id = audit.Id });
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id) {
            var audit = await _db.EnergyAudits
                .FirstOrDefaultAsync(x => x.Id == id);

            if (audit == null)
                return NotFound();

            if (audit.Status != EnergyAuditStatus.Draft) {
                TempData["ErrorMessage"] =
                    "Only Draft audits can be completed.";

                return RedirectToPage("./Details", new { id });
            }

            var observationCount = await _db.EnergyAuditItems
                .CountAsync(x => x.EnergyAuditId == id);

            if (observationCount == 0) {
                TempData["ErrorMessage"] =
                    "You must save at least one workstation observation before completing this audit.";

                return RedirectToPage("./Details", new { id });
            }

            audit.Status = EnergyAuditStatus.Completed;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Energy audit completed successfully. It is now ready for review.";

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostReviewAsync(int id) {
            var audit = await _db.EnergyAudits
                .FirstOrDefaultAsync(x => x.Id == id);

            if (audit == null)
                return NotFound();

            if (audit.Status != EnergyAuditStatus.Completed) {
                TempData["ErrorMessage"] =
                    "Only Completed audits can be reviewed.";

                return RedirectToPage("./Details", new { id });
            }

            audit.Status = EnergyAuditStatus.Reviewed;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Energy audit reviewed successfully.";

            return RedirectToPage("./Details", new { id });
        }

        private static string? Clean(string? value) {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
