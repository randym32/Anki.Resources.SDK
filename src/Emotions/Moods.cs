// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace Anki.Resources.SDK
{

/// <summary>
/// A helper class to hold the allow minimum and maximum
/// </summary>
public class MinMax
{
    /// <summary>
    /// The minimum allowed value for the emotion type.
    /// </summary>
    public float min {get; internal set; }  =-1.0f;

    /// <summary>
    /// The maximum allowed value for the emotion type.
    /// </summary>
    public float max {get; internal set; } =1.0f;
}

public partial class Assets
{
    /// <summary>
    /// The allowed value range for each dimension of the emotion 
    /// </summary>
    readonly Dictionary<string, MinMax> emotionRanges= new Dictionary<string, MinMax>();
    /// <summary>
    /// The allowed value range for each dimension of the emotion 
    /// </summary>
    public IReadOnlyDictionary<string, MinMax> EmotionRanges => emotionRanges;

    /// <summary>
    /// The built in table of simple moods.
    /// </summary>
    readonly Dictionary<string,string> simpleMoods= new Dictionary<string,string>()
    {
        {"Default",""},
        {"Frustrated",""},
        {"HighStim",""},
        {"LowStim",""},
        {"MedStim",""},
    };
    /// <summary>
    /// The built in table of simple moods.
    /// </summary>
    public IReadOnlyDictionary<string,string> SimpleMoods=>simpleMoods;


    /// <summary>
    /// Loads the information from the moods file
    /// </summary>
    /// <param name="configPath">Path to the config folder</param>
    void LoadMoods(string configPath)
    {
        // get the path to the moods file
        var path = Path.Combine(configPath, "mood_config.json");

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get the configuration
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues    = true
            };
        var d = Util.ToDict(JsonSerializer.Deserialize<Dictionary<string, object>>(text, JSONOptions));

        // Set up the allowed value ranges
        // Poopulate some defaults (these will get overriden)
        emotionRanges["Happy"     ] = new MinMax();
        emotionRanges["Confident" ] = new MinMax();
        emotionRanges["Social"    ] = new MinMax();
        emotionRanges["Stimulated"] = new MinMax();

        // internalize each of the items
        // Give ranges to emotion dimensions
        if (d.TryGetValue("valueRanges", out var valueRanges))
            foreach (var _item in (object[])valueRanges)
            {
                // Create a range of acceptable values for this emotion type
                var mm = new MinMax();
                var item = (Dictionary<string,object>)_item;
                mm.max = (float)(double)item["max"];
                mm.min = (float)(double)item["min"];
                emotionRanges[(string)item["emotionType"]] = mm;
            }

        // Todo: the decay graphs, etc
    }

    /// <summary>
    /// The table mapping each dimension of emotion to its allow range of values
    /// </summary>
    public IReadOnlyDictionary<string, MinMax> EmotionValueRanges => emotionRanges;
}
}


