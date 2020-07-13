// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Anki.Resources.SDK
{

/// <summary>
/// The Weather behavior
/// </summary>
public partial class Weather:BehaviorCoordinator
{
    /// <summary>
    /// Maps a weather condition ( e.g. "Cloudy", "Cold") to the text
    /// localization key.
    /// The key used in the BehaviourStrings.json file to look up the
    /// localized text.  It isn't the thing to say directly
    /// </summary>
    readonly Dictionary<string,string> condition2Say  = new Dictionary<string,string>();

    /// <summary>
    /// Maps a weather condition to the text-localization key
    /// </summary>
    /// <value>Maps a weather condition to the key employed in text localization</value>
    public IReadOnlyDictionary<string,string> Condition2Say=>condition2Say;

    /// <summary>
    /// Maps the Weather Company weather string to Vector's weather condition
    /// </summary>
    readonly Dictionary<string,string> weatherCompanyCondition2Condition  = new Dictionary<string,string>();

    /// <summary>
    /// Maps the Weather Company weather string to Vector's weather condition
    /// </summary>
    /// <value>Maps the varied weather conditions reported by the Weather
    /// Company API to the weather conditions used internally.</value>
    public IReadOnlyDictionary<string,string> WeatherCompanyCondition2Condition=>weatherCompanyCondition2Condition;

    /// <summary>
    /// Constructs the Weather behavior object
    /// </summary>
    /// <param name="configPath">Path to the config folder</param>
    internal Weather(string configPath):base(new List<string>()
        {
            "weather_fahrenheit_indicator",
            "weather_celsius_indicator",
            "weather_negative_indicator",
            "weather_temp_0",
            "weather_temp_1",
            "weather_temp_2",
            "weather_temp_3",
            "weather_temp_4",
            "weather_temp_5",
            "weather_temp_6",
            "weather_temp_7",
            "weather_temp_8",
            "weather_temp_9"
            },
            new List<string>()
            {
            "TemperatureDoubleDig",
            "TemperatureNegDoubleDig",
            "TemperatureSingleDig",
            "TemperatureNegSingleDig",
            "TemperatureTripleDig",
            "TemperatureNegTripleDig",
            "WeatherCondCloudy_01",
            "WeatherCondColdClear_01",
            "WeatherCondRain_01",
            "WeatherCondSnow_01",
            "WeatherCondStars_01",
            "WeatherCondSunny_01",
            "WeatherCondThunderstorms_01",
            "WeatherCondWindy_01"
            }
    )
    {
        // Get the configuration
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues    = true
            };
        // get the path to the mapping to localization id file
        var path = Path.Combine(configPath, "condition_to_tts.json");

        // Get the text for the file
        var text = File.ReadAllText(path);
        var d = JsonSerializer.Deserialize<Dictionary<string, string>[]>(text, JSONOptions);

        // Create the mapping from weather condition to the text localization id
        foreach (var c in d)
            condition2Say[c["Condition"]]= c["Say"];

        // get the path to the moods file
        path = Path.Combine(configPath, "weatherResponseMaps/dev_map.json");
        text = File.ReadAllText(path);
        d = JsonSerializer.Deserialize<Dictionary<string, string>[]>(text, JSONOptions);
        foreach (var c in d)
            weatherCompanyCondition2Condition[c["APIValue"]]= c["CladType"];
        path = Path.Combine(configPath, "weatherResponseMaps/weather_weathercompany.json");
        text = File.ReadAllText(path);
        d = JsonSerializer.Deserialize<Dictionary<string, string>[]>(text, JSONOptions);
        foreach (var c in d)
            weatherCompanyCondition2Condition[c["APIValue"]]= c["CladType"];
    }
}

}
