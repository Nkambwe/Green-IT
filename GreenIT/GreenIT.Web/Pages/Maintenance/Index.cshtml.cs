
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Maintenance {
    public class IndexModel : PageModel {
        private readonly GreenITDbContext _db;

        public IndexModel(GreenITDbContext db) {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalRecords { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(TotalRecords / (double)PageSize);

        public decimal TotalMaintenanceCost { get; set; }

        public int UpcomingMaintenanceCount { get; set; }

        public List<MaintenanceViewModel> MaintenanceRecords { get; set; } = new();

        public class MaintenanceViewModel {
            public int Id { get; set; }
            public string AssetTag { get; set; } = string.Empty;
            public string AssetName { get; set; } = string.Empty;
            public string MaintenanceType { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime MaintenanceDate { get; set; }
            public DateTime? NextMaintenanceDate { get; set; }
            public decimal? MaintenanceCost { get; set; }
            public string? ServiceProvider { get; set; }
        }

        public async Task OnGetAsync() {
            var query = _db.AssetMaintenanceRecords
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm)) {
                var term = SearchTerm.Trim();

                query = query.Where(x =>
                    x.ITAsset.AssetTag.Contains(term) ||
                    x.ITAsset.Name.Contains(term) ||
                    x.MaintenanceType.Contains(term) ||
                    x.Description.Contains(term) ||
                    (x.ServiceProvider != null &&
                     x.ServiceProvider.Contains(term)));
            }

            TotalRecords = await query.CountAsync();

            TotalMaintenanceCost = await query
                .Where(x => x.MaintenanceCost != null)
                .SumAsync(x => x.MaintenanceCost ?? 0m);

            UpcomingMaintenanceCount = await _db.AssetMaintenanceRecords
                .CountAsync(x =>
                    x.NextMaintenanceDate != null &&
                    x.NextMaintenanceDate >= DateTime.Today);

            if (PageNumber < 1)
                PageNumber = 1;

            if (TotalPages > 0 && PageNumber > TotalPages)
                PageNumber = TotalPages;

            MaintenanceRecords = await query
                .OrderByDescending(x => x.MaintenanceDate)
                .ThenByDescending(x => x.Id)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new MaintenanceViewModel
                {
                    Id = x.Id,
                    AssetTag = x.ITAsset.AssetTag,
                    AssetName = x.ITAsset.Name,
                    MaintenanceType = x.MaintenanceType,
                    Description = x.Description,
                    MaintenanceDate = x.MaintenanceDate,
                    NextMaintenanceDate = x.NextMaintenanceDate,
                    MaintenanceCost = x.MaintenanceCost,
                    ServiceProvider = x.ServiceProvider
                })
                .ToListAsync();
        }
    }
}
