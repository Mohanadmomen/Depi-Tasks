using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELHORu7FKNTMMv1otAFHA5KcqJz9YJaJyZNlU2pL4X5HiJz2NBarJ4PMx6xugN9InQ==");

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "City", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { 2, "Cairo", "admin@test.com", "Admin User", "AQAAAAIAAYagAAAAEK1wbCmNMrQXPeUUEqdsRCMvlqJZFqLJEu/B+3qoanJORmSZPHGEiFr7t/LtQLwGaA==", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "");
        }
    }
}
