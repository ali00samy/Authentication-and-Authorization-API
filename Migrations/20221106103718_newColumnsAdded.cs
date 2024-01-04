using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authantication.Migrations
{
    public partial class newColumnsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RefreshToken",
                table: "users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryDate",
                table: "users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryDate",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "users");
        }
    }
}
