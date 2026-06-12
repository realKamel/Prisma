using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Services.DataSeeding;

public class UserHierarchyConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(User);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Load the JSON object into memory
        var jsonObject = JObject.Load(reader);

        // Look up the "userType" property from your JSON
        var userType = jsonObject["Discriminator"]?.ToString();

        // Instatitate the correct concrete class based on the value
        User user = userType switch
        {
            "Teacher" => new Teacher(), // Replace with your actual concrete class name
            "Assistant" => new Assistant(), // Example for other types
            "Student" => new Student(),
            "Admin" => new Admin(),
            _ => throw new NotSupportedException($"The user type '{userType}' is not supported.")
        };

        // Populate all the base and child properties (Id, Title, etc.) onto our new object
        using (var subReader = jsonObject.CreateReader())
        {
            serializer.Populate(subReader, user);
        }

        return user;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Only needed if you are serializing/saving back to JSON.");
    }
}