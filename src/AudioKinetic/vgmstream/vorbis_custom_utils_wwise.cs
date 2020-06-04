/*
   Copyright © 2020 Randall Maas. All rights reserved.
   Adapted from vorbis_custom_utils_wwise.c at
        https://github.com/losnoco/vgmstream/tree/master/src/coding
   See COPYING-vgmstream

    Copyright (c) 2008-2019 Adam Gashlin, Fastelbja, Ronny Elfert, bnnm,
                            Christopher Snowhill, NicknineTheEagle, bxaimc,
                            Thealexbarney, CyberBotX, et al

    Portions Copyright (c) 2004-2008, Marko Kreen
    Portions Copyright 2001-2007  jagarl / Kazunori Ueno <jagarl@creator.club.ne.jp>
    Portions Copyright (c) 1998, Justin Frankel/Nullsoft Inc.
    Portions Copyright (C) 2006 Nullsoft, Inc.
    Portions Copyright (c) 2005-2007 Paul Hsieh
    Portions Public Domain originating with Sun Microsystems

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Anki.AudioKinetic
{
/**
 * Inits a vorbis stream of some custom variety.
 *
 * Normally Vorbis packets are stored in .ogg, which is divided into OggS pages/packets, and the first packets contain necessary
 * Vorbis setup. For custom vorbis the OggS layer is replaced/optimized, the setup can be modified or stored elsewhere
 * (i.e.- in the .exe) and raw Vorbis packets may be modified as well, presumably to shave off some kb and/or obfuscate.
 * We'll manually read/modify the data and decode it with libvorbis calls.
 *
 * Reference: https://www.xiph.org/vorbis/doc/libvorbis/overview.html
 */
partial class Vorbis
{
    /// <summary>
    /// The number of channels in the audio file
    /// </summary>
    internal uint NumChannels;

    /// <summary>
    /// The number of samples per second from audio file
    /// </summary>
    internal uint SampleRate;

    /* Wwise Vorbis: saved data to reconstruct modified packets */
    bool[] mode_blockflag;
    uint mode_bits;
    bool prev_blockflag;

    /**
     * Wwise stores a reduced setup, and packets have mini headers with the size, and data packets
     * may reduced as well. The format evolved over time so there are many variations.
     * The Wwise implementation uses Tremor (fixed-point Vorbis) but shouldn't matter.
     *
     * Format reverse-engineered by hcs in ww2ogg (https://github.com/hcs64/ww2ogg).
     */
    readonly byte blocksize_0_exp, blocksize_1_exp;

    /// <summary>
    /// Creates an object that will later transcode the data stream
    /// </summary>
    /// <param name="blocksize_0_exp"></param>
    /// <param name="blocksize_1_exp"></param>
    internal Vorbis(byte blocksize_0_exp, byte blocksize_1_exp)
    {
        this.blocksize_0_exp=blocksize_0_exp;
        this.blocksize_1_exp=blocksize_1_exp;
    }

    /// <summary>
    /// This completely converts the input.  The sound files are so small this is reasonable.
    /// </summary>
    /// <param name="inStream">The input stream for the sound file</param>
    /// <param name="setup_ofs">The offset to the setup area</param>
    /// <param name="audio_ofs">The offset to the audio packaets area</param>
    /// <returns>A Vorbis stream</returns>
    internal NVorbis.Contracts.IPacketProvider Transcode(BinaryReader inStream, uint setup_ofs, uint audio_ofs)
    {
        return new VorbisPacketProvider(_Transcode(inStream,setup_ofs, audio_ofs));
    }

