using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThormaBackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "logs",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "logs",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "logs",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "logs",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "logs",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "logs");
        }
    }
}
