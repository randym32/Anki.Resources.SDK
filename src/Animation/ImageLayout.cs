// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Blackwood;
using RCM;
using System.Collections.Generic;
using System.IO;

namespace Anki.Resources.SDK
{

/// <summary>
/// A screen layout defines rectangular areas on the display where images and
/// sprite sequences will be drawn.
/// </summary>
public class ImageLayout
{
    /// <summary>
    /// An array of sprite boxes for showing icons and other images.
    /// </summary>
    /// <value>
    /// An array of sprite boxes for showing icons and other images.
    /// </value>
    public IReadOnlyCollection<SpriteBox> spriteBoxLayout {get;set; }

    /// <summary>
    /// The name of the layer.  (This name is also defined in vic-anim and
    /// libcozmo_engine)  The animation engine may use this to select the
    /// procedure(s) in charge managing the layer and sprite boxes.
    /// </summary>
    /// <value>
    /// The name of the layer.
    /// </value>
    public string layerName {get;set; }

    /// <summary>
    /// Looks up the sprite box for the given name
    /// </summary>
    /// <param name="name">The name of the sprite box to lookup</param>
    /// <returns>null if not found, otherwise the spritebox for teh given name</returns>
    internal SpriteBox SpriteBox(string name)
    {
        foreach (var s in spriteBoxLayout)
            if (name == s.spriteBoxName)
                return s;
        return null;
    }
}


/// <summary>
/// A sprite box defines a rectangular area on the display to draw an image of sprite sequence.  
/// </summary>
public class SpriteBox
{
    /// <summary>
    ///  The name of the sprite box.  The animation engine may use this to
    ///  select the procedure(s) in charge managing the layer and sprite boxes.
    ///  If an image map is available for this animation, the sprite sequence
    ///  it describes will be displayed in this rectangle.
    /// </summary>
    /// <value>
    /// The name of the sprite box.
    /// </value>
    public string spriteBoxName {get; set;}


    /// <summary>
    /// “CustomHue” if the PNG images should be converted from gray scale to the
    /// colour using the current eye colour setting.
    /// 
    /// “RGBA” if the image should be drawn as is.
    /// </summary>
    /// <value>
    /// “CustomHue” if the PNG images should be converted from gray scale to the
    /// colour using the current eye colour setting.
    /// 
    /// “RGBA” if the image should be drawn as is.
    /// </value>
    public string spriteRenderMethod {get;set;}

    /// <summary>
    /// The width of the sprite box.
    /// Unit: pixels
    /// </summary>
    /// <value>
    /// The width of the sprite box.
    /// Unit: pixels
    /// </value>
    public int width {get; set;}

    /// <summary>
    /// The height of the sprite box.
    /// Unit: pixels
    /// </summary>
    /// <value>
    /// The height of the sprite box.
    /// Unit: pixels
    /// </value>
    public int height {get; set;}


    /// <summary>
    /// The x coordinate of the upper left hand corner of the sprite box.  The
    /// x coordinate can be outside of the display area; only the portion of
    /// the image within the display area will be shown.  This allows an image
    /// to slide in.
    /// Unit: pixels
    /// </summary>
    /// <value>
    /// The x coordinate of the upper left hand corner of the sprite box.
    /// </value>
    public int x {get; set;}

    /// <summary>
    /// The y coordinate of the upper left hand corner of the sprite box.  The
    /// y coordinate can be outside of the display area; only the portion of
    /// the image within the display area (0..95) will be shown.  This allows
    /// an image to slide in.
    /// Unit: pixels
    /// </summary>
    /// <value>
    /// The y coordinate of the upper left hand corner of the sprite box.
    /// </value>
    public int y {get; set;}
}


partial class Assets
{
    /// <summary>
    /// Maps the trigger name to the display layout filename.
    /// </summary>
    Dictionary<string, string> imageLayoutTriggerName2Filename= new Dictionary<string, string>();
    /// <summary>
    /// Maps the trigger name to the display layout filename.
    /// </summary>
    /// <value>
    /// Maps the trigger name to the display layout filename.
    /// </value>
    public IReadOnlyDictionary<string, string> ImageLayoutTriggerName2Filename => imageLayoutTriggerName2Filename;

    /// <summary>
    /// The maps the image layouts partial file name to the path.
    /// </summary>
    Dictionary<string, string> imageLayoutPathCache;

    /// <summary>
    /// Provides a list of the image layout trigger names.
    /// </summary>
    /// <value>
    /// A list of the image layout trigger names.
    /// </value>
    public IReadOnlyCollection<string> ImageLayoutTriggerNames
    {
        get
        {
            return imageLayoutTriggerName2Filename.Keys;
        }
    }

    /// <summary>
    /// Looks up the full path of the image layout given the partial file name
    /// </summary>
    /// <param name="partialFileName">The partial file name, as might be given in one of the configuration files</param>
    /// <returns>null on error, otherwise the full path</returns>
    string ImageLayoutFullPath(string partialFileName)
    {
        // See if the file name cache has been built
        if (null == imageLayoutPathCache)
        {
            // Construct a cross reference within the image layouts path
            var path = Path.Combine(cozmoResourcesPath, "assets/compositeImageResources/imageLayouts");
            imageLayoutPathCache = FS.BuildNameToRelativePathXref(path);
        }

        // look up the file name for the foo
        return imageLayoutPathCache.TryGetValue(partialFileName, out var fullPath) ? fullPath : null;
    }


    /// <summary>
    /// Looks up the full path to the image layout given the trigger name
    /// </summary>
    /// <param name="triggerName">The animation trigger name</param>
    /// <returns>null on error, otherwise the full path to the image layout </returns>
    string ImageLayoutFullPathForTrigger(string triggerName)
    {
        // look up name for the file
        if (!imageLayoutTriggerName2Filename.TryGetValue(triggerName, out var partialName))
            return null;
        return ImageLayoutFullPath(partialName);
    }


    /// <summary>
    /// Looks up the path to the image layout given the trigger name
    /// </summary>
    /// <param name="triggerName">The animation trigger name</param>
    /// <returns>null on error, otherwise The relative path to the image layout </returns>
    string ImageLayoutPath(string triggerName)
    {
        // Get the full path
        var path = ImageLayoutFullPathForTrigger(triggerName);
        if (null == path)
            return null;

        // Remove the base path
        return FS.RemoveBasePath(cozmoResourcesPath, path);
    }

}
}
