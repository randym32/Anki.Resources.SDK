// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Drawing;
using System.IO;
using System.Text;

namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// display size
    /// </summary>
    uint displayWidth;
    uint displayHeight;

    /// <summary>
    /// Vector's display size
    /// </summary>
    const uint VectorDisplayWidth  = 184;
    const uint VectorDisplayHeight = 96;
    /// <summary>
    /// Cozmo's display size
    /// </summary>
    const uint CozmoDisplayWidth   = 128;
    const uint CozmoDisplayHeight  = 64;

    /// <summary>
    /// Analyzes the sprite sequence for issues
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="spriteSequence">The sprite sequence to analyze</param>
    /// <returns>The errors, if any</returns>
    public static void Analyze(StringBuilder errs, SpriteSequence spriteSequence)
    {
        // Check that the name matches
        var fileBaseName=Path.GetFileName(spriteSequence.fmt);
        fileBaseName = fileBaseName.Substring(0,fileBaseName.LastIndexOf('_'));
        if (spriteSequence.name != fileBaseName)
        {
            errs.AppendLine($"{spriteSequence.name}: sprite images have a different base name ({fileBaseName}) from folder.");
        }

        // Check the number of frames
        if (spriteSequence.Count < 1)
        {
            errs.AppendLine($"{spriteSequence.name}: sprite sequence doesn't contain any names.");
            return;
        }

#if false
        // Check the duration
        if (spriteSequence.length_ms < 5)
        {
            errs.AppendLine($"{spriteSequence.name}: sprite sequence duration is far too short.");
            return errs;
        }
        // Check the framerate
        if (spriteSequence.FramesPerSecond > 15)
        {
            errs.AppendLine($"{spriteSequence.name}: sprite sequence frame rate is too high.");
            return errs;
        }
#endif
    }

    /// <summary>
    /// Analyzes the sprite sequence for issues
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="spriteSequence">The sprite sequence to analyze</param>
    /// <param name="width">The width of the sprite display area</param>
    /// <param name="height">The height of the sprite display area</param>
    /// <returns>The errors, if any</returns>
    static void Analyze(StringBuilder errs, SpriteSequence spriteSequence, uint width, uint height)
    {
        // Scan over each of the images and check the size
        int idx = 0;
        foreach (var img in spriteSequence.Bitmaps)
        {
            // Check the image
            if (img.Width != width || img.Height != height)
            {
                errs.AppendLine($"{spriteSequence.name}[{idx}]: image is wrong size.  Expected {width}x{height} got {img.Width}x{img.Height}");
            }

            // And dispose of the resources
            img.Dispose();
            idx++;
        }
    }



    /// <summary>
    /// Analyzes the independent sprite for issues
    /// </summary>
    /// <param name="spriteName">The name of the independent sprite to examine</param>
    /// <returns>The errors, if any</returns>
    public StringBuilder AnalyzeIndependentSprite(string spriteName)
    {
        var errs = new StringBuilder();
        // Get the sprite
        using var img =IndependentSprite(spriteName);
        if (null == img)
        {
            errs.AppendLine($"{spriteName}: Could not find independent sprite");
            return errs;
        }

        // Analyze it
        //Analyze(errs, spriteName, img, displayWidth, displayHeight);

        // Return the results
        return errs;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="spriteName">The name of the sprite sequence or independent sprite to look up</param>
    /// <param name="width">The width of the sprite display area</param>
    /// <param name="height">The height of the sprite display area</param>
    void AnalyzeSprite(StringBuilder errs, string errKey, string spriteName, uint width, uint height)
    {
        // Look up the sprite sequence
        var seq = SpriteSequence(spriteName);
        if (null != seq)
        {
            // Check the sprite sequence for size issues
            Analyze(errs, seq, width, height);
            return ;
        }

        // Look up the independent sprite
        var img =IndependentSprite(spriteName);
        if (null != img)
        {
            // Check the image size out
            //Analyze(errs, spriteName, img, width, height);
            img.Dispose();
            return ;
        }

        // Error: can't find the image!
        errs.AppendLine($"{errKey}: Could not find sprite sequence or independent sprite with the name '{spriteName}'");
    }
}
}
