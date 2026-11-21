
using System.ComponentModel.DataAnnotations;
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Assignments {
    public class ReturnModel : PageModel {
        private readonly GreenITDbContext _db;

        public ReturnModel(GreenITDbContext db) {
            _db = db;
        }

        public AssetAssignment? Assignment { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();

        [BindProperty]
        public ReturnInput Input { get; set; } = new();

        public class ReturnInput {
            public int AssignmentId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime ReturnedDate { get; set; } = DateTime.Today;

            [Required]
            public AssetStatus NewStatus { get; set; } = AssetStatus.InStorage;

            public string? Notes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id) {
            var loaded = await LoadAssignmentAsync(id);

            if (!loaded)
                return NotFound();

            Input.AssignmentId = Assignment!.Id;
            Input.ReturnedDate = DateTime.Today;
            Input.NewStatus = AssetStatus.InStorage;

            LoadStatusOptions();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync() {
            var loaded = await LoadAssignmentAsync(Input.AssignmentId);

            if (!loaded)
                return NotFound();

            if (Assignment!.ReturnedDate != null) {
                ModelState.AddModelError(string.Empty,
                    "This assignment has already been returned.");
            }

            if (!Enum.IsDefined(typeof(AssetStatus), Input.NewStatus)) {
                ModelState.AddModelError("Input.NewStatus",
                    "Select a valid asset status.");
            }

            if (Input.ReturnedDate.Date < Assignment.AssignedDate.Date) {
                ModelState.AddModelError("Input.ReturnedDate",
                    "The return date cannot be earlier than the assignment date.");
            }

            LoadStatusOptions();

            if (!ModelState.IsValid)
                return Page();

            var now = DateTime.UtcNow;
            var asset = Assignment.ITAsset;

            // Close the active assignment.
            Assignment.ReturnedDate = Input.ReturnedDate.Date;

            // Update the asset's current status.
            asset.Status = Input.NewStatus;
            asset.UpdatedAtUtc = now;

            // Record the return in the asset lifecycle history.
            _db.AssetLifecycleEvents.Add(new AssetLifecycleEvent
            {
                ITAssetId = asset.Id,
                EventType = AssetLifecycleEventType.Returned,
                EventDate = Input.ReturnedDate.Date,
                FromDepartmentId = Assignment.DepartmentId,
                ToDepartmentId = null,
                Description =
                    $"Asset returned from {Assignment.Department.Name}. " +
                    $"New asset status: {Input.NewStatus}. " +
                    (string.IsNullOrWhiteSpace(Input.Notes)
                        ? string.Empty
                        : $"Return notes: {Input.Notes.Trim()}"),
                CreatedAtUtc = now
            });

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Asset {asset.AssetTag} was returned successfully. " +
                $"New status: {Input.NewStatus}.";

            return RedirectToPage("./Index");
        }

        private async Task<bool> LoadAssignmentAsync(int id) {
            Assignment = await _db.AssetAssignments
                .Include(x => x.ITAsset)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            return Assignment != null;
        }

        private void LoadStatusOptions() {
            StatusOptions = Enum.GetValues<AssetStatus>()
                .Select(status => new SelectListItem
                {
                    Value = status.ToString(),
                    Text = status.ToString()
                })
                .ToList();
        }
    }
}
