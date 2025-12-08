using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LedgerFlow.LedgerSummaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LedgerSummary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCredits = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDebits = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerSummary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LedgerSummaryTransaction",
                columns: table => new
                {
                    LedgerSummaryId = table.Column<int>(type: "int", nullable: false),
                    TransactionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerSummaryTransaction", x => new { x.LedgerSummaryId, x.TransactionsId });
                    table.ForeignKey(
                        name: "FK_LedgerSummaryTransaction_LedgerSummary_LedgerSummaryId",
                        column: x => x.LedgerSummaryId,
                        principalTable: "LedgerSummary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LedgerSummaryTransaction_Transaction_TransactionsId",
                        column: x => x.TransactionsId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerSummaryTransaction_TransactionsId",
                table: "LedgerSummaryTransaction",
                column: "TransactionsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LedgerSummaryTransaction");

            migrationBuilder.DropTable(
                name: "LedgerSummary");

            migrationBuilder.DropTable(
                name: "Transaction");
        }
    }
}
