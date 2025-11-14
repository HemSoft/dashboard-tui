namespace DashboardTui.Models;

using System.Text.Json.Serialization;

/// <summary>
/// WeatherAPI.com response model
/// </summary>
public record WeatherApiResponse
{
    [JsonPropertyName("location")]
    public required LocationInfo Location { get; init; }

    [JsonPropertyName("current")]
    public required CurrentWeather Current { get; init; }

    [JsonPropertyName("forecast")]
    public ForecastInfo? Forecast { get; init; }
}

public record LocationInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("region")]
    public string? Region { get; init; }

    [JsonPropertyName("country")]
    public required string Country { get; init; }

    [JsonPropertyName("localtime")]
    public required string LocalTime { get; init; }
}

public record CurrentWeather
{
    [JsonPropertyName("temp_c")]
    public double TempCelsius { get; init; }

    [JsonPropertyName("temp_f")]
    public double TempFahrenheit { get; init; }

    [JsonPropertyName("condition")]
    public required WeatherCondition Condition { get; init; }

    [JsonPropertyName("humidity")]
    public double Humidity { get; init; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; init; }

    [JsonPropertyName("last_updated")]
    public required string LastUpdated { get; init; }
}

public record WeatherCondition
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }
}

public record ForecastInfo
{
    [JsonPropertyName("forecastday")]
    public List<ForecastDayApi>? ForecastDay { get; init; }
}

public record ForecastDayApi
{
    [JsonPropertyName("date")]
    public required string Date { get; init; }

    [JsonPropertyName("day")]
    public required DayWeather Day { get; init; }
}

public record DayWeather
{
    [JsonPropertyName("maxtemp_f")]
    public double MaxTempF { get; init; }

    [JsonPropertyName("mintemp_f")]
    public double MinTempF { get; init; }

    [JsonPropertyName("condition")]
    public required WeatherCondition Condition { get; init; }
}
