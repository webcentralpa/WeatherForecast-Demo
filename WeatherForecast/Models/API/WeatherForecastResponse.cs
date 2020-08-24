using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherForecast.Models.API
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class WeatherForecastResponse
    {
        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("init")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime ForecastDate { get; set; }

        [JsonProperty("dataseries")]
        public List<DailyForecastDataItem> DailyForecastDataItems { get; set; }

        public override string ToString() {
            return this.ToJson();
        }
    }

    public partial class DailyForecastDataItem
    {
        [JsonProperty("date")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Date { get; set; }

        [JsonProperty("weather")]
        public string WeatherCode { get; set; }

        public string Weather =>
            WeatherCode switch
            {
			    "clear" => "Clear",
			    "pcloudy" => "Partly Cloudy",
			    "mcloudy" => "Cloudy",
			    "cloudy" => "Very Cloudy",
			    "fog" => "Foggy",
			    "lightrain" => "Light rain or showers",
			    "oshower" => "Occasional showers",
			    "ishower" => "Isolated showers",
			    "lightsnow" => "Light or occasional snow",
			    "rain" => "Rain",
			    "snow" => "Snow",
			    "rainsnow" => "Mixed Precipitation",
                "tstorm" => "Thunderstorm possible",
                "ts" => "Thunderstorm possible",
                "tsrain" => "Thunderstorm",
			    "windy" => "Windy",
                _ => ""
            };

        [JsonProperty("temp2m")]
        public TemperatureMinMax TemperatureMinMax { get; set; }

        [JsonProperty("wind10m_max")]
        public int WindSpeedCode { get; set; }

        public string WindSpeed =>
            WindSpeedCode switch
            {
                1 => "Below 0.3 m/s (calm)",
                2 => "0.3 - 3.4 m/s (light)",
                3 => "3.4 - 8.0 m/s (moderate)",
                4 => "8.0 - 10.8 m/s (fresh)",
                5 => "10.8 - 17.2 m/s (strong)",
                6 => "17.2 - 24.5 m/s (gale)",
                7 => "24.5 - 32.6 m/s (storm)",
                8 => "Over 32.6 m/s (hurricane)",
                _ => ""
            };
    }

    public partial class TemperatureMinMax
    {
        [JsonProperty("max")]
        public long MaxCelsius { get; set; }

        [JsonProperty("min")]
        public long MinCelsius { get; set; }

        public long MaxFahrenheit => 32 + (long)(MaxCelsius / 0.5556);

        public long MinFahrenheit => 32 + (long)(MinCelsius / 0.5556);
    }

    public partial class WeatherForecastResponse
    {
        public static WeatherForecastResponse FromJson(string json) => JsonConvert.DeserializeObject<WeatherForecastResponse>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this WeatherForecastResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    class CustomDateTimeConverter : DateTimeConverterBase
    {

        /// <summary>
        /// DateTime format
        /// </summary>
        private const string Format = "yyyyMMdd";

        /// <summary>
        /// Writes value to JSON
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="value">Value to be written</param>
        /// <param name="serializer">JSON serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(Format));
        }

        /// <summary>
        /// Reads value from JSON
        /// </summary>
        /// <param name="reader">JSON reader</param>
        /// <param name="objectType">Target type</param>
        /// <param name="existingValue">Existing value</param>
        /// <param name="serializer">JSON serialized</param>
        /// <returns>Deserialized DateTime</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            // only take the first 8 digits ("init" field has two extra digits...)
            var s = reader.Value.ToString().Substring(0, 8);
            DateTime result;
            if (DateTime.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return DateTime.Now;
        }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }


}
