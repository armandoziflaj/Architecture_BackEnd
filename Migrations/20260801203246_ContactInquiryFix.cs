using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sulozeqi_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class ContactInquiryFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""ContactInquiries"" DROP COLUMN IF EXISTS ""RowVersion"";
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ContactInquiries",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInquiries_IsRead",
                table: "ContactInquiries",
                column: "IsRead");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContactInquiries_IsRead",
                table: "ContactInquiries");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ContactInquiries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000);
        }
    }
}
