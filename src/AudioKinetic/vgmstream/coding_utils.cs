/* Adapted https://github.com/losnoco/vgmstream/tree/master/src/coding/coding_utils.c
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
partial class Vorbis
{
    delegate uint dBitRead(uint num_bits);

    /* Read bits (max 32) from buf and update the bit offset. Vorbis packs values in LSB order and byte by byte.
     * (ex. from 2 bytes 00100111 00000001 we can could read 4b=0111 and 6b=010010, 6b=remainder (second value is split into the 2nd byte) */
    static dBitRead BitReader(BinaryReader inStream)
    {
        var pos = 8; /* bit sub-offset */
        byte buf = 0;
        return (uint num_bits) =>
        {
            if (num_bits > 32 || num_bits <= 0) return 0;
            uint value = 0; /* set all bits to 0 */
            for (var i = 0; i < num_bits; i++)
            {
                if (pos == 8)
                {
                    pos = 0;
                    buf = inStream.ReadByte();
                }
                var bit_buf = (1U << pos) & 0xFF;   /* bit check for buf */

                if (0 != (buf & bit_buf))        /* is bit in buf set? */
                    value |= 1U << i;          /* set bit */

                pos++;                          /* new byte starts */
            }
            return value;
        };
    }
    delegate void dBitWrite(uint num_bits, uint value);
    /* Write bits (max 32) to buf and update the bit offset. Vorbis packs values in LSB order and byte by byte.
     * (ex. writing 1101011010 from b_off 2 we get 01101011 00001101 (value split, and 11 in the first byte skipped)*/
    static dBitWrite BitWriter(BinaryWriter outStream)
    {
        byte buf = 0;
        var pos = 0; /* bit sub-offset */
        return (uint num_bits, uint value) =>
        {
            if (num_bits > 32 || num_bits <= 0) return;

            for (var i = 0; i < num_bits; i++)
            {
                byte bit_buf = (byte)(1U << pos);   /* bit to set in buf */

                if (0 != (value & (1 << i)))      /* is bit in val set? */
                    buf |= bit_buf;    /* set bit */
                //else
                //    buf &= (byte)~bit_buf;   /* unset bit */

                pos++;                          /* new byte starts */
                if (pos == 8)
                {
                    pos = 0;
                    outStream.Write(buf);
                    buf = 0;
                }
            }
        };
    }
}
}

