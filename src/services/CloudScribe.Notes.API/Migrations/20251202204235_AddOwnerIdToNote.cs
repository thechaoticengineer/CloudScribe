using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudScribe.Notes.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owner_id",
                table: "notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "notes");
        }
    }
}
