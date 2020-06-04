// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Anki.Resources.SDK
{
/// <summary>
/// Maps a trigger name to animation name
/// </summary>
class CTFM
{
    /// <summary>
    /// The animation name.  Effectively this is the JSON file (without the
    /// “.json” suffix) for the animation.
    /// </summary>
    public string AnimName {get;set;}

    /// <summary>
    /// This is the animation trigger name to match when looking up the animation.
    /// </summary>
    public string CladEvent {get;set;}
}

/// <summary>
/// Maps a trigger name to animation name
/// </summary>
class CozmoCTFM
{
    public CTFM[] Pairs {get; set;}
}

/// <summary>
/// Maps a trigger name to display layout JSON file
/// </summary>
class CTLayout
{
    /// <summary>
    /// This is the animation trigger name to match when looking up the animation.
    /// </summary>
    public string CladEvent {get;set;}


    /// <summary>
    /// The name of the JSON file (without the “.json” suffix) for the display layout.
    /// </summary>
    public string LayoutName {get;set;}
}


/// <summary>
/// Maps a trigger name to the image map JSON file
/// </summary>
class CTMap
{
    /// <summary>
    /// This is the animation trigger name to match when looking up the animation.
    /// </summary>
    public string CladEvent {get;set;}


    /// <summary>
    /// The name of the JSON file (without the “.json” suffix) for the image map.
    /// </summary>
    public string MapName {get;set; }
}

partial class Assets
{
    /// <summary>
    /// Loads a mapping of trigger name to the JSON file name
    /// </summary>
    /// <param name="path">The path to the JSON mapping file</param>
    /// <returns>A dictionary mapping the trigger name to the JSON file name</returns>
    Dictionary<string, string> LoadCladToAnimMap(string path)
    {
        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get it in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        CTFM[] items;
        if (AssetsType.Cozmo == AssetsType)
        {
            // Load Cozmo's particular format
            var pairs = JsonSerializer.Deserialize<CozmoCTFM>(text, JSONOptions);
            items = pairs.Pairs;
        }
        else
            items = JsonSerializer.Deserialize<CTFM[]>(text, JSONOptions);

        // Create a more useful mapping
        var ret = new Dictionary<string, string>();
        foreach (var item in items)
            ret[item.CladEvent] = item.AnimName;
        return ret;
    }


    /// <summary>
    /// Loads a mapping of trigger name to the JSON file name
    /// </summary>
    /// <param name="path">The path to the JSON mapping file</param>
    /// <returns>A dictionary mapping the trigger name to the JSON file name</returns>
    static Dictionary<string, string> LoadCladToLayoutMap(string path)
    {
        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get it in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        var items = JsonSerializer.Deserialize<CTLayout[]>(text, JSONOptions);

        // Create a more useful mapping
        var ret = new Dictionary<string, string>();
        foreach (var item in items)
            ret[item.CladEvent] = item.LayoutName;
        return ret;
    }


    /// <summary>
    /// Loads a mapping of trigger name to the JSON file name
    /// </summary>
    /// <param name="path">The path to the JSON mapping file</param>
    /// <returns>A dictionary mapping the trigger name to the JSON file name</returns>
    static Dictionary<string, string> LoadCladToMapMap(string path)
    {
        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get it in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        var items = JsonSerializer.Deserialize<CTMap[]>(text, JSONOptions);

        // Create a more useful mapping
        var ret = new Dictionary<string, string>();
        foreach (var item in items)
            ret[item.CladEvent] = item.MapName;
        return ret;
    }


    /// <summary>
    /// Loads the trigger name (aka CLAD) to file mappings
    /// </summary>
    void LoadCladToFileMaps()
    {
        // Load the trigger maps
        if (AssetsType.Vector == AssetsType)
        {
            var CTFMpath = Path.Combine(cozmoResourcesPath, "assets/cladToFileMaps");
            animationTriggerName2GroupName  = LoadCladToAnimMap(Path.Combine(CTFMpath, "AnimationTriggerMap.json"));
            backpackTriggerName2Filename    = LoadCladToAnimMap(Path.Combine(CTFMpath, "BackpackAnimationTriggerMap.json"));
            cubeTriggerName2Filename        = LoadCladToAnimMap(Path.Combine(CTFMpath, "CubeAnimationTriggerMap.json"));


            //layout stuff
            imageLayoutTriggerName2Filename = LoadCladToLayoutMap(Path.Combine(CTFMpath, "CompositeImageLayoutMap.json"));
            imageMapTriggerName2Filename    = LoadCladToMapMap   (Path.Combine(CTFMpath, "CompositeImageMapMap.json"));
            // There is also a CompositeImageMotionSequenceMap.json but it is empty
        }
        else
        {
            // Cozmo has a different location
            animationTriggerName2GroupName  = LoadCladToAnimMap(Path.Combine(cozmoResourcesPath, "assets/animationGroupMaps", "AnimationTriggerMap.json"));
            cubeTriggerName2Filename        = LoadCladToAnimMap(Path.Combine(cozmoResourcesPath, "assets/cubeAnimationGroupMaps", "CubeAnimationTriggerMap.json"));
        }
    }
}
}
