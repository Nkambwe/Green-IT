
using GreenIT.Domain.Entities;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Sustainability {
    public class DeleteModel : PageModel {
        private readonly GreenITDbContext _db;

        public DeleteModel(GreenITDbContext db) {
            _db = db;
        }

        public SustainabilityAction Action { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id) {
            var action = await _db.SustainabilityActions
                .AsNoTracking()
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (action == null)
                return NotFound();

            Action = action;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id) {
            var action = await _db.SustainabilityActions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (action == null)
                return NotFound();

            _db.SustainabilityActions.Remove(action);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Sustainability action deleted successfully.";

            return RedirectToPage("Index");
        }
    }
}
