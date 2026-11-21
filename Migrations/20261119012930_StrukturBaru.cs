using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWalletApi.Migrations
{
    /// <inheritdoc />
    public partial class StrukturBaru : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Akun",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomorAkun = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NamaPemilik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorHp = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DibuatPada = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Akun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transaksi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KodeTransaksi = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Jenis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AkunAsalId = table.Column<int>(type: "int", nullable: true),
                    AkunTujuanId = table.Column<int>(type: "int", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BiayaAdmin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoSebelum = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoSesudah = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Keterangan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KunciIdempotensi = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WaktuTransaksi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaksi", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Akun_NomorAkun",
                table: "Akun",
                column: "NomorAkun",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Akun_NomorHp",
                table: "Akun",
                column: "NomorHp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaksi_KodeTransaksi",
                table: "Transaksi",
                column: "KodeTransaksi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaksi_KunciIdempotensi",
                table: "Transaksi",
                column: "KunciIdempotensi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Akun");

            migrationBuilder.DropTable(
                name: "Transaksi");
        }
    }
}
