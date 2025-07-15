using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bikya.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "ShippingInfos",
                newName: "Status");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ShippingInfos",
                newName: "status");
        }
    }
}
