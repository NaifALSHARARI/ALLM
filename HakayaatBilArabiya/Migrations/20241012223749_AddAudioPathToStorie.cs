using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HakayaatBilArabiya.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioPathToStorie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioPath",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioPath",
                table: "Stories");
        }
    }
}
