// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RCM;

namespace Anki.Resources.SDK
{
/// <summary>
/// This is used to as part of the vision scheduler configuration.
/// It specifies the frequency to run a given image processing step, for each
/// of the vision processing subsystems modes. 1 means "runs every frame,"
/// 4 every fourth frame, and so on.  
/// </summary>
public class VisionModeSetting
{
    /// <summary>
    /// The name of the image processing step.
    /// </summary>
    public string mode {get;set; }

    /// <summary>
    /// When in low "mode" run the image processing step every n frames.
    /// This value must be a power of two.
    /// </summary>
    public uint low  {get;set; }

    /// <summary>
    /// When in medium "mode" run the image processing step every n frames.
    /// This value must be a power of two.
    /// </summary>
    public uint med  {get;set; }

    /// <summary>
    /// When in high "mode" run the image processing step every n frames.  This
    /// value must be a power of two.
    /// </summary>
    public uint high {get;set; }

    /// <summary>
    /// When in medium "mode" run the image processing step every n frames.
    /// This value must be a power of two.
    /// </summary>
    public uint standard  {get;set; }

    /// <summary>
    /// A "heuristic weighting to drive separation of heavy-weight tasks between
    /// frames where 1 should indicate our lowest cost process e.g. "Markers" is
    /// ~16x as resource intensive as "CheckingQuality"
    /// </summary>
    public uint relativeCost {get;set; }
}

partial class Assets
{
    /// <summary>
    /// The configuration settings for the vision subsystem
    /// </summary>
    Dictionary<string, object> visionConfig;

    /// <summary>
    /// The vision scheduler configuration specifies the frequency to run a
    /// given image processing step, for each of the vision processing
    /// subsystems modes.
    /// </summary>
    public readonly IReadOnlyDictionary<string, VisionModeSetting> VisionScheduleConfig = new Dictionary<string, VisionModeSetting>();

    /// <summary>
    /// Loads the vision subsystem configuredion
    /// </summary>
    void LoadVision()
    {
        // Get the text for the file
        var text = File.ReadAllText(Path.Combine(cozmoResourcesPath, "config/engine/vision_config.json"));

        // Get it in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        visionConfig = Util.ToDict(JsonSerializer.Deserialize<Dictionary<string,object>>(text, JSONOptions));

        // Load the vision schedule mediator
        var vpath= Path.Combine(cozmoResourcesPath, "config/engine/visionScheduleMediator_config.json");
        if (File.Exists(vpath))
        {
            text = File.ReadAllText(vpath);
            var tmp  = JsonSerializer.Deserialize<Dictionary<string,VisionModeSetting[]>>(text, JSONOptions);
            // Rearrange so that we can look up by item
            var config = (Dictionary<string, VisionModeSetting>) VisionScheduleConfig;
            foreach (var x in tmp["VisionModeSettings"])
            {
                config[x.mode] = x;
            }
        }

        // Load the information about the classifiers
        LoadClassifierInfo();
    }
}
}
