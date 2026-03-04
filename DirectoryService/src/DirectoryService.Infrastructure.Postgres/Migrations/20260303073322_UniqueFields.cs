using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_positions_position_name",
                table: "positions",
                newName: "ix_positions_name");

            migrationBuilder.CreateIndex(
                name: "ix_department_identifier",
                table: "departments",
                column: "department_identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_department_identifier",
                table: "departments");

            migrationBuilder.RenameIndex(
                name: "ix_positions_name",
                table: "positions",
                newName: "IX_positions_position_name");
        }
    }
}