    /// <summary>
    /// This completely converts the input.  The sound files are so small this is reasonable.
    /// </summary>
    /// <param name="inStream">The input stream for the sound file</param>
    /// <returns>A Vorbis stream</returns>
    internal IEnumerable<NVorbis.Contracts.IPacket> _Transcode(BinaryReader inStream, uint setup_ofs, uint audio_ofs)
    {
        // Convert the header
        var pos = inStream.BaseStream.Position;
        inStream.BaseStream.Position=pos+setup_ofs;
        /* rebuild headers */
        // Set up the stream
        var dest1 = new MemoryStream();

        /* new identification packet */
        var pkt= new BinaryWriter(dest1, Encoding.ASCII, true);
        build_header_identification(pkt,  blocksize_0_exp, blocksize_1_exp);
        pkt.Dispose();
        dest1.Position=0;
        yield return new VorbisPacket(dest1);

        /* new comment packet */
        var dest2 = new MemoryStream();
        pkt = new BinaryWriter(dest2, Encoding.ASCII, true);
        build_header_comment(pkt);
        pkt.Dispose();
        dest2.Position=0;
        yield return new VorbisPacket(dest2);

        /* rebuild setup packet */
        var dest3 = new MemoryStream();
        pkt = new BinaryWriter(dest3, Encoding.ASCII, true);
        rebuild_setup(pkt, inStream);
        pkt.Dispose();
        dest3.Position=0;
        yield return new VorbisPacket(dest3);

        // Convert each of the source packets
        inStream.BaseStream.Position=pos+audio_ofs;
        var canContinue = true;
        while (canContinue)
        {
            // Set up the stream
            var dest = new MemoryStream();
            var destWriter = new BinaryWriter(dest, Encoding.ASCII, true);

            canContinue = vorbis_custom_parse_packet_wwise(destWriter, inStream);
            destWriter.Dispose();
            dest.Position=0;
            yield return new VorbisPacket(dest);
        }
    }


    /// <summary>
    /// Transforms a Wwise data packet into a real Vorbis one (depending on config) 
    /// </summary>
    /// <param name="outStream"></param>
    /// <param name="inStream"></param>
    /// <returns>true if there are more packets, false if not</returns>
    bool vorbis_custom_parse_packet_wwise(BinaryWriter outStream, BinaryReader inStream)
    {
        /* reconstruct a Wwise packet, if needed; final bytes may be bigger than packet_size so we get the header offsets here */
        if (inStream.BaseStream.Position+2 >= inStream.BaseStream.Length)
            return false;
        return rebuild_packet(outStream, inStream);
    }


    /* **************************************************************************** */
    /* INTERNAL HELPERS                                                             */
    /* **************************************************************************** */

    /* loads info from a wwise packet header */
    static uint get_packet_header(BinaryReader inStream)
    {
        /* packet size doesn't include header size */
        /* size 2 */
        return inStream.ReadUInt16();
    }


    /// <summary>
    /// Transforms a Wwise data packet into a real Vorbis one (depending on config) 
    /// </summary>
    /// <param name="outStream"></param>
    /// <param name="inStream"></param>
    /// <returns>true if there are more packets, false if not</returns>
    bool rebuild_packet(BinaryWriter outStream, BinaryReader inStream)
    {
        /* read Wwise packet header */
        var packet_size = get_packet_header(inStream);
        var pos = inStream.BaseStream.Position;
        if (pos+packet_size > inStream.BaseStream.Length)
            return false;
        ww2ogg_generate_vorbis_packet(outStream, inStream,packet_size);
        if (pos+packet_size >= inStream.BaseStream.Length)
            return false;
        inStream.BaseStream.Position = pos+packet_size;
        return true;
    }

    /* Transforms a Wwise setup packet into a real Vorbis one (depending on config). */
    void rebuild_setup(BinaryWriter outStream, BinaryReader inStream)
    {
        /* read Wwise packet header */
        var size = get_packet_header(inStream);
        var newPos = inStream.BaseStream.Position+size;

        ww2ogg_generate_vorbis_setup(outStream, inStream);
        inStream.BaseStream.Position=newPos;
    }

    void build_header_identification(BinaryWriter buf, byte blocksize_0_exp, byte blocksize_1_exp)
    {
        var blocksizes = (blocksize_0_exp << 4) | (blocksize_1_exp);

        buf.Write((byte)0x01);                        /* packet_type (id) */
        buf.Write(Encoding.ASCII.GetBytes("vorbis")); /* id */
        buf.Write((int) 0x00);                        /* vorbis_version (fixed) */
        buf.Write((byte) NumChannels);                /* audio_channels */
        buf.Write((int)  SampleRate);                 /* audio_sample_rate */
        buf.Write((int)  0x00);                       /* bitrate_maximum (optional hint) */
        buf.Write((int)  0x00);                       /* bitrate_nominal (optional hint) */
        buf.Write((int)  0x00);                       /* bitrate_minimum (optional hint) */
        buf.Write((byte) blocksizes);                 /* blocksize_0 + blocksize_1 nibbles */
        buf.Write((byte) 0x01);                       /* framing_flag (fixed) */
    }

