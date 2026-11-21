
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Printing {
    public class IndexModel : PageModel {
        private readonly GreenITDbContext _db;

        public IndexModel(GreenITDbContext db) {
            _db = db;
        }

        public IList<GreenIT.Domain.Entities.PrintingRecord> Records { get; set; } = new List<GreenIT.Domain.Entities.PrintingRecord>();

        public int TotalRecords { get; set; }
        public int TotalPagesPrinted { get; set; }
        public int TotalDuplexPages { get; set; }
        public int TotalColourPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync() {
            if (PageNumber < 1)
                PageNumber = 1;

            var query = _db.PrintingRecords
                .AsNoTracking()
                .Include(x => x.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm)) {
                var term = SearchTerm.Trim();

                query = query.Where(x =>
                    x.Department.Name.Contains(term) ||
                    x.Year.ToString().Contains(term) ||
                    x.Month.ToString().Contains(term) ||
                    x.Source!.Contains(term));
            }

            TotalRecords = await query.CountAsync();

            TotalPages = (int)Math.Ceiling(
                TotalRecords / (double)PageSize);

            if (TotalPages > 0 && PageNumber > TotalPages)
                PageNumber = TotalPages;

            var summary = await query
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Pages = g.Sum(x => x.TotalPages),
                    Duplex = g.Sum(x => x.DuplexPages),
                    Colour = g.Sum(x => x.ColourPages)
                })
                .FirstOrDefaultAsync();

            TotalPagesPrinted = summary?.Pages ?? 0;
            TotalDuplexPages = summary?.Duplex ?? 0;
            TotalColourPages = summary?.Colour ?? 0;

            Records = await query
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ThenBy(x => x.Department.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
