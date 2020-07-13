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

/// <summary>
/// Convenience structure for the cube lights animations
/// </summary>
class CubeLights
{
    /// <summary>
    /// Each color corresponds to each of the 3 lower back pack lights.  Each
    /// color is represented as an array of 4 floats (red, green, blue, and
    /// alpha), in the range 0..1.  Alpha is always 1.
    /// </summary>
    public int[][] offColors {get;set;}

    /// <summary>
    /// The “off” duration for each of the 3 back pack lights.  This is the
    /// duration to show each cube light in its corresponding “off” color (in
    /// offColors).
    /// </summary>
    public int[] offPeriod_ms {get;set;}

    /// <summary>
    /// This holds how many milliseconds each light’s clock is advanced from the
    /// clock driving the animation.  This is used to stagger each lights
    /// progression through the animation sequence.
    /// </summary>
    public int[] offset {get;set;}

    /// <summary>
    /// Each color corresponds to each of the 3 lower back pack lights.   Each
    /// color is represented as an array of 4 floats (red, green, blue, and
    /// alpha), in the range 0..1.  Alpha is always 1.
    /// </summary>
    public int[][] onColors {get;set;}

    /// <summary>
    /// The “on” duration for each of the3 lights.  This is the duration
    /// to show each light in its corresponding “on” color (in onColors).
    /// </summary>
    public int[] onPeriod_ms {get;set;}

    /// <summary>
    /// The time to transition from the on color to the off color.
    /// </summary>
    public int[] transitionOffPeriod_ms {get;set;}

    /// <summary>
    /// The time to transition from the off color to the on color.
    /// </summary>
    public int[] transitionOnPeriod_ms {get;set;}
}

/// <summary>
/// This describes one of the light sequences
/// </summary>
class CubeLightSequence
{
    /// <summary>
    /// Default is true. Optional.
    /// </summary>
    public bool canBeOverridden {get;set;}

    /// <summary>
    /// if zero, do this until told to stop, otherwise perform the animation
    /// for this period and proceed to next structure or stop.
    /// </summary>
    public float duration_ms {get;set;}

    /// <summary>
    /// A structure describing the light patterns.
    /// </summary>
    public CubeLights pattern {get;set;}

    /// <summary>
    /// A text name that is associated with this structure. Optional.
    /// </summary>
    public string patternDebugName {get;set;}
}


partial class Assets
{
    /// <summary>
    /// Maps the animation trigger name to cube lights animation filename (without the extension)
    /// </summary>
    Dictionary<string, string> cubeTriggerName2Filename;

    /// <summary>
    /// The maps the cube light sequence partial file name to the path
    /// </summary>
    Dictionary<string, string> cubeLightsPathCache;

    #region Cube Lights cache
    /// <summary>
    /// A cache to hold the cube lights pattern
    /// </summary>
    readonly Dictionary<string, WeakReference> _cubeLightsCache = new Dictionary<string, WeakReference>();

    /// <summary>
    /// Looks up the light animation entry in the cache
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <returns>null on error, otherwise A sequence of light patterns to display</returns>
    IReadOnlyList<LightsPattern> CubeLightsCache(string animationName)
    {
        // Is there an entry in the cache for this?
        if (!_cubeLightsCache.TryGetValue(animationName, out var wref))
            return null;
        return (IReadOnlyList<LightsPattern>) wref.Target;
    }

    /// <summary>
    /// Add the item to the cache, if there isn't one already
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <param name="val">A sequence of light patterns to display</param>
    void CubeLightsCache(string animationName, IReadOnlyList<LightsPattern> val)
    {
        // Is there an entry in the cache for this animation?
        if (_cubeLightsCache.TryGetValue(animationName, out var wref)
           && null != wref.Target)
            return ;

        // Add an item to the cache
        _cubeLightsCache[animationName] = new WeakReference(val);
    }
    #endregion

    /// <summary>
    /// Provides a list of the cube light animation trigger names.
    /// </summary>
    /// <value>
    /// Provides a list of the cube light animation trigger names.
    /// </value>
    public IReadOnlyCollection<string> CubeLightsTriggerNames
    {
        get
        {
            return cubeTriggerName2Filename.Keys;
        }
    }


