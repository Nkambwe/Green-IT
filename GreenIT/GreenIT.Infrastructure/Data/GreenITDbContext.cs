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

        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

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
    }
}
