
using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages.Sustainability {
    public class IndexModel : PageModel {
        private readonly GreenITDbContext _db;

        public IndexModel(GreenITDbContext db) {
            _db = db;
        }

        public IList<SustainabilityAction> Actions { get; set; }
            = new List<SustainabilityAction>();

        public int TotalActions { get; set; }
        public int CompletedActions { get; set; }
        public int InProgressActions { get; set; }
        public int OverdueActions { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DepartmentFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public SustainabilityPriority? PriorityFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public SustainabilityActionStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public List<SelectListItem> Departments { get; set; } = new();

        public async Task OnGetAsync() {
            if (PageNumber < 1)
                PageNumber = 1;

            await LoadDepartmentsAsync();

            var baseQuery = _db.SustainabilityActions
                .AsNoTracking()
                .AsQueryable();

            TotalActions = await baseQuery.CountAsync();

            CompletedActions = await baseQuery.CountAsync(x =>
                x.Status == SustainabilityActionStatus.Completed);

            InProgressActions = await baseQuery.CountAsync(x =>
                x.Status == SustainabilityActionStatus.InProgress);

            OverdueActions = await baseQuery.CountAsync(x =>
                x.TargetDate.HasValue &&
                x.TargetDate.Value.Date < DateTime.Today &&
                x.Status != SustainabilityActionStatus.Completed &&
                x.Status != SustainabilityActionStatus.Cancelled);

            var query = baseQuery
                .Include(x => x.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm)) {
                var term = SearchTerm.Trim();

                query = query.Where(x =>
                    x.Title.Contains(term) ||
                    x.Description.Contains(term));
            }

            if (DepartmentFilter.HasValue) {
                query = query.Where(x =>
                    x.DepartmentId == DepartmentFilter.Value);
            }

            if (PriorityFilter.HasValue) {
                query = query.Where(x =>
                    x.Priority == PriorityFilter.Value);
            }

            if (StatusFilter.HasValue) {
                query = query.Where(x =>
                    x.Status == StatusFilter.Value);
            }

            var filteredCount = await query.CountAsync();

            TotalPages = (int)Math.Ceiling(
                filteredCount / (double)PageSize);

            if (TotalPages > 0 && PageNumber > TotalPages)
                PageNumber = TotalPages;

            Actions = await query
                .OrderBy(x => x.Status == SustainabilityActionStatus.Completed)
                .ThenBy(x => x.TargetDate)
                .ThenBy(x => x.Title)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        private async Task LoadDepartmentsAsync() {
            Departments = await _db.Departments
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();
        }
    }
}
