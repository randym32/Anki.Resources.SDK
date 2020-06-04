// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Drawing;
using RCM;
using System;

namespace Anki.Resources.SDK
{

class VisionModeSetting
{
    public string mode {get;set; }
    public int low  {get;set; }
    public int med  {get;set; }
    public int hide {get;set; }
    public int standard  {get;set; }
    public int relativeCost {get;set; }
}

partial class Assets
{
    /// <summary>
    /// The configuration settings for the vision subsystem
    /// </summary>
    Dictionary<string, object> visionConfig;

    /// <summary>
    /// 
    /// </summary>
    readonly Dictionary<string, VisionModeSetting> visionScheduleConfig = new Dictionary<string, VisionModeSetting>();

    /// <summary>
    /// A catalog of the tflite models
    /// </summary>
    Dictionary<string,string> tfliteFiles;

    /// <summary>
    /// A catalog of the openCV classifiers
    /// </summary>
    Dictionary<string,string> openCVClassifiers;


    /// <summary>
    /// 
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
            foreach (var x in tmp["VisionModeSettings"])
            {
                visionScheduleConfig[x.mode] = x;
            }
        }

        // Scoop up the tflite files
        tfliteFiles = Util.BuildNameToRelativePathXref(Path.Combine(cozmoResourcesPath,"config/engine/vision", "tflite"));
        openCVClassifiers     = Util.BuildNameToRelativePathXref(Path.Combine(cozmoResourcesPath,"config/engine/vision", "yaml"));
        // Load labels
        //
    }
}
}
