// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace Anki.Resources.SDK
{

/// <summary>
/// The EmotionEvent describes how the emotions respond to a given event.
/// </summary>
public class EmotionEvent
{
    /// <summary>
    /// The name of the event.
    /// </summary>
    public string name {get; set; }

    /// <summary>
    /// The impact on the emotion state.
    /// </summary>
    public IReadOnlyList<EmotionAffector> emotionAffectors {get; set; }

    /// <summary>
    /// This is a "time ratio" describing how the value decays.  Optional.
    /// </summary>
    public DecayGraph repetitionPenalty {get; set; }
}


/// <summary>
/// The EmotionAffector describes how an emotion dimension should be modified.
/// </summary>
public class EmotionAffector
{
    /// <summary>
    /// The dimension or type of emotion ("Happy", "Confident", "Stimulated",
    /// "Social", or "Trust")
    /// </summary>
    public string emotionType {get; set; }

    /// <summary>
    /// The value to add to the emotional state.  The range is -1 to 1
    /// </summary>
    public float value {get; set; }
}


partial class Assets
{
    /// <summary>
    /// Maps an emotion event name to how the emotion event is handled.
    /// </summary>
    Dictionary<string,EmotionEvent> emotionEvents;

    /// <summary>
    /// Maps an emotion event name to how the emotion event is handled.
    /// </summary>
    public IReadOnlyDictionary<string, EmotionEvent> EmotionEvents => emotionEvents;

    /// <summary>
    /// Loads the information from the emotion events files
    /// </summary>
    /// <param name="configPath">Path to the config folder</param>
    void LoadEmotionEvents(string configPath)
    {
        // Get the dictionary
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues    = true
            };

        emotionEvents = new Dictionary<string,EmotionEvent>();

        // Enumerate over the directory
        var files = Directory.EnumerateFiles(configPath, "*.json", SearchOption.AllDirectories);
        foreach (string currentFile in files)
        {
            // Get the text for the file
            var text = File.ReadAllText(currentFile);
            // Convert it
            // Vector style emotion event seems to have just the one type,
            // but Cozmo has two, so support falling back to it
            try
            {
                var d = JsonSerializer.Deserialize<Dictionary<string, EmotionEvent[]>>(text, JSONOptions);

                // Add the events to the table
                foreach (var evt in d["emotionEvents"])
                {
                    emotionEvents[evt.name]=evt;
                }
            }
            catch(Exception)
            {
                // Single emotion event
                var evt = JsonSerializer.Deserialize<EmotionEvent>(text, JSONOptions);
                emotionEvents[evt.name]=evt;
            }
        }
    }

}
}