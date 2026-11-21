
using GreenIT.Domain.Entities;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Printing {
    public class DetailsModel : PageModel {
        private readonly GreenITDbContext _db;

        public DetailsModel(GreenITDbContext db) {
            _db = db;
        }

        public PrintingRecord Record { get; set; } = null!;

        public decimal DuplexPercentage { get; set; }
        public decimal ColourPercentage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id) {
            var record = await _db.PrintingRecords
                .AsNoTracking()
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
                return NotFound();

            Record = record;

            if (Record.TotalPages > 0) {
                DuplexPercentage = Math.Round(
                    (decimal)Record.DuplexPages / Record.TotalPages * 100, 1);

                ColourPercentage = Math.Round(
                    (decimal)Record.ColourPages / Record.TotalPages * 100, 1);
            }

            return Page();
        }
    }
}
