using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Infrastructure.Services.DataSeeding;

public class QuestionConverter : JsonConverter
{
    // Tells Newtonsoft to only apply this converter to the base Question class
    public override bool CanConvert(Type objectType) => objectType == typeof(Question);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Load the JSON object into memory
        var jsonObject = JObject.Load(reader);

        // Look up the "QuestionType" property from your JSON
        var questionType = jsonObject["QuestionType"]?.ToString();

        // Instatitate the correct concrete class based on the value
        Question question = questionType switch
        {
            "MCQ" => new MCQQuestion(), // Replace with your actual concrete class name
            "Written" => new WrittenQuestion(), // Example for other types
            _ => throw new NotSupportedException($"The question type '{questionType}' is not supported.")
        };

        // Populate all the base and child properties (Id, Title, etc.) onto our new object
        using (var subReader = jsonObject.CreateReader())
        {
            serializer.Populate(subReader, question);
        }

        return question;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Only needed if you are serializing/saving back to JSON.");
    }
}