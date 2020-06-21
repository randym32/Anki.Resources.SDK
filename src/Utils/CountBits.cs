// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

namespace RCM
{
partial class Util
{
    /// <summary>
    /// Counts the number of bits set
    /// </summary>
    /// <param name="i">value to count bits in</param>
    /// <returns>The number of bits set</returns>
    public static uint NumberOfBitsSet(uint i)
    {
         i = i - ((i >> 1) & 0x55555555);
         i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
         return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
    }
}
}
