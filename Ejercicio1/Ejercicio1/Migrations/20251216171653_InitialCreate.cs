using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ejercicio1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispositivos",
                columns: table => new
                {
                    DispositivoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispositivos", x => x.DispositivoId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    ReservaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispositivoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(DAY, 1, GETDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.ReservaId);
                    table.ForeignKey(
                        name: "FK_Reservas_Dispositivos_DispositivoId",
                        column: x => x.DispositivoId,
                        principalTable: "Dispositivos",
                        principalColumn: "DispositivoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Dispositivos",
                columns: new[] { "DispositivoId", "Nombre" },
                values: new object[,]
                {
                    { 1, "Laptop HP" },
                    { 2, "Monitor Samsung" },
                    { 3, "Teclado Logitech" },
                    { 4, "Mouse Razer" },
                    { 5, "Tablet iPad" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "UsuarioId", "NombreCompleto" },
                values: new object[,]
                {
                    { 1, "Juan Garcia" },
                    { 2, "Maria Lopez" },
                    { 3, "Pedro Martinez" },
                    { 4, "Ana Rodriguez" }
                });

            migrationBuilder.InsertData(
                table: "Reservas",
                columns: new[] { "ReservaId", "DispositivoId", "FechaFin", "FechaInicio", "UsuarioId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 12, 21, 18, 16, 52, 986, DateTimeKind.Local).AddTicks(3795), new DateTime(2025, 12, 14, 18, 16, 52, 986, DateTimeKind.Local).AddTicks(3737), 1 },
                    { 2, 2, new DateTime(2025, 12, 13, 18, 16, 52, 986, DateTimeKind.Local).AddTicks(3798), new DateTime(2025, 12, 6, 18, 16, 52, 986, DateTimeKind.Local).AddTicks(3797), 2 },
                    { 3, 3, new DateTime(2025, 12, 23, 18, 16, 52, 986, DateTimeKind.Local).AddTicks(3800), new DateTime(2025, 12, 19, 18, 16, 52, 986, DateTimeKind.Local).AddTicks(3799), 3 },
                    { 4, 4, new DateTime(2025, 12, 18, 18, 16, 52, 986, DateTimeKind.Local).AddTicks(3803), new DateTime(2025, 12, 15, 18, 16, 52, 986, DateTimeKind.Local).AddTicks(3802), 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispositivos_Nombre",
                table: "Dispositivos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_DispositivoId",
                table: "Reservas",
                column: "DispositivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UsuarioId",
                table: "Reservas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Dispositivos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
