using GreenIT.Domain.Entities;
using GreenIT.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GreenIT.Infrastructure.Data {
    public class GreenITDbContext : IdentityDbContext<ApplicationUser> {
        public GreenITDbContext(
            DbContextOptions<GreenITDbContext> options)
            : base(options) {
        }

        // Organisation
        public DbSet<Department> Departments { get; set; }

        // IT Asset Management
        public DbSet<ITAsset> ITAssets { get; set; }
        public DbSet<AssetAssignment> AssetAssignments { get; set; }
        public DbSet<AssetMaintenance> AssetMaintenanceRecords { get; set; }
        public DbSet<AssetLifecycleEvent> AssetLifecycleEvents { get; set; }

        // Energy Monitoring
        public DbSet<EnergyAudit> EnergyAudits { get; set; }
        public DbSet<EnergyAuditItem> EnergyAuditItems { get; set; }

        // Paper and Printing
        public DbSet<PrintingRecord> PrintingRecords { get; set; }

        // Sustainability
        public DbSet<SustainabilityAction> SustainabilityActions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            ConfigureDepartment(modelBuilder);
            ConfigureAssets(modelBuilder);
            ConfigureEnergyAudits(modelBuilder);
            ConfigurePrinting(modelBuilder);
            ConfigureSustainability(modelBuilder);
        }

        private static void ConfigureDepartment(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Departments");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Code)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(x => x.Code)
                    .IsUnique();
            });

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.FullName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private static void ConfigureAssets(ModelBuilder modelBuilder) {
            modelBuilder.Entity<ITAsset>(entity =>
            {
                entity.ToTable("ITAssets");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.AssetTag)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.SerialNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Manufacturer)
                    .HasMaxLength(100);

                entity.Property(x => x.Model)
                    .HasMaxLength(100);

                entity.Property(x => x.Location)
                    .HasMaxLength(150);

                entity.Property(x => x.PurchaseCost)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Notes)
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(x => x.AssetTag)
                    .IsUnique();

                entity.HasIndex(x => x.SerialNumber);

                entity.HasOne(x => x.Department)
                    .WithMany(x => x.ITAssets)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AssetAssignment>(entity =>
            {
                entity.ToTable("AssetAssignments");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.AssignedToUserId)
                    .HasMaxLength(450);

                entity.Property(x => x.Notes)
                    .HasMaxLength(500);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.ITAsset)
                    .WithMany(x => x.Assignments)
                    .HasForeignKey(x => x.ITAssetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Department)
                    .WithMany(x => x.AssetAssignments)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.ITAssetId,
                    x.ReturnedDate
                });
            });

            modelBuilder.Entity<AssetMaintenance>(entity =>
            {
                entity.ToTable("AssetMaintenance");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.MaintenanceType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.MaintenanceCost)
                    .HasPrecision(18, 2);

                entity.Property(x => x.ServiceProvider)
                    .HasMaxLength(150);

                entity.Property(x => x.PerformedBy)
                    .HasMaxLength(150);

                entity.Property(x => x.Notes)
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.ITAsset)
                    .WithMany(x => x.MaintenanceRecords)
                    .HasForeignKey(x => x.ITAssetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.MaintenanceDate);
            });

            modelBuilder.Entity<AssetLifecycleEvent>(entity =>
            {
                entity.ToTable("AssetLifecycleEvents");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ReferenceNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.RecordedByUserId)
                    .HasMaxLength(450);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.ITAsset)
                    .WithMany(x => x.LifecycleEvents)
                    .HasForeignKey(x => x.ITAssetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Department>()
                    .WithMany()
                    .HasForeignKey(x => x.FromDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Department>()
                    .WithMany()
                    .HasForeignKey(x => x.ToDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.EventDate);
            });
        }

        private static void ConfigureEnergyAudits(ModelBuilder modelBuilder) {
            modelBuilder.Entity<EnergyAudit>(entity =>
            {
                entity.ToTable("EnergyAudits");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.AuditReference)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.AuditorUserId)
                    .HasMaxLength(450);

                entity.Property(x => x.Notes)
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(x => x.AuditReference)
                    .IsUnique();

                entity.HasOne(x => x.Department)
                    .WithMany(x => x.EnergyAudits)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.AuditDate);
            });

            modelBuilder.Entity<EnergyAuditItem>(entity =>
            {
                entity.ToTable("EnergyAuditItems");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Observation)
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.EnergyAudit)
                    .WithMany(x => x.Items)
                    .HasForeignKey(x => x.EnergyAuditId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.ITAsset)
                    .WithMany(x => x.EnergyAuditItems)
                    .HasForeignKey(x => x.ITAssetId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private static void ConfigurePrinting(ModelBuilder modelBuilder) {
            modelBuilder.Entity<PrintingRecord>(entity =>
            {
                entity.ToTable("PrintingRecords");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Source)
                    .HasMaxLength(150);

                entity.Property(x => x.Notes)
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.Department)
                    .WithMany(x => x.PrintingRecords)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.DepartmentId,
                    x.Year,
                    x.Month
                }).IsUnique();
            });
        }

        private static void ConfigureSustainability(ModelBuilder modelBuilder) {
            modelBuilder.Entity<SustainabilityAction>(entity =>
            {
                entity.ToTable("SustainabilityActions");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1500)
                    .IsRequired();

                entity.Property(x => x.AssignedToUserId)
                    .HasMaxLength(450);

                entity.Property(x => x.Notes)
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAtUtc)
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne(x => x.Department)
                    .WithMany(x => x.SustainabilityActions)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => x.TargetDate);
            });
        }
    }
}