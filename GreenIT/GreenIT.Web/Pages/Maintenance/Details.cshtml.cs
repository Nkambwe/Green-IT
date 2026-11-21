
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Maintenance {
    public class DetailsModel : PageModel {
        private readonly GreenITDbContext _db;

        public DetailsModel(GreenITDbContext db) {
            _db = db;
        }

        public GreenIT.Domain.Entities.AssetMaintenance Record { get; set; }
            = null!;

        public async Task<IActionResult> OnGetAsync(int id) {
            var record = await _db.AssetMaintenanceRecords
                .AsNoTracking()
                .Include(x => x.ITAsset)
                    .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null)
                return NotFound();

            Record = record;

            return Page();
        }
    }
}
