// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.IO;

namespace RCM
{

/// <summary>
/// This is an interface to allow access to resources within a folder or archive
/// </summary>
public interface IFolderWrapper:IDisposable
{
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="relativePath">The name of file within the wrapper</param>
    /// <returns>true if the file exists within the wrapper, false otherwise</returns>
    bool Exists(string relativePath);

    /// <summary>
    /// This creates a stream for the given resources within the container
    /// </summary>
    /// <param name="relativePath">The name of file within the container</param>
    /// <returns>A stream that can be used to access the file data</returns>
    Stream Stream(string relativePath);
}
}
