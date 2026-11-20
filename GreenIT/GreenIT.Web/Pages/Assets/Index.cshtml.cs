
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Assets {
    public class IndexModel : PageModel {
        private readonly GreenITDbContext _db;

        public IndexModel(GreenITDbContext db) {
            _db = db;
        }

        public IList<ITAsset> Assets { get; set; } = new List<ITAsset>();

        public List<SelectListItem> DepartmentOptions { get; set; } = new();
        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DepartmentFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; private set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }

        public int StartRecord =>
            TotalRecords == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

        public int EndRecord =>
            Math.Min(PageNumber * PageSize, TotalRecords);

        public async Task OnGetAsync() {
            if (PageNumber < 1)
                PageNumber = 1;

            await LoadFilterOptionsAsync();

            var query = _db.ITAssets
                .AsNoTracking()
                .Include(x => x.Department)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(SearchTerm)) {
                var search = SearchTerm.Trim();

                query = query.Where(x =>
                    x.AssetTag.Contains(search) ||
                    x.Name.Contains(search) ||
                    (x.SerialNumber != null &&
                     x.SerialNumber.Contains(search)) ||
                    (x.Manufacturer != null &&
                     x.Manufacturer.Contains(search)) ||
                    (x.Model != null &&
                     x.Model.Contains(search)));
            }

            // Department filter
            if (DepartmentFilter.HasValue) {
                query = query.Where(x =>
                    x.DepartmentId == DepartmentFilter.Value);
            }

            // Category filter
            if (CategoryFilter.HasValue) {
                query = query.Where(x =>
                    (int)x.Category == CategoryFilter.Value);
            }

            // Status filter
            if (StatusFilter.HasValue) {
                query = query.Where(x =>
                    (int)x.Status == StatusFilter.Value);
            }

            TotalRecords = await query.CountAsync();

            TotalPages = (int)Math.Ceiling(
                TotalRecords / (double)PageSize);

            if (TotalPages > 0 && PageNumber > TotalPages)
                PageNumber = TotalPages;

            Assets = await query
                .OrderBy(x => x.AssetTag)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        private async Task LoadFilterOptionsAsync() {
            DepartmentOptions = await _db.Departments
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
                .ToListAsync();

            CategoryOptions = Enum.GetValues<AssetCategory>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString(),
                    Value = ((int)x).ToString()
                })
                .ToList();

            StatusOptions = Enum.GetValues<AssetStatus>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString(),
                    Value = ((int)x).ToString()
                })
                .ToList();
        }
    }
}
