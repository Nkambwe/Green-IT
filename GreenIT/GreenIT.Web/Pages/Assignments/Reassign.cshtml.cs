
using System.ComponentModel.DataAnnotations;
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Assignments {
    public class ReassignModel : PageModel {
        private readonly GreenITDbContext _db;

        public ReassignModel(GreenITDbContext db) {
            _db = db;
        }

        public AssetAssignment? Assignment { get; set; }

        public List<SelectListItem> Departments { get; set; } = new();

        [BindProperty]
        public ReassignmentInput Input { get; set; } = new();

        public class ReassignmentInput {
            public int AssignmentId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Select a destination department.")]
            public int DepartmentId { get; set; }

            public string? AssignedToUserId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime ReassignmentDate { get; set; } = DateTime.Today;

            public string? Notes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id) {
            var loaded = await LoadAssignmentAsync(id);

            if (!loaded)
                return NotFound();

            Input.AssignmentId = Assignment!.Id;
            Input.DepartmentId = Assignment.DepartmentId;
            Input.AssignedToUserId = Assignment.AssignedToUserId;
            Input.ReassignmentDate = DateTime.Today;

            await LoadDepartmentsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync() {
            var loaded = await LoadAssignmentAsync(Input.AssignmentId);

            if (!loaded)
                return NotFound();

            await LoadDepartmentsAsync();

            var newStaffId = Clean(Input.AssignedToUserId);
            var oldStaffId = Clean(Assignment!.AssignedToUserId);

            var departmentExists = await _db.Departments
                .AnyAsync(x => x.Id == Input.DepartmentId && x.IsActive);

            if (!departmentExists) {
                ModelState.AddModelError(
                    "Input.DepartmentId",
                    "Select a valid active department.");
            }

            if (Input.ReassignmentDate.Date < Assignment.AssignedDate.Date) {
                ModelState.AddModelError(
                    "Input.ReassignmentDate",
                    "The reassignment date cannot be earlier than the current assignment date.");
            }

            bool departmentChanged =
                Assignment.DepartmentId != Input.DepartmentId;

            bool staffChanged =
                !string.Equals(oldStaffId, newStaffId,
                    StringComparison.OrdinalIgnoreCase);

            if (!departmentChanged && !staffChanged) {
                ModelState.AddModelError(
                    string.Empty,
                    "Change the department or staff member before reassignment.");
            }

            if (!ModelState.IsValid)
                return Page();

            var now = DateTime.UtcNow;
            var asset = Assignment.ITAsset;
            var previousDepartmentId = Assignment.DepartmentId;

            // Close the previous assignment.
            Assignment.ReturnedDate = Input.ReassignmentDate.Date;

            // Create the new active assignment.
            var newAssignment = new AssetAssignment
            {
                ITAssetId = asset.Id,
                DepartmentId = Input.DepartmentId,
                AssignedToUserId = newStaffId,
                AssignedDate = Input.ReassignmentDate.Date,
                Notes = Clean(Input.Notes),
                CreatedAtUtc = now
            };

            // Update the asset's current department and status.
            asset.DepartmentId = Input.DepartmentId;
            asset.Status = AssetStatus.InUse;
            asset.UpdatedAtUtc = now;

            _db.AssetAssignments.Add(newAssignment);

            // Record the reassignment in lifecycle history.
            _db.AssetLifecycleEvents.Add(new AssetLifecycleEvent
            {
                ITAssetId = asset.Id,
                EventType = AssetLifecycleEventType.Reassigned,
                EventDate = Input.ReassignmentDate.Date,
                FromDepartmentId = previousDepartmentId,
                ToDepartmentId = Input.DepartmentId,
                Description =
                    $"Asset reassigned from {Assignment.Department.Name} " +
                    $"to the selected department. " +
                    $"Previous staff ID: {oldStaffId ?? "Not specified"}. " +
                    $"New staff ID: {newStaffId ?? "Not specified"}. " +
                    (string.IsNullOrWhiteSpace(Input.Notes)
                        ? string.Empty
                        : $"Notes: {Input.Notes.Trim()}"),
                CreatedAtUtc = now
            });

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Asset {asset.AssetTag} was reassigned successfully.";

            return RedirectToPage("./Index");
        }

        private async Task<bool> LoadAssignmentAsync(int id) {
            Assignment = await _db.AssetAssignments
                .Include(x => x.ITAsset)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x =>
                    x.Id == id && x.ReturnedDate == null);

            return Assignment != null;
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

        private static string? Clean(string? value) {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