    /// <summary>
    /// Looks up the full path given the partial file name
    /// </summary>
    /// <param name="partialFileName">The partial file name, as might be given in one of the configuration files</param>
    /// <returns>null on error, otherwise the full path</returns>
    string CubeLightsPath(string partialFileName)
    {
        // See if the file name cache has been built
        if (null == cubeLightsPathCache)
        {
            // Construct a cross reference within the cube lights path
            var path = Path.Combine(cozmoResourcesPath, "config/engine/lights/cubeLights");
            cubeLightsPathCache = Util.BuildNameToRelativePathXref(path);
        }

        // look up the file name for the foo
        return cubeLightsPathCache.TryGetValue(partialFileName, out var fullPath) ? fullPath : null;
    }


    /// <summary>
    /// Looks up the pattern to play the cube lights in given the trigger name
    /// </summary>
    /// <param name="triggerName">The animation trigger name</param>
    /// <returns>A sequence of light patterns to display</returns>
    public IReadOnlyList<LightsPattern> CubeLightsForTrigger(string triggerName)
    {
        // See if the description is already loaded
        var ret = CubeLightsCache(triggerName);
        if (null != ret)
            return (IReadOnlyList<LightsPattern>) ret;

        // look up name for the file
        if (!cubeTriggerName2Filename.TryGetValue(triggerName, out var partialName))
            return null;
        var path = CubeLightsPath(partialName);

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get it in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        // Decode the JSON file. The format varies a little between Cozmo and Vector
        CubeLightSequence[] topLevel;
        if (AssetsType.Vector == AssetsType)
            topLevel = JsonSerializer.Deserialize<CubeLightSequence[]>(text, JSONOptions);
        else
        {
            // Cozmo has more level of indirection
            var tmp = JsonSerializer.Deserialize<Dictionary<string,CubeLightSequence[]>>(text, JSONOptions);
            // TODO: check the number of keys
            topLevel = tmp[partialName];
        }

        // Convert it into the final internal form
        var sequences = new List<LightsPattern>();
        foreach (var t in topLevel)
        {
            // Create something to hold info about this part of the sequence
            var s = new LightsPattern();
            s.name            = t.patternDebugName;
            s.canBeOverridden = t.canBeOverridden;
            s.duration_ms     = t.duration_ms;
            var lightKeyFrames = new List<LightFrame>[4];
            s.lightKeyFrames = lightKeyFrames;
            sequences.Add(s);

            // Convert the patterns for the lights
            var lights = t.pattern;
            for (var idx = 0; idx < 4; idx++)
            {
                // Start the trigger time with the offset for the light
                uint triggerTime = (uint)lights.offset[idx];

                // Create the on frame first
                var rgba       = lights.onColors[idx];
                // Ignore alpha
                var color      = Color.FromArgb(255, rgba[0], rgba[1], rgba[2]);
                var lightFrame = new LightFrame(triggerTime, (uint)lights.onPeriod_ms[idx], color);
                var a = new List<LightFrame>{lightFrame};

                // Compute the trigger time of the off period
                triggerTime += (uint)(lights.onPeriod_ms[idx] + lights.transitionOnPeriod_ms[idx]);

                // Create the off frame next
                rgba         = lights.offColors[idx];
                // Ignore alpha
                color        = Color.FromArgb(255, rgba[0], rgba[1], rgba[2]);
                lightFrame   = new LightFrame(triggerTime, (uint)lights.offPeriod_ms[idx], color);
                triggerTime += (uint)(lights.offPeriod_ms[idx] + lights.transitionOffPeriod_ms[idx]);
                a.Add(lightFrame);

                var duration = triggerTime - lights.offset[idx];

                // Put these key frames for the light in
                lightKeyFrames[idx] = a;
            }
        }

        var seq = sequences.ToArray();

        // cache it
        CubeLightsCache(triggerName, seq);

        // return it
        return seq;
    }
}
}
