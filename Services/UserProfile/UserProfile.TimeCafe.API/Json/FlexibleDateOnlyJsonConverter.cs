using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserProfile.TimeCafe.API.Json;

public sealed class FlexibleDateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("BirthDate must be a string.");

        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            throw new JsonException("BirthDate is empty.");

        if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
            return dateOnly;

        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
            return DateOnly.FromDateTime(dto.UtcDateTime);

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            return DateOnly.FromDateTime(dt);

        throw new JsonException("The JSON value is not in a supported DateOnly format.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
