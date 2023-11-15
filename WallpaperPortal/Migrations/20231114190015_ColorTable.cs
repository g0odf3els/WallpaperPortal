using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WallpaperPortal.Migrations
{
    /// <inheritdoc />
    public partial class ColorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    A = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    R = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    G = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    B = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => new { x.A, x.R, x.G, x.B });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ColorFile",
                columns: table => new
                {
                    FilesId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ColorsA = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ColorsR = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ColorsG = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ColorsB = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorFile", x => new { x.FilesId, x.ColorsA, x.ColorsR, x.ColorsG, x.ColorsB });
                    table.ForeignKey(
                        name: "FK_ColorFile_Colors_ColorsA_ColorsR_ColorsG_ColorsB",
                        columns: x => new { x.ColorsA, x.ColorsR, x.ColorsG, x.ColorsB },
                        principalTable: "Colors",
                        principalColumns: new[] { "A", "R", "G", "B" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColorFile_Files_FilesId",
                        column: x => x.FilesId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ColorFile_ColorsA_ColorsR_ColorsG_ColorsB",
                table: "ColorFile",
                columns: new[] { "ColorsA", "ColorsR", "ColorsG", "ColorsB" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColorFile");

            migrationBuilder.DropTable(
                name: "Colors");
        }
    }
}
