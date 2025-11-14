namespace DashboardTui.Services;

using System.Text.Json;
using DashboardTui.Models;

/// <summary>
/// Weather service implementation using WeatherAPI.com
/// </summary>
public class WeatherApiService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.weatherapi.com/v1";

    public WeatherApiService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new ArgumentException("API key cannot be empty", nameof(apiKey));
        }
    }

    public async Task<WeatherData> GetWeatherWithForecastAsync(string location, int days = 3, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("Location cannot be empty", nameof(location));
        }

        if (days < 1 || days > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(days), "Forecast days must be between 1 and 10");
        }

        var url = $"{BaseUrl}/forecast.json?key={_apiKey}&q={Uri.EscapeDataString(location)}&days={days}&aqi=no";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Failed to deserialize weather data");

            return MapToWeatherData(apiResponse);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve weather data: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse weather data: {ex.Message}", ex);
        }
    }

    public async Task<WeatherData> GetCurrentWeatherAsync(string location, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("Location cannot be empty", nameof(location));
        }

        var url = $"{BaseUrl}/current.json?key={_apiKey}&q={Uri.EscapeDataString(location)}&aqi=no";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Failed to deserialize weather data");

            return MapToWeatherData(apiResponse);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve weather data: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse weather data: {ex.Message}", ex);
        }
    }

    private static WeatherData MapToWeatherData(WeatherApiResponse response)
    {
        // For US locations, use state abbreviation; for others, show country
        var isUSA = response.Location.Country.Equals("United States of America", StringComparison.OrdinalIgnoreCase) ||
                    response.Location.Country.Equals("USA", StringComparison.OrdinalIgnoreCase) ||
                    response.Location.Country.Equals("US", StringComparison.OrdinalIgnoreCase);

        var locationName = isUSA && !string.IsNullOrEmpty(response.Location.Region)
            ? $"{response.Location.Name}, {GetStateAbbreviation(response.Location.Region)}"
            : !string.IsNullOrEmpty(response.Location.Region)
                ? $"{response.Location.Name}, {response.Location.Region}, {response.Location.Country}"
                : $"{response.Location.Name}, {response.Location.Country}";

        var forecast = response.Forecast?.ForecastDay?.Select(f => new ForecastDay
        {
            Date = DateTime.Parse(f.Date),
            MaxTempF = f.Day.MaxTempF,
            MinTempF = f.Day.MinTempF,
            Condition = f.Day.Condition.Text
        }).ToList();

        return new WeatherData
        {
            Location = locationName,
            TemperatureFahrenheit = response.Current.TempFahrenheit,
            Condition = response.Current.Condition.Text,
            LastUpdate = DateTime.Parse(response.Current.LastUpdated),
            Humidity = response.Current.Humidity,
            WindSpeedKph = response.Current.WindKph,
            Forecast = forecast
        };
    }

    private static string GetStateAbbreviation(string stateName)
    {
        var states = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Alabama"] = "AL",
            ["Alaska"] = "AK",
            ["Arizona"] = "AZ",
            ["Arkansas"] = "AR",
            ["California"] = "CA",
            ["Colorado"] = "CO",
            ["Connecticut"] = "CT",
            ["Delaware"] = "DE",
            ["Florida"] = "FL",
            ["Georgia"] = "GA",
            ["Hawaii"] = "HI",
            ["Idaho"] = "ID",
            ["Illinois"] = "IL",
            ["Indiana"] = "IN",
            ["Iowa"] = "IA",
            ["Kansas"] = "KS",
            ["Kentucky"] = "KY",
            ["Louisiana"] = "LA",
            ["Maine"] = "ME",
            ["Maryland"] = "MD",
            ["Massachusetts"] = "MA",
            ["Michigan"] = "MI",
            ["Minnesota"] = "MN",
            ["Mississippi"] = "MS",
            ["Missouri"] = "MO",
            ["Montana"] = "MT",
            ["Nebraska"] = "NE",
            ["Nevada"] = "NV",
            ["New Hampshire"] = "NH",
            ["New Jersey"] = "NJ",
            ["New Mexico"] = "NM",
            ["New York"] = "NY",
            ["North Carolina"] = "NC",
            ["North Dakota"] = "ND",
            ["Ohio"] = "OH",
            ["Oklahoma"] = "OK",
            ["Oregon"] = "OR",
            ["Pennsylvania"] = "PA",
            ["Rhode Island"] = "RI",
            ["South Carolina"] = "SC",
            ["South Dakota"] = "SD",
            ["Tennessee"] = "TN",
            ["Texas"] = "TX",
            ["Utah"] = "UT",
            ["Vermont"] = "VT",
            ["Virginia"] = "VA",
            ["Washington"] = "WA",
            ["West Virginia"] = "WV",
            ["Wisconsin"] = "WI",
            ["Wyoming"] = "WY",
            ["District of Columbia"] = "DC"
        };

        return states.TryGetValue(stateName, out var abbr) ? abbr : stateName;
    }
}
