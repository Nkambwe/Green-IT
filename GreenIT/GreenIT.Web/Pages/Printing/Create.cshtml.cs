
using GreenIT.Domain.Entities;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GreenIT.Web.Pages.Printing {
    public class CreateModel : PageModel {
        private readonly GreenITDbContext _db;

        public CreateModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty]
        public PrintingRecord Input { get; set; } = new();

        public List<SelectListItem> Departments { get; set; }
            = new();

        public List<SelectListItem> Months { get; set; }
            = new();

        public async Task OnGetAsync() {
            Input.Year = DateTime.Today.Year;
            Input.Month = DateTime.Today.Month;
            Input.Source = "Manual Entry";

            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync() {
            await LoadDropdownsAsync();

            // Validate department
            var departmentExists = await _db.Departments
                .AnyAsync(x => x.Id == Input.DepartmentId);

            if (!departmentExists) {
                ModelState.AddModelError(
                    "Input.DepartmentId",
                    "Please select a valid department.");
            }

            // Validate reporting period
            if (Input.Year < 2000 || Input.Year > DateTime.Today.Year + 1) {
                ModelState.AddModelError(
                    "Input.Year",
                    "Please enter a valid reporting year.");
            }

            if (Input.Month < 1 || Input.Month > 12) {
                ModelState.AddModelError(
                    "Input.Month",
                    "Please select a valid reporting month.");
            }

            // Validate page counts
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

            // Check for duplicate department and reporting period
            var duplicateExists = await _db.PrintingRecords.AnyAsync(x =>
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

            Input.Source = string.IsNullOrWhiteSpace(Input.Source)
                ? "Manual Entry"
                : Input.Source.Trim();

            Input.Notes = string.IsNullOrWhiteSpace(Input.Notes)
                ? null
                : Input.Notes.Trim();

            _db.PrintingRecords.Add(Input);

            try {
                await _db.SaveChangesAsync();
            } catch (DbUpdateException) {
                ModelState.AddModelError(
                    string.Empty,
                    "The printing record could not be saved. Please check whether a record already exists for this department and period.");

                return Page();
            }

            TempData["SuccessMessage"] =
                "Printing record created successfully.";

            return RedirectToPage("./Index");
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