    static void build_header_comment(BinaryWriter buf)
    {
        buf.Write((byte)0x03);                        /* packet_type (comments) */
        buf.Write(Encoding.ASCII.GetBytes("vorbis")); /* id */
        var vendor = Encoding.ASCII.GetBytes("Anki.AudioKinetic");
        buf.Write((int) vendor.Length);               /* vendor_length */
        buf.Write(vendor);                            /* vendor_string */
        buf.Write((int)0);                            /* user_comment_list_length */
        buf.Write((byte) 0x01);                       /* framing_flag (fixed) */
    }

    /* **************************************************************************** */
    /* INTERNAL WW2OGG STUFF                                                        */
    /* **************************************************************************** */
    /* The following code was mostly and manually converted from hcs's ww2ogg.
     * Could be simplified but roughly tries to preserve the structure in case fixes have to be backported.
     *
     * Some validations are ommited (ex. read/write), as incorrect data should be rejected by libvorbis.
     * To avoid GCC complaining all values are init to 0, and some that do need it are init again, for clarity.
     * Reads/writes unsigned ints as most are bit values less than 32 and with no sign meaning.
     */


    /* Copy packet as-is or rebuild first byte if mod_packets is used.
     * (ref: https://www.xiph.org/vorbis/doc/Vorbis_I_spec.html#x1-720004.3) */
    void ww2ogg_generate_vorbis_packet(BinaryWriter outStream, BinaryReader inStream, uint packet_size)
    {
        var next_offset=packet_size + inStream.BaseStream.Position;
        /* rebuild first bits of packet type and window info (for the i-MDCT) */
        if (null == mode_blockflag)
        { /* config error */
            throw new System.Exception("Wwise Vorbis: didn't load mode_blockflag\n");
        }

        /* audio packet type */
        var w_bits = BitWriter(outStream);
        var r_bits = BitReader(inStream);
        w_bits(1, 0);

        /* collect this packet mode from the first byte */
        var mode_number = r_bits( mode_bits); /* max 6b */
        w_bits(mode_bits, mode_number);
        var remainder = r_bits(8 - mode_bits);

        /* adjust window info */
        if (mode_blockflag[mode_number])
        {
            /* long window: peek at next frame to find flags */
            bool next_blockflag = false;
            /* check if more data / not eof */
            if (next_offset + 2 <= inStream.BaseStream.Length)
            {
                // save the position so that we can go back
                var saved_pos = inStream.BaseStream.Position;
                inStream.BaseStream.Position = next_offset;
                var next_packet_size = get_packet_header(inStream);

                if (next_packet_size > 0)
                {
                    /* get next first byte to read next_mode_number */
                    var r_bitsNext= BitReader(inStream);
                    var next_mode_number = r_bitsNext(mode_bits); /* max 6b */

                    next_blockflag = mode_blockflag[next_mode_number];
                }

                // restore the saved position
                inStream.BaseStream.Position = saved_pos;
            }

            w_bits(1, prev_blockflag?1U:0U);
            w_bits(1, next_blockflag?1U:0U);
        }

        prev_blockflag = mode_blockflag[mode_number]; /* save for next packet */

        w_bits(8 - mode_bits, remainder); /* this *isn't* byte aligned (ex. could be 10 bits written) */

        /* remainder of packet (not byte-aligned when using mod_packets) */
        for (var i = 1; i < packet_size; i++)
        {
            var c= r_bits(8);
            w_bits(8, c);
        }

        /* remove trailing garbage bits */
        w_bits(7,0);
    }



