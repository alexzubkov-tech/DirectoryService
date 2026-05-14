using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DirectoryServiceDbContext))]
    [Migration("20260513143000_AddUniqueLocationAddressIndex")]
    public partial class AddUniqueLocationAddressIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX ix_locations_address
                ON locations (
                    (address->>'Country'),
                    (address->>'City'),
                    (address->>'Street'),
                    (address->>'BuildingNumber')
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_locations_address;");
        }
    }
}
