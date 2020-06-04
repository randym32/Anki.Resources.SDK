using System;
using System.Collections.Generic;
using NVorbis;
using NVorbis.Contracts;

namespace Anki.AudioKinetic
{

/// <summary>
/// This is used to vend packets (when pulled) from the internal transcoder
/// </summary>
partial class VorbisPacketProvider:NVorbis.Contracts.IPacketProvider
{
    /// <summary>
    ///  Let the NVorbis library know that we do no support seeking.
    /// </summary>
    public bool CanSeek => false;
    /// <summary>
    ///  Gets the serial number of this provider's data stream.
    /// </summary>
    public int StreamSerial { get; }
    /// <summary>
    /// Gets the total number of granule available in the stream.
    /// Not supported
    /// </summary>
    /// <returns></returns>
    public long GetGranuleCount() => throw new NotSupportedException();

    /// <summary>
    /// Not supported
    /// </summary>
    /// <param name="granulePos"></param>
    /// <param name="preRoll"></param>
    /// <param name="getPacketGranuleCount"></param>
    /// <returns></returns>
    public long SeekTo(long granulePos, int preRoll, GetPacketGranuleCount getPacketGranuleCount) => throw new NotSupportedException();

    /// <summary>
    /// This is a convenience enumerator to pull each successive packet from the
    /// underlying sytem
    /// </summary>
    readonly IEnumerator<IPacket> enumerator;

    /// <summary>
    /// The packet most recently peeked
    /// </summary>
    IPacket a;

    /// <summary>
    /// Creates a packet provider that vends packets from an enumerator
    /// </summary>
    /// <param name="itr">The enumerator to vend from</param>
    internal VorbisPacketProvider(IEnumerable<NVorbis.Contracts.IPacket> itr)
    {
        enumerator = itr.GetEnumerator();
    }

    /// <summary>
    /// Gets the next packet in the stream without advancing to the next packet position.
    /// </summary>
    /// <returns>The NVorbis.Contracts.IPacket instance for the next packet if available, otherwise null</returns>
    public IPacket GetNextPacket()
    {
        if (null == a)
            PeekNextPacket();
        var ret = a;
        a= null;
        return ret;
    }

    /// <summary>
    ///  Gets the next packet in the stream without advancing to the next packet position.
    /// </summary>
    /// <returns>The NVorbis.Contracts.IPacket instance for the next packet if available, otherwise null</returns>
    public IPacket PeekNextPacket()
    {
        if (null != a)
            return a;
        if (!enumerator.MoveNext())
            return null;
        a = enumerator.Current;
        return a;
    }
}
}
