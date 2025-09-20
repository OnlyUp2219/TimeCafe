using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Main.TimeCafe.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillingTypes",
                columns: table => new
                {
                    BillingTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillingTypeName = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BillingT__5233EF23A53230B5", x => x.BillingTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ClientStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StatusName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ClientSt__C8EE2063B82E1A46", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    GenderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GenderName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Genders__4E24E9F72615BBD8", x => x.GenderId);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    ThemeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ThemeName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    TechnicalName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Themes__FBB3E4D9A31F20F0", x => x.ThemeId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                columns: table => new
                {
                    TransactionTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionTypeName = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transact__20266D0B29C99FD2", x => x.TransactionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    MiddleName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    GenderId = table.Column<int>(type: "integer", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    AccessCardNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: true),
                    RefusalReason = table.Column<string>(type: "text", maxLength: 1000, nullable: true),
                    Photo = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Clients__E67E1A244E706CCA", x => x.ClientId);
                    table.ForeignKey(
                        name: "FK__Clients__GenderI__2354350C",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "GenderId");
                    table.ForeignKey(
                        name: "FK__Clients__StatusI__24485945",
                        column: x => x.StatusId,
                        principalTable: "ClientStatuses",
                        principalColumn: "StatusId");
                });

            migrationBuilder.CreateTable(
                name: "Tariffs",
                columns: table => new
                {
                    TariffId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TariffName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DescriptionTitle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", maxLength: 1000, nullable: true),
                    Icon = table.Column<byte[]>(type: "bytea", nullable: true),
                    ThemeId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "now()"),
                    LastModified = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "now()"),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    BillingTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tariffs__E3E2E6E5A4F4E6F7", x => x.TariffId);
                    table.ForeignKey(
                        name: "FK_Tariffs_BillingTypes",
                        column: x => x.BillingTypeId,
                        principalTable: "BillingTypes",
                        principalColumn: "BillingTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Tariffs__ThemeId__2BE97B0D",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "ThemeId");
                });

            migrationBuilder.CreateTable(
                name: "ClientAdditionalInfo",
                columns: table => new
                {
                    InfoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    InfoText = table.Column<string>(type: "text", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ClientAd__4DEC9D7A0A15EECD", x => x.InfoId);
                    table.ForeignKey(
                        name: "FK__ClientAdd__Clien__2818EA29",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhoneConfirmations",
                columns: table => new
                {
                    ConfirmationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    ConfirmationCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    GeneratedTime = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PhoneConf__9B4F4D6A6E9A7B4E", x => x.ConfirmationId);
                    table.ForeignKey(
                        name: "FK__PhoneConf__Clien__5303482E",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    VisitId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    TariffId = table.Column<int>(type: "integer", nullable: true),
                    BillingTypeId = table.Column<int>(type: "integer", nullable: true),
                    EntryTime = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ExitTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    VisitCost = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Visits__4D3AA1DEECB6D8F4", x => x.VisitId);
                    table.ForeignKey(
                        name: "FK__Visits__BillingT__4979DDF4",
                        column: x => x.BillingTypeId,
                        principalTable: "BillingTypes",
                        principalColumn: "BillingTypeId");
                    table.ForeignKey(
                        name: "FK__Visits__ClientId__47919582",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Visits__TariffId__4885B9BB",
                        column: x => x.TariffId,
                        principalTable: "Tariffs",
                        principalColumn: "TariffId");
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "integer", nullable: true),
                    VisitId = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Financia__55433A6B57033591", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK__Financial__Clien__4E3E9311",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Financial__Trans__4F32B74A",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionTypes",
                        principalColumn: "TransactionTypeId");
                    table.ForeignKey(
                        name: "FK__Financial__Visit__5026DB83",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "VisitId");
                });

            migrationBuilder.InsertData(
                table: "BillingTypes",
                columns: new[] { "BillingTypeId", "BillingTypeName" },
                values: new object[,]
                {
                    { 1, "Почасовая" },
                    { 2, "Поминутная" }
                });

            migrationBuilder.InsertData(
                table: "ClientStatuses",
                columns: new[] { "StatusId", "StatusName" },
                values: new object[,]
                {
                    { 1, "Черновик" },
                    { 2, "Активный" },
                    { 3, "Отказ от услуг" }
                });

            migrationBuilder.InsertData(
                table: "Genders",
                columns: new[] { "GenderId", "GenderName" },
                values: new object[,]
                {
                    { 1, "Мужской" },
                    { 2, "Женский" },
                    { 3, "Не указан" }
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "ThemeId", "TechnicalName", "ThemeName" },
                values: new object[,]
                {
                    { 1, "Red", "Красный" },
                    { 2, "Orange", "Оранжевый" },
                    { 3, "Amber", "Янтарный" },
                    { 4, "Yellow", "Желтый" },
                    { 5, "Lime", "Лаймовый" },
                    { 6, "Green", "Зеленый" },
                    { 7, "Emerald", "Изумрудный" },
                    { 8, "Teal", "Бирюзовый" },
                    { 9, "Cyan", "Голубой" },
                    { 10, "Sky", "Небесный" },
                    { 11, "Blue", "Синий" },
                    { 12, "Indigo", "Индиго" },
                    { 13, "Violet", "Фиолетовый" },
                    { 14, "Purple", "Пурпурный" },
                    { 15, "Fuchsia", "Фуксия" },
                    { 16, "Pink", "Розовый" },
                    { 17, "Rose", "Роза" },
                    { 18, "Slate", "Сланец" },
                    { 19, "Gray", "Серый" },
                    { 20, "Zinc", "Цинк" },
                    { 21, "Neutral", "Нейтральный" },
                    { 22, "Stone", "Камень" }
                });

            migrationBuilder.InsertData(
                table: "TransactionTypes",
                columns: new[] { "TransactionTypeId", "TransactionTypeName" },
                values: new object[,]
                {
                    { 1, "Пополнение" },
                    { 2, "Списание" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingTypes_Name",
                table: "BillingTypes",
                column: "BillingTypeName",
                unique: true,
                filter: "\"BillingTypeName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAdditionalInfo_ClientId",
                table: "ClientAdditionalInfo",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AccessCardNumber",
                table: "Clients",
                column: "AccessCardNumber",
                unique: true,
                filter: "\"AccessCardNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_GenderId",
                table: "Clients",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_PhoneNumber",
                table: "Clients",
                column: "PhoneNumber",
                unique: true,
                filter: "\"PhoneNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_StatusId",
                table: "Clients",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ClientId",
                table: "FinancialTransactions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionDate",
                table: "FinancialTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionTypeId",
                table: "FinancialTransactions",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_VisitId",
                table: "FinancialTransactions",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "UQ__Genders__F7C1771527CC73EB",
                table: "Genders",
                column: "GenderName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhoneConfirmations_ClientId",
                table: "PhoneConfirmations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_BillingTypeId",
                table: "Tariffs",
                column: "BillingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_TariffName",
                table: "Tariffs",
                column: "TariffName",
                unique: true,
                filter: "\"TariffName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_ThemeId",
                table: "Tariffs",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Themes_TechnicalName",
                table: "Themes",
                column: "TechnicalName",
                unique: true,
                filter: "\"TechnicalName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Themes_ThemeName",
                table: "Themes",
                column: "ThemeName",
                unique: true,
                filter: "\"ThemeName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_Name",
                table: "TransactionTypes",
                column: "TransactionTypeName",
                unique: true,
                filter: "\"TransactionTypeName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_BillingTypeId",
                table: "Visits",
                column: "BillingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ClientId",
                table: "Visits",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_EntryTime",
                table: "Visits",
                column: "EntryTime");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ExitTime",
                table: "Visits",
                column: "ExitTime");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_TariffId",
                table: "Visits",
                column: "TariffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientAdditionalInfo");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "PhoneConfirmations");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Tariffs");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "ClientStatuses");

            migrationBuilder.DropTable(
                name: "BillingTypes");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
