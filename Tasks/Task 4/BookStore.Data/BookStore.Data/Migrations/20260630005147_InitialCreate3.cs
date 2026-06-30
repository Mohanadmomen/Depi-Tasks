using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEp5aV9SFbq6iLagOQ8mh+0Jurtjb7OvMIKiEnwNXDQkVDyb90MxAaI4jtHvk5kTEA==");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEp5aV9SFbq6iLagOQ8mh+0Jurtjb7OvMIKiEnwNXDQkVDyb90MxAaI4jtHvk5kTEA==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELHORu7FKNTMMv1otAFHA5KcqJz9YJaJyZNlU2pL4X5HiJz2NBarJ4PMx6xugN9InQ==");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEK1wbCmNMrQXPeUUEqdsRCMvlqJZFqLJEu/B+3qoanJORmSZPHGEiFr7t/LtQLwGaA==");
        }
    }
}
