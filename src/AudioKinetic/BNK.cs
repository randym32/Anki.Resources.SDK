// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Anki.AudioKinetic
{

/// <summary>
/// This class is used to scan sound bank (BNK) files.  These are containers of
/// many "assets" used to create sounds in response to dynamic game play.
/// </summary>
/// <remarks>
/// <list type="table">
///  <item><term>The audio bus hierarchy</term> <description>
///    A sound bank has setups for how the sounds flow from files (and other
///    inputs), thru mixers, and other filters to the output.</description></item>
///  <item><term>Sound effects</term> <description></description></item>
///  <item><term>Music compositions</term> <description>These probably are used 
///     heavily in Cozmo, but appear unused in Vector</description></item>
///  <item><term>State transition management</term> <description>How altering
///     the settings of effects during play.</description></item>
///  <item><term>Audio events &amp; actions</term><description>A map of audio
///     events to the actions to carry out when that event occurs, such as
///     playing a sound, stopping other sounds, changing mixer settings, and so
///     on</description></item>
///  <item><term>Other sound banks</term> <description>The set of other sound
///     bank files to load.</description></item>
///  <item><term>Sounds</term><description>WEM sound files, either embedded, or
///    a reference to an external file.</description></item>
///  </list>
/// </remarks>
/// <example>
/// You can pipe the sounds into NAudio using code like:
///  <code>
///    var WEM = soundBank.WEM( .. the id for the sound ..)
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
/// <remarks>
/// The sound bank files decoding is based on the format documented at
/// http://wiki.xentax.com/index.php/Wwise_SoundBank_(*.bnk)
/// 
/// Sound banks are binary files composed of series tagged sections.  Some
/// sections are
/// Vector's Bank files have the following sections in them:
/// <list type="bullet">
///  <item><description>BKHD</description></item>
///  <item><description>INIT</description></item>
///  <item><description>STMG</description></item>
///  <item><description>HIRC</description></item>
///  <item><description>ENVS</description></item>
///  <item><description>PLAT</description></item>
///  <item><description>DIDX </description></item>
///  <item><description>DATA</description></item>
///  <item><description>STID - this is used once, just to call out the Vector_VO .bnk file</description></item>
/// </list>
///  
///   The following may be added in the future
/// <list type="bullet">
///  <item><description>Switch groups</description></item>
///  <item><description>Switches</description></item>
///  <item><description>State group</description></item>
///  <item><description>States</description></item>
///  <item><description>Custom states</description></item>
///  <item><description>Game parameters</description></item>
///  <item><description>Audio buses</description></item>
/// </list>
/// </remarks>
public partial class BNKReader:IDisposable
{
    /// <summary>
    /// The count of the number of users of this stream
    /// </summary>
    int refCnt;

    /// <summary>
    /// The ID of this soundbank
    /// </summary>
    uint Id;

    /// <summary>
    /// This is the folder or archive holding the sound bank file, and thus any
    /// referenced .BNK or .WEM file
    /// </summary>
    readonly IFolderWrapper folderWrapper;

    /// <summary>
    /// The source stream holding the data
    /// </summary>
    readonly Stream srcStream;

    /// <summary>
    /// The name for the soundbank
    /// </summary>
    readonly string soundBankName;

    /// <summary>
    /// The folder for the sound bank files
    /// </summary>
    readonly string subfolder;

    /// <summary>
    /// Creates an object to access the BNK file
    /// </summary>
    /// <param name="soundBankName">The name of the soundbank</param>
    /// <param name="folderWrapper">The file/archive wrapper that can access
    /// the sound bank information</param>
    /// <param name="stream">Stream with the sound bank file</param>
    /// <param name="events">An optional list of the events provided by this soundbank</param>
    /// <param name="files">An optional list of the sound files provided by this soundbank</param>
    /// <param name="subfolder">An optional folder for the files for this bank</param>
    /// <remarks>
    /// Call Open() to begin reading the file
    /// </remarks>
    internal BNKReader(string soundBankName, IFolderWrapper folderWrapper,
        Stream stream, IReadOnlyList<EventInfo> events, IReadOnlyList<FileInfo> files,
        string subfolder=null)
    {
        this.soundBankName = soundBankName;
        this.folderWrapper = folderWrapper;
        this.subfolder     = subfolder?.Length>0?subfolder:null;
        // Create something to read the segment
        srcStream    = stream;
        binaryReader = new BinaryReader(srcStream, Encoding.UTF8, true);

        // Add in info about the events
        if (null != events)
            foreach (var evt in events)
             objectId2Info[evt.Name.ToString().ToUpper()]=evt;

        // Add in info about the events
        if (null != files)
            foreach (var file in files)
             objectId2Info[file.ID]=file;
    }

    /// <summary>
    /// Increment the reference count for this object.
    /// Decrement the count using Dispose().
    /// </summary>
    /// <returns>This object</returns>
    public BNKReader Retain()
    {
        Interlocked.Increment(ref refCnt);
        return this;
    }

    /// <summary>
    /// Dispose of any internal resources.
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
    /// Dispose of the binary stream.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Done with the binary reader
            if (null != binaryReader)
                binaryReader.Dispose();
            if (null != srcStream)
                srcStream.Dispose();
            folderWrapper.Dispose();
        }
    }


    /// <summary>
    /// Look up a WEM audio file object for the given file id
    /// </summary>
    /// <param name="WEMFileId">The identifier of the WEM file to retrieve</param>
    /// <returns>null on error, otherwise the object to access the file</returns>
    /// <remarks>
    /// The Data Index section is a table of file identifiers and their
    /// positions/sizes embedded in the data segment.
    /// </remarks>
    public WEMReader WEM(uint WEMFileId)
    {
        // Look up any record for sound file
        // This record, if it exists, will indicate whether we need to open a
        // different sound bank file, an external WEM file, or the one in this
        // file.  However, I have not yet explored it enough to know that it
        // works well.
        objectId2Info.TryGetValue(WEMFileId, out var x);
        //if (null != x && x is FileInfo)
        {
            // Check to see if the file exits; if so, use that over whatever is in the file
            var fname= WEMFileId + ".wem";
            if (null != subfolder)
            {
                fname = Path.Join(subfolder,fname);
            }
            var WEMStream = folderWrapper.Stream(fname);
            if (null != WEMStream)
            {
                var WEM = new WEMReader(WEMStream);
                WEM.Open();
                return WEM;
            }
        }

        if (null == x || !(x is FileInfo y))
            return null;

        // This doesn't actually seem to work. The internal WEM files usually
        // are just a bit of pre-roll
        if (0 == y.Offset && 0 == y.Size)
            return null;
        lock (srcStream)
        {
            // create BinaryStream that can only read the given segment
            var data = new byte[y.Size];
            srcStream.Position=DATA_ofs + y.Offset;
            srcStream.Read(data,0, (int) y.Size);

            // Lets copy it to a memory stream
            var WEMStream = new MemoryStream(data);
            // And create a WEM reader
            var reader = new WEMReader(WEMStream);
            // If the file type isn't recognized, skip it
            if (!reader.Open())
            {
                reader.Dispose();
                return null;
            }
            // return the reader
            return reader;
        }
    }

}

}
