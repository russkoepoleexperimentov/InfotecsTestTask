using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DeltaSeconds = table.Column<int>(type: "integer", nullable: false),
                    FirstExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AverageExcecutionTime = table.Column<int>(type: "integer", nullable: false),
                    AverageValue = table.Column<float>(type: "real", nullable: false),
                    MedianValue = table.Column<float>(type: "real", nullable: false),
                    MinimumValue = table.Column<float>(type: "real", nullable: false),
                    MaximumValue = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExceutionTime = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<float>(type: "real", nullable: false),
                    FileResultId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Values_Results_FileResultId",
                        column: x => x.FileResultId,
                        principalTable: "Results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Results_FileName",
                table: "Results",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Values_Date",
                table: "Values",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Values_FileResultId",
                table: "Values",
                column: "FileResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Values");

            migrationBuilder.DropTable(
                name: "Results");
        }
    }
}
