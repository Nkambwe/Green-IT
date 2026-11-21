
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Energy {
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
        public int DraftCount { get; set; }
        public int CompletedCount { get; set; }
        public int ReviewedCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(TotalRecords / (double)PageSize);

        public List<AuditViewModel> Audits { get; set; } = new();

        public class AuditViewModel {
            public int Id { get; set; }
            public string AuditReference { get; set; } = string.Empty;
            public string DepartmentName { get; set; } = string.Empty;
            public DateTime AuditDate { get; set; }
            public EnergyAuditStatus Status { get; set; }
            public string? AuditorUserId { get; set; }
            public int WorkstationCount { get; set; }
        }

        public async Task OnGetAsync() {
            var query = _db.EnergyAudits
                .AsNoTracking()
                .Include(x => x.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm)) {
                var term = SearchTerm.Trim();

                query = query.Where(x =>
                    x.AuditReference.Contains(term) ||
                    x.Department.Name.Contains(term) ||
                    (x.AuditorUserId != null &&
                     x.AuditorUserId.Contains(term)));
            }

            TotalRecords = await query.CountAsync();

            DraftCount = await _db.EnergyAudits.CountAsync(
                x => x.Status == EnergyAuditStatus.Draft);

            CompletedCount = await _db.EnergyAudits.CountAsync(
                x => x.Status == EnergyAuditStatus.Completed);

            ReviewedCount = await _db.EnergyAudits.CountAsync(
                x => x.Status == EnergyAuditStatus.Reviewed);

            if (PageNumber < 1)
                PageNumber = 1;

            if (TotalPages > 0 && PageNumber > TotalPages)
                PageNumber = TotalPages;

            Audits = await query
                .OrderByDescending(x => x.AuditDate)
                .ThenByDescending(x => x.Id)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new AuditViewModel
                {
                    Id = x.Id,
                    AuditReference = x.AuditReference,
                    DepartmentName = x.Department.Name,
                    AuditDate = x.AuditDate,
                    Status = x.Status,
                    AuditorUserId = x.AuditorUserId,
                    WorkstationCount = x.Items.Count()
                })
                .ToListAsync();
        }
    }
}
