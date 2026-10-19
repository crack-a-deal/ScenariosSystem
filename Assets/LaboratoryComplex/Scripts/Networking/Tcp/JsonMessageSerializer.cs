using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace Networking
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonMessageSerializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            _options.Converters.Add(new BaseQuestionDtoConverter());
            _options.Converters.Add(new UserBaseAnswerDtoConverter());
        }

        public string Serialize<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return JsonSerializer.Serialize(value, value.GetType(), _options);
        }

        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options)
                ?? throw new InvalidOperationException(
                    $"Unable to deserialize {typeof(T).Name}");
        }
    }

    public class BaseQuestionDtoConverter : JsonConverter<BaseQuestionDTO>
    {
        public override BaseQuestionDTO Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);

            JsonElement json = document.RootElement;

            if (!json.TryGetProperty(nameof(BaseQuestionDTO.Type), out JsonElement typeElement))
                throw new JsonException("Question type is missing.");

            int typeValue = typeElement.GetInt32();

            Type questionDtoType = (QuestionType)typeValue switch
            {
                QuestionType.SingleChoice => typeof(SingleChoiceQuestionDTO),
                QuestionType.MultipleChoice => typeof(MultipleChoiceQuestionDTO),
                QuestionType.TrueFalse => typeof(TrueFalseQuestionDTO),
                QuestionType.OpenAnswer => typeof(OpenAnswerQuestionDTO),
                QuestionType.Matching => typeof(MatchingQuestionDTO),
                QuestionType.Ordering => typeof(OrderingQuestionDTO),

                _ => throw new JsonException($"Unknown question type: {typeValue}")
            };

            return (BaseQuestionDTO?)JsonSerializer.Deserialize(
                json.GetRawText(),
                questionDtoType,
                options)
                ?? throw new JsonException("Unable to deserialize question.");
        }

        public override void Write(Utf8JsonWriter writer, BaseQuestionDTO value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }

    public sealed class UserBaseAnswerDtoConverter : JsonConverter<UserBaseAnswerDTO>
    {
        private const string Discriminator = "$answerType";

        public override UserBaseAnswerDTO Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);

            JsonElement root = document.RootElement;

            if (!root.TryGetProperty(Discriminator, out JsonElement typeElement))
                throw new JsonException($"Missing {Discriminator} property.");

            string json = root.GetRawText();

            return typeElement.GetString() switch
            {
                "SingleChoice" => JsonSerializer.Deserialize<UserSingleChoiceAnswerDTO>(json, options)!,
                "MultipleChoice" => JsonSerializer.Deserialize<UserMultipleChoiceAnswerDTO>(json, options)!,
                "TrueFalse" => JsonSerializer.Deserialize<UserTrueFalseAnswerDTO>(json, options)!,
                "OpenAnswer" => JsonSerializer.Deserialize<UserOpenAnswerDTO>(json, options)!,
                "Matching" => JsonSerializer.Deserialize<UserMatchingAnswerDTO>(json, options)!,
                "Ordering" => JsonSerializer.Deserialize<UserOrderingAnswerDTO>(json, options)!,
                _ => throw new JsonException($"Unsupported answer type: {typeElement.GetString()}")
            };
        }

        public override void Write(Utf8JsonWriter writer, UserBaseAnswerDTO value, JsonSerializerOptions options)
        {
            string answerType = value switch
            {
                UserSingleChoiceAnswerDTO => "SingleChoice",
                UserMultipleChoiceAnswerDTO => "MultipleChoice",
                UserTrueFalseAnswerDTO => "TrueFalse",
                UserOpenAnswerDTO => "OpenAnswer",
                UserMatchingAnswerDTO => "Matching",
                UserOrderingAnswerDTO => "Ordering",
                _ => throw new JsonException($"Unsupported answer type: {value.GetType().Name}")
            };

            using var document = JsonDocument.Parse(JsonSerializer.Serialize(value, value.GetType(), options));

            writer.WriteStartObject();
            writer.WriteString(Discriminator, answerType);

            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                if (property.Name != Discriminator)
                    property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }
    }
}