    /* Rebuild a Wwise setup (simplified with removed stuff), recreating all six setup parts.
     * (ref: https://www.xiph.org/vorbis/doc/Vorbis_I_spec.html#x1-650004.2.4) */
    void ww2ogg_generate_vorbis_setup(BinaryWriter outStream, BinaryReader inStream)
    {
        /* packet header */
        outStream.Write((byte)0x05);            /* packet_type (setup) */
        outStream.Write(Encoding.ASCII.GetBytes("vorbis")); /* id */

        /* Codebooks */
        var codebook_count_less1 = inStream.ReadByte();
        outStream.Write((byte)codebook_count_less1);
        int codebook_count = codebook_count_less1 + 1;

        var r_bits = BitReader(inStream);
        var w_bits = BitWriter(outStream);

        /* rebuild Wwise codebooks: external (referenced by id) in simplified format */
        for (var i = 0; i < codebook_count; i++)
        {
            var codebook_id = r_bits(10);
            ww2ogg_codebook_library_rebuild_by_id(w_bits, codebook_id);
        }


        /* Time domain transforms */
        w_bits(6, 0);
        w_bits(16, 0);

        /* rest of setup is altered, reconstruct */

        /* Floors */
        var floor_count_less1 = r_bits(6);
        w_bits(6, floor_count_less1);
        var floor_count = floor_count_less1 + 1;

        for (var i = 0; i < floor_count; i++)
        {
            uint[] floor1_partition_class_list = new uint[32]; /* max 5b */
            uint[] floor1_class_dimensions_list = new uint[16 + 1]; /* max 4b+1 */

            // Always floor type 1
            w_bits(16, 1);

            var floor1_partitions = r_bits(5);
            w_bits(5, floor1_partitions);

            var maximum_class = 0;
            for (var j = 0; j < floor1_partitions; j++)
            {
                var floor1_partition_class = r_bits(4);
                w_bits(4, floor1_partition_class);

                floor1_partition_class_list[j] = floor1_partition_class;

                if (floor1_partition_class > maximum_class)
                    maximum_class = (int)floor1_partition_class;
            }

            for (var j = 0; j <= maximum_class; j++)
            {
                var class_dimensions_less1 = r_bits(3);
                w_bits(3, class_dimensions_less1);

                floor1_class_dimensions_list[j] = class_dimensions_less1 + 1;

                var class_subclasses = r_bits(2);
                w_bits(2, class_subclasses);

                if (0 != class_subclasses)
                {
                    var masterbook = r_bits(8);
                    w_bits(8, masterbook);

                    if (masterbook >= codebook_count)
                    {
                        throw new System.Exception("Wwise Vorbis: invalid floor1 masterbook\n");
                    }
                }

                for (var k = 0; k < (1U << (int)class_subclasses); k++)
                {
                    var subclass_book_plus1 = r_bits(8);
                    w_bits(8, subclass_book_plus1);

                    int subclass_book = (int)subclass_book_plus1 - 1;
                    if (subclass_book >= 0 && subclass_book >= codebook_count)
                    {
                        throw new System.Exception("Wwise Vorbis: invalid floor1 subclass book\n");
                    }
                }
            }

            var floor1_multiplier_less1 = r_bits(2);
            w_bits(2, floor1_multiplier_less1);

            var rangebits = r_bits(4);
            w_bits(4, rangebits);

            for (var j = 0; j < floor1_partitions; j++)
            {
                var current_class_number = floor1_partition_class_list[j];
                for (var k = 0; k < floor1_class_dimensions_list[current_class_number]; k++)
                {
                    var X = r_bits(rangebits);
                    w_bits(rangebits, X);
                }
            }
        }


        /* Residues */
        var residue_count_less1 = r_bits(6);
        w_bits(6, residue_count_less1);
        var residue_count = residue_count_less1 + 1;

        for (var i = 0; i < residue_count; i++)
        {
            uint[] residue_cascade = new uint[64 + 1]; /* 6b +1 */

            var residue_type = r_bits(2);
            w_bits(16, residue_type); /* 2b to 16b */

            if (residue_type > 2)
            {
                throw new System.Exception("Wwise Vorbis: invalid residue type\n");
            }

            var residue_begin = r_bits(24);
            w_bits(24, residue_begin);
            var residue_end = r_bits(24);
            w_bits(24, residue_end);
            var residue_partition_size_less1 = r_bits(24);
            w_bits(24, residue_partition_size_less1);
            var residue_classifications_less1 = r_bits(6);
            w_bits(6, residue_classifications_less1);
            var residue_classbook = r_bits(8);
            w_bits(8, residue_classbook);
            var residue_classifications = residue_classifications_less1 + 1;

            if (residue_classbook >= codebook_count)
            {
                throw new System.Exception("Wwise Vorbis: invalid residue classbook\n");
            }

            for (var j = 0; j < residue_classifications; j++)
            {
                uint high_bits = 0;

                var low_bits = r_bits(3);
                w_bits(3, low_bits);

                var bitflag = r_bits(1);
                w_bits(1, bitflag);
                if (0 != bitflag)
                {
                    high_bits = r_bits(5);
                    w_bits(5, high_bits);
                }

                residue_cascade[j] = high_bits * 8 + low_bits;
            }

            for (var j = 0; j < residue_classifications; j++)
            {
                for (var k = 0; k < 8; k++)
                {
                    if (0 != (residue_cascade[j] & (1 << k)))
                    {
                        var residue_book = r_bits(8);
                        w_bits(8, residue_book);

                        if (residue_book >= codebook_count)
                        {
                            throw new System.Exception("Wwise Vorbis: invalid residue book\n");
                        }
                    }
                }
            }
        }


        /* Mappings */
        var mapping_count_less1 = r_bits(6);
        w_bits(6, mapping_count_less1);
        var mapping_count = mapping_count_less1 + 1;

        for (var i = 0; i < mapping_count; i++)
        {
            // always mapping type 0, the only one
            w_bits(16, 0);

            var submaps_flag = r_bits(1);
            w_bits(1, submaps_flag);

            uint submaps = 1;
            if (0 != submaps_flag)
            {
                var submaps_less1 = r_bits(4);
                w_bits(4, submaps_less1);
                submaps = submaps_less1 + 1;
            }

            var square_polar_flag = r_bits(1);
            w_bits(1, square_polar_flag);

            if (0 != square_polar_flag)
            {
                var coupling_steps_less1 = r_bits(8);
                w_bits(8, coupling_steps_less1);
                var coupling_steps = coupling_steps_less1 + 1;

                for (var j = 0; j < coupling_steps; j++)
                {
                    var magnitude_bits = ww2ogg_tremor_ilog(NumChannels - 1);
                    var angle_bits = ww2ogg_tremor_ilog(NumChannels - 1);

                    var magnitude = r_bits(magnitude_bits);
                    w_bits(magnitude_bits, magnitude);
                    var angle = r_bits(angle_bits);
                    w_bits(angle_bits, angle);

                    if (angle == magnitude || magnitude >= NumChannels || angle >= NumChannels)
                    {
                        throw new System.Exception(string.Format("Wwise Vorbis: invalid coupling (angle={0}, mag={1}, ch={2})\n", angle, magnitude, NumChannels));
                    }
                }
            }

            // a rare reserved field not removed by Ak!
            var mapping_reserved = r_bits(2);
            w_bits(2, mapping_reserved);
            if (0 != mapping_reserved)
            {
                throw new System.Exception("Wwise Vorbis: mapping reserved field nonzero\n");
            }

            if (submaps > 1)
            {
                for (var j = 0; j < NumChannels; j++)
                {
                    var mapping_mux = r_bits(4);
                    w_bits(4, mapping_mux);
                    if (mapping_mux >= submaps)
                    {
                        throw new System.Exception("Wwise Vorbis: mapping_mux >= submaps\n");
                    }
                }
            }

            for (var j = 0; j < submaps; j++)
            {
                // Another! Unused time domain transform configuration placeholder!
                var time_config = r_bits(8);
                w_bits(8, time_config);

                var floor_number = r_bits(8);
                w_bits(8, floor_number);
                if (floor_number >= floor_count)
                {
                    throw new System.Exception("Wwise Vorbis: invalid floor mapping\n");
                }

                var residue_number = r_bits(8);
                w_bits(8, residue_number);
                if (residue_number >= residue_count)
                {
                    throw new System.Exception("Wwise Vorbis: invalid residue mapping\n");
                }
            }
        }


        /* Modes */
        var mode_count_less1 = r_bits(6);
        w_bits(6, mode_count_less1);
        var mode_count = mode_count_less1 + 1;

        /* up to max mode_count */
        mode_blockflag = new bool[64+1];  /* max 6b+1; flags 'n stuff */
        mode_bits = ww2ogg_tremor_ilog(mode_count - 1); /* for mod_packets */

        for (var i = 0; i < mode_count; i++)
        {
            var block_flag = r_bits(1);
            w_bits(1, block_flag);

            mode_blockflag[i] = (block_flag != 0); /* for mod_packets */

            w_bits(16, 0);
            w_bits(16, 0);

            var mapping = r_bits(8);
            w_bits(8, mapping);
            if (mapping >= mapping_count)
            {
                throw new System.Exception("Wwise Vorbis: invalid mode mapping\n");
            }
        }


        /* end flag */
        w_bits(1, 1);

        /* remove trailing garbage bits */
        w_bits(7, 0);
    }


