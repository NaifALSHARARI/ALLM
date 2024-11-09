using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HakayaatBilArabiya.Migrations
{
    /// <inheritdoc />
    public partial class RenameEducationalGameToInteractiveActivit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InteractiveActivities");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Stories");

            migrationBuilder.CreateTable(
                name: "InteractiveActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoryId = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveActivities_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveActivities_StoryId",
                table: "InteractiveActivities",
                column: "StoryId");
        }
    }
}
