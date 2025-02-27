using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

// This class was created solely for the porpuse
// of converting DateOnly objects to and from JSON
// in the format "dd/MM/yyyy"

namespace BoardGameGeekLike.Properties
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "dd/MM/yyyy";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateOnly.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}