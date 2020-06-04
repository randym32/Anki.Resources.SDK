/* Adapted from https://github.com/losnoco/vgmstream/tree/master/src/coding/ptadpcm_decoder.c
   Copyright © 2020 Randall Maas. All rights reserved.
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
using System.IO;

namespace Anki.AudioKinetic
{
partial class AudioAssets
{
    /**
     * IMA ADPCM algorithms (expand one nibble to one sample, based on prev sample/history and step table).
     * Nibbles are usually grouped in blocks/chunks, with a header, containing 1 or N channels
     *
     * All IMAs are mostly the same with these variations:
     * - interleave: blocks and channels are handled externally (layouts) or internally (mixed channels)
     * - block header: none (external), normal (4 bytes of history 16b + step 8b + reserved 8b) or others; per channel/global
     * - expand type: IMA style or variations; low or high nibble first
     */

    static readonly short[] ADPCMTable = {
        7, 8, 9, 10, 11, 12, 13, 14,
        16, 17, 19, 21, 23, 25, 28, 31,
        34, 37, 41, 45, 50, 55, 60, 66,
        73, 80, 88, 97, 107, 118, 130, 143,
        157, 173, 190, 209, 230, 253, 279, 307,
        337, 371, 408, 449, 494, 544, 598, 658,
        724, 796, 876, 963, 1060, 1166, 1282, 1411,
        1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024,
        3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484,
        7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
        15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
        32767
    };

    static readonly short[] IMA_IndexTable = {
        -1, -1, -1, -1, 2, 4, 6, 8,
        -1, -1, -1, -1, 2, 4, 6, 8 
    };


    /* Original IMA expansion, but using MULs rather than shift+ADDs (faster for newer processors).
     * There is minor rounding difference between ADD and MUL expansions, noticeable/propagated in non-headered IMAs. */
    static void std_ima_expand_nibble_mul(byte s, ref int  hist1, ref int step_index)
    {
        /* simplified through math from:
         *  - diff = (code + 1/2) * (step / 4)
         *   > diff = (code + 1/2) * step) / 4) * (2 / 2)
         *    > diff = (code + 1/2) * 2 * step / 8
         * final diff = [signed] ((code * 2 + 1) * step) / 8 */

        var sample_nibble = s &0xf;
        var sample_decoded = hist1;
        var step = ADPCMTable[step_index];

        var delta = (sample_nibble & 0x7);
        delta = ((delta * 2 + 1) * step) >> 3;
        if (0 != (sample_nibble & 8)) delta = -delta;
        sample_decoded += delta;

        if (sample_decoded > 32727)
            hist1 = 32767;
        else if (sample_decoded < -32768)
            hist1 = -32768;
        else
            hist1 = sample_decoded;
        step_index += IMA_IndexTable[sample_nibble];
        if (step_index < 0)
            step_index=0;
        if (step_index > 88) 
            step_index=88;
    }

    /// <summary>
    /// Decodes a frame of WWise-IMA sound into PCM samples
    /// </summary>
    /// <param name="stream">The input data stream</param>
    /// <param name="outBuf">The output sample buffer</param>
    /// <remarks>mono XBOX-IMA with header endianness and alt nibble expand (verified vs AK test demos)</remarks>
    internal static void Decode_wwise_ima(BinaryReader stream, short[] outBuf)
    {
        // Start
        int hist1 = stream.ReadInt16();
        int step_index = stream.ReadByte();
        if (step_index < 0)
            step_index=0;
        if (step_index > 88)
            step_index=88;
        stream.ReadByte(); // RCM padding?
        /* write header sample (even samples per block, skips last nibble) */
        outBuf[0] = (short)(hist1);
        var sample_count = 1;

        /* decode nibbles (layout: all nibbles from one channel) */
        for (var i = 0; i < 0x20-1; i++)
        {
            var b = stream.ReadByte();
            /* low nibble first */
            std_ima_expand_nibble_mul(b, ref hist1, ref step_index);
            outBuf[sample_count++] = (short)(hist1);
            std_ima_expand_nibble_mul((byte)(b>>4), ref hist1, ref step_index);
            outBuf[sample_count++] = (short)(hist1);
        }
        var bb = stream.ReadByte();
        /* low nibble first */
        std_ima_expand_nibble_mul(bb, ref hist1, ref step_index);
        outBuf[sample_count++] = (short)(hist1);
        /* must skip last nibble like other XBOX-IMAs, often needed (ex. Bayonetta 2 sfx) */
    }
}
}

