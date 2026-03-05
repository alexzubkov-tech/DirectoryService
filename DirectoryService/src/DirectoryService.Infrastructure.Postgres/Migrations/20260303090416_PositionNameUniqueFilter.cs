using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PositionNameUniqueFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_positions_name",
                table: "positions");

            migrationBuilder.CreateIndex(
                name: "ix_positions_name_active",
                table: "positions",
                column: "position_name",
                unique: true,
                filter: "\"is_active\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_positions_name_active",
                table: "positions");

            migrationBuilder.CreateIndex(
                name: "ix_positions_name",
                table: "positions",
                column: "position_name",
                unique: true);
        }
    }
}
