using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenIT.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGreenITManagementModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnergyAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AuditorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnergyAudits_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ITAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurchaseCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ITAssets_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrintingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TotalPages = table.Column<int>(type: "int", nullable: false),
                    DuplexPages = table.Column<int>(type: "int", nullable: false),
                    ColourPages = table.Column<int>(type: "int", nullable: false),
                    PrintJobs = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintingRecords_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SustainabilityActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SustainabilityActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SustainabilityActions_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AssetAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ITAssetId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAssignments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetAssignments_ITAssets_ITAssetId",
                        column: x => x.ITAssetId,
                        principalTable: "ITAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetLifecycleEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ITAssetId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromDepartmentId = table.Column<int>(type: "int", nullable: true),
                    ToDepartmentId = table.Column<int>(type: "int", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetLifecycleEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetLifecycleEvents_Departments_FromDepartmentId",
                        column: x => x.FromDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetLifecycleEvents_Departments_ToDepartmentId",
                        column: x => x.ToDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetLifecycleEvents_ITAssets_ITAssetId",
                        column: x => x.ITAssetId,
                        principalTable: "ITAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetMaintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ITAssetId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaintenanceCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ServiceProvider = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMaintenance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetMaintenance_ITAssets_ITAssetId",
                        column: x => x.ITAssetId,
                        principalTable: "ITAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnergyAuditItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnergyAuditId = table.Column<int>(type: "int", nullable: false),
                    ITAssetId = table.Column<int>(type: "int", nullable: true),
                    IsSleepModeEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsAutomaticShutdownEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsMonitorPowerSavingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyAuditItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnergyAuditItems_EnergyAudits_EnergyAuditId",
                        column: x => x.EnergyAuditId,
                        principalTable: "EnergyAudits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnergyAuditItems_ITAssets_ITAssetId",
                        column: x => x.ITAssetId,
                        principalTable: "ITAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_DepartmentId",
                table: "AssetAssignments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_ITAssetId_ReturnedDate",
                table: "AssetAssignments",
                columns: new[] { "ITAssetId", "ReturnedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetLifecycleEvents_EventDate",
                table: "AssetLifecycleEvents",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_AssetLifecycleEvents_FromDepartmentId",
                table: "AssetLifecycleEvents",
                column: "FromDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetLifecycleEvents_ITAssetId",
                table: "AssetLifecycleEvents",
                column: "ITAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetLifecycleEvents_ToDepartmentId",
                table: "AssetLifecycleEvents",
                column: "ToDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaintenance_ITAssetId",
                table: "AssetMaintenance",
                column: "ITAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaintenance_MaintenanceDate",
                table: "AssetMaintenance",
                column: "MaintenanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyAuditItems_EnergyAuditId",
                table: "EnergyAuditItems",
                column: "EnergyAuditId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyAuditItems_ITAssetId",
                table: "EnergyAuditItems",
                column: "ITAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyAudits_AuditDate",
                table: "EnergyAudits",
                column: "AuditDate");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyAudits_AuditReference",
                table: "EnergyAudits",
                column: "AuditReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnergyAudits_DepartmentId",
                table: "EnergyAudits",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ITAssets_AssetTag",
                table: "ITAssets",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ITAssets_DepartmentId",
                table: "ITAssets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ITAssets_SerialNumber",
                table: "ITAssets",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PrintingRecords_DepartmentId_Year_Month",
                table: "PrintingRecords",
                columns: new[] { "DepartmentId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityActions_DepartmentId",
                table: "SustainabilityActions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityActions_Status",
                table: "SustainabilityActions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityActions_TargetDate",
                table: "SustainabilityActions",
                column: "TargetDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetAssignments");

            migrationBuilder.DropTable(
                name: "AssetLifecycleEvents");

            migrationBuilder.DropTable(
                name: "AssetMaintenance");

            migrationBuilder.DropTable(
                name: "EnergyAuditItems");

            migrationBuilder.DropTable(
                name: "PrintingRecords");

            migrationBuilder.DropTable(
                name: "SustainabilityActions");

            migrationBuilder.DropTable(
                name: "EnergyAudits");

            migrationBuilder.DropTable(
                name: "ITAssets");
        }
    }
}
