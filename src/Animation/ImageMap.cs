// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System.Collections.Generic;
using System.IO;

namespace Anki.Resources.SDK
{
/// <summary>
/// An image map describes which images and sprite sequences to display.
/// </summary>
class ImageMap
{
    /// <summary>
    /// An array of sprite boxes for showing sprite sequences.
    /// </summary>
    public IReadOnlyCollection<SpriteMapBox> images {get;set;}

    /// <summary>
    ///  The name of the layer.  The animation engine may use this to select
    ///  the procedure(s) in charge managing the layer and sprite boxes.
    /// </summary>
    public string layerName {get;set;}
}


/// <summary>
/// The name of the image or sprite sequence to display in the sprite box.
/// </summary>
class SpriteMapBox
{
    /// <summary>
    /// The name of the sprite box.
    /// </summary>
    public string spriteBoxName {get; set;}

    /// <summary>
    /// The name of a sprite sequence.
    /// </summary>
    public string spriteName {get; set;}
}


partial class Assets
{
    /// <summary>
    /// Maps the trigger name to the image map filename
    /// </summary>
    Dictionary<string, string> imageMapTriggerName2Filename = new Dictionary<string, string>();

    /// <summary>
    /// The maps the image maps partial file name to the path
    /// </summary>
    Dictionary<string, string> imageMapPathCache;

#if false
    /// <summary>
    /// Provides a list of the image map trigger names.
    /// </summary>
    IReadOnlyCollection<string> ImageMapTriggerNames
    {
        get
        {
            return imageMapTriggerName2Filename.Keys;
        }
    }
    #endif


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
            imageMapPathCache = Util.BuildNameToRelativePathXref(path);
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

#if false
    /// <summary>
    /// Looks up the path to the image map given the trigger name
    /// </summary>
    /// <param name="triggerName">The image map trigger name</param>
    /// <returns>null on error, otherwise The relative path to the image layout </returns>
    string ImageMapPathForTrigger(string triggerName)
    {
        // Get the full path
        var path = ImageMapFullPathForTrigger(triggerName);
        if (null == path)
            return null;

        // Remove the base path
        return Util.RemoveBasePath(cozmoResourcesPath, path);
    }
#endif
}
}
