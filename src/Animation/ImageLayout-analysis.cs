// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Text;

namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// Analyzes the composite image structure
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="compositeImage">The composite image to check</param>
    public void Analyze(StringBuilder errs, string errKey, CompositeImage compositeImage)
    {
        foreach (var layout in compositeImage.layouts)
        {
            Analyze(errs, $"{errKey}:{layout.layerName}", layout);
        }
    }


    /// <summary>
    /// Analyzes the image layout structure
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="imageLayout">The image layout to check</param>
    void Analyze(StringBuilder errs, string errKey, ImageLayout imageLayout)
    {
        // Check that the layer name is valid
        if (null == imageLayout.layerName || 0 == imageLayout.layerName.Length)
            errs.AppendLine($"{errKey}: the layer name is missing or empty");

        if (null == imageLayout.spriteBoxLayout)
            errs.AppendLine($"{errKey}: the sprite box layout array is required");

        // Check each of the images
        var idx = 0;
        foreach (var spriteBox in imageLayout.spriteBoxLayout)
        {
            Analyze(errs, $"{errKey}:spriteBoxLayout[{idx}]", spriteBox);
            idx++;
        }
    }


    /// <summary>
    /// Analyzes the sprite box structure
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="spriteBox">The sprite box to check</param>
    void Analyze(StringBuilder errs, string errKey, SpriteBox spriteBox)
    {
        // Check that the sprite box name is valid
        if (null == spriteBox.spriteBoxName || 0 == spriteBox.spriteBoxName.Length)
            errs.AppendLine($"{errKey}: the sprite box name is missing or empty");

        // Check the size of the box
        if (spriteBox.width>displayWidth)
            errs.AppendLine($"{errKey}: the sprite width ({spriteBox.width}) is larger than the display ({displayWidth})");
        if (spriteBox.height>displayHeight)
            errs.AppendLine($"{errKey}: the sprite height ({spriteBox.height}) is larger than the display ({displayHeight})");

        // Check the sprite render method
        if (null == spriteBox.spriteRenderMethod)
            errs.AppendLine($"{errKey}: spriteRenderMethod is missing.");
        else if (!("CustomHue" == spriteBox.spriteRenderMethod || "RGBA" == spriteBox.spriteRenderMethod))
            errs.AppendLine($"{errKey}: spriteRenderMethod ({spriteBox.spriteRenderMethod}) must be 'CustomHue' or 'RGBA'.");
    }


    /// <summary>
    /// Analyzes the sprite box keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The sprite box to check</param>
    void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.SpriteBox keyframe)
    {
        // Check that the layer name is valid
        if (null == keyframe.Layer || 0 == keyframe.Layer.Length)
            errs.AppendLine($"{errKey}: the layer name is missing or empty");

        #if false
        // Check the coordinates
        var x= keyframe.XPos;
        var mwidth=displayWidth-1;
        if (x<0 || x>=displayWidth)
            errs.AppendLine($"{errKey}: xPos ({x}) is outside of the range 0..{mwidth}");

        var y= keyframe.YPos;
        var mheight=displayHeight-1;
        if (y<0 || y>=displayHeight)
            errs.AppendLine($"{errKey}: yPos ({y}) is outside of the range 0..{mheight}");

        // Check that the box fits on the display
        var x2=x+keyframe.Width;
        var y2=y+keyframe.Height;
        if (x2>=displayWidth)
            errs.AppendLine($"{errKey}: the xPos ({x}) + width ({keyframe.Width}) is outside of the range 0..{mwidth}");
        if (y2>=displayHeight)
            errs.AppendLine($"{errKey}: yPos ({y}) + height ({keyframe.Height}) is outside of the range 0..{mheight}");
        #else
        // Check the size of the box
        if (keyframe.Width>displayWidth)
            errs.AppendLine($"{errKey}: the sprite width ({keyframe.Width}) is larger than the display ({displayWidth})");
        if (keyframe.Height>=displayHeight)
            errs.AppendLine($"{errKey}: the sprite height ({keyframe.Height}) is larger than the display ({displayHeight})");
        #endif


        // Check the opacity
        var alpha = keyframe.Alpha;
        if (alpha < 0.0f || alpha > 100.0f)
            errs.AppendLine($"{errKey}: alpha ({alpha}) is outside of the range 0..100%");

        // Check the sprite render method
        if (null == keyframe.RenderMethod)
            errs.AppendLine($"{errKey}: renderMethod is missing.");
        else if (!("CustomHue" == keyframe.RenderMethod || "RGBA" == keyframe.RenderMethod))
            errs.AppendLine($"{errKey}: renderMethod ({keyframe.RenderMethod}) must be 'CustomHue' or 'RGBA'.");

        // Check the reference to independent sprites and sprite sequences
        var assetName = keyframe.AssetName;
        if (  null == assetName)
            errs.AppendLine($"{errKey}: The assetName is missing");

        if ( "clear_sprite_box" != assetName)
            AnalyzeSprite(errs, errKey, assetName, keyframe.Width, keyframe.Height);
    }
}
}
