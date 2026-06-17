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
            // Order matters on MySQL: IX_Subjects_UserId backs the
            // FK_Subjects_Users_UserId constraint and cannot be dropped while it
            // is the only index serving that FK. Alter the collation first, then
            // create the composite (UserId, Name) index — whose leading UserId
            // column can serve the FK — and only then drop the old index.
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subjects",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserId_Name",
                table: "Subjects",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.DropIndex(name: "IX_Subjects_UserId", table: "Subjects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse in FK-safe order: recreate the single-column index so it
            // can back the FK, then drop the composite index, then revert the
            // collation.
            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserId",
                table: "Subjects",
                column: "UserId");

            migrationBuilder.DropIndex(name: "IX_Subjects_UserId_Name", table: "Subjects");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subjects",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldCollation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
