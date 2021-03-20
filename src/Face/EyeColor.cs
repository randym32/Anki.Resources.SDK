// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
// read eye_color_config.json here

using Blackwood;
using System.Collections.Generic;
using System.IO;

namespace Anki.Resources.SDK
{

/// <summary>
/// A color template for the eye colors.
/// </summary>
public class HSColor
{
    /// <summary>
    /// The hue value, in degrees.
    /// </summary>
    /// <value>
    /// The hue value, in degrees.
    /// </value>
    public float Hue;

    /// <summary>
    /// The saturation value.
    /// </summary>
    /// <value>
    /// The saturation value.
    /// </value>
    public float Saturation;
}

partial class Assets
{
    /// <summary>
    /// This maps a colors fanciful appellation to the hue and saturation to
    /// employ when drawing with it.
    /// </summary>
    IReadOnlyDictionary<string, HSColor> eyeColors;

    /// <summary>
    /// null on error, otherwise the eye color configurations
    /// </summary>
    /// <value>The eye color configurations.</value>
    public IReadOnlyDictionary<string, HSColor> EyeColors
    {
        get
        {
            // Return cached value
            if (null != eyeColors)
                return eyeColors;

            // Construct a cross reference within the colors
            var path = Path.Combine(cozmoResourcesPath, "config/engine/eye_color_config.json");

            // Get it in a convenient form

            // Load the layouts file
            // Get the text for the file
            var text = File.ReadAllText(path);
            return eyeColors = JSONDeserializer.Deserialize<IReadOnlyDictionary<string, HSColor>>(text);
        }
    }
}
}
