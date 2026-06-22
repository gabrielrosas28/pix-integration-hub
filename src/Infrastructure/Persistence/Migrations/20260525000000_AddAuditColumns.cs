using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Auditorias",
                newName: "AuditoriaId");

            migrationBuilder.AddColumn<int>(
                name: "ContaId",
                table: "Auditorias",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HorarioRegistro",
                table: "Auditorias",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "StatusPagamento",
                table: "Auditorias",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TxId",
                table: "Auditorias",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Auditorias",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Raw",
                table: "Auditorias",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "Auditorias",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PayloadAcao",
                table: "Auditorias",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PayloadConfirmacao",
                table: "Auditorias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamento",
                table: "Auditorias",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HorarioConfirmacao",
                table: "Auditorias",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ContaId", table: "Auditorias");
            migrationBuilder.DropColumn(name: "HorarioRegistro", table: "Auditorias");
            migrationBuilder.DropColumn(name: "StatusPagamento", table: "Auditorias");
            migrationBuilder.DropColumn(name: "TxId", table: "Auditorias");
            migrationBuilder.DropColumn(name: "Descricao", table: "Auditorias");
            migrationBuilder.DropColumn(name: "Raw", table: "Auditorias");
            migrationBuilder.DropColumn(name: "Valor", table: "Auditorias");
            migrationBuilder.DropColumn(name: "PayloadAcao", table: "Auditorias");
            migrationBuilder.DropColumn(name: "PayloadConfirmacao", table: "Auditorias");
            migrationBuilder.DropColumn(name: "DataPagamento", table: "Auditorias");
            migrationBuilder.DropColumn(name: "HorarioConfirmacao", table: "Auditorias");

            migrationBuilder.RenameColumn(
                name: "AuditoriaId",
                table: "Auditorias",
                newName: "Id");
        }
    }
}
