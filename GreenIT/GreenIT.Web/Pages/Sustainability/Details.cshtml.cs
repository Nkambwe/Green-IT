
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Sustainability {
    public class DetailsModel : PageModel {
        private readonly GreenITDbContext _db;

        public DetailsModel(GreenITDbContext db) {
            _db = db;
        }

        public SustainabilityAction Action { get; set; } = null!;

        public string? AssignedToName { get; set; }

        public bool IsOverdue { get; set; }

        public async Task<IActionResult> OnGetAsync(int id) {
            var action = await _db.SustainabilityActions
                .AsNoTracking()
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (action == null)
                return NotFound();

            Action = action;

            if (!string.IsNullOrWhiteSpace(Action.AssignedToUserId)) {
                AssignedToName = await _db.Users
                    .AsNoTracking()
                    .Where(x => x.Id == Action.AssignedToUserId)
                    .Select(x => x.FullName)
                    .FirstOrDefaultAsync();
            }

            IsOverdue =
                Action.TargetDate.HasValue &&
                Action.TargetDate.Value.Date < DateTime.Today &&
                Action.Status != SustainabilityActionStatus.Completed &&
                Action.Status != SustainabilityActionStatus.Cancelled;

            return Page();
        }
    }
}
