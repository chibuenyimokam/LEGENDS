using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LegendPay.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwoFactorCode",
                table: "AdminAccounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFactorExpiration",
                table: "AdminAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AdminAccounts",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "Password", "Role", "TwoFactorCode", "TwoFactorExpiration" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "nwaeze.adaku@gmail.com", "Adaku", true, "Nwaeze", "$2a$12$1x0FKmuHNzklamegKSwrSusPA45X1XWIvnMmtRbiwSuATHHILsnle", "Admin", null, null },
                    { new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "programmingwithKami@gmail.com", "Mitchel", true, "Aziken", "$2a$12$D1.b9QgzLVlmP/9m7.GAhOX/FknZ/lFIhO7kbh.66gwp2HY1sZdHe", "Admin", null, null },
                    { new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$12$CzlvE3HbR/LZa6RF.O2V0O0R5pL/nzpctbJMQMaltYh7II1JvCXTy", "Chibuenyim", true, "Okam", "your-bcrypt-hash-here", "Admin", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AdminAccounts",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));

            migrationBuilder.DeleteData(
                table: "AdminAccounts",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"));

            migrationBuilder.DeleteData(
                table: "AdminAccounts",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"));

            migrationBuilder.DropColumn(
                name: "TwoFactorCode",
                table: "AdminAccounts");

            migrationBuilder.DropColumn(
                name: "TwoFactorExpiration",
                table: "AdminAccounts");
        }
    }
}
