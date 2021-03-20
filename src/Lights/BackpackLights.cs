// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Blackwood;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Anki.Resources.SDK
{
/// <summary>
/// Convenience structure for the backpack lights animations
/// </summary>
class BackpackLights
{
    /// <summary>
    /// Each color corresponds to each of the 3 lower back pack lights.  Each
    /// color is represented as an array of 4 floats (red, green, blue, and
    /// alpha), in the range 0..1.  Alpha is always 1.
    /// </summary>
    public float[][] offColors {get;set;}

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
    public float[][] onColors {get;set;}

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



partial class Assets
{
    /// <summary>
    /// Maps the animation trigger name to backpack animation filename (without the extension)
    /// </summary>
    Dictionary<string, string> backpackTriggerName2Filename= new Dictionary<string, string>();

    /// <summary>
    /// The maps the backpack light sequence partial file name to the path
    /// </summary>
    Dictionary<string, string> backpackLightsPathCache;

    #region Backpack Lights cache
    /// <summary>
    /// A cache to hold the backpack lights pattern
    /// </summary>
    readonly Dictionary<string, WeakReference> _backpackLightsCache = new Dictionary<string, WeakReference>();

    /// <summary>
    /// Looks up the light animation  entry in the cache
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <returns>null on error, otherwise A sequence of light patterns to display</returns>
    IReadOnlyList<LightsPattern> BackpackLightsCache(string animationName)
    {
        // Is there an entry in the cache for this?
        if (!_backpackLightsCache.TryGetValue(animationName, out var wref))
            return null;
        return (IReadOnlyList<LightsPattern>) wref.Target;
    }

    /// <summary>
    /// Add the item to the cache, if there isn't one already
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <param name="val">A sequence of light patterns to display</param>
    void BackpackLightsCache(string animationName, IReadOnlyList<LightsPattern> val)
    {
        // Is there an entry in the cache for this animation?
        if (_backpackLightsCache.TryGetValue(animationName, out var wref)
           && null != wref.Target)
            return ;

        // Add an item to the cache
        _backpackLightsCache[animationName] = new WeakReference(val);
    }
    #endregion

    /// <summary>
    /// Provides a list of the backpack light animation trigger names.
    /// </summary>
    /// <value>
    /// Provides a list of the backpack light animation trigger names.
    /// </value>
    public IReadOnlyCollection<string> BackpackLightsTriggerNames
    {
        get
        {
            return backpackTriggerName2Filename.Keys;
        }
    }

    /// <summary>
    /// Looks up the full path given the partial file name
    /// </summary>
    /// <param name="partialFileName">The partial file name, as might be given in one of the configuration files</param>
    /// <returns>null on error, otherwise the full path</returns>
    string BackpackLightsPath(string partialFileName)
    {
        // See if the file name cache has been built
        if (null == backpackLightsPathCache)
        {
            // Construct a cross reference within the backpack lights path
            var path = Path.Combine(cozmoResourcesPath, "config/engine/lights/backpackLights");
            backpackLightsPathCache = FS.BuildNameToRelativePathXref(path);
        }

        // look up the file name for the foo
        return backpackLightsPathCache.TryGetValue(partialFileName, out var fullPath) ? fullPath : null;
    }


    /// <summary>
    /// Looks up the pattern to play the backpack lights in given the trigger name
    /// </summary>
    /// <param name="triggerName">The animation trigger name</param>
    /// <returns>A sequence of light patterns to display</returns>
    public IReadOnlyList<LightsPattern> BackpackLightsForTrigger(string triggerName)
    {
        // See if the description is already loaded
        var ret = BackpackLightsCache(triggerName);
        if (null != ret)
            return ret;

        // look up name for the file
        if (!backpackTriggerName2Filename.TryGetValue(triggerName, out var partialName))
            return null;
        var path = BackpackLightsPath(partialName);

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get it in a convenient form
        var lights     = JSONDeserializer.Deserialize<BackpackLights>(text);
        var numLights = AssetsType.Cozmo == AssetsType?4:3;
        var lightKeyFrames = new List<LightFrame>[numLights];

        // Convert it into the final internal form
        for (var idx = 0; idx < numLights; idx++)
        {
            // Start the trigger time with the offset for the light
            uint triggerTime = (uint) lights.offset[idx];

            // Create the on frame first
            var rgba       = lights.onColors[idx];
            // Ignore alpha
            var color      = Color.FromArgb(255, (byte)(255*rgba[0]), (byte)(255 * rgba[1]), (byte)(255 * rgba[2]));
            var lightFrame = new LightFrame(triggerTime, (uint)lights.onPeriod_ms[idx], color);
            var a          = new List<LightFrame>{lightFrame };

            // Compute the trigger time of the off period
            triggerTime += (uint)(lights.onPeriod_ms[idx] + lights.transitionOnPeriod_ms[idx]);

            // Create the off frame next
            rgba         = lights.offColors[idx];
            // Ignore alpha
            color        = Color.FromArgb(255, (byte)(255 * rgba[0]), (byte)(255 * rgba[1]), (byte)(255 * rgba[2]));
            lightFrame   = new LightFrame(triggerTime, (uint)lights.offPeriod_ms[idx], color);
            triggerTime += (uint)(lights.offPeriod_ms[idx] + lights.transitionOffPeriod_ms[idx]);
            var duration = triggerTime - lights.offset[idx];

            a.Add(lightFrame);

            // Put these key frames for the light in
            lightKeyFrames[idx] = a;
        }
        var s         = new LightsPattern();
        s.lightKeyFrames = lightKeyFrames;

        // Convert it back to an array
        var sequences = new LightsPattern[1] { s };

        // cache it
        BackpackLightsCache(triggerName, sequences);

        // return it
        return sequences;
    }
}
}
