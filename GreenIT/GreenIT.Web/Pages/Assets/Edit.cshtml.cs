
using System.ComponentModel.DataAnnotations;
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Assets {
    public class EditModel : PageModel {
        private readonly GreenITDbContext _db;

        public EditModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty]
        public AssetInputModel Input { get; set; } = new();

        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Conditions { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new();

        public class AssetInputModel {
            [Required(ErrorMessage = "Asset tag is required.")]
            public string AssetTag { get; set; } = string.Empty;

            public string? SerialNumber { get; set; }

            [Required(ErrorMessage = "Asset name is required.")]
            public string Name { get; set; } = string.Empty;

            public AssetCategory Category { get; set; }

            public string? Manufacturer { get; set; }
            public string? Model { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Please select a department.")]
            public int DepartmentId { get; set; }

            public string? Location { get; set; }

            [DataType(DataType.Date)]
            public DateTime? PurchaseDate { get; set; }

            [DataType(DataType.Date)]
            public DateTime? WarrantyExpiryDate { get; set; }

            [Range(typeof(decimal), "0", "9999999999999999",
                ErrorMessage = "Purchase cost cannot be negative.")]
            public decimal? PurchaseCost { get; set; }

            public AssetCondition Condition { get; set; }

            public AssetStatus Status { get; set; }

            public string? Notes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id) {
            var asset = await _db.ITAssets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (asset == null)
                return NotFound();

            Input = new AssetInputModel
            {
                AssetTag = asset.AssetTag,
                SerialNumber = asset.SerialNumber,
                Name = asset.Name,
                Category = asset.Category,
                Manufacturer = asset.Manufacturer,
                Model = asset.Model,
                DepartmentId = asset.DepartmentId,
                Location = asset.Location,
                PurchaseDate = asset.PurchaseDate,
                WarrantyExpiryDate = asset.WarrantyExpiryDate,
                PurchaseCost = asset.PurchaseCost,
                Condition = asset.Condition,
                Status = asset.Status,
                Notes = asset.Notes
            };

            await LoadDropdownsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id) {
            await LoadDropdownsAsync();

            var asset = await _db.ITAssets
                .FirstOrDefaultAsync(x => x.Id == id);

            if (asset == null)
                return NotFound();

            if (!Enum.IsDefined(typeof(AssetCategory), Input.Category))
                ModelState.AddModelError(
                    "Input.Category", "Select a valid category.");

            if (!Enum.IsDefined(typeof(AssetCondition), Input.Condition))
                ModelState.AddModelError(
                    "Input.Condition", "Select a valid condition.");

            if (!Enum.IsDefined(typeof(AssetStatus), Input.Status))
                ModelState.AddModelError(
                    "Input.Status", "Select a valid status.");

            if (Input.PurchaseDate.HasValue &&
                Input.WarrantyExpiryDate.HasValue &&
                Input.WarrantyExpiryDate.Value.Date <
                Input.PurchaseDate.Value.Date) {
                ModelState.AddModelError(
                    "Input.WarrantyExpiryDate",
                    "Warranty expiry cannot be earlier than the purchase date.");
            }

            var departmentExists = await _db.Departments
                .AnyAsync(x => x.Id == Input.DepartmentId && x.IsActive);

            if (!departmentExists) {
                ModelState.AddModelError(
                    "Input.DepartmentId",
                    "Please select an active department.");
            }

            var assetTag = Input.AssetTag.Trim();

            if (!string.IsNullOrWhiteSpace(assetTag)) {
                var tagExists = await _db.ITAssets
                    .AnyAsync(x => x.Id != id && x.AssetTag == assetTag);

                if (tagExists) {
                    ModelState.AddModelError(
                        "Input.AssetTag",
                        "This asset tag is already registered to another asset.");
                }
            }

            if (!ModelState.IsValid)
                return Page();

            // Update the existing asset.
            asset.AssetTag = assetTag;
            asset.SerialNumber = Clean(Input.SerialNumber);
            asset.Name = Input.Name.Trim();
            asset.Category = Input.Category;
            asset.Manufacturer = Clean(Input.Manufacturer);
            asset.Model = Clean(Input.Model);
            asset.DepartmentId = Input.DepartmentId;
            asset.Location = Clean(Input.Location);
            asset.PurchaseDate = Input.PurchaseDate;
            asset.WarrantyExpiryDate = Input.WarrantyExpiryDate;
            asset.PurchaseCost = Input.PurchaseCost;
            asset.Condition = Input.Condition;
            asset.Status = Input.Status;
            asset.Notes = Clean(Input.Notes);
            asset.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Asset {asset.AssetTag} was updated successfully.";

            return RedirectToPage("./Details", new { id = asset.Id });
        }

        private async Task LoadDropdownsAsync() {
            Departments = await _db.Departments
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
                .ToListAsync();

            Categories = CreateOptions<AssetCategory>();
            Conditions = CreateOptions<AssetCondition>();
            Statuses = CreateOptions<AssetStatus>();
        }

        private static List<SelectListItem> CreateOptions<T>()
            where T : struct, Enum {
            return Enum.GetValues<T>()
                .Select(value => new SelectListItem
                {
                    Text = value.ToString(),
                    Value = Convert.ToInt32(value).ToString()
                })
                .ToList();
        }

        private static string? Clean(string? value) {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
