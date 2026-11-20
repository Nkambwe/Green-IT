
using GreenIT.Domain.Enums;
using GreenIT.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Web.Pages {
    public class IndexModel : PageModel {
        private readonly GreenITDbContext _db;

        public IndexModel(GreenITDbContext db) {
            _db = db;
        }

        // Summary cards
        public int TotalAssets { get; set; }
        public int TotalEnergyAudits { get; set; }
        public int TotalPrintingRecords { get; set; }
        public int TotalSustainabilityActions { get; set; }
        public int TotalDepartments { get; set; }

        // Asset indicators
        public int AssetsInUse { get; set; }
        public int AssetsUnderMaintenance { get; set; }

        public List<StatusSummaryItem> AssetStatusSummary { get; set; }
            = new();

        // Printing indicators
        public int TotalPagesPrinted { get; set; }
        public int TotalDuplexPages { get; set; }
        public decimal DuplexPercentage { get; set; }

        public List<PrintingMonthSummary> PrintingTrend { get; set; }
            = new();

        // Energy indicators
        public int TotalEnergyAuditItems { get; set; }
        public int SleepModeEnabled { get; set; }
        public int AutomaticShutdownEnabled { get; set; }
        public int MonitorPowerSavingEnabled { get; set; }

        public decimal SleepModePercentage { get; set; }
        public decimal AutomaticShutdownPercentage { get; set; }
        public decimal MonitorPowerSavingPercentage { get; set; }

        // Sustainability indicators
        public int CompletedActions { get; set; }
        public int OpenActions { get; set; }
        public decimal ActionCompletionPercentage { get; set; }

        public async Task OnGetAsync() {
            // -----------------------------------------
            // 1. SUMMARY CARDS
            // -----------------------------------------

            TotalAssets = await _db.ITAssets
                .AsNoTracking()
                .CountAsync();

            TotalEnergyAudits = await _db.EnergyAudits
                .AsNoTracking()
                .CountAsync();

            TotalSustainabilityActions = await _db.SustainabilityActions
                .AsNoTracking()
                .CountAsync();

            TotalDepartments = await _db.Departments
                .AsNoTracking()
                .CountAsync(x => x.IsActive);

            // -----------------------------------------
            // 2. ASSET STATUS
            // -----------------------------------------

            var assetStatusCounts = await _db.ITAssets
                .AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            AssetsInUse = assetStatusCounts
                .Where(x => x.Status == AssetStatus.InUse)
                .Select(x => x.Count)
                .FirstOrDefault();

            AssetsUnderMaintenance = assetStatusCounts
                .Where(x => x.Status == AssetStatus.UnderMaintenance)
                .Select(x => x.Count)
                .FirstOrDefault();

            AssetStatusSummary = Enum.GetValues<AssetStatus>()
                .Select(status =>
                {
                    var count = assetStatusCounts
                        .Where(x => x.Status == status)
                        .Select(x => x.Count)
                        .FirstOrDefault();

                    return new StatusSummaryItem
                    {
                        Name = status.ToString(),
                        Count = count,
                        Percentage = TotalAssets == 0
                            ? 0
                            : Math.Round(count * 100m / TotalAssets, 1)
                    };
                })
                .ToList();

            // -----------------------------------------
            // 3. PRINTING INDICATORS
            // Last 12 months
            // -----------------------------------------

            var currentMonth = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                1);

            var startMonth = currentMonth.AddMonths(-11);

            var printingQuery = _db.PrintingRecords
                .AsNoTracking()
                .Where(x =>
                    (x.Year > startMonth.Year ||
                     (x.Year == startMonth.Year &&
                      x.Month >= startMonth.Month)) &&
                    (x.Year < currentMonth.Year ||
                     (x.Year == currentMonth.Year &&
                      x.Month <= currentMonth.Month)));

            TotalPrintingRecords = await printingQuery.CountAsync();

            var printingTotals = await printingQuery
                .GroupBy(x => 1)
                .Select(g => new
                {
                    TotalPages = g.Sum(x => x.TotalPages),
                    DuplexPages = g.Sum(x => x.DuplexPages)
                })
                .FirstOrDefaultAsync();

            TotalPagesPrinted = printingTotals?.TotalPages ?? 0;
            TotalDuplexPages = printingTotals?.DuplexPages ?? 0;

            DuplexPercentage = TotalPagesPrinted == 0
                ? 0
                : Math.Round(
                    TotalDuplexPages * 100m / TotalPagesPrinted, 1);

            var monthlyPrinting = await printingQuery
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalPages = g.Sum(x => x.TotalPages)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            PrintingTrend = monthlyPrinting
                .Select(x => new PrintingMonthSummary
                {
                    Month = new DateTime(x.Year, x.Month, 1)
                        .ToString("MMM yyyy"),
                    TotalPages = x.TotalPages
                })
                .ToList();

            // -----------------------------------------
            // 4. ENERGY AUDIT INDICATORS
            // -----------------------------------------

            var energyItems = await _db.EnergyAuditItems
                .AsNoTracking()
                .Select(x => new
                {
                    x.IsSleepModeEnabled,
                    x.IsAutomaticShutdownEnabled,
                    x.IsMonitorPowerSavingEnabled
                })
                .ToListAsync();

            TotalEnergyAuditItems = energyItems.Count;

            SleepModeEnabled = energyItems
                .Count(x => x.IsSleepModeEnabled);

            AutomaticShutdownEnabled = energyItems
                .Count(x => x.IsAutomaticShutdownEnabled);

            MonitorPowerSavingEnabled = energyItems
                .Count(x => x.IsMonitorPowerSavingEnabled);

            if (TotalEnergyAuditItems > 0) {
                SleepModePercentage = Math.Round(
                    SleepModeEnabled * 100m / TotalEnergyAuditItems, 1);

                AutomaticShutdownPercentage = Math.Round(
                    AutomaticShutdownEnabled * 100m /
                    TotalEnergyAuditItems, 1);

                MonitorPowerSavingPercentage = Math.Round(
                    MonitorPowerSavingEnabled * 100m /
                    TotalEnergyAuditItems, 1);
            }

            // -----------------------------------------
            // 5. SUSTAINABILITY ACTIONS
            // -----------------------------------------

            CompletedActions = await _db.SustainabilityActions
                .AsNoTracking()
                .CountAsync(x =>
                    x.Status == SustainabilityActionStatus.Completed);

            OpenActions = await _db.SustainabilityActions
                .AsNoTracking()
                .CountAsync(x =>
                    x.Status != SustainabilityActionStatus.Completed &&
                    x.Status != SustainabilityActionStatus.Cancelled);

            ActionCompletionPercentage =
                TotalSustainabilityActions == 0
                    ? 0
                    : Math.Round(
                        CompletedActions * 100m /
                        TotalSustainabilityActions, 1);
        }

        public class StatusSummaryItem {
            public string Name { get; set; } = string.Empty;
            public int Count { get; set; }
            public decimal Percentage { get; set; }
        }

        public class PrintingMonthSummary {
            public string Month { get; set; } = string.Empty;
            public int TotalPages { get; set; }
        }
    }
}
