using System.Text.Json;
using System.Text.Json.Serialization;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Services.DataSeeding;

public class UserHierarchyConverter : JsonConverter<User>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(User).IsAssignableFrom(typeToConvert);

    public override User Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Copy the reader to look ahead into the JSON object
        Utf8JsonReader readerClone = reader;

        if (readerClone.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        string discriminator = string.Empty;

        // Search for the "Discriminator" property key
        while (readerClone.Read())
        {
            if (readerClone.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = readerClone.GetString();
                readerClone.Read();

                if (string.Equals(propertyName, "Discriminator", StringComparison.OrdinalIgnoreCase))
                {
                    discriminator = readerClone.GetString();
                    break;
                }
            }
            else if (readerClone.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
        }

        // Deserialize based on the detected discriminator string
        return discriminator switch
        {
            "Teacher" => JsonSerializer.Deserialize<Teacher>(ref reader, options),
            "Assistant" => JsonSerializer.Deserialize<Assistant>(ref reader, options),
            "Student" => JsonSerializer.Deserialize<Student>(ref reader, options),
            _ => JsonSerializer.Deserialize<User>(ref reader, options) // Fallback to base
        };
    }

    public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
    {
        // Serialize the runtime type directly (Teacher, Student, etc.) instead of the base abstract type
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}