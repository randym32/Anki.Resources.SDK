// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Blackwood;
using RCM;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// The maps the independent partial file name to the path
    /// </summary>
    Dictionary<string, string> independentSpritesPathCache;

    /// <summary>
    /// Provides a list of the independent sprite names
    /// </summary>
    /// <value>
    /// Provides a list of the independent sprite names
    /// </value>
    public IReadOnlyCollection<string> IndependentSpriteNames
    {
        get
        {
            // build the file name cache, if has not been built
            BuildIndependentSpritePaths();
            return independentSpritesPathCache.Keys;
        }
    }

    /// <summary>
    /// This scans and builds up the table of independent sprite names
    /// </summary>
    /// <remarks>
    /// This scans the following paths:
    ///     cozmo_resources/assets/sprites/independentSprites
    ///     cozmo_resources/config/sprites/independentSprites
    ///     cozmo_resources/config/facePNGs
    ///     cozmo_resources/config/devOnlySprites/independentSprites
    /// </remarks>
    void BuildIndependentSpritePaths()
    {
        // See if the file name cache has been built
        if (null != independentSpritesPathCache)
            return;
        // Create the cache
        independentSpritesPathCache = new Dictionary<string, string>();

        // Construct a cross reference within the animation groups path
        // Add in the sprites in reverse order so that the priority items wipe them out
        for (int idx = config.independentSpritesSearchPaths.Length - 1; idx >= 0; idx--)
        {
            // Create the path
            var path = Path.Combine(cozmoResourcesPath, config.independentSpritesSearchPaths[idx]);

            // Add in each of the the sprites, possibly wiping out the lower priority ones
            foreach (var kv in FS.BuildNameToRelativePathXref(path, "png"))
            {
                independentSpritesPathCache[kv.Key] = kv.Value;
            }
        }
    }


    /// <summary>
    /// Looks up the full path given the partial file name
    /// </summary>
    /// <param name="partialFileName">The partial file name, as might be given
    /// in one of the configuration files</param>
    /// <returns>null on error, otherwise the full path</returns>
    string IndependentSpriteFullPath(string partialFileName)
    {
        // build the file name cache, if has not been built
        BuildIndependentSpritePaths();

        // look up the file name for the foo
        return independentSpritesPathCache.TryGetValue(partialFileName, out var fullPath) ? fullPath : null;
    }


    /// <summary>
    /// This is use to provide the path to the sprite, relative to the resource base
    /// </summary>
    /// <param name="spriteName"></param>
    /// <returns>null on error, otherwise the sprite path relative to the resources base</returns>
    public string IndependentSpritePath(string spriteName)
    {
        // Look up the path to the image
        var path = IndependentSpriteFullPath(spriteName);
        if (null == path)
            return null;

        // Remove the base path
        return FS.RemoveBasePath(cozmoResourcesPath, path);
    }


    /// <summary>
    /// This is used to the sprite
    /// </summary>
    /// <param name="spriteName">The name of the sprite</param>
    /// <returns>The sprite</returns>
    public Bitmap IndependentSprite(string spriteName)
    {
        // Look up the path to the image
        var path = IndependentSpriteFullPath(spriteName);
        if (null == path)
            return null;

        // Open the file, at least try
        return RCM.Util.ImageOpen(path);
    }
}
}
