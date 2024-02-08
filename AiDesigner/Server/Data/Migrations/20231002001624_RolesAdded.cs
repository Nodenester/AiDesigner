using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDesigner.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class RolesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "8cf4210b-6bfc-4c34-82b2-d3c1756d8980", null, "Administrator", "ADMINISTRATOR" });
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "cc98cd23-a530-46fc-935a-d2cb744672dd", null, "BetaTester", "BetaTester" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8cf4210b-6bfc-4c34-82b2-d3c1756d8980");
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cc98cd23-a530-46fc-935a-d2cb744672dd");
        }
    }
}
