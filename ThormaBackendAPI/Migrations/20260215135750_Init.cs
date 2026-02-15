using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace ThormaBackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "festok",
                columns: table => new
                {
                    Azon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Nev = table.Column<string>(type: "longtext", nullable: false),
                    Szuletett = table.Column<int>(type: "int", nullable: false),
                    Meghalt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_festok", x => x.Azon);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "kepek",
                columns: table => new
                {
                    Leltar = table.Column<string>(type: "varchar(255)", nullable: false),
                    Fazon = table.Column<int>(type: "int", nullable: false),
                    Cim = table.Column<string>(type: "longtext", nullable: false),
                    Keszult = table.Column<int>(type: "int", nullable: false),
                    Anyag = table.Column<string>(type: "longtext", nullable: false),
                    Technika = table.Column<string>(type: "longtext", nullable: false),
                    Szeles = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    Magas = table.Column<decimal>(type: "decimal(5,1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kepek", x => x.Leltar);
                    table.ForeignKey(
                        name: "FK_kepek_festok_Fazon",
                        column: x => x.Fazon,
                        principalTable: "festok",
                        principalColumn: "Azon",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_kepek_Fazon",
                table: "kepek",
                column: "Fazon");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kepek");

            migrationBuilder.DropTable(
                name: "festok");
        }
    }
}
