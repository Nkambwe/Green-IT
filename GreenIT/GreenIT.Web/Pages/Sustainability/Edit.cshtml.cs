
using System.ComponentModel.DataAnnotations;
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Sustainability {
    public class EditModel : PageModel {
        private readonly GreenITDbContext _db;

        public EditModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> Departments { get; set; } = new();

        public List<SelectListItem> Users { get; set; } = new();

        public class InputModel {
            public int Id { get; set; }

            [Required]
            [StringLength(200)]
            public string Title { get; set; } = string.Empty;

            [Required]
            [StringLength(1500)]
            public string Description { get; set; } = string.Empty;

            public int? DepartmentId { get; set; }

            public string? AssignedToUserId { get; set; }

            public SustainabilityPriority Priority { get; set; }

            public SustainabilityActionStatus Status { get; set; }

            [Range(0, 100)]
            public int ProgressPercentage { get; set; }

            [Required]
            public DateTime StartDate { get; set; }

            public DateTime? TargetDate { get; set; }

            [StringLength(1000)]
            public string? Notes { get; set; }
        }

        // -------------------------------------------------
        // GET: LOAD EXISTING ACTION
        // -------------------------------------------------

        public async Task<IActionResult> OnGetAsync(int id) {
            var action = await _db.SustainabilityActions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (action == null)
                return NotFound();

            Input = new InputModel
            {
                Id = action.Id,
                Title = action.Title,
                Description = action.Description,
                DepartmentId = action.DepartmentId,
                AssignedToUserId = action.AssignedToUserId,
                Priority = action.Priority,
                Status = action.Status,
                ProgressPercentage = action.ProgressPercentage,
                StartDate = action.StartDate,
                TargetDate = action.TargetDate,
                Notes = action.Notes
            };

            await LoadDropdownsAsync(action.AssignedToUserId);

            return Page();
        }

        // -------------------------------------------------
        // POST: UPDATE ACTION
        // -------------------------------------------------

        public async Task<IActionResult> OnPostAsync(int id) {
            if (id != Input.Id)
                return BadRequest();

            var action = await _db.SustainabilityActions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (action == null)
                return NotFound();

            // Reload dropdowns if validation fails.
            await LoadDropdownsAsync(Input.AssignedToUserId);

            // ---------------------------------------------
            // BASIC VALIDATION
            // ---------------------------------------------

            if (!Enum.IsDefined(typeof(SustainabilityPriority), Input.Priority)) {
                ModelState.AddModelError(
                    "Input.Priority", "Select a valid priority.");
            }

            if (!Enum.IsDefined(typeof(SustainabilityActionStatus), Input.Status)) {
                ModelState.AddModelError(
                    "Input.Status", "Select a valid status.");
            }

            if (Input.StartDate == default) {
                ModelState.AddModelError(
                    "Input.StartDate", "A start date is required.");
            }

            if (Input.TargetDate.HasValue &&
                Input.StartDate != default &&
                Input.TargetDate.Value.Date < Input.StartDate.Date) {
                ModelState.AddModelError(
                    "Input.TargetDate",
                    "Target date cannot be earlier than the start date.");
            }

            // ---------------------------------------------
            // DEPARTMENT VALIDATION
            // ---------------------------------------------

            if (Input.DepartmentId.HasValue) {
                var departmentExists = await _db.Departments
                    .AnyAsync(x =>
                        x.Id == Input.DepartmentId.Value &&
                        x.IsActive);

                if (!departmentExists) {
                    ModelState.AddModelError(
                        "Input.DepartmentId",
                        "Select a valid active department.");
                }
            }

            // ---------------------------------------------
            // ASSIGNED USER VALIDATION
            // ---------------------------------------------

            if (!string.IsNullOrWhiteSpace(Input.AssignedToUserId)) {
                var userExists = await _db.Users
                    .AnyAsync(x =>
                        x.Id == Input.AssignedToUserId &&
                        x.IsActive);

                if (!userExists) {
                    ModelState.AddModelError(
                        "Input.AssignedToUserId",
                        "Select a valid active user.");
                }
            }

            // ---------------------------------------------
            // PROGRESS VALIDATION
            // ---------------------------------------------

            if (Input.ProgressPercentage < 0 ||
                Input.ProgressPercentage > 100) {
                ModelState.AddModelError(
                    "Input.ProgressPercentage",
                    "Progress must be between 0 and 100.");
            }

            if (Input.Status == SustainabilityActionStatus.Completed &&
                Input.ProgressPercentage != 100) {
                ModelState.AddModelError(
                    "Input.ProgressPercentage",
                    "A completed action must have 100% progress.");
            }

            if (Input.Status != SustainabilityActionStatus.Completed &&
                Input.ProgressPercentage == 100) {
                ModelState.AddModelError(
                    "Input.ProgressPercentage",
                    "Only completed actions can have 100% progress.");
            }

            if (Input.Status == SustainabilityActionStatus.NotStarted &&
                Input.ProgressPercentage != 0) {
                ModelState.AddModelError(
                    "Input.ProgressPercentage",
                    "An action that has not started must have 0% progress.");
            }

            if (!ModelState.IsValid)
                return Page();

            // ---------------------------------------------
            // UPDATE RECORD
            // ---------------------------------------------

            var previousStatus = action.Status;

            action.Title = Input.Title.Trim();
            action.Description = Input.Description.Trim();

            action.DepartmentId = Input.DepartmentId;

            action.AssignedToUserId =
                string.IsNullOrWhiteSpace(Input.AssignedToUserId)
                    ? null
                    : Input.AssignedToUserId;

            action.Priority = Input.Priority;
            action.Status = Input.Status;

            action.ProgressPercentage = Input.ProgressPercentage;

            action.StartDate = Input.StartDate.Date;

            action.TargetDate = Input.TargetDate?.Date;

            action.Notes = string.IsNullOrWhiteSpace(Input.Notes)
                ? null
                : Input.Notes.Trim();

            // ---------------------------------------------
            // COMPLETION DATE MANAGEMENT
            // ---------------------------------------------

            if (Input.Status == SustainabilityActionStatus.Completed) {
                // Preserve the original completion date if
                // the action was already completed.
                action.CompletedDate =
                    previousStatus == SustainabilityActionStatus.Completed
                        ? action.CompletedDate ?? DateTime.Today
                        : DateTime.Today;
            } else {
                // Clear completion date when reopened or cancelled.
                action.CompletedDate = null;
            }

            // Preserve CreatedAtUtc.
            action.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Sustainability action updated successfully.";

            return RedirectToPage("Details", new { id = action.Id });
        }

        // -------------------------------------------------
        // DROPDOWN DATA
        // -------------------------------------------------

        private async Task LoadDropdownsAsync(
            string? selectedUserId = null) {
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

            var usersQuery = _db.Users
                .AsNoTracking()
                .Where(x => x.IsActive);

            // Retain an existing selection if its user has
            // subsequently become inactive.
            if (!string.IsNullOrWhiteSpace(selectedUserId)) {
                usersQuery = _db.Users
                    .AsNoTracking()
                    .Where(x =>
                        x.IsActive || x.Id == selectedUserId);
            }

            Users = await usersQuery
                .OrderBy(x => x.FullName)
                .Select(x => new SelectListItem
                {
                    Value = x.Id,
                    Text = x.FullName
                })
                .ToListAsync();
        }
    }
}
