
using GreenIT.Domain.Entities;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GreenIT.Web.Pages.Printing {
    public class EditModel : PageModel {
        private readonly GreenITDbContext _db;

        public EditModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty]
        public PrintingRecord Input { get; set; } = new();

        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> Months { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id) {
            var record = await _db.PrintingRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
                return NotFound();

            Input = record;

            await LoadDropdownsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id) {
            if (id != Input.Id)
                return BadRequest();

            await LoadDropdownsAsync();

            var record = await _db.PrintingRecords
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
                return NotFound();

            var departmentExists = await _db.Departments
                .AnyAsync(x => x.Id == Input.DepartmentId);

            if (!departmentExists) {
                ModelState.AddModelError(
                    "Input.DepartmentId",
                    "Please select a valid department.");
            }

            if (Input.Year < 2000 ||
                Input.Year > DateTime.Today.Year + 1) {
                ModelState.AddModelError(
                    "Input.Year",
                    "Please enter a valid reporting year.");
            }

            if (Input.Month < 1 || Input.Month > 12) {
                ModelState.AddModelError(
                    "Input.Month",
                    "Please select a valid reporting month.");
            }

            if (Input.TotalPages < 0) {
                ModelState.AddModelError(
                    "Input.TotalPages",
                    "Total pages cannot be negative.");
            }

            if (Input.DuplexPages < 0 ||
                Input.DuplexPages > Input.TotalPages) {
                ModelState.AddModelError(
                    "Input.DuplexPages",
                    "Duplex pages must be between zero and total pages.");
            }

            if (Input.ColourPages < 0 ||
                Input.ColourPages > Input.TotalPages) {
                ModelState.AddModelError(
                    "Input.ColourPages",
                    "Colour pages must be between zero and total pages.");
            }

            if (Input.PrintJobs < 0) {
                ModelState.AddModelError(
                    "Input.PrintJobs",
                    "Print jobs cannot be negative.");
            }

            var duplicateExists = await _db.PrintingRecords.AnyAsync(x =>
                x.Id != id &&
                x.DepartmentId == Input.DepartmentId &&
                x.Year == Input.Year &&
                x.Month == Input.Month);

            if (duplicateExists) {
                ModelState.AddModelError(
                    string.Empty,
                    "A printing record already exists for this department and reporting period.");
            }

            if (!ModelState.IsValid)
                return Page();

            // Update only the editable fields.
            // Preserve Id, CreatedAtUtc and existing Source/Notes handling.

            record.DepartmentId = Input.DepartmentId;
            record.Year = Input.Year;
            record.Month = Input.Month;
            record.TotalPages = Input.TotalPages;
            record.DuplexPages = Input.DuplexPages;
            record.ColourPages = Input.ColourPages;
            record.PrintJobs = Input.PrintJobs;

            record.Source = string.IsNullOrWhiteSpace(Input.Source)
                ? null
                : Input.Source.Trim();

            record.Notes = string.IsNullOrWhiteSpace(Input.Notes)
                ? null
                : Input.Notes.Trim();

            try {
                await _db.SaveChangesAsync();
            } catch (DbUpdateException) {
                ModelState.AddModelError(
                    string.Empty,
                    "The printing record could not be updated. Please check whether another record already exists for this department and period.");

                return Page();
            }

            TempData["SuccessMessage"] =
                "Printing record updated successfully.";

            return RedirectToPage("./Details", new { id });
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

            Months = Enumerable.Range(1, 12)
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = CultureInfo.CurrentCulture
                        .DateTimeFormat.GetMonthName(x)
                })
                .ToList();
        }
    }
}
