// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Anki.Resources.SDK
{
/// <summary>
/// A composite image defines layers on the display with rectangular areas
/// where images and sprite sequences will be drawn.
/// </summary>
public partial class CompositeImage
{
    /// <summary>
    /// The layout defines layers with rectangular areas where images and
    /// sprite sequences will be drawn.
    /// </summary>
    public readonly ImageLayout[] Layouts;

    /// <summary>
    /// An image map describes which images and sprite sequences to display.
    /// </summary>
    public readonly ImageMap[] Maps;

    /// <summary>
    /// Creates the composite image map structure
    /// </summary>
    /// <param name="layoutPath">The path to the layout file</param>
    /// <param name="mapPath">The path to the image map file</param>
    internal CompositeImage(string layoutPath, string mapPath)
    {
        // Get it in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues    = true
            };

        // Load the layouts file
        // Get the text for the file
        var text = File.ReadAllText(layoutPath);
        Layouts = JsonSerializer.Deserialize<ImageLayout[]>(text, JSONOptions);
        
        // Load image map file (optional)
        if (null != mapPath)
        {
            // Get the text for the file
            text = File.ReadAllText(mapPath);

            // Get it in a convenient form
            Maps = JsonSerializer.Deserialize<ImageMap[]>(text, JSONOptions);
        }

        // We would build a cross reference dictionary if was worth it...
    }


    /// <summary>
    /// Enumerates the layers in the composite image
    /// </summary>
    public IEnumerable<string> LayerNames
    {
        get
        {
            foreach (var layout in Layouts)
                 yield return layout.layerName;
        }
    }

    /// <summary>
    /// Returns the layout for the given layer name
    /// </summary>
    /// <param name="layerName">The name of the layer to fetch</param>
    /// <returns>null if the layer does not exist (or is undefined), otherwise
    /// the list of sprite boxes defining the layout</returns>
    public IReadOnlyCollection<SpriteBox> Layout(string layerName)
    {
        // Scan over the layers (it's not worth it build a dictonary)
        foreach (var layer in Layouts)
            if (layerName == layer.layerName)
                return layer.spriteBoxLayout;
        return null;
    }

    /// <summary>
    /// Returns a mapping from sprite box name to the sprite name to use, for
    /// the given layer
    /// </summary>
    /// <param name="layerName">The name of the layer to fetch</param>
    /// <returns>null if the layer does not exist (or is undefined), otherwise
    /// a dictionary mapping the sprite box name to the sprite name to employ.
    /// </returns>
    public Dictionary<string,string> ImageMap(string layerName)
    {
        // Check to see if no maps were defined
        if (null == Maps)
            return null;

        // Create a dictionary mapping the sprite box name to sprite name
        // First, find the mapping for the layer name
        foreach (var m in Maps)
        {
            // Skip irrelevant layers
            if (layerName != m.layerName)
                continue;

            // Create the dictionary now
            var ret = new Dictionary<string,string>();
            foreach (var im in m.images)
                ret[im.spriteBoxName] = im.spriteName;
            return ret;
        }

        // The layer wasn't found
        return null;
    }
}

partial class Assets
{
    /// <summary>
    /// A cache to reuse the image layouts
    /// </summary>
    readonly Dictionary<string, WeakReference> compositeImageCache = new Dictionary<string, WeakReference>();

    /// <summary>
    /// Looks up the composite image for the given trigger name
    /// </summary>
    /// <param name="triggerName">The image layout trigger name</param>
    /// <returns>The composite image</returns>
    public CompositeImage CompositeImageForTrigger(string triggerName)
    {
        // See if the description is already loaded
        if (compositeImageCache.TryGetValue(triggerName, out var wref))
        {
            var ret= (CompositeImage) wref.Target;
            if (null != ret)
                return ret;
        }


        // look up name for the file
        var layoutPath = ImageLayoutFullPathForTrigger(triggerName);
        if (null == layoutPath)
            return null;
        // Get the full path for the map
        var mapPath = ImageMapFullPathForTrigger(triggerName);

        // Create the object
        var image = new CompositeImage(layoutPath, mapPath);

        // Add an item to the cache
        compositeImageCache[triggerName] = new WeakReference(image);

        // return it
        return image;
    }
}
}
