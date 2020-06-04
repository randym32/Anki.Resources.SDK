// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

using Anki.AudioKinetic;
using NAudio.Wave;
using System;
using System.IO;

namespace Anki.AudioKinetic
{

/// <summary>
/// A helper to stream the WWise encoded IMA 
/// </summary>
public class IMAWaveProvider : IWaveProvider, IDisposable
{
    /// <summary>
    /// The access to the underlying data stream
    /// </summary>
    BinaryReader reader;

    /// <summary>
    /// The end position of the stream
    /// </summary>
    readonly long end;

    /// <summary>
    /// A buffer to read bytes into
    /// </summary>
    readonly short[] chSamples= new short[0x40];

    /// <summary>
    /// A buffer holding the decoded, multiplexed samples
    /// </summary>
    short[] samples;

    /// <summary>
    /// A buffer to hold multiplexed samples
    /// </summary>
    readonly short[] dest;

    /// <summary>
    /// The index into the sample buffer to continue copying from
    /// </summary>
    int index=0;

    /// <summary>
    /// Initializes the provider of audio sounds
    /// </summary>
    /// <param name="reader">The binary reader that provides access to the bytes</param>
    /// <param name="numBytes">The max number of bytes to read from the reader</param>
    /// <param name="sampleRate">The sample rate</param>
    /// <param name="numChannels">The number of channels</param>
    internal IMAWaveProvider(BinaryReader reader, int numBytes, int sampleRate, int numChannels)
    {
        // Set up the wave format
        SetWaveFormat(sampleRate, numChannels);
        this.reader=reader;
        end = reader.BaseStream.Position+numBytes;

        // Allocate a buffer to read the bytes into
         dest    = new short[64*numChannels];
    }

    /// <summary>
    /// Disposes the reader stream
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && reader != null)
        {
            reader.Dispose();
        }
    }

    /// <summary>
    /// Disposes the reader stream
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Sets the wave format
    /// </summary>
    /// <param name="sampleRate"></param>
    /// <param name="numChannels"></param>
    public void SetWaveFormat(int sampleRate, int numChannels)
    {
        this.WaveFormat = new WaveFormat(sampleRate, numChannels);
    }

    /// <summary>
    /// Provides the wave format
    /// </summary>
    public WaveFormat WaveFormat { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int Read(byte[] buffer, int offset, int count)
    {
        WaveBuffer waveBuffer = new WaveBuffer(buffer);
        var outIdx=0;
        var outBuf = waveBuffer.ShortBuffer;
        if (null == reader)
            return 0;
        while (true)
        {
            // Do we need to replenish the bufer?
            if (null == samples)
            {
                index=0;
                // Check that we haven't reached the end of the stream
                // Todo: find a better way to capture the end of the line
                if (reader.BaseStream.Position >= end)
                {
                    reader.Dispose();
                    reader=null;
                    return 0;
                }
                // For each of the channels decode the samples and multiplex them in
                var numChannels = WaveFormat.Channels;

                // Get the decode sample buffer for the 1st channel
                AudioAssets.Decode_wwise_ima(reader,chSamples);

                if (1 == numChannels)
                    samples = chSamples;
                else
                {
                    // place it per the channels
                    var L = chSamples.Length;
                    for (var sampIdx = 0; sampIdx  < L; sampIdx ++)
                        dest[sampIdx *numChannels] = chSamples[sampIdx ];

                    for (var ch =1; ch < numChannels; ch++)
                    {
                        // Check that we haven't reached the end of the stream
                        // Todo: find a better way to capture the end of the line
                        if (reader.BaseStream.Position >= end)
                        {
                            reader.Dispose();
                            reader = null;
                            return 0;
                        }

                        // Get the decode sample buffer
                        AudioAssets.Decode_wwise_ima(reader,chSamples);
                        for (var sampIdx = 0; sampIdx  < L; sampIdx ++)
                            dest[sampIdx *numChannels+1] = chSamples[sampIdx ];
                    }
                    samples = dest;
                }
            }

            // Copy the samples out
            var len = samples.Length;
            for (; count > 0 && index < len; index++, count-=2)
                outBuf[outIdx++] = samples[index];
            // Have we consumed the whole buffer?
            if (index >= samples.Length)
            {
                samples = null;
            }
            // Are we done?
            if (count < 2)
                return outIdx * 2;
        }
    }
}

}