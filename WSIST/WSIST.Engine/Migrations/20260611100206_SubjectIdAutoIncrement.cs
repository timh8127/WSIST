using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WSIST.Engine.Migrations
{
    /// <inheritdoc />
    public partial class SubjectIdAutoIncrement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Move the seeded system subjects from ids 0..5 to -6..-1 *in place*
            // (renumbering instead of delete+insert keeps every Tests.Subject
            // reference intact), then turn the column into auto-increment so
            // user-created subjects get database-generated ids. Renumbering must
            // happen first: MySQL won't accept an auto-increment column that
            // still holds a 0. FK checks stay off through the ALTER because the
            // column is referenced by FK_Tests_Subjects_Subject.
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 0;");
            migrationBuilder.Sql("UPDATE `Subjects` SET `Id` = `Id` - 6 WHERE `IsSystem` = 1 AND `Id` BETWEEN 0 AND 5;");
            migrationBuilder.Sql("UPDATE `Tests` SET `Subject` = `Subject` - 6 WHERE `Subject` BETWEEN 0 AND 5;");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Subjects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 0;");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Subjects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.Sql("UPDATE `Subjects` SET `Id` = `Id` + 6 WHERE `IsSystem` = 1 AND `Id` BETWEEN -6 AND -1;");
            migrationBuilder.Sql("UPDATE `Tests` SET `Subject` = `Subject` + 6 WHERE `Subject` BETWEEN -6 AND -1;");
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 1;");
        }
    }
}
