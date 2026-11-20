using GreenIT.Domain.Entities;
using GreenIT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenIT.Infrastructure.Data.Seeding {
    public static class GreenITSeeder {
        public static async Task SeedAsync(IServiceProvider services) {
            var db = services.GetRequiredService<GreenITDbContext>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("GreenITSeeder");

            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () => {
                // Prevent duplicate demonstration data.
                if (await db.Departments.AnyAsync(x => x.Code.StartsWith("PBU-"))) {
                    logger.LogInformation("GreenIT data already exists. Seeding skipped.");

                    return;
                }

                await using var transaction = await db.Database.BeginTransactionAsync();

                var today = DateTime.UtcNow.Date;

                // -------------------------------------------------
                // 1. DEPARTMENTS
                // -------------------------------------------------

                var departments = new List<Department>
            {
                new Department
                {
                    Code = "PBU-FIN",
                    Name = "Finance",
                    Description = "Finance department",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Department
                {
                    Code = "PBU-HR",
                    Name = "Human Resources",
                    Description = "Human Resource department",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Department
                {
                    Code = "PBU-OPS",
                    Name = "Operations",
                    Description = "Operations department",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Department
                {
                    Code = "PBU-IT",
                    Name = "Information Technology",
                    Description = "IT department",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Department
                {
                    Code = "PBU-ADM",
                    Name = "Administration",
                    Description = "Administration department",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            };

                db.Departments.AddRange(departments);
                // Save departments to generate their database IDs.
                await db.SaveChangesAsync();

                // -------------------------------------------------
                // 2. IT ASSETS
                // -------------------------------------------------

                var categories = Enum.GetValues<AssetCategory>();
                var conditions = Enum.GetValues<AssetCondition>();

                var assets = new List<ITAsset>();

                for (int i = 1; i <= 60; i++) {
                    var department = departments[(i - 1) % departments.Count];

                    var category = categories[(i - 1) % categories.Length];

                    var condition = conditions[(i - 1) % conditions.Length];

                    var status = i % 12 == 0
                        ? AssetStatus.UnderMaintenance
                        : i % 15 == 0
                            ? AssetStatus.InStorage
                            : i % 20 == 0
                                ? AssetStatus.Retired
                                : AssetStatus.InUse;

                    var asset = new ITAsset
                    {
                        AssetTag = $"IT-{i:0000}",
                        SerialNumber = $"SN-{i:0000}",
                        Name = $"{category} Equipment {i:000}",
                        Category = category,
                        Manufacturer = i % 2 == 0 ? "Dell" : "HP",
                        Model = i % 2 == 0 ? "Latitude Series" : "ProDesk Series",
                        Department = department,
                        Location = department.Name,
                        PurchaseDate = today.AddMonths(-((i % 48) + 1)),
                        WarrantyExpiryDate = today.AddMonths((i % 24) - 12),
                        PurchaseCost = 800000 + (i * 25000),
                        Condition = condition,
                        Status = status,
                        Notes = "Organisational asset.",
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    assets.Add(asset);
                }

                db.ITAssets.AddRange(assets);

                try {
                    await db.SaveChangesAsync();
                } catch (DbUpdateException ex) {
                    logger.LogError(ex, "Failed while saving IT assets.");

                    Console.WriteLine("OUTER ERROR:");
                    Console.WriteLine(ex.Message);

                    Console.WriteLine("INNER ERROR:");
                    Console.WriteLine(ex.InnerException?.Message);

                    throw;
                }

                // -------------------------------------------------
                // 3. ASSET ASSIGNMENTS
                // -------------------------------------------------

                var assignments = new List<AssetAssignment>();

                foreach (var asset in assets) {
                    if (asset.Status != AssetStatus.InUse)
                        continue;

                    assignments.Add(new AssetAssignment
                    {
                        ITAsset = asset,
                        Department = asset.Department!,
                        AssignedDate = asset.PurchaseDate ?? today,
                        ReturnedDate = null,
                        Notes = "DEMONSTRATION DATA - Current asset assignment.",
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }

                db.AssetAssignments.AddRange(assignments);

                // -------------------------------------------------
                // 4. ASSET MAINTENANCE RECORDS
                // -------------------------------------------------

                var maintenanceRecords = new List<AssetMaintenance>();

                foreach (var asset in assets.Where((x, index) => index % 5 == 0)) {
                    maintenanceRecords.Add(new AssetMaintenance
                    {
                        ITAsset = asset,
                        MaintenanceType = "Preventive Maintenance",
                        Description = "Equipment inspection and servicing.",
                        MaintenanceDate = today.AddMonths(-2),
                        NextMaintenanceDate = today.AddMonths(4),
                        MaintenanceCost = 50000,
                        ServiceProvider = "Service Provider",
                        PerformedBy = "IT Support",
                        Notes = "MAINTENENCE DATA",
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }

                db.AssetMaintenanceRecords.AddRange(maintenanceRecords);

                // -------------------------------------------------
                // 5. ASSET LIFECYCLE EVENTS
                // -------------------------------------------------

                var lifecycleEvents = new List<AssetLifecycleEvent>();

                foreach (var asset in assets) {
                    lifecycleEvents.Add(new AssetLifecycleEvent
                    {
                        ITAsset = asset,
                        EventType = AssetLifecycleEventType.Acquired,
                        EventDate = asset.PurchaseDate ?? today,
                        ToDepartmentId = asset.DepartmentId,
                        Description = "Asset acquisition record.",
                        ReferenceNumber = $"ACQ-{asset.AssetTag}",
                        CreatedAtUtc = DateTime.UtcNow
                    });

                    if (asset.Status == AssetStatus.InUse) {
                        lifecycleEvents.Add(new AssetLifecycleEvent
                        {
                            ITAsset = asset,
                            EventType = AssetLifecycleEventType.Assigned,
                            EventDate = asset.PurchaseDate ?? today,
                            ToDepartmentId = asset.DepartmentId,
                            Description = "Asset assigned.",
                            ReferenceNumber = $"PBU-ASG-{asset.AssetTag}",
                            CreatedAtUtc = DateTime.UtcNow
                        });
                    }
                }

                db.AssetLifecycleEvents.AddRange(lifecycleEvents);

                // -------------------------------------------------
                // 6. ENERGY AUDITS
                // Four quarterly audits for each department.
                // -------------------------------------------------

                var energyAudits = new List<EnergyAudit>();

                for (int departmentIndex = 0;
                     departmentIndex < departments.Count;
                     departmentIndex++) {
                    var department = departments[departmentIndex];

                    for (int quarter = 1; quarter <= 4; quarter++) {
                        var auditDate = today.AddMonths(-(quarter * 3));

                        var audit = new EnergyAudit
                        {
                            AuditReference =
                                $"PBU-EA-{departmentIndex + 1:00}-{quarter:00}",

                            Department = department,
                            AuditDate = auditDate,
                            Status = quarter % 2 == 0
                                ? EnergyAuditStatus.Reviewed
                                : EnergyAuditStatus.Completed,

                            Notes = "Synthetic energy audit."
                        };

                        var departmentAssets = assets
                            .Where(x => x.Department == department &&
                                        x.Category != AssetCategory.Other)
                            .Take(3)
                            .ToList();

                        foreach (var asset in departmentAssets) {
                            var item = new EnergyAuditItem
                            {
                                ITAsset = asset,
                                IsSleepModeEnabled = asset.Id % 2 == 0,
                                IsAutomaticShutdownEnabled = asset.Id % 3 == 0,
                                IsMonitorPowerSavingEnabled = asset.Id % 4 != 0,
                                Observation =
                                    "Workstation power settings observation.",
                                CreatedAtUtc = DateTime.UtcNow
                            };

                            audit.Items.Add(item);
                        }

                        energyAudits.Add(audit);
                    }
                }

                db.EnergyAudits.AddRange(energyAudits);

                // -------------------------------------------------
                // 7. PRINTING RECORDS
                // Twelve months for each department.
                // -------------------------------------------------

                var printingRecords = new List<PrintingRecord>();

                for (int departmentIndex = 0;
                     departmentIndex < departments.Count;
                     departmentIndex++) {
                    var department = departments[departmentIndex];

                    for (int monthOffset = 11; monthOffset >= 0; monthOffset--) {
                        var period = today.AddMonths(-monthOffset);

                        var totalPages =
                            1200 + (departmentIndex * 350) +
                            ((11 - monthOffset) * 75);

                        var duplexPages = (int)(totalPages * 0.35);
                        var colourPages = (int)(totalPages * 0.20);

                        printingRecords.Add(new PrintingRecord
                        {
                            Department = department,
                            Year = period.Year,
                            Month = period.Month,
                            TotalPages = totalPages,
                            DuplexPages = duplexPages,
                            ColourPages = colourPages,
                            PrintJobs = Math.Max(1, totalPages / 8),
                            Source = "DEPT_DATA",
                            Notes = "Synthetic monthly printing activity."
                            ,
                            CreatedAtUtc = DateTime.UtcNow
                        });
                    }
                }

                db.PrintingRecords.AddRange(printingRecords);

                // -------------------------------------------------
                // 8. SUSTAINABILITY ACTIONS
                // -------------------------------------------------

                var actions = new List<SustainabilityAction>();

                for (int i = 1; i <= 12; i++) {
                    var status = (SustainabilityActionStatus)
                        (((i - 1) % 4) + 1);

                    var progress = status switch
                    {
                        SustainabilityActionStatus.NotStarted => 0,
                        SustainabilityActionStatus.InProgress => 50,
                        SustainabilityActionStatus.Completed => 100,
                        SustainabilityActionStatus.OnHold => 25,
                        _ => 0
                    };

                    var action = new SustainabilityAction
                    {
                        Title = $"Sustainability Initiative {i:00}",
                        Description = "An initiative for improving IT resource efficiency.",

                        Department = departments[(i - 1) % departments.Count],

                        Priority = (SustainabilityPriority)
                            (((i - 1) % 4) + 1),

                        Status = status,
                        ProgressPercentage = progress,

                        StartDate = today.AddDays(-(i * 15)),
                        TargetDate = today.AddDays(i * 10),

                        CompletedDate = status ==
                            SustainabilityActionStatus.Completed
                                ? today.AddDays(-5)
                                : null,

                        Notes = "SYSTEM DATA",
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    actions.Add(action);
                }

                db.SustainabilityActions.AddRange(actions);

                // -------------------------------------------------
                // SAVE ALL DEMONSTRATION DATA
                // -------------------------------------------------

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                logger.LogInformation("GreenIT demonstration data seeded successfully.");
            });
        }
    }
}
