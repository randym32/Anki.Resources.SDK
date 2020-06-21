// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.
//
// This file is likely to change a lot.  The animation names require, in the worst
// case, scanning all of the files....
using FlatBuffers;
using RCM;
using System;
using System.Collections.Generic;
using System.IO;

namespace Anki.Resources.SDK
{
partial class Assets
{
    /// <summary>
    /// The maps the animation partial file name to the path.
    /// This is used since there are potentially several locations for the file to be. 
    /// </summary>
    readonly Dictionary<string, string> animationBinPaths=new Dictionary<string, string>();

    /// <summary>
    /// This is used to track which files have been scanned
    /// </summary>
    readonly Dictionary<string, string> animFileScanned=new Dictionary<string, string>();

    /// <summary>
    /// This maps a clip name to the name of the file that holds it
    /// </summary>
    readonly Dictionary<string, string> animation2BinPath=new Dictionary<string, string>();
        

    #region Animation cache
    /// <summary>
    /// A cache to hold the binary animation files
    /// </summary>
    readonly Dictionary<string, WeakReference> _animationCache = new Dictionary<string, WeakReference>();

    /// <summary>
    /// Looks up the animation entry in the cache
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <returns>null on error, otherwise an animation object of type
    /// Anki.VectAnim.Keyframes, Anki.CozmoAnim.Keyframes or TBD JSON
    /// animation</returns>
    object AnimationCache(string animationName)
    {
        // Is there an entry in the cache for this?
        if (!_animationCache.TryGetValue(animationName, out var wref))
            return null;
        return wref.Target;
    }

    /// <summary>
    /// Add the item to the cache, if there isn't one already
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <param name="val">an animation object of type
    /// Anki.VectAnim.Keyframes, Anki.CozmoAnim.Keyframes or TBD JSON
    /// animation</param>
    void AnimationCache(string animationName, object val)
    {
        // Is there an entry in the cache for this animation?
        if (_animationCache.TryGetValue(animationName, out var wref)
           && null != wref.Target)
            return ;

        // Add an item to the cache
        _animationCache[animationName] = new WeakReference(val);
    }
    #endregion

    /// <summary>
    /// This scans and builds up the table of animation names
    /// </summary>
    void BuildAnimationPaths()
    {
        // See if the file name cache has been built
        if (0 != animationBinPaths.Count)
            return;
        // Construct a cross reference within the animation bin path
        var path = Path.Combine(cozmoResourcesPath, "assets/animations");
        foreach (var kv in Util.BuildNameToRelativePathXref(path, "bin"))
        {
            animationBinPaths[kv.Key] = kv.Value;
        }

        // Construct a cross reference within the animation JOSN path, and
        // append it to the existing cross referenc table
        path = Path.Combine(cozmoResourcesPath, "config/engine/animations");
        foreach (var kv in Util.BuildNameToRelativePathXref(path, "bin"))
        {
            animationBinPaths[kv.Key] = kv.Value;
        }
    }

    /// <summary>
    /// Provides a list of the animation names
    /// </summary>
    public IReadOnlyCollection<string> AnimationNames
    {
        get
        {
            // Scan to build up the animation names, if we haven't done so already
            BuildAnimationPaths();
            ScanAllBinFiles(null, out _);

            // Return the keys
            return animation2BinPath.Keys;
        }
    }

    /// <summary>
    /// Tracks which animation clips are defined in multiple files.
    /// </summary>
    readonly Dictionary<string, string> animMultiplyDefined = new Dictionary<string, string>();
    /// <summary>
    /// Tracks which animation clips are defined in multiple files.
    /// </summary>
    public IReadOnlyDictionary<string, string> AnimMultiplyDefined => animMultiplyDefined ;

    /// <summary>
    /// Add a mapping of the animation name to the file path
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <param name="filePath">The path to the animation file (.json or .bin)</param>
    void AnimCachePath(string animationName, string filePath)
    {
        // Is the file alread cached?
        if (!animation2BinPath.TryGetValue(animationName, out var prevFilePath))
            animation2BinPath[animationName] = filePath;
        else if (prevFilePath != filePath)
        {
            // The clip has been defined in multiple files
            // Make a note of it for later
            animMultiplyDefined[animationName]=$"Clip is defined in multiple files ({prevFilePath} and {filePath})";
        }
    }


    /// <summary>
    /// Looks up the path to the animation file, given the clip name
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <returns>null on error, otherwise the path to the animation file (.json or .bin)</returns>
    string AnimationPathForName(string animationName)
    {
        // See if it has already been cache
        if (animation2BinPath.TryGetValue(animationName, out var animPath))
            return animPath;

        // Scan to build up the animation names, if we haven't done so already
        BuildAnimationPaths();

        // Look up the potential path for the file
        // This successively trims off stuff after the underscore to help find
        // the file with the clip name.  This saves scanning all of the files
        var partialName = animationName;
        var fileName    = animationName;
        while(true)
        {
            // Look up the potential path for the file
            if (animationBinPaths.TryGetValue(fileName, out animPath))
                return animPath;

            // Try removing the item with the underscore
            // first, find the index of the underscore
            var idx = partialName.LastIndexOf('_');

            // stop if the underscore wasn't found
            if (idx < 0)
                break;

            // Remove the stuff at and afer that underscore
            partialName = partialName.Substring(0, idx);
            // Sanity check.  If there are multiple , we often have to do a hard scan
            if (animationBinPaths.TryGetValue(partialName+"_02", out _))
                break;
            fileName = partialName+"_01";
        }

        // Ok, downshift and scan all of the files
        if (null != ScanAllBinFiles(animationName, out animPath))
            return animPath;
        return null;
    }

