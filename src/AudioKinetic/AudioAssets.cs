// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Anki.AudioKinetic
{

/// <summary>
/// This class is used to wrap the audio assets in Vector and Cozmo.
/// These assets are Audio-Kinetic WWise soundbank files and other supporting
/// files.
/// </summary>
/// <remarks>
/// This supports the audio assets as bare files, as is done with Vector
/// resources; or in a zip file, as is done with Cozmo resources.
/// 
/// Only features that are present in Cozmo or Vector are given much attention;
/// other WWise features are ignored.
/// AudioKinetic , Wwise Fundamentals (2015)
/// 
/// References:
/// <list type="bullet">
///  <item><term><see href="https://www.audiokinetic.com/download/documents/Wwise_Fundamentals.pdf">AudioKinetic WWise Fundamentals</see></term></item>
///  <item><term><see href="https://www.audiokinetic.com/files/?get=2015.1.9_5624/Wwise_UserGuide_en.pdf">AudioKinetic, Wwise User’s Guide</see></term></item>
///  <item><term><see href="https://www.audiokinetic.com/download/documents/WwiseProjectAdventure_en.pdf">AudioKinetic, The Wwise Project Adventure</see></term></item>
///  <item><term><see href="https://www.audiokinetic.com/download/documents/Wwise_GetStartedGuide.pdf">AudioKinetic, Get Started Using Wwise</see></term></item>
///  <item><term><see href="https://www.audiokinetic.com/download/lessons/wwise101_en.pdf">AudioKinetic, Wwise 101</see></term></item>
///  <item><term><see href="https://www.audiokinetic.com/library/edge/?source=WwiseFundamentalApproach&amp;id=understanding_events_understanding_eventsAudioKinetic">Wwise Fundamentals, Understanding Events</see></term></item>
/// </list>
/// </remarks>
/// <example>
/// You can pipe the sounds into NAudio using code like:
///  <code>
///    var audioAssets = new AudioAssets(.. path to sound folder ...);
///    var WEM = audioAssets.WEM( .. the id for the sound ..);
///    var waveOut      = new WaveOut();
///    var waveProvider = WEM.WaveProvider();
///    waveOut.Init(waveProvider);
///    waveOut.Play();
///    Thread.Sleep(30000);
///    waveOut.Stop();
///    waveProvider.Dispose();
///    waveOut.Dispose();
///  </code>
/// </example>
public partial class AudioAssets:IDisposable
{
    /// <summary>
    /// The count of the number of users of this stream
    /// </summary>
    int refCnt;

    /// <summary>
    /// The object that mediates access to resources within a folder or archive
    /// </summary>
    IFolderWrapper folderWrapper;

    /// <summary>
    /// This is used as a cache of the sound banks
    /// </summary>
    readonly Dictionary<string,BNKReader> cache = new Dictionary<string,BNKReader>();

    /// <summary>
    /// Creates an object that can access the audio system resources
    /// </summary>
    /// <param name="basePath">A path a zip file, or a /sound folder, which
    /// can contain bare files, or an AudioAssets.zip container</param>
    /// <remarks>
    /// This automatically detects if the audio assets are wrapped or not
    /// </remarks>
    public AudioAssets(string basePath)
    {
        // Check to see if the path has an archive of the audio assets
        var zipPath = Path.Combine(basePath,"AudioAssets.zip");
        if (".ZIP" == Path.GetExtension(basePath).ToUpper())
            Open(new ZipWrapper(basePath));
        else if (System.IO.File.Exists(zipPath))
            Open(new ZipWrapper(zipPath));
        else
            Open(new FolderWrapper(basePath));
    }

    /// <summary>
    /// Creates an object that can access the audio system resources
    /// </summary>
    /// <param name="folderWrapper">The file/archive wrapper that can access
    /// the sound bank information</param>
    public AudioAssets(IFolderWrapper folderWrapper)
    {
        Open(folderWrapper);
    }


    /// <summary>
    /// Opens up the assets area and reads the meta data
    /// </summary>
    /// <param name="folderWrapper">The file/archive wrapper that can access
    /// the sound bank information</param>
    void Open(IFolderWrapper folderWrapper)
    {
        // Kep the base path to the sound bank the files
        this.folderWrapper = folderWrapper;

        // Load the soundbank bundle info.  This is is in Vector-style resources
        SoundbankBundleInfo();
        // This indicates we are working with a Cozmo-style audio assets file
        PluginInfo();
        SoundBanksInfo();
    }

    /// <summary>
    /// Increment the reference count for this object.
    /// Decrement the count using Dispose().
    /// </summary>
    /// <returns>This object</returns>
    public AudioAssets Retain()
    {
        Interlocked.Increment(ref refCnt);
        return this;
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
        if (!disposing)
            return;

        foreach (var item in cache.Values)
            item.Dispose();
        if (null != folderWrapper)
            folderWrapper.Dispose();
    }

    /// <summary>
    /// Opens the file sound bank reader for the given sound bank name.  The
    /// path to the file is looked up from the configuration file.
    /// </summary>
    /// <param name="soundBankName">The name of the sound bank file; a caseless
    /// match is used</param>
    /// <param name="language">A language specifier, to select from the
    /// alternatives; if NULL the first bank found (without regard to its
    /// language) is used. (Ignored for now).</param>
    /// <returns>null on error, otherwise the sound bank reader</returns>
    /// <remarks>The banks can be localized, but there isn't any reason here</remarks>
    public BNKReader SoundBank(string soundBankName, string language=null)
    {
        // Check the cache for the reader
        var key = soundBankName.ToUpper();
        if (cache.TryGetValue(key, out var _reader))
        {
            // increment the reference count so that when it calls Dispose()
            // things won't be out of sync
            _reader.Retain();
            return _reader;
        }

        // Try to get information about the location of the soundbank file
        if (!soundBank2Info.TryGetValue(key, out var entry))
            return null;

        // Try to get the path to the file
        if (null == entry.Path)
            return null;

        // Get the file stream
        var bnkStream = folderWrapper.Stream(entry.Path);

        // Open the file
        var reader = new BNKReader(soundBankName, folderWrapper, bnkStream, entry.Events, entry.Files);

        // Open the reader
        reader.Open();

        // Cache the reader for resource reuse
        cache[key] = reader;
        // increment the reference count so that when it calls Dispose()
        // things won't be out of sync
        return reader.Retain();
    }

    /// <summary>
    /// Opens the WEM file corresponding to the given file ID.
    /// The file may be embedded within the soundbank file, or may be external.
    /// </summary>
    /// <param name="fileID">The identifier for the WEM to retrieve</param>
    /// <param name="language">A language specifier, to select from the
    /// alternatives; if NULL the first bank found (without regard to its
    /// language) is used. (Ignored for now).</param>
    /// <returns>An object that can read the WEM file</returns>
    /// <remarks>This scans all of the sound banks until it finds one</remarks>
    /// <remarks>The banks can be localized, but there isn't any reason here</remarks>
    public WEMReader WEM(uint fileID, string language=null)
    {
        // Scan each of the sound banks for the file
        foreach (var soundBankKV in soundBank2Info)
        {
            // Open up the sound bank
            var BNK = SoundBank(soundBankKV.Key, language);
            if (null == BNK)
                continue;

            // See if the sound bank has the file id
            var ret = BNK.WEM(fileID);
            // We're with it for now
            BNK.Dispose();
            if (null != ret)
                return ret;
        }

        // Now file 
        return null;
    }
}
}
