using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Values_Date",
                table: "Values");

            migrationBuilder.DropIndex(
                name: "IX_Values_FileResultId",
                table: "Values");

            migrationBuilder.CreateIndex(
                name: "IX_Values_FileResultId_Date",
                table: "Values",
                columns: new[] { "FileResultId", "Date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Results_AverageExcecutionTime",
                table: "Results",
                column: "AverageExcecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_Results_AverageValue",
                table: "Results",
                column: "AverageValue");

            migrationBuilder.CreateIndex(
                name: "IX_Results_FirstExecutionTime",
                table: "Results",
                column: "FirstExecutionTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Values_FileResultId_Date",
                table: "Values");

            migrationBuilder.DropIndex(
                name: "IX_Results_AverageExcecutionTime",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_AverageValue",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_FirstExecutionTime",
                table: "Results");

            migrationBuilder.CreateIndex(
                name: "IX_Values_Date",
                table: "Values",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Values_FileResultId",
                table: "Values",
                column: "FileResultId");
        }
    }
}
