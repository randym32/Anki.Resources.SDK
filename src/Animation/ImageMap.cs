// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Blackwood;
using System.Collections.Generic;
using System.IO;

namespace Anki.Resources.SDK
{
/// <summary>
/// An image map describes which images and sprite sequences to display.
/// </summary>
public class ImageMap
{
    /// <summary>
    /// An array of sprite boxes for showing sprite sequences.
    /// </summary>
    /// <value>
    /// An array of sprite boxes for showing sprite sequences.
    /// </value>
    public IReadOnlyCollection<SpriteMapBox> images {get;set;}

    /// <summary>
    ///  The name of the layer.  The animation engine may use this to select
    ///  the procedure(s) in charge managing the layer and sprite boxes.
    /// </summary>
    /// <value>
    ///  The name of the layer.
    /// </value>
    public string layerName {get;set;}
}


/// <summary>
/// The name of the image or sprite sequence to display in the sprite box.
/// </summary>
public class SpriteMapBox
{
    /// <summary>
    /// The name of the sprite box.
    /// </summary>
    /// <value>
    /// The name of the sprite box.
    /// </value>
    public string spriteBoxName {get; set;}

    /// <summary>
    /// The name of a sprite sequence.
    /// </summary>
    /// <value>
    /// The name of a sprite sequence.
    /// </value>
    public string spriteName {get; set;}
}


partial class Assets
{
    /// <summary>
    /// Maps the trigger name to the image map filename.
    /// </summary>
    Dictionary<string, string> imageMapTriggerName2Filename = new Dictionary<string, string>();

    /// <summary>
    /// Maps the trigger name to the image map filename.
    /// </summary>
    /// <value>
    /// Maps the trigger name to the image map filename.
    /// </value>
    public IReadOnlyDictionary<string, string> ImageMapTriggerName2Filename => imageMapTriggerName2Filename;

    /// <summary>
    /// The maps the image maps partial file name to the path
    /// </summary>
    Dictionary<string, string> imageMapPathCache;


    /// <summary>
    /// Looks up the full path given the partial file name
    /// </summary>
    /// <param name="partialFileName">The partial file name, as might be given in one of the configuration files</param>
    /// <returns>null on error, otherwise the full path</returns>
    string ImageMapPath(string partialFileName)
    {
        // See if the file name cache has been built
        if (null == imageMapPathCache)
        {
            // Construct a cross reference within the cube lights path
            var path = Path.Combine(cozmoResourcesPath, "assets/compositeImageResources/imageMaps");
            imageMapPathCache = FS.BuildNameToRelativePathXref(path);
        }

        // look up the file name for the foo
        return imageMapPathCache.TryGetValue(partialFileName, out var fullPath) ? fullPath : null;
    }


    /// <summary>
    /// Looks up the image map in given the trigger name
    /// </summary>
    /// <param name="triggerName">The animation trigger name</param>
    /// <returns>null on error, otherwise The full path to the image layout </returns>
    string ImageMapFullPathForTrigger(string triggerName)
    {
        // look up name for the file
        if (!imageMapTriggerName2Filename.TryGetValue(triggerName, out var partialName))
            return null;
        return ImageMapPath(partialName);
    }
}
}
