
using System.ComponentModel.DataAnnotations;
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Energy {
    public class CreateModel : PageModel {
        private readonly GreenITDbContext _db;

        public CreateModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty]
        public AuditInput Input { get; set; } = new();

        public List<SelectListItem> Departments { get; set; } = new();

        public class AuditInput {
            [Required]
            [StringLength(50)]
            public string AuditReference { get; set; } = string.Empty;

            [Range(1, int.MaxValue, ErrorMessage = "Select a department.")]
            public int DepartmentId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime AuditDate { get; set; } = DateTime.Today;

            [StringLength(450)]
            public string? AuditorUserId { get; set; }

            [StringLength(1000)]
            public string? Notes { get; set; }
        }

        public async Task OnGetAsync() {
            await LoadDepartmentsAsync();

            Input.AuditReference = await GenerateAuditReferenceAsync();
        }

        public async Task<IActionResult> OnPostAsync() {
            await LoadDepartmentsAsync();

            var reference = Input.AuditReference.Trim();

            var referenceExists = await _db.EnergyAudits
                .AnyAsync(x => x.AuditReference == reference);

            if (referenceExists) {
                ModelState.AddModelError(
                    "Input.AuditReference",
                    "This audit reference already exists.");
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

            var audit = new EnergyAudit
            {
                AuditReference = reference,
                DepartmentId = Input.DepartmentId,
                AuditDate = Input.AuditDate.Date,
                Status = EnergyAuditStatus.Draft,
                AuditorUserId = Clean(Input.AuditorUserId),
                Notes = Clean(Input.Notes),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.EnergyAudits.Add(audit);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Energy audit created successfully. You can now add workstation observations.";

            return RedirectToPage("./Details", new { id = audit.Id });
        }

        private async Task LoadDepartmentsAsync() {
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

        private async Task<string> GenerateAuditReferenceAsync() {
            var prefix = $"EA-{DateTime.Today:yyyyMMdd}-";

            var lastReference = await _db.EnergyAudits
                .AsNoTracking()
                .Where(x => x.AuditReference.StartsWith(prefix))
                .OrderByDescending(x => x.AuditReference)
                .Select(x => x.AuditReference)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastReference)) {
                var suffix = lastReference.Substring(prefix.Length);

                if (int.TryParse(suffix, out var lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"{prefix}{nextNumber:D3}";
        }

        private static string? Clean(string? value) {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
