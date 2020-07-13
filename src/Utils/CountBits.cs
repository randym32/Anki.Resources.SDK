// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

namespace RCM
{
partial class Util
{
    /// <summary>
    /// Counts the number of bits set
    /// </summary>
    /// <param name="value">The value to count bits in</param>
    /// <returns>The number of bits set</returns>
    public static uint NumberOfBitsSet(uint value)
    {
         value = value - ((value >> 1) & 0x55555555);
         value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
         return (((value + (value >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
    }
}
}
