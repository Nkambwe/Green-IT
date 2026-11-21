
using GreenIT.Domain.Entities;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Printing {
    public class DeleteModel : PageModel {
        private readonly GreenITDbContext _db;

        public DeleteModel(GreenITDbContext db) {
            _db = db;
        }

        public PrintingRecord Record { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id) {
            var record = await _db.PrintingRecords
                .AsNoTracking()
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
                return NotFound();

            Record = record;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id) {
            var record = await _db.PrintingRecords
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
                return NotFound();

            _db.PrintingRecords.Remove(record);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Printing record deleted successfully.";

            return RedirectToPage("./Index");
        }
    }
}
