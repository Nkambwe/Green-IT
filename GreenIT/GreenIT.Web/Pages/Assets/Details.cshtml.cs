
using GreenIT.Domain.Entities;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Assets {
    public class DetailsModel : PageModel {
        private readonly GreenITDbContext _db;

        public DetailsModel(GreenITDbContext db) {
            _db = db;
        }

        public ITAsset Asset { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id) {
            if (!id.HasValue)
                return NotFound();

            var asset = await _db.ITAssets
                .AsNoTracking()
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id.Value);

            if (asset == null)
                return NotFound();

            Asset = asset;

            return Page();
        }
    }
}
