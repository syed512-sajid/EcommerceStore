using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceStore.Migrations
{
    public partial class FixCustomerOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create Customers table if not exists
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                              .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            // 2. Insert a default customer for old orders
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Name", "Email" },
                values: new object[] { 1, "Unknown Customer", "unknown@example.com" }
            );

            // 3. Add CustomerId column as NOT NULL with default 1
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1 // all existing orders point to default customer
            );

            // 4. Create index
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId"
            );

            // 5. Add FK constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders"
            );

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders"
            );

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Orders"
            );

            migrationBuilder.DropTable(
                name: "Customers"
            );
        }
    }
}
