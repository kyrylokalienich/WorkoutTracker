using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCognitoSub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CognitoSub",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_CognitoSub",
                table: "users",
                column: "CognitoSub",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_CognitoSub",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CognitoSub",
                table: "users");
        }
    }
}
