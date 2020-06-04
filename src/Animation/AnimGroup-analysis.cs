// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.Collections.Generic;
using System.Text;

namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// A helper to see if the given emotion type is one of the dimensions.
    /// Note: "Default" is not accepted here; use EmotionNameIsValid();
    /// </summary>
    /// <param name="emotionType">The name of an emotion dimension</param>
    /// <returns>true if the given string is a valid emotion type; false otherwise</returns>
    bool EmotionTypeIsValid(string emotionType)
    {
        return emotionRanges.ContainsKey(emotionType);
    }


    /// <summary>
    /// A helper to see if the given emotion type is one of the dimensions.
    /// or is "Default"
    /// </summary>
    /// <param name="emotionName">"Default" or The name of an emotion dimension</param>
    /// <returns>true if the given string is a valid emotion name; false otherwise</returns>
    bool EmotionNameIsValid(string emotionName)
    {
        // Check for default
        if ("Default" == emotionName)
            return true;
        // Check to see if it is the name of a dimension of emotion.
        return EmotionTypeIsValid(emotionName);
    }


    /// <summary>
    /// A helper to see if the given emotion type is one of the dimensions.
    /// or is "Default"
    /// </summary>
    /// <param name="emotionName">"Default" or The name of an emotion dimension</param>
    /// <returns>true if the given string is a valid emotion name; false otherwise</returns>
    bool SimpleMoodNameIsValid(string emotionName)
    {
        // Check to see if it is the name of a dimension of emotion.
        return simpleMoods.ContainsKey(emotionName);
    }

    /// <summary>
    /// Analyzes the animation(s) and support files implied by the given
    /// animation trigger name.
    /// </summary>
    /// <param name="errs">The errors found within the animation tree</param>
    /// <param name="triggerName">The animation trigger name</param>
    public void AnalyzeAnimationTriggerName(StringBuilder errs, string triggerName)
    {
        // look up name for the animation group file
        if (!animationTriggerName2GroupName.TryGetValue(triggerName, out var partialName))
        {
            errs.AppendLine($"the animation trigger name '{triggerName}' is not recognized");
            return ;
        }

        // Check the name
        if (0 != partialName.IndexOf("ag_", StringComparison.Ordinal))
        {
            errs.AppendLine($"animation trigger '{triggerName}': the animation group name '{partialName}' should start with the prefix 'ag_'");
        }

        // Get the group of animations
        var animGroup = AnimationGroupForTrigger(triggerName);

        // Check that there was a group of animations
        if (null == animGroup)
        {
            errs.AppendLine($"'{partialName}': The file does not contain any animations: the 'Animations' key is missing or the file is malformed.");
            return;
        }
        if (0 == animGroup.Count)
        {
            errs.AppendLine($"'{partialName}': The file does not list any animations.");
        }

        // Dig in, and analyze the items
        Analyze(errs, $"{partialName}", animGroup);
    }

    /// <summary>
    /// Analyzes the group of animations
    /// </summary>
    /// <param name="errs">The errors found within the animation tree</param>
    /// <param name="errKey">The label to help identify which animation this
    /// error is referring to</param>
    /// <param name="group">The animation group</param>
    void Analyze(StringBuilder errs, string errKey, IReadOnlyCollection<AnimationGroupItem> group)
    {
        // Check out each animation in the group
        var idx=0;
        foreach (var item in group)
        {
            // Check each item in the structure
            if (item.Weight < 0.0f)
            {
                errs.AppendLine($"{errKey}:{idx} The probability weighting (Weight) is negative ({item.Weight}); it must be >= 0");
            }
            else if (item.Weight == 0.0f)
            {
                errs.AppendLine($"{errKey}:{idx} The probability weighting (Weight) is zero; it should be > 0");
            }
            if (item.CooldownTime_Sec < 0.0f)
            {
                errs.AppendLine($"{errKey}:{idx} The cooldown period (CooldownTime_Sec) is negative ({item.CooldownTime_Sec}); it must be >= 0");
            }
            if (item.HeadAngleMin_Deg < -25.0f || item.HeadAngleMin_Deg > 45.0f)
            {
                errs.AppendLine($"{errKey}:{idx}: The minimum head angle (HeadAngleMin_Deg) is {item.HeadAngleMin_Deg}, outside of the allowed range.  It should be in the range -22.0° to 45.0°, and must be > -25°.");
            }
            if (item.HeadAngleMax_Deg < -25.0f || item.HeadAngleMax_Deg > 45.0f)
            {
                errs.AppendLine($"{errKey}:{idx}: The maximum head angle (HeadAngleMax_Deg) is {item.HeadAngleMax_Deg}, outside of the allowed range.  It should be in the range -22.0° to 45.0°, and must be > -25°.");
            }
            if (item.HeadAngleMax_Deg < item.HeadAngleMin_Deg)
            {
                errs.AppendLine($"{errKey}:{idx}: The maximum head angle (HeadAngleMax_Deg) is less than minimum head angle (HeadAngleMin_Deg)  The maximum should be >= to the minimum");
            }
            if (!SimpleMoodNameIsValid(item.Mood))
            {
                errs.AppendLine($"{errKey}:{idx}: The mood is '{item.Mood}', which is not a recognized mood name.");
            }
            if (null == item.Name || 0 == item.Name.Length)
            {
                errs.AppendLine($"{errKey}:{idx}: The animation name (Name) is missing or empty.");
            }
            else
            {
                // Get the animation for the group
                var anim = AnimationForName(item.Name);

                if (null == anim)
                {
                    errs.AppendLine($"{errKey}:{idx}: The animation clip '{item.Name}' can't be found in the animations.");
                }
                #if false
                else if (anim is CozmoAnim.Keyframes cozmoAnim)
                {
                    // Process the Cozmo animation
                    // TODO
                }
                else if (anim is VectorAnim.Keyframes vectAnim)
                {
                    // Process the Vector animation
                    Analyze(errs, $"clip '{item.Name}'", vectAnim);
                }
                else if (anim is Dictionary<string,object>[] jAnim)
                {
                    // Process the JSON form of a Vector animation
                    // TODO
                }
                else
                {
                    ;
                }
                #endif
            }
            idx++;
        }
    }
}
}