    /* rebuilds a Wwise codebook into a Vorbis codebook */
    static void ww2ogg_codebook_library_rebuild(dBitWrite w_bits, dBitRead r_bits)
    {
        w_bits(24, 0x564342); /* "VCB" */
        var dimensions = r_bits(4);
        w_bits(16, dimensions); /* 4 to 16 */
        var entries = r_bits(14);
        w_bits(24, entries); /* 14 to 24*/

        /* codeword lengths */
        var ordered = r_bits(1);
        w_bits(1, ordered);
        if (0 != ordered)
        {
            var initial_length = r_bits(5);
            w_bits(5, initial_length);

            var current_entry = 0;
            while (current_entry < entries)
            {
                var number_bits = ww2ogg_tremor_ilog((uint)(entries - current_entry));

                var number = r_bits(number_bits);
                w_bits(number_bits, number);
                current_entry += (int) number;
            }
            if (current_entry > entries)
            {
                throw new System.Exception("Wwise Vorbis: current_entry out of range\n");
            }
        }
        else
        {
            var codeword_length_length = r_bits(3);
            var sparse = r_bits(1);
            w_bits(1, sparse);

            if (0 == codeword_length_length || 5 < codeword_length_length)
            {
                throw new System.Exception("Wwise Vorbis: nonsense codeword length\n");
            }

            for (var i = 0; i < entries; i++)
            {
                var present_bool = true;
                if (0!= sparse)
                {
                    var present = r_bits(1);
                    w_bits(1, present);

                    present_bool = (0 != present);
                }

                if (present_bool)
                {
                    var codeword_length = r_bits(codeword_length_length);
                    w_bits(5, codeword_length); /* max 7 (3b) to 5 */
                }
            }
        }


        /* lookup table */
        var lookup_type = r_bits(1);
        w_bits(4, lookup_type); /* 1 to 4 */

        if (0 == lookup_type)
        {
            //VGM_LOG("Wwise Vorbis: no lookup table\n");
        }
        else if (1 == lookup_type)
        {
            //VGM_LOG("Wwise Vorbis: lookup type 1\n");
            var min = r_bits(32);
            w_bits(32, min);
            var max = r_bits(32);
            w_bits(32, max);
            var value_length = r_bits(4);
            w_bits(4, value_length);
            var sequence_flag = r_bits(1);
            w_bits(1, sequence_flag);

            var quantvals = ww2ogg_tremor_book_maptype1_quantvals(entries, dimensions);
            for (var i = 0; i < quantvals; i++)
            {
                var val_bits = value_length + 1;

                var val = r_bits(val_bits);
                w_bits(val_bits, val);
            }
        }
        else if (2 == lookup_type)
        {
            throw new System.Exception("Wwise Vorbis: didn't expect lookup type 2\n");
        }
        else
        {
            throw new System.Exception("Wwise Vorbis: invalid lookup type\n");
        }


        /* check that we used exactly all bytes */
        /* note: if all bits are used in the last byte there will be one extra 0 byte */
        //if (0 != cb_size && iw->b_off / 8 + 1 != cb_size)
        //{
            //VGM_LOG("Wwise Vorbis: codebook size mistach (expected 0x%x, wrote 0x%lx)\n", cb_size, iw->b_off/8+1);
        //}
    }


