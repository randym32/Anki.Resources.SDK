// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace Anki.Resources.SDK
{

/// <summary>
/// Vector, while his system starts up, plays an animation on the screen.  The
/// boot animation file is a series of uncompressed frames that are played in a
/// loop. Each frame is 184x96 pixels; each pixel is in the RGB565 format.
/// 
/// This encodes the animation file.
/// </summary>
public partial class BootAnimationEncoder
{
    /// <summary>
    /// This holds the data for the animation stream
    /// </summary>
    readonly List<byte[]> buffers = new List<byte[]>();

    /// <summary>
    /// The number of frames in the animation
    /// </summary>
    public int NumFrames => buffers.Count;


    public BootAnimationEncoder()
    {
    }


    /// <summary>
    /// Adds the image to the list of frames
    /// </summary>
    /// <param name="image">The image frame to add to the file</param>
    public void AddFrame(Bitmap image)
    {
        // Create thing to hold the results
        using (var dest = new Bitmap(Assets.VectorDisplayWidth, Assets.VectorDisplayHeight,
            PixelFormat.Format16bppRgb565))
        {
            // Scale the image and copy it into the destination buffer
            using (var g = Graphics.FromImage(dest))
                g.DrawImage(image, 0, 0);
                //g.DrawImageUnscaled(image, 0, 0);

            // Get a copy of the byte array 
            var r  = new Rectangle(0, 0, Assets.VectorDisplayWidth, Assets.VectorDisplayHeight);
            var bd = dest.LockBits(r, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            var frameData = new byte[Assets.VectorDisplayWidth * Assets.VectorDisplayHeight * 2];

            // TODO: do I need to strip out padding?
            Marshal.Copy(bd.Scan0, frameData, 0, frameData.Length);
            dest.UnlockBits(bd);

            // Add the buffer tot he end of the array
            buffers.Add(frameData);
        }
    }

    // Create a temporary file
    // Write
    // Move/replace
    // Delete the other one
}
}


