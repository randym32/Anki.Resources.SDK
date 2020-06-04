// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.IO;

namespace RCM
{

/// <summary>
/// This is a class to allow access to resources within a folder.
/// It is a sibling to ZipWrapper that can access resources within a zip file
/// </summary>
public class FolderWrapper:IFolderWrapper
{
    /// <summary>
    /// The path to the folder holding the configuration
    /// </summary>
    readonly string basePath;

    /// <summary>
    /// Creates an object that can access resources within a folder
    /// </summary>
    /// <param name="basePath">The basepath to the folder</param>
    public FolderWrapper(string basePath)
    {
        // Keep the base path to the files
        this.basePath = basePath;
    }

    /// <summary>
    /// Dispose of any internal resources
    /// </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the binary stream
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
    }


    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="relativePath">The name of file within the wrapper</param>
    /// <returns>true if the file exists within the wrapper, false otherwise</returns>
    public bool Exists(string relativePath)
    {
        // Get full path to the file
        var filePath = Path.Combine(basePath, relativePath);
        return File.Exists(filePath);
    }

    /// <summary>
    /// This creates a stream for the given resources within the folder
    /// </summary>
    /// <param name="relativePath">The name of file within the wrapper</param>
    /// <returns>A stream that can be used to access the file</returns>
    public Stream Stream(string relativePath)
    {
        // Get full path to the file
        var filePath = Path.Combine(basePath, relativePath);
        if (!File.Exists(filePath))
            return null;
        // Open a file stream for it
        return File.OpenRead(filePath);
    }
}
}