    /* rebuilds an external Wwise codebook referenced by id to a Vorbis codebook */
    static int ww2ogg_codebook_library_rebuild_by_id(dBitWrite w_bits, uint codebook_id)
    {
        /* Wwise codebook buffer */
        var tmp =new BinaryReader(new MemoryStream(wvc_list_aotuv603[codebook_id]));
        ww2ogg_codebook_library_rebuild(w_bits, BitReader(tmp));
        tmp.Dispose();
        return 1;
    }

    /* fixed-point ilog from Xiph's Tremor */
    static uint ww2ogg_tremor_ilog(uint v)
    {
        uint ret = 0;
        while (0!=v)
        {
            ret++;
            v >>= 1;
        }
        return (ret);
    }

    /* quantvals-something from Xiph's Tremor */
    static uint ww2ogg_tremor_book_maptype1_quantvals(uint entries, uint dimensions)
    {
        /* get us a starting hint, we'll polish it below */
        var bits = ww2ogg_tremor_ilog(entries);
        var vals = entries >> (int)((bits - 1) * (dimensions - 1) / dimensions);

        while (true)
        {
            ulong acc = 1;
            ulong acc1 = 1;
            uint i;
            for (i = 0; i < dimensions; i++)
            {
                acc *= vals;
                acc1 *= vals + 1;
            }
            if (acc <= entries && acc1 > entries)
            {
                return (vals);
            }
            else
            {
                if (acc > entries)
                {
                    vals--;
                }
                else
                {
                    vals++;
                }
            }
        }
    }
}
}
