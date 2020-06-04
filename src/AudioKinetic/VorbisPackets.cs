// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using NVorbis;
using System;
using System.IO;

namespace Anki.AudioKinetic
{

/// <summary>
/// This is a class to allow passing Vorbis style packets to the NVorbis assembly.
/// These packets are vended by the VorbisPacketProvider, and come from a
/// transcoder in the vgmstream ara
/// </summary>
partial class VorbisPacket:DataPacket, IDisposable
    {
    /// <summary>
    /// A tool to read bytes out
    /// </summary>
    BinaryReader reader;

    /// <summary>
    /// This wraps a single packet
    /// </summary>
    /// <param name="stream">The memory stream holding the packet</param>
    internal VorbisPacket(Stream stream)
    {
        reader= new BinaryReader(stream);
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
        reader.Dispose();
    }

    /// <summary>
    /// Reads the next byte of the packet.
    /// </summary>
    /// <returns>The next byte if available, otherwise -1.</returns>
    override protected int ReadNextByte()
    {
        // Check for the end
        if (reader.BaseStream.Position >= reader.BaseStream.Length)
            return -1;
        // Get the next byte
        return reader.ReadByte();
    }

    /// <summary>
    /// Gets the total number of bits in the packet.
    /// </summary>
    protected override int TotalBits => (int)(reader.BaseStream.Length * 8);
}
}
