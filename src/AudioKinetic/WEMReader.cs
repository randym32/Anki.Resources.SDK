// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System;
using System.IO;
using System.Text;
using NAudio.Vorbis;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace Anki.AudioKinetic
{

/// <summary>
/// This is used to open up the WEM audio files and WEM segment in the SoundBank
/// (BNK) file and setup something that NAudio can access.
/// </summary>
/// <remarks>
/// A lot of the format info came from:
/// <list type="bullet">
///  <item><description><see href="http://bobdoleowndu.github.io/mgsv/documentation/soundswapping.html">http://bobdoleowndu.github.io/mgsv/documentation/soundswapping.html</see></description></item>
///  <item><description><see href="https://github.com/losnoco/vgmstream/blob/master/src/meta/wwise.c">VGMStream's WWise support</see></description></item>
/// </list>
/// 
/// Making this a kind of plug-in/extension to NAudio was done because it is
/// NAudio is ubiquitous, powerful in ways that matter.  There is an existing
/// framework (NVorbis) that we use here -- similar to how vgmstream uses
/// libvorbis -- to decode the audiostream from Cozmo resources.  By
/// integrating with NAudio, it is easy to hook into Anki.Vector.SDK, and many
/// other potential programs without learning new tricks.
/// </remarks>
/// <example>
/// You can pipe the sounds into NAudio using code like:
///  <code>
///    var WEM = new WEMReader("some path to a .wem");
///    WEM.Open();
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
public class WEMReader:IDisposable
{
    /// <summary>
    /// RIFF header tag
    /// </summary>
    static readonly uint RIFF = Util.Tag("RIFF");
    /// <summary>
    /// WAVE section tag
    /// </summary>
    static readonly uint WAVE = Util.Tag("WAVE");
    static readonly uint fmt_ = Util.Tag("fmt ");
    /// <summary>
    /// Junk section tag
    /// </summary>
    static readonly uint JUNK = Util.Tag("JUNK");
    /// <summary>
    /// data section tag
    /// </summary>
    static readonly uint DATA = Util.Tag("data");
    /// <summary>
    /// AudioKinetic extra data segment
    /// </summary>
    static readonly uint akd_ = Util.Tag("akd ");
    static readonly uint smpl = Util.Tag("smpl");

    /// <summary>
    /// The binary reader used to access the data in the file
    /// </summary>
    readonly BinaryReader binaryReader;

    /// <summary>
    /// The Audio format of the encapsulated data.
    /// </summary>
    /// <remarks>
    /// 
    /// <list type="table">
    ///  <listheader><term>value</term><description>Description</description></listheader>
    ///  <item><term>0xffff</term> <description>WWise's Vorbis stream, using Tremor (fixed-point Vorbis)</description></item>
    ///  <item><term>0x0002</term> <description>WWise IMA ADPCM</description></item>
    ///  </list>
    /// </remarks>
    public uint AudioFormat {get; internal set; }


    /// <summary>
    /// The number of samples per second from audio file
    /// </summary>
    public uint SampleRate {get; internal set; }

    /// <summary>
    /// The number of channels in the audio file
    /// </summary>
    public uint NumChannels {get; internal set; }

    /// <summary>
    /// The average number of bytes per second
    /// </summary>
    public uint AvgBytesPerSec {get; internal set; }

    /// <summary>
    /// The block alignment
    /// </summary>
    public uint BlockAlign {get; internal set; }

    /// <summary>
    /// The number of bits per sample
    /// </summary>
    public uint BitsPerSample {get; internal set; }


    /// <summary>
    /// The start of the data
    /// </summary>
    long Data_start=-1;
    long Data_size;
    
    /// <summary>
    /// The offset within the data segment to Vorbis info
    /// </summary>
    uint setup_ofs, audio_ofs;

    /// <summary>
    /// Provides access the a WEM sound file
    /// </summary>
    /// <param name="filePath">The path to the sound file</param>
    /// <remarks>
    /// Call Open() to begin reading the file
    /// </remarks>
    public WEMReader(string filePath) : this(System.IO.File.OpenRead(filePath))
    {
    }

    /// <summary>
    /// Provides access the a WEM sound file or segment
    /// </summary>
    /// <param name="stream">Stream with the sound file embedded</param>
    /// <remarks>
    /// Call Open() to begin reading the file
    /// </remarks>
    public WEMReader(Stream stream)
    {
        // Create something to read the segment
        binaryReader = new BinaryReader(stream, Encoding.UTF8, true);
    }

    /// <summary>
    /// Dispose of any extra resources
    /// </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Dispose fo the binary stream
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        if (null != binaryReader)
        {
            binaryReader.Dispose();
        }
    }

    /// <summary>
    /// Opens the file and checks out its basic format
    /// </summary>
    /// <returns>false if there was an error, true if the file was opened successfully</returns>
    public bool Open()
    {
        // Need to put some sort of lock in here.
        if (0 != AudioFormat)
            return true;

        // Read the header of the file
        var hdrTag = binaryReader.ReadUInt32();
        if (RIFF != hdrTag)
            return false;
        // RIFF size
        var RIFF_size = binaryReader.ReadUInt32();
        var hdrTag2   = binaryReader.ReadUInt32();
        if (WAVE != hdrTag2)
            return false;

        // Do the initial scan of the file
        while (true)
        {
            // Stop the music if we've reached the end
            if (binaryReader.BaseStream.Length <= binaryReader.BaseStream.Position)
                return true;
            // Read the section
            binaryReader.BaseStream.Position = ReadSection();
        }
    }


    /// <summary>
    /// Reads a section in
    /// </summary>
    /// <returns>The position of the next section</returns>
    long ReadSection()
    {
        // Read the section tag and size
        var tag         = binaryReader.ReadUInt32();
        var sectionSize = binaryReader.ReadUInt32();
        // Make a note of the end of the section
        var nextSection = binaryReader.BaseStream.Position + sectionSize;

        // Dispatch to the reader
        // - we ignore sections with nothing interesting in them
        if (fmt_ == tag)
        {
            // This will say how many channels and the format
            ReadFmt();
        }
        else if (JUNK == tag)
        {
        }
        else if (DATA == tag)
        {
            // If the file exists, there isn't enough space to hold the data segment,
            // so we should not use the segment suggested here.
            if (nextSection > binaryReader.BaseStream.Length)
            {
                // The section size doesn't match, don't use it
            }
            else
            {
                // This is the actual data for the audio stream.
                // Make a note of it
                Data_start = binaryReader.BaseStream.Position;
                Data_size = sectionSize;
            }
        }
        else if (akd_ == tag)
        {
            // AudioKinetic extra data
        }
        else if (smpl == tag)
        {
            // Decode the looping setup
            ReadSmpl();
        }
        else
        {
            //
        }
        return nextSection;
    }

    /// <summary>
    /// An estimate the duration of the sound, in seconds.
    /// (0 on error)
    /// </summary>
    public float Duration
    {
        get
        {
            // First effort to estimate the duration
            if (0 == numSamples && 0 != NumChannels * BitsPerSample && 0 != SampleRate)
            {
                numSamples = (uint)(((float)Data_size*8) / (NumChannels * BitsPerSample));
            }

            // Estimate the duration
            if (0 != SampleRate)
                return (float)numSamples/(float)SampleRate;
            return 0;
        }
    }

    /// <summary>
    /// This is a Vorbis transcoder to convert from WWise's Vorbis into the
    /// more standard Ogg Vrobis
    /// </summary>
    Vorbis transcoder;

    /// <summary>
    /// Returns a stream of the data for the sound, in its format
    /// </summary>
    /// <returns>null on error, otherwise a stream of the data</returns>
    public Stream DataStream()
    {
        // Check for a lack of a data stream
        if (Data_start <0)
            return null;
        // We'll reuse the underlying stream
        var st = binaryReader.BaseStream;
        st.Position = Data_start;
        return st;
    }

    /// <summary>
    /// Returns the data for the sound, in its format
    /// </summary>
    /// <returns>null on error, otherwise the data</returns>
    byte[] Data()
    {
        // Check for a lcack of a data stream
        if (Data_start <0)
            return null;
        // We'll reuse the underlying stream
        var st = binaryReader.BaseStream;
        st.Position = Data_start;
        return binaryReader.ReadBytes((int)Data_size);
    }

    /// <summary>
    /// This provides an NAudio WaveProvider that can be used to play this file
    /// </summary>
    /// <returns>null on error, otherwise a wave provider</returns>
    /// <example>
    ///  <code>
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
    public IWaveProvider WaveProvider()
    {
        if (Data_start < 0)
            return null;

        // Sanity check that the file header was scanned.
        // The caller should do this, but may not have.
        if (0 == AudioFormat)
            Open();


        binaryReader.BaseStream.Position = Data_start;

        // Get the WaveProvider that matches the format
        if (2 == AudioFormat)
            return new IMAWaveProvider(binaryReader, (int) Data_size, (int) SampleRate, (int) NumChannels);
                var st = binaryReader.BaseStream;
                st.Position = Data_start;
        var vorbisPackets = transcoder.Transcode(binaryReader,setup_ofs, audio_ofs);
        return new SampleToWaveProvider(new VorbisSampleProvider(vorbisPackets));
    }

    uint numSamples;

    /// <summary>
    /// This reads the format section
    /// </summary>
    void ReadFmt()
    {
        var pos        = binaryReader.BaseStream.Position;

        // The audio format is given here
        AudioFormat    = binaryReader.ReadUInt16();
        // Get the parameters that we'll use to decode the audio file
        // note: some of these parameters aren't stored here; with the internal
        // Vorbis format, the extra header has those
        NumChannels    = binaryReader.ReadUInt16();
        SampleRate     = binaryReader.ReadUInt32();
        AvgBytesPerSec = binaryReader.ReadUInt32();
        BlockAlign     = binaryReader.ReadUInt16();
        BitsPerSample  = binaryReader.ReadUInt16();
        var extraSize  = binaryReader.ReadUInt16();

        // There is maybe some extra stuff, but I'm going to ignore them unless
        // we have no choice
        if (2 == AudioFormat)
        {
            // IMA Wave format
            return;
        }
        if (0xffff != AudioFormat)
        {
            // This is a format that we don't support.  (Most likely it is a
            // format not supported on Vector.
            throw new System.Exception("Unsupported audio format");
        }

        // This is a Vorbis style audio, get information
        binaryReader.BaseStream.Position = pos +0x18;
        numSamples = binaryReader.ReadUInt32();
        binaryReader.BaseStream.Position = pos +0x18+0x10;
        setup_ofs  = binaryReader.ReadUInt32();
        audio_ofs  = binaryReader.ReadUInt32();
        binaryReader.BaseStream.Position = pos +0x18+0x28;
        var blocksize_1_exp = binaryReader.ReadByte();
        var blocksize_0_exp = binaryReader.ReadByte();
        transcoder= new  Vorbis(blocksize_0_exp, blocksize_1_exp);
        transcoder.NumChannels=NumChannels;
        transcoder.SampleRate=SampleRate;
    }

    /// <summary>
    /// Read the looping setup
    /// </summary>
    void ReadSmpl()
    {
        var a1 = binaryReader.ReadUInt32();
        var a2 = binaryReader.ReadUInt32();
        var a3 = binaryReader.ReadUInt32();
        var a4 = binaryReader.ReadUInt32();
        //0x10
        var a5 = binaryReader.ReadUInt32();
        var a6 = binaryReader.ReadUInt32();
        var a7 = binaryReader.ReadUInt32();
        var loopingEnabled = binaryReader.ReadUInt32();
        //0x20
        var b1 = binaryReader.ReadUInt32();
        var b2 = binaryReader.ReadUInt32();
        var b3 = binaryReader.ReadUInt32();
        var loopStartSample = binaryReader.ReadUInt32();
        var loopEndSample = binaryReader.ReadUInt32();
        var c1 = binaryReader.ReadUInt32();
        var c2 = binaryReader.ReadUInt32();
    }
}

}
