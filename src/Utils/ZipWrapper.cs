// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace RCM
{
/// <summary>
/// This is a class to allow access to resources within an archive.
/// It is a sibling to FolderWrapper that can access resources within a folder
/// </summary>
public class ZipWrapper:IFolderWrapper
{
    /// <summary>
    /// The count of the number of users of this stream
    /// </summary>
    int refCnt;

    /// <summary>
    /// The path to the folder holding the configuration
    /// </summary>
    readonly string basePath;

    /// <summary>
    /// The archive accessor object
    /// </summary>
    readonly ZipArchive zip;

    /// <summary>
    /// The file underlying the zip object
    /// </summary>
    readonly FileStream file;

    /// <summary>
    /// Creates an object that can access resources within an archive
    /// </summary>
    /// <param name="path">The path to the archive file</param>
    public ZipWrapper(string path)
    {
        basePath=path;
        file = File.OpenRead(path);
        zip = new ZipArchive(file, ZipArchiveMode.Read);
    }

    /// <summary>
    /// Increment the reference count for this object.
    /// Decrement the count using Dispose().
    /// </summary>
    public void Retain()
    {
        Interlocked.Increment(ref refCnt);
    }

    /// <summary>
    /// Dispose of any internal resources
    /// </summary>
    public void Dispose()
    {
        // reduce the internal reference count
        if (Interlocked.Decrement(ref refCnt) >= 0)
            return;

        // Dispose of unmanaged resources.
        Dispose(true);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Dispose of the archive
    /// </summary>
    /// <param name="disposing">true, right?</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (null != zip)
                zip.Dispose();
            if (null != file)
                file.Dispose();
        }
    }

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="relativePath">The name of file within the wrapper</param>
    /// <returns>true if the file exists within the wrapper, false otherwise</returns>
    public bool Exists(string relativePath)
    {
        // Get the entry within the file
        try
        {
            var entry = zip.GetEntry(relativePath.Replace("\\", "/",StringComparison.InvariantCultureIgnoreCase));
            if (null == entry)
                return false;
            return true;
        }
        catch(Exception){ }
        return false;
    }

    /// <summary>
    /// This creates a stream for the given resources within the container
    /// </summary>
    /// <param name="relativePath">The name of file within the container</param>
    /// <returns>null on error, otherwise a stream that can be used to access the file data</returns>
    public Stream Stream(string relativePath)
    {
        // Get the entry within the file
        try
        {
            var entry = zip.GetEntry(relativePath.Replace("\\", "/",StringComparison.InvariantCultureIgnoreCase));
            if (null == entry)
                return null;
            // Get the file data stream
            var nonSeekableStream = entry.Open();
            // Since we need to be able to seek, lets copy it to a memory stream
            var ret = new MemoryStream();
            nonSeekableStream.CopyTo(ret);
            nonSeekableStream.Dispose();
            // For reasons known only to the designers, we have to position the
            // read location in the memory stream back at the beginning.
            ret.Position=0;
            return ret;
        }
        catch(Exception){ }
        return null;
    }
}
}
