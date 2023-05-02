using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xenon.LayoutInfo.Serialization
{

    internal class Font_JsonConverter : JsonConverter<Font>
    {
        public override Font Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<LWJFont>(reader.GetString()).GetFont();
        }
        public override void Write(Utf8JsonWriter writer, Font value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(JsonSerializer.Serialize(new LWJFont(value)));
        }
    }

    internal class Color_JsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<LWJColor>(reader.GetString()).GetColor();
        }
        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(JsonSerializer.Serialize(new LWJColor(value)));
        }
    }

    internal class Point_JsonConverter : JsonConverter<Point>
    {
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<LWJPoint>(reader.GetString()).GetPoint();
        }
        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(JsonSerializer.Serialize(new LWJPoint(value)));
        }
    }

    internal class Size_JsonConverter : JsonConverter<Size>
    {
        public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<LWJSize>(reader.GetString()).GetSize();
        }
        public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(JsonSerializer.Serialize(new LWJSize(value)));
        }
    }

    internal class Rectangle_JsonConverter : JsonConverter<Rectangle>
    {
        public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<LWJRect>(reader.GetString()).GetRectangle();
        }
        public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(JsonSerializer.Serialize(new LWJRect(value)));
        }
    }





}
