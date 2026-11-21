
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Assignments {
    public class IndexModel : PageModel {
        private readonly GreenITDbContext _db;

        public IndexModel(GreenITDbContext db) {
            _db = db;
        }

        public List<AssignmentViewModel> Assignments { get; set; } = new();

        public int ActiveCount { get; set; }
        public int ReturnedCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalRecords { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(TotalRecords / (double)PageSize);
        public async Task OnGetAsync() {
            var query = _db.AssetAssignments
                .AsNoTracking();

            ActiveCount = await query.CountAsync(x => x.ReturnedDate == null);
            ReturnedCount = await query.CountAsync(x => x.ReturnedDate != null);

            TotalRecords = await query.CountAsync();

            if (PageNumber < 1)
                PageNumber = 1;

            if (TotalPages > 0 && PageNumber > TotalPages)
                PageNumber = TotalPages;

            Assignments = await query
                .OrderByDescending(x => x.AssignedDate)
                .ThenByDescending(x => x.Id)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new AssignmentViewModel
                {
                    Id = x.Id,
                    AssetTag = x.ITAsset.AssetTag,
                    AssetName = x.ITAsset.Name,
                    Department = x.Department.Name,
                    AssignedToUserId = x.AssignedToUserId,
                    AssignedDate = x.AssignedDate,
                    ReturnedDate = x.ReturnedDate
                })
                .ToListAsync();
        }

        public class AssignmentViewModel {
            public int Id { get; set; }
            public string AssetTag { get; set; } = string.Empty;
            public string AssetName { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public string? AssignedToUserId { get; set; }
            public DateTime AssignedDate { get; set; }
            public DateTime? ReturnedDate { get; set; }
        }
    }
}
