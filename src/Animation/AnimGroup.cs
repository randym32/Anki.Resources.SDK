// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace Anki.Resources.SDK
{

/// <summary>
/// The AnimationGroupItem structure describes the specific animation clip to
/// use.  It may also specify some head movement, with some variability; this
/// is optional.  If enabled, the head is to move to some angle (between the
/// given min and max) while the animation plays.
/// </summary>
public class AnimationGroupItem
{
    /// <summary>
    /// The minimum duration, after this animation has completed, before it can
    /// be used again.  Typically 0.0
    /// </summary>
    public float CooldownTime_Sec {get; set;}

    /// <summary>
    /// The head is to move to random angle greater (or equal) to this.
    /// Only used if UseHeadAngle is true.
    /// </summary>
    public float HeadAngleMin_Deg {get; set;}

    /// <summary>
    /// The head is to move to random angle les than (or equal) to this.
    /// Only used if UseHeadAngle is true.
    /// </summary>
    public float HeadAngleMax_Deg {get; set;}

    /// <summary>
    /// The name of a simple moode or “Default”
    /// </summary>
    public string Mood {get; set;}

    /// <summary>
    /// The name of the animation clip to play.  This clip is defined within one
    /// of the animation binary or JSON files.
    /// </summary>
    public string Name {get; set;}

    /// <summary>
    /// Optional, default is false
    /// </summary>
    public bool UseHeadAngle {get; set;}

    /// <summary>
    ///  Typically 1.0
    /// </summary>
    public float Weight {get; set;}
}

/// <summary>
/// An array of animations to select from to play.
/// </summary>
class AnimationGroup
{
    /// <summary>
    /// An array of animations to select from to play.
    /// </summary>
    public IReadOnlyCollection<AnimationGroupItem> Animations {get;set; }
}

partial class Assets
{
    /// <summary>
    /// The maps the animation groups partial file name to the path
    /// </summary>
    Dictionary<string, string> animationGroupsPathCache;

    /// <summary>
    /// A cache to hold the animation group
    /// </summary>
    readonly Dictionary<string,IReadOnlyCollection<AnimationGroupItem>> animationGroupCache = new Dictionary<string,IReadOnlyCollection<AnimationGroupItem>>();

    /// <summary>
    /// Maps the animation trigger name to animation group filename (without the extension)
    /// The animation name.  Effectively this is the JSON file (without the
    /// “.json” suffix) for the animation.
    /// </summary>
    Dictionary<string, string> animationTriggerName2GroupName;
    /// <summary>
    /// Maps the animation trigger name to animation group filename (without the extension)
    /// The animation name.  Effectively this is the JSON file (without the
    /// “.json” suffix) for the animation.
    /// </summary>
    public IReadOnlyDictionary<string, string> AnimationTriggerName2GroupName => animationTriggerName2GroupName;

    /// <summary>
    /// Provides a list of the animation trigger names.
    /// </summary>
    public IReadOnlyCollection<string> AnimationTriggerNames
    {
        get
        {
            // build the file name cache, if has not been built
            BuildSpriteSequencePaths();
            return animationTriggerName2GroupName.Keys;
        }
    }

    /// <summary>
    /// Looks up the full path given the partial file name
    /// </summary>
    /// <param name="partialFileName">The partial file name, as might be given in one of the configuration files</param>
    /// <returns>null on error, otherwise the full path</returns>
    string AnimationGroupPath(string partialFileName)
    {
        // See if the file name cache has been built
        if (null == animationGroupsPathCache)
        {
            // Construct a cross reference within the animation groups path
            var path = Path.Combine(cozmoResourcesPath, "assets/animationGroups");
            animationGroupsPathCache = Util.BuildNameToRelativePathXref(path);
        }

        // look up the file name for the foo
        return animationGroupsPathCache.TryGetValue(partialFileName, out var fullPath) ? fullPath : null;
    }


    /// <summary>
    /// Looks up the animation group given the trigger name
    /// </summary>
    /// <param name="triggerName">The animation trigger name</param>
    /// <returns>A set of possible animations to display</returns>
    public IReadOnlyCollection<AnimationGroupItem> AnimationGroupForTrigger(string triggerName)
    {
        // See if the description is already loaded
        if (animationGroupCache.TryGetValue(triggerName, out var ret))
            return ret;

        // look up name for the animation group file
        if (!animationTriggerName2GroupName.TryGetValue(triggerName, out var partialName))
            return null;

        // Look up the path to the namtion
        var path = AnimationGroupPath(partialName);

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get the animation group file in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        var item = JsonSerializer.Deserialize<AnimationGroup>(text, JSONOptions);

        // cache it
        if (null != item.Animations)
            animationGroupCache[triggerName] = item.Animations;

        // return it
        return item.Animations;
    }
}
}
