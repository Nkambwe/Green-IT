
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Sustainability {
    public class CreateModel : PageModel {
        private readonly GreenITDbContext _db;

        public CreateModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty]
        public SustainabilityAction Input { get; set; } = new();

        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> Users { get; set; } = new();

        public async Task OnGetAsync() {
            Input.StartDate = DateTime.Today;
            Input.Priority = SustainabilityPriority.Medium;
            Input.Status = SustainabilityActionStatus.NotStarted;
            Input.ProgressPercentage = 0;

            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync() {
            await LoadDropdownsAsync();

            if (string.IsNullOrWhiteSpace(Input.Title))
                ModelState.AddModelError("Input.Title", "Title is required.");

            if (Input.Title?.Length > 200)
                ModelState.AddModelError("Input.Title", "Title cannot exceed 200 characters.");

            if (string.IsNullOrWhiteSpace(Input.Description))
                ModelState.AddModelError("Input.Description", "Description is required.");

            if (Input.Description?.Length > 1500)
                ModelState.AddModelError("Input.Description", "Description cannot exceed 1,500 characters.");

            if (Input.Notes?.Length > 1000)
                ModelState.AddModelError("Input.Notes", "Notes cannot exceed 1,000 characters.");

            if (Input.DepartmentId.HasValue) {
                var departmentExists = await _db.Departments
                    .AnyAsync(x => x.Id == Input.DepartmentId.Value);

                if (!departmentExists)
                    ModelState.AddModelError("Input.DepartmentId", "Please select a valid department.");
            }

            if (!string.IsNullOrWhiteSpace(Input.AssignedToUserId)) {
                var userExists = await _db.Users.AnyAsync(x =>
                    x.Id == Input.AssignedToUserId && x.IsActive);

                if (!userExists)
                    ModelState.AddModelError("Input.AssignedToUserId",
                        "Please select a valid active user.");
            }

            if (!Enum.IsDefined(typeof(SustainabilityPriority), Input.Priority))
                ModelState.AddModelError("Input.Priority", "Please select a valid priority.");

            if (!Enum.IsDefined(typeof(SustainabilityActionStatus), Input.Status))
                ModelState.AddModelError("Input.Status", "Please select a valid status.");

            if (Input.ProgressPercentage < 0 || Input.ProgressPercentage > 100)
                ModelState.AddModelError("Input.ProgressPercentage",
                    "Progress must be between 0 and 100.");

            if (Input.StartDate == default)
                ModelState.AddModelError("Input.StartDate", "Start date is required.");

            if (Input.TargetDate.HasValue &&
                Input.TargetDate.Value.Date < Input.StartDate.Date) {
                ModelState.AddModelError("Input.TargetDate",
                    "Target date cannot be earlier than the start date.");
            }

            if (Input.Status == SustainabilityActionStatus.Completed) {
                if (Input.ProgressPercentage != 100)
                    ModelState.AddModelError("Input.ProgressPercentage",
                        "A completed action must have 100% progress.");
            } else {
                if (Input.ProgressPercentage == 100)
                    ModelState.AddModelError("Input.ProgressPercentage",
                        "Only completed actions can have 100% progress.");
            }

            if (Input.Status == SustainabilityActionStatus.NotStarted &&
                Input.ProgressPercentage != 0) {
                ModelState.AddModelError("Input.ProgressPercentage",
                    "A not-started action must have 0% progress.");
            }

            if (!ModelState.IsValid)
                return Page();

            Input.Title = Input.Title??"".Trim();
            Input.Description = Input.Description?? "".Trim();
            Input.Notes = string.IsNullOrWhiteSpace(Input.Notes)
                ? null
                : Input.Notes.Trim();

            Input.CompletedDate =
                Input.Status == SustainabilityActionStatus.Completed
                    ? DateTime.Today
                    : null;

            Input.CreatedAtUtc = DateTime.UtcNow;
            Input.UpdatedAtUtc = null;

            _db.SustainabilityActions.Add(Input);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Sustainability action created successfully.";

            return RedirectToPage("./Details", new { id = Input.Id });
        }

        private async Task LoadDropdownsAsync() {
            Departments = await _db.Departments
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();

            Users = await _db.Users
                .AsNoTracking()
                .Where(x => x.IsActive)
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
