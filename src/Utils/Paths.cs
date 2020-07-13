// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RCM
{
public partial class Util
{
    /// <summary>
    /// A place to cache the path
    /// </summary>
    static string _assemblyDirectory;

    /// <summary>
    /// The path of the executing assembly.
    /// </summary>
    /// <value>
    /// The path of the executing assembly.
    /// </value>
    /// <remarks>
    /// https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
    /// </remarks>
    public static string AssemblyDirectory
    {
        get
        {
            // USe the cached value
            if (null != _assemblyDirectory)
                return _assemblyDirectory;

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return _assemblyDirectory = Path.GetDirectoryName(path);
        }
    }

    /// <summary>
    /// This is use to remove the base path from the full path
    /// </summary>
    /// <param name="basePath">The path that should be removed</param>
    /// <param name="path">The path that is to be modified</param>
    /// <returns>The resulting path</returns>
    public static string RemoveBasePath(string basePath, string path)
    {
        if (null == basePath || null == path)
            return null;
        // See if it matches
        if (0 != string.Compare(path, 0, basePath, 0, basePath.Length, StringComparison.InvariantCultureIgnoreCase))
            return path;
        var l = basePath.Length;
        if (path.Length <= l)
            return "";
        if ('/' == path[l] || '\\' == path[l]) l++;

        // Return the portion after it
        return path.Substring(l);
    }


    /// <summary>
    /// A helper to go from partial file name to the full file name
    /// </summary>
    /// <param name="path">The path to search within</param>
    /// <param name="extension">The file name extension (defualt is json)</param>
    /// <returns>The dictionary mapping the partial file names to the path</returns>
    public static Dictionary<string, string> BuildNameToRelativePathXref(string path, string extension="json")
    {
        var ret = new Dictionary<string, string>();
        // The directory might not exist, check for that.
        if (!Directory.Exists(path))
            return ret;

        // Enumerate over the directory
        var files = Directory.EnumerateFiles(path, "*."+extension, SearchOption.AllDirectories);
        foreach (string currentFile in files)
        {
            // map the partial file name to the relative path
            ret[Path.GetFileNameWithoutExtension(currentFile)] = currentFile;
        }

        // return the resulting cross reference table
        return ret;
    }
}
}
