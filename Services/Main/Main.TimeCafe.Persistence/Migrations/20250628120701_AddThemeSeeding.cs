using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TimeCafe.Persistence.Migrations;

/// <inheritdoc />
public partial class AddThemeSeeding : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 1);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 2);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 3);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 4);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 5);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 6);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 7);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 8);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 9);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 10);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 11);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 12);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 13);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 14);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 15);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 16);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 17);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 18);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 19);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 20);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 21);

        migrationBuilder.DeleteData(
            table: "Themes",
            keyColumn: "ThemeId",
            keyValue: 22);
    }
}
