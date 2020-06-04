// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.Text;

namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// Analyzes the image map structure
    /// </summary>
    /// <param name="errs">The errors found within the image maps</param>
    /// <param name="errKey">The label to help identify which image map this
    /// error is referring to</param>
    public void AnalyzeMaps(StringBuilder errs, string errKey)
    {
        // Checks each of the animation names
        foreach (var triggerName in imageMapTriggerName2Filename.Keys)
        {
            // Check that each image map key is in the layouts
            if (!imageLayoutTriggerName2Filename.ContainsKey(triggerName))
            {
                errs.AppendLine($"{errKey}: no corresponding layout for the name '{triggerName}'");
                continue;
            }

            // Open the composite image and analyze it
            var ci = CompositeImageForTrigger(triggerName);
            foreach (var map in ci.maps)
            {
                // Check that each of the map's layer has a defined layer
                var layout = ci.Layout(map.layerName);
                if (null == layout)
                {
                    errs.AppendLine($"{errKey}:{triggerName}: no corresponding layout layer for the name '{map.layerName}'");
                    continue;
                }

                // Check the image mappings out
                Analyze(errs, $"{errKey}:{triggerName}", map, layout);
            }
        }
    }


    /// <summary>
    /// Analyzes the image map structure
    /// </summary>
    /// <param name="errs">The errors found within the image maps</param>
    /// <param name="errKey">The label to help identify which image map this
    /// error is referring to</param>
    /// <param name="imageMap">The image map to check</param>
    void Analyze(StringBuilder errs, string errKey, ImageMap imageMap, IReadOnlyCollection<SpriteBox> layout)
    {
        // Check that the layer name is valid
        if (null == imageMap.layerName || 0 == imageMap.layerName.Length)
            errs.AppendLine($"{errKey}: the layer name is missing or empty");

        if (null == imageMap.images)
            errs.AppendLine($"{errKey}: the images array is required");

        // Check that each of the map's layer has a defined layer
        if (null == layout)
        {
            errs.AppendLine($"{errKey}: no corresponding layout layer for the name '{imageMap.layerName}'");
            return;
        }

        // Check each of the images
        var idx = 0;
        foreach (var spriteMapBox in imageMap.images)
        {
            Analyze(errs, $"{errKey}:images[{idx}]", spriteMapBox, layout);
            idx++;
        }
    }


    /// <summary>
    /// Analyzes the sprite map box structure
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="spriteMapBox">The sprite map box to check</param>
    void Analyze(StringBuilder errs, string errKey, SpriteMapBox spriteMapBox, IReadOnlyCollection<SpriteBox> layout)
    {
        SpriteBox spriteBox = null;
        // Check that the sprite box name is valid
        if (null == spriteMapBox.spriteBoxName || 0 == spriteMapBox.spriteBoxName.Length)
            errs.AppendLine($"{errKey}: the sprite box name is missing or empty");
        else
        {
            // Look up the box that it goes with
            foreach (var s in layout)
                if (spriteMapBox.spriteBoxName == s.spriteBoxName)
                {
                    spriteBox = s;
                    break;
                }
            // Check to see that the sprite box is in the layout
            if (null == spriteBox)
                errs.AppendLine($"{errKey}: the sprite box ({spriteMapBox.spriteBoxName}) isn't defined in the layer");
        }

        // Check the reference to independent sprites and sprite sequences
        if (  null == spriteMapBox.spriteName || 0 == spriteMapBox.spriteName.Length)
        {
            errs.AppendLine($"{errKey}: The spriteName is missing");
            return;
        }
        
        // Get the sprite sequence
        var spriteSeq = SpriteSequence(spriteMapBox.spriteName);
        if (null == spriteSeq)
        {
            errs.AppendLine($"{errKey}: The sprite sequence {spriteMapBox.spriteName} could not be found.");
            return;
        }
        if (spriteMapBox.spriteName != spriteSeq.name)
        {
            errs.AppendLine($"{errKey}: The case of the sprite sequence name {spriteMapBox.spriteName} doesnt match that used in the assets folder {spriteSeq.name}");
        }
        // Check the size of the sprites
        if (null != spriteBox)
            Analyze(errs, spriteSeq, (uint) spriteBox.width, (uint) spriteBox.height);
    }
}
}
