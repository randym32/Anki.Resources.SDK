// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Anki.Resources.SDK
{
/// <summary>
/// Vector, while his system starts up, plays an animation on the screen.  The
/// boot animation file is a series of uncompressed frames that are played in a
/// loop. Each frame is 184x96 pixels; each pixel is in the RGB565 format.
/// 
/// This decodes the animation file.
/// </summary>
public partial class BootAnimationDecoder
{
    /// <summary>
    /// This holds the data for the animation stream
    /// </summary>
    readonly byte[] buffer;

    /// <summary>
    /// The number of frames in the animation
    /// </summary>
    public int NumFrames { get; }


    /// <summary>
    /// This loads and decodes the "raw" boot animation file.
    /// </summary>
    /// <param name="path">The path to the file</param>
    public BootAnimationDecoder(string path)
    {
        // Load the animation
        using (var fs = new FileStream(path, FileMode.Open))
        using (var memoryStream = new MemoryStream((int)fs.Length))
        {
            fs.CopyTo(memoryStream);
            buffer = memoryStream.ToArray();
        }
        // Compute the number of frames
        NumFrames = buffer.Length / (Assets.VectorDisplayWidth * Assets.VectorDisplayHeight * 2);
    }


    /// <summary>
    /// This decodes a frame of the animation and returns it as a bitmap
    /// </summary>
    /// <param name="frameIdx">The frame number to decode</param>
    /// <returns></returns>
    public Bitmap Frame(int frameIdx)
    {
        // Check that it is in range
        if (frameIdx >= NumFrames)
            return null;

        // Compute the offset to the start of the frame
        var startOfs = Assets.VectorDisplayWidth * Assets.VectorDisplayHeight * 2*frameIdx;

        // The conversion of each frame to bitmap takes some input from the
        // following stackover flow note that emphasized padding for alignment
        // https://stackoverflow.com/questions/55765996/c-how-to-load-a-raw-image-format-rgb565-into-a-bitmap

            // Create empty Bitmap and inject byte arrays data into bitmap's data area
            var bmp   = new Bitmap(Assets.VectorDisplayWidth, Assets.VectorDisplayHeight, PixelFormat.Format16bppRgb565);

        // Lock the bitmap's bits.  
        var rect   = new Rectangle(0, 0, Assets.VectorDisplayWidth, Assets.VectorDisplayHeight);
        var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format16bppRgb565);

        var ptrToFirstPixel = bmpData.Scan0;

        // *** Attention:  Bitmap specification requires, to pad row size to a multiple of 4 Bytes 
        // *** See:        https://upload.wikimedia.org/wikipedia/commons/c/c4/BMPfileFormat.png
        // *** Solution:   Copy buffer[] to buffer2[] and pay attention to padding (!!) at the end of each row
        Byte[] buffer2 = new Byte[Assets.VectorDisplayHeight * bmpData.Stride];
        for (int y = 0; y < 240; y++)
            Buffer.BlockCopy(buffer, y * Assets.VectorDisplayWidth * 2 + startOfs,
                             buffer2,y * bmpData.Stride, Assets.VectorDisplayWidth * 2);

        // *** Use padded buffer2 instead of buffer1
        Marshal.Copy(buffer2, 0, ptrToFirstPixel, buffer2.Length);
        bmp.UnlockBits(bmpData);

        // REturn the new bitmap.
        return bmp;
    }
}
}


