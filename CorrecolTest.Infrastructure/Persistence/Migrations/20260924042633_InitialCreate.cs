using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorrecolTest.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pais",
                columns: table => new
                {
                    PaisCodigo = table.Column<short>(type: "smallint", nullable: false),
                    PaisIso1 = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    PaisIso2 = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    PaisNombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    PaisCapital = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pais", x => x.PaisCodigo);
                });

            migrationBuilder.CreateTable(
                name: "DepartamentosColombia",
                columns: table => new
                {
                    DptColCodigoDane = table.Column<int>(type: "int", nullable: false),
                    DptColNombredelDepartamento = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    DptColPaisCodigo = table.Column<short>(type: "smallint", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartamentosColombia", x => x.DptColCodigoDane);
                    table.UniqueConstraint("AK_DepartamentosColombia_DptColCodigoDane_DptColPaisCodigo", x => new { x.DptColCodigoDane, x.DptColPaisCodigo });
                    table.ForeignKey(
                        name: "FK_DepartamentosColombia_Pais_DptColPaisCodigo",
                        column: x => x.DptColPaisCodigo,
                        principalTable: "Pais",
                        principalColumn: "PaisCodigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DivisionPoliticaColombia",
                columns: table => new
                {
                    DvsPltColCodigoDane = table.Column<int>(type: "int", nullable: false),
                    DvsPltColNombreMunicipio = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    DvsPltColDptColCodigoDane = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DivisionPoliticaColombia", x => x.DvsPltColCodigoDane);
                    table.UniqueConstraint("AK_DivisionPoliticaColombia_DvsPltColCodigoDane_DvsPltColDptColCodigoDane", x => new { x.DvsPltColCodigoDane, x.DvsPltColDptColCodigoDane });
                    table.ForeignKey(
                        name: "FK_DivisionPoliticaColombia_DepartamentosColombia_DvsPltColDptColCodigoDane",
                        column: x => x.DvsPltColDptColCodigoDane,
                        principalTable: "DepartamentosColombia",
                        principalColumn: "DptColCodigoDane",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    ClnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClnTpoIdnId = table.Column<short>(type: "smallint", nullable: false),
                    ClnNumeroIdentificacion = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ClnRazonSocial = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    ClnPaisCodigo = table.Column<short>(type: "smallint", nullable: false),
                    ClnDptColCodigoDane = table.Column<int>(type: "int", nullable: true),
                    ClnDvsPltColCodigoDane = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.ClnId);
                    table.CheckConstraint("CK_Cliente_CiudadDepartamento", "[ClnDvsPltColCodigoDane] IS NULL OR [ClnDptColCodigoDane] IS NOT NULL");
                    table.CheckConstraint("CK_Cliente_Identificacion", "LEN(LTRIM(RTRIM([ClnNumeroIdentificacion]))) > 0");
                    table.CheckConstraint("CK_Cliente_RazonSocial", "LEN(LTRIM(RTRIM([ClnRazonSocial]))) > 0");
                    table.CheckConstraint("CK_Cliente_TipoIdentificacion", "[ClnTpoIdnId] IN (1,2,3,4,5)");
                    table.ForeignKey(
                        name: "FK_Cliente_DepartamentosColombia_ClnDptColCodigoDane_ClnPaisCodigo",
                        columns: x => new { x.ClnDptColCodigoDane, x.ClnPaisCodigo },
                        principalTable: "DepartamentosColombia",
                        principalColumns: new[] { "DptColCodigoDane", "DptColPaisCodigo" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_DivisionPoliticaColombia_ClnDvsPltColCodigoDane_ClnDptColCodigoDane",
                        columns: x => new { x.ClnDvsPltColCodigoDane, x.ClnDptColCodigoDane },
                        principalTable: "DivisionPoliticaColombia",
                        principalColumns: new[] { "DvsPltColCodigoDane", "DvsPltColDptColCodigoDane" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Pais_ClnPaisCodigo",
                        column: x => x.ClnPaisCodigo,
                        principalTable: "Pais",
                        principalColumn: "PaisCodigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ClnDptColCodigoDane_ClnPaisCodigo",
                table: "Cliente",
                columns: new[] { "ClnDptColCodigoDane", "ClnPaisCodigo" });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ClnDvsPltColCodigoDane_ClnDptColCodigoDane",
                table: "Cliente",
                columns: new[] { "ClnDvsPltColCodigoDane", "ClnDptColCodigoDane" });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ClnPaisCodigo",
                table: "Cliente",
                column: "ClnPaisCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ClnTpoIdnId_ClnNumeroIdentificacion",
                table: "Cliente",
                columns: new[] { "ClnTpoIdnId", "ClnNumeroIdentificacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartamentosColombia_DptColNombredelDepartamento",
                table: "DepartamentosColombia",
                column: "DptColNombredelDepartamento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartamentosColombia_DptColPaisCodigo",
                table: "DepartamentosColombia",
                column: "DptColPaisCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionPoliticaColombia_DvsPltColDptColCodigoDane",
                table: "DivisionPoliticaColombia",
                column: "DvsPltColDptColCodigoDane");

            migrationBuilder.CreateIndex(
                name: "IX_Pais_PaisNombre",
                table: "Pais",
                column: "PaisNombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "DivisionPoliticaColombia");

            migrationBuilder.DropTable(
                name: "DepartamentosColombia");

            migrationBuilder.DropTable(
                name: "Pais");
        }
    }
}