    /// <summary>
    /// Scans all bin files until the one with the clip is found
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip)</param>
    /// <param name="animPath">The path to the animation file</param>
    /// <returns>null on error, otherwise the animation clips from the file</returns>
    object ScanAllBinFiles(string animationName, out string animPath)
    {
        foreach (var kv in animationBinPaths)
        {
            // Skip it if the file has already been scanned
            if (animFileScanned.ContainsKey(kv.Key)) continue;

            // Read the file
            var filePath = kv.Value;
            var b = File.ReadAllBytes(filePath);
            // Make a note that we scanned the file
            animFileScanned[kv.Key]="";
            object ret= null;

            // Interpret the binary file
            if (AssetsType.Vector == AssetsType)
            {
                var anim = Anki.VectorAnim.AnimClips.GetRootAsAnimClips(new ByteBuffer(b));
                ret = anim;

                // Register the mapping of the clip name to this file
                var L = anim.ClipsLength;
                for (var idx =0; idx < L; idx++)
                {
                    var clip = anim.Clips(idx);
                    // Register the file to go for the clip
                    AnimCachePath(clip?.Name, filePath);

                    // Cache the clip
                    AnimationCache(clip?.Name, clip);
                }
            }
            else
            {
                var anim = Anki.CozmoAnim.AnimClips.GetRootAsAnimClips(new ByteBuffer(b));
                ret = anim;

                // Register the mapping of the clip name to this file
                var L = anim.ClipsLength;
                for (var idx =0; idx < L; idx++)
                {
                    var clip = anim.Clips(idx);
                    // Register the file to go for the clip
                    AnimCachePath(clip?.Name, filePath);

                    // Cache the clip
                    AnimationCache(clip?.Name, clip);
                 }
            }
            // See if it has been found
            if (null != animationName &&
                animation2BinPath.TryGetValue(animationName, out animPath))
                return ret;
        }

        // The file wasn't found
        animPath = null;
        return null;
    }


    /// <summary>
    /// Looks up the animation for the name
    /// </summary>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <returns>null on error, otherwise an animation object of type
    /// Anki.VectAnim.Keyframes, Anki.CozmoAnim.Keyframes or TBD JSON
    /// animation</returns>
    public object AnimationForName(string animationName)
    {
        // Try the binary file
        // See if the description is already loaded
        var ret = AnimationCache(animationName);
        if (null != ret)
            return ret;

        // Look up a file for this animation clip
        var filePath = AnimationPathForName(animationName);
        if (null == filePath)
            return null;

        // Handle reading the file based on its extension
        var ext = Path.GetExtension(filePath).ToUpper();
        object animClips, clip;
        if (".BIN" == ext)
        {
            // Read the file
            var b = File.ReadAllBytes(filePath);
            // Make a note that we scanned the file
            animFileScanned[Path.GetFileNameWithoutExtension(filePath)]="";

            // Interpret the binary file
            if (AssetsType.Vector == AssetsType)
            {
                animClips = Anki.VectorAnim.AnimClips.GetRootAsAnimClips(new ByteBuffer(b));
            }
            else
            {
                animClips = Anki.CozmoAnim.AnimClips.GetRootAsAnimClips(new ByteBuffer(b));
            }
            // Scan the animation file for the clips that we are interested
            clip = GetClip(animClips, animationName, filePath);
            if (null != clip)
                return clip;
        }
        // Scan the animation bin files until we find
        animClips = ScanAllBinFiles(animationName, out filePath);
        // Scan the animation file for the clips that we are interested
        clip = GetClip(animClips, animationName, filePath);
        if (null != clip)
            return clip;

        if (".JSON" == ext)
        {
            // Try the JSON file
        }

        // Try the sprite sequence
        //var spriteSequence = SpriteSequence(animationName);
        //if (null != spriteSequence)
        //    return spriteSequence;

        // Give up
        return null;
    }

    /// <summary>
    /// Scans the animation file for the clip that we are interested
    /// </summary>
    /// <param name="animClips">The animation clips</param>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    /// <param name="filePath">The path to the animation file (.json or .bin)</param>
    /// <returns>null on error, otherwise an animation object of type
    /// Anki.VectAnim.Keyframes, Anki.CozmoAnim.Keyframes or TBD JSON
    /// animation</returns>
    object GetClip(object animClips, string animationName, string filePath)
    {
        // Interpret the binary file
        if (AssetsType.Vector == AssetsType)
        {
            var anim = (Anki.VectorAnim.AnimClips)animClips;

            // Register the mapping of the clip name to this file
            var L = anim.ClipsLength;
            for (var idx = 0; idx < L; idx++)
            {
                var clip = anim.Clips(idx);
                // Register the file to go for the clip
                AnimCachePath(clip?.Name, filePath);

                // Cache the clip
                AnimationCache(clip?.Name, clip);
             }

            // Look for the clip
            for (var idx = 0; idx < L; idx++)
            {
                var clip = anim.Clips(idx);

                if (animationName == clip?.Name)
                    return clip?.Keyframes;
            }
        }
        else
        {
            var anim = (Anki.CozmoAnim.AnimClips) animClips;

            // Register the mapping of the clip name to this file
            var L = anim.ClipsLength;
            for (var idx = 0; idx < L; idx++)
            {
                var clip = anim.Clips(idx);
                // Register the file to go for the clip
                AnimCachePath(clip?.Name, filePath);

                // Cache the clip
                AnimationCache(clip?.Name, clip);
             }
            // Look for the clip
            for (var idx = 0; idx < L; idx++)
            {
                var clip = anim.Clips(idx);

                if (animationName == clip?.Name)
                    return clip?.Keyframes;
            }
        }
        return null;
    }

}
}
