using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSIST.Engine.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_UserId",
                table: "Subjects");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserId_Name",
                table: "Subjects",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_UserId_Name",
                table: "Subjects");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserId",
                table: "Subjects",
                column: "UserId");
        }
    }
}
