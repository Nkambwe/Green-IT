
using System.ComponentModel.DataAnnotations;
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Assignments {
    public class CreateModel : PageModel {
        private readonly GreenITDbContext _db;

        public CreateModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty]
        public AssignmentInput Input { get; set; } = new();

        public List<SelectListItem> Assets { get; set; } = new();
        public List<SelectListItem> Departments { get; set; } = new();

        public class AssignmentInput {
            [Range(1, int.MaxValue, ErrorMessage = "Select an asset.")]
            public int ITAssetId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Select a department.")]
            public int DepartmentId { get; set; }

            public string? AssignedToUserId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime AssignedDate { get; set; } = DateTime.Today;

            public string? Notes { get; set; }
        }

        public async Task OnGetAsync() {
            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync() {
            await LoadDropdownsAsync();

            var asset = await _db.ITAssets
                .FirstOrDefaultAsync(x => x.Id == Input.ITAssetId);

            if (asset == null) {
                ModelState.AddModelError(
                    "Input.ITAssetId", "The selected asset does not exist.");
            } else {
                var isAssigned = await _db.AssetAssignments
                    .AnyAsync(x => x.ITAssetId == Input.ITAssetId &&
                                   x.ReturnedDate == null);

                if (isAssigned) {
                    ModelState.AddModelError(
                        "Input.ITAssetId",
                        "This asset already has an active assignment.");
                }

                if (asset.Status != AssetStatus.InUse &&
                    asset.Status != AssetStatus.InStorage) {
                    ModelState.AddModelError(
                        "Input.ITAssetId",
                        "Only assets in use or storage can be assigned.");
                }
            }

            var departmentExists = await _db.Departments
                .AnyAsync(x => x.Id == Input.DepartmentId && x.IsActive);

            if (!departmentExists) {
                ModelState.AddModelError(
                    "Input.DepartmentId",
                    "Select a valid active department.");
            }

            if (!ModelState.IsValid)
                return Page();

            var assignment = new AssetAssignment
            {
                ITAssetId = Input.ITAssetId,
                DepartmentId = Input.DepartmentId,
                AssignedToUserId = Clean(Input.AssignedToUserId),
                AssignedDate = Input.AssignedDate.Date,
                Notes = Clean(Input.Notes),
                CreatedAtUtc = DateTime.UtcNow
            };

            asset!.Status = AssetStatus.InUse;
            asset.UpdatedAtUtc = DateTime.UtcNow;

            _db.AssetAssignments.Add(assignment);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Asset {asset.AssetTag} was assigned successfully.";

            return RedirectToPage("./Index");
        }

        private async Task LoadDropdownsAsync() {
            var assignedAssetIds = _db.AssetAssignments
                .Where(x => x.ReturnedDate == null)
                .Select(x => x.ITAssetId);

            Assets = await _db.ITAssets
                .AsNoTracking()
                .Where(x =>
                    (x.Status == AssetStatus.InUse ||
                     x.Status == AssetStatus.InStorage) &&
                    !assignedAssetIds.Contains(x.Id))
                .OrderBy(x => x.AssetTag)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.AssetTag + " - " + x.Name
                })
                .ToListAsync();

            Departments = await _db.Departments
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
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
