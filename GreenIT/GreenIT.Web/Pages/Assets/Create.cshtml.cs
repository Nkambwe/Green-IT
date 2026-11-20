
using System.ComponentModel.DataAnnotations;
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Assets {
    public class CreateModel : PageModel {
        private readonly GreenITDbContext _db;

        public CreateModel(GreenITDbContext db) {
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

            public AssetCategory Category { get; set; } = AssetCategory.Laptop;

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

            public AssetCondition Condition { get; set; } = AssetCondition.Good;

            public AssetStatus Status { get; set; } = AssetStatus.InUse;

            public string? Notes { get; set; }
        }

        public async Task OnGetAsync() {
            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync() {
            await LoadDropdownsAsync();

            if (!Enum.IsDefined(typeof(AssetCategory), Input.Category))
                ModelState.AddModelError("Input.Category", "Select a valid category.");

            if (!Enum.IsDefined(typeof(AssetCondition), Input.Condition))
                ModelState.AddModelError("Input.Condition", "Select a valid condition.");

            if (!Enum.IsDefined(typeof(AssetStatus), Input.Status))
                ModelState.AddModelError("Input.Status", "Select a valid status.");

            if (Input.PurchaseDate.HasValue &&
                Input.WarrantyExpiryDate.HasValue &&
                Input.WarrantyExpiryDate.Value.Date < Input.PurchaseDate.Value.Date) {
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
                    .AnyAsync(x => x.AssetTag == assetTag);

                if (tagExists) {
                    ModelState.AddModelError(
                        "Input.AssetTag",
                        "This asset tag is already registered.");
                }
            }

            if (!ModelState.IsValid)
                return Page();

            var asset = new ITAsset
            {
                AssetTag = assetTag,
                SerialNumber = Clean(Input.SerialNumber),
                Name = Input.Name.Trim(),
                Category = Input.Category,
                Manufacturer = Clean(Input.Manufacturer),
                Model = Clean(Input.Model),
                DepartmentId = Input.DepartmentId,
                Location = Clean(Input.Location),
                PurchaseDate = Input.PurchaseDate,
                WarrantyExpiryDate = Input.WarrantyExpiryDate,
                PurchaseCost = Input.PurchaseCost,
                Condition = Input.Condition,
                Status = Input.Status,
                Notes = Clean(Input.Notes),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.ITAssets.Add(asset);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Asset {asset.AssetTag} was registered successfully.";

            return RedirectToPage("./Index");
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
