// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Anki.Resources.SDK
{

/// <summary>
/// This is a wrapper around an sprite sequence
/// </summary>
public class SpriteSequence
{
    /// <summary>
    /// The path to the cozmo resources
    /// </summary>
    readonly string cozmoResourcesPath;

    /// <summary>
    /// The format of the sprite sequence filename
    /// </summary>
    /// <remarks>You should not use this</remarks>
    public readonly string FileNameFormat;

    /// <summary>
    /// The number of frames in the sequence
    /// </summary>
    public int Count {get; internal set;}

    /// <summary>
    /// The name of the sprite sequence.
    /// </summary>
    public readonly string Name;

#if false
    /// <param name="length_ms">The duration of the animation (when played)</param>
    /// <summary>
    /// The duration of the animation (when played)
    /// </summary>
    internal readonly int length_ms;
#endif

    /// <summary>
    /// Creates a wrapper a sprite sequence
    /// </summary>
    /// <param name="name">The name of the sprite sequence.</param>
    /// <param name="cozmoResourcesPath">The path to the cozmo resources</param>
    /// <param name="fmt">The sprite sequence formatter</param>
    internal SpriteSequence(string name, string cozmoResourcesPath, string fmt) 
    {
        this.cozmoResourcesPath = cozmoResourcesPath;
        this.FileNameFormat = fmt;
        this.Name           = name;
    }

#if false
    /// <summary>
    /// The frame rate
    /// </summary>
    public float FramesPerSecond
    {
        get
        {
            return (Count * 1000.0f) / (float)length_ms;
        }
    }
#endif

    /// <summary>
    /// This is used to list each of the sprites for the sequence name.
    /// The sprites can then be loaded or copied.
    /// </summary>
    /// <returns>An enumeration of the sprite paths relative to the resources base</returns>
    IEnumerable<string> Paths()
    {
        // Enumerate over the files
        for (int idx = 0; ; idx++)
        {
            var path = String.Format(FileNameFormat, idx);
            if (!File.Exists(Path.Combine(cozmoResourcesPath, path)))
                break;
            // Return the results
            yield return path;
        }
    }

    /// <summary>
    /// This is used to load each of the sprites in the sequence
    /// </summary>
    /// <returns>An  enumeration of the sprites</returns>
    public IEnumerable<Bitmap> Bitmaps
    {
        get
        {
        // Enumerate over the files in the sprite sequence
        foreach (var path in Paths())
        {
            // Open the file, at least try
            yield return Util.ImageOpen(Path.Combine(cozmoResourcesPath, path));
        }
        }
    }
}


public partial class Assets
{
    /// <summary>
    /// The maps the independent partial file name to the path.
    /// The name is uppercase to make caseless matches easy.
    /// </summary>
    Dictionary<string, string> spriteSequencePathCache;

    /// <summary>
    /// Provides a list of the sprite sequence names, preserving the case used
    /// in the file system.
    /// </summary>
    public IEnumerable<string> SpriteSequenceNames
    {
        get
        {
            // Build the file name cache, if has not been built
            BuildSpriteSequencePaths();
            // Iterate over the names, but convert it into a nice printable form
            foreach (var sprite in spriteSequencePathCache.Values)
                yield return Path.GetFileName(sprite);
        }
    }


    /// <summary>
    /// This scans and builds up the table of sprite sequence names
    /// </summary>
    /// <remarks>
    /// This scans the following paths:
    ///     cozmo_resources/assets/sprites/spriteSequences
    ///     cozmo_resources/assets/faceAnimations
    ///     cozmo_resources/config/sprites/spriteSequences
    ///     cozmo_resources/config/devOnlySprites/spriteSequences
    /// </remarks>
    void BuildSpriteSequencePaths()
    {
        // See if the file name cache has been built
        if (null != spriteSequencePathCache)
            return;
        // Create the cache
        spriteSequencePathCache = new Dictionary<string, string>();

        // Construct a cross reference within the animation groups path
        foreach (var path in config.spriteSequencesSearchPaths)
        {
            // Create the path
            var folderPath  = Path.Combine(cozmoResourcesPath, path);
            // Skip it if the folder doesn't exist
            if (!Directory.Exists(folderPath))
                continue;

            // Add in each of the directories
            foreach (var seqPath in Directory.EnumerateDirectories(folderPath))
            {
                // Since the path is the full path, retain just the name
                var seqName = Path.GetFileName(seqPath);
                spriteSequencePathCache[seqName.ToUpper()] = Path.Combine(path, seqName);
            }
        }
    }

    /// <summary>
    /// This is used to provide a sprite sequence for the sequence name.
    /// </summary>
    /// <param name="sequenceName">The name of the sprite sequence</param>
    /// <returns>An enumeration of the sprite paths relative to the resources base</returns>
    public SpriteSequence SpriteSequence(string sequenceName)
    {
        // Build the sprite sequence paths cache
        BuildSpriteSequencePaths();

        // Construct a path looking for the animation group
        if (!spriteSequencePathCache.TryGetValue(sequenceName.ToUpper(), out var sequencePath))
            return null;
        var name=sequenceName;

        // The file inside of the folder don't always have the exact same name.
        // Figure that out from first name
        var d = Directory.EnumerateFiles(Path.Combine(cozmoResourcesPath,sequencePath));
        var e= d.GetEnumerator();
        e.MoveNext();
        var template= e.Current;
        sequenceName = Path.GetFileName(template.Substring(0,template.LastIndexOf('_')));

        // The path and file name prefix
        sequencePath = Path.Combine(sequencePath, sequenceName + "_");

        // probe to see the number of digits to use with the template
        // There may be a better way, but this works
        var fmt = "{0:D5}.png";
        if (File.Exists(Path.Combine(cozmoResourcesPath, sequencePath + "00.png")))
            fmt = "{0:D2}.png";
        else if (File.Exists(Path.Combine(cozmoResourcesPath, sequencePath + "000.png")))
            fmt = "{0:D3}.png";
        else if (File.Exists(Path.Combine(cozmoResourcesPath, sequencePath + "0000.png")))
            fmt = "{0:D4}.png";

        // Creates the wrapper around the sprites
        var ret= new SpriteSequence(name, cozmoResourcesPath, sequencePath+fmt);
        // Since it is faster here, count the number of items
        var dir=new Dictionary<string,string>();
        foreach (var x in d)
            dir[Path.GetFileName(x)]=x;
        for (int I = dir.Count; I>=0; I--)
        {
            if (dir.ContainsKey(sequenceName+"_"+String.Format(fmt,I)))
            {
                ret.Count = I+1;
                break;
            }
        }


        // Creates the wrapper around the sprites
        return ret;
    }
}
}
