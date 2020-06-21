//  Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Drawing;

/// <summary>
/// My namespace used to contain helper utilities that are not specific to the
/// rest of the framework
/// </summary>
namespace RCM
{

/// <summary>
/// A class to hold a variety of helper utilities
/// </summary>
public partial class Util
{
#if false
    /// <summary>
    /// A cache to hold the bitmaps
    /// </summary>
    static readonly MemoryCache bitmapCache = new MemoryCache("bitmaps");
#endif

    /// <summary>
    /// Open a bitmap image at the given path
    /// </summary>
    /// <param name="imagePath"></param>
    /// <returns>The bitmap image</returns>
    public static Bitmap ImageOpen(string imagePath)
    {
#if false
        // See if the image is already loaded
        var cret = bitmapCache.Get(imagePath);
        if (null != cret)
            return (Bitmap)cret;
#endif

        // We "double clutch" to work around a bug in the Windows Image handling
        // stack (specifically GdipSaveImageToStream API).  This forces a conversion
        // away, and then everything is good.
        using var D = new Bitmap(imagePath);
        var ret = new Bitmap(D);
        // cache it
        //bitmapCache.Set(imagePath, ret, policy);
        return ret;
    }
}
}
