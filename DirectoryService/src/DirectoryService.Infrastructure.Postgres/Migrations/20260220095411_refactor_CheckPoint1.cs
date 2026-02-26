using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class refactor_CheckPoint1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_departments_department_id",
                table: "department_positions");

            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_positions_position_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_department_id_position_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_department_id_location_id",
                table: "department_locations");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_department_id",
                table: "department_positions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "location_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_departments_department_id",
                table: "department_positions",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_positions_position_id",
                table: "department_positions",
                column: "position_id",
                principalTable: "positions",
                principalColumn: "position_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_departments_department_id",
                table: "department_positions");

            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_positions_position_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_department_id",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_department_id_position_id",
                table: "department_positions",
                columns: new[] { "department_id", "position_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_department_id_location_id",
                table: "department_locations",
                columns: new[] { "department_id", "location_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_departments_department_id",
                table: "department_locations",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "location_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_departments_department_id",
                table: "department_positions",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_positions_position_id",
                table: "department_positions",
                column: "position_id",
                principalTable: "positions",
                principalColumn: "position_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
