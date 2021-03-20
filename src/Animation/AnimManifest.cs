// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Blackwood;
using System.Collections.Generic;
using System.IO;

namespace Anki.Resources.SDK
{

/// <summary>
/// Animation manifest entry, giving the name of the 
/// </summary>
class AnimationLength
{
    /// <summary>
    /// The name of the animation clip within one of the animation binary files.
    /// </summary>
    public string name {get;set;}

    /// <summary>
    /// The duration of the animation (when played)
    /// </summary>
    public int length_ms {get;set;}

    internal AnimationLength()
    {
    }
    /// <summary>
    /// A constructor for the animation
    /// </summary>
    /// <param name="name">The name of the animation clip within one of the animation binary files.</param>
    /// <param name="length_ms">The duration of the animation (when played)</param>
    internal AnimationLength(string name, int length_ms)
    {
        this.name = name;
        this.length_ms = length_ms;
    }
}


partial class Assets
{
    #region load animation manifest
    /// <summary>
    /// maps the animation clip name to the duration (in ms)
    /// </summary>
    readonly Dictionary<string, int> animation2Duration_ms = new Dictionary<string, int>();

    /// <summary>
    /// Loads the information from the animation manifest file
    /// </summary>
    /// <param name="assetsPath">Path to the assets folder</param>
    void LoadAnimationManifest(string assetsPath)
    {
        // get the path to the animation file
        var path = Path.Combine(assetsPath, "anim_manifest.json");
        // Skip it if the file doesn't exist
        if (!File.Exists(path))
            return;

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get it in a convenient form
        var items = JSONDeserializer.Deserialize<AnimationLength[]>(text);

        // internalize each of the items
        foreach (var item in items)
            animation2Duration_ms[item.name] = item.length_ms;
    }
    #endregion

    /// <summary>
    /// The duration of the animation (when played)
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <returns>0 on error, otherwise the duration (ms)</returns>
    public int AnimationDuration(string animationName)
    {
        // look up the duration
        if (animation2Duration_ms.TryGetValue(animationName, out var length))
            return length;
        return 0;
    }
}
}
