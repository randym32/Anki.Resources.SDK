// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.Text;

namespace RCM
{
/// <summary>
/// A class intended to help read these files nicely
/// </summary>
public partial class Util
{
    static Util()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Map a 4-character tag string to the UInt32 form
    /// </summary>
    /// <param name="tagStr">The 4 character tag string</param>
    /// <returns>The representation as a machine word</returns>
    public static uint Tag(string tagStr)
    {
        return BitConverter.ToUInt32(Encoding.GetEncoding(1252).GetBytes(tagStr), 0);
    }
}
}
