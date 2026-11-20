using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectAssetAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddHeartbeatSecurityFkAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VisualSecuritys",
                table: "VisualSecuritys");

            migrationBuilder.RenameTable(
                name: "VisualSecuritys",
                newName: "VisualSecurities");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "VisualLabs",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OperatingSystem",
                table: "VisualLabs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                table: "VisualLabs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Hostname",
                table: "VisualLabs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "VisualLabs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenUtc",
                table: "VisualLabs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "VisualSecurities",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "VisualSecurities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VisualSecurities",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "VisualSecurities",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VisualSecurities",
                table: "VisualSecurities",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_VisualLabs_Hostname",
                table: "VisualLabs",
                column: "Hostname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisualSecurities_AssetId",
                table: "VisualSecurities",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualSecurities_CreatedAtUtc",
                table: "VisualSecurities",
                column: "CreatedAtUtc");

            migrationBuilder.AddForeignKey(
                name: "FK_VisualSecurities_VisualLabs_AssetId",
                table: "VisualSecurities",
                column: "AssetId",
                principalTable: "VisualLabs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualSecurities_VisualLabs_AssetId",
                table: "VisualSecurities");

            migrationBuilder.DropIndex(
                name: "IX_VisualLabs_Hostname",
                table: "VisualLabs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VisualSecurities",
                table: "VisualSecurities");

            migrationBuilder.DropIndex(
                name: "IX_VisualSecurities_AssetId",
                table: "VisualSecurities");

            migrationBuilder.DropIndex(
                name: "IX_VisualSecurities_CreatedAtUtc",
                table: "VisualSecurities");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "VisualLabs");

            migrationBuilder.DropColumn(
                name: "LastSeenUtc",
                table: "VisualLabs");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "VisualSecurities");

            migrationBuilder.RenameTable(
                name: "VisualSecurities",
                newName: "VisualSecuritys");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "VisualLabs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "OperatingSystem",
                table: "VisualLabs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                table: "VisualLabs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Hostname",
                table: "VisualLabs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "VisualSecuritys",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "VisualSecuritys",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VisualSecuritys",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VisualSecuritys",
                table: "VisualSecuritys",
                column: "Id");
        }
    }
}
