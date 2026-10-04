using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNotifiedForDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isCompleted",
                table: "TodoItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "NotifiedForDueDate",
                table: "TodoItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TodoItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifiedForDueDate",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TodoItems");

            migrationBuilder.AddColumn<bool>(
                name: "isCompleted",
                table: "TodoItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
