// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Drawing;

namespace Anki.Resources.SDK
{

/// <summary>
/// This is used to hold the image from the camera and converted sizes
/// </summary>
public class ImageContainer : IDisposable
{
    /// <summary>
    /// The source image.  The default is BGR, openCV's preferred format.
    /// </summary>
    internal Mat image;

    /// <summary>
    /// Whether the image is in RGB or BGR order.
    /// </summary>
    bool inRGBOrder1;

    // the following are unscaled resized versions
    /// <summary>
    /// The size of the first cached image buffer
    /// </summary>
    Size size1;

    /// <summary>
    /// The type of the image pixel elements
    /// </summary>
    Emgu.CV.CvEnum.DepthType depth1;

    /// <summary>
    /// The cached resized image
    /// </summary>
    Mat image1;

    /// <summary>
    /// The size of the second cached image buffer
    /// </summary>
    Size size2;

    /// <summary>
    /// Whether the image is in RGB or BGR order.
    /// </summary>
    bool inRGBOrder2;

    /// <summary>
    /// The type of the image pixel elements
    /// </summary>
    Emgu.CV.CvEnum.DepthType depth2;

    /// <summary>
    /// The cached resized image
    /// </summary>
    Mat image2;

    /// <summary>
    /// The size of the second cached image buffer
    /// </summary>
    Size size3;

    /// <summary>
    /// Whether the image is in RGB or BGR order.
    /// </summary>
    bool inRGBOrder3;

    /// <summary>
    /// The type of the image pixel elements
    /// </summary>
    Emgu.CV.CvEnum.DepthType depth3;

    /// <summary>
    /// The cached resized image
    /// </summary>
    Mat image3;

    /// <summary>
    /// Set if the object was already disposed
    /// </summary>
    private bool disposedValue;

    /// <summary>
    /// Creates the container of the image and it's various sizes
    /// </summary>
    /// <param name="image">The original image</param>
    public ImageContainer(Mat image)
    {
        this.image = image;
    }

    /// <summary>
    /// Dispose of the internal stuff
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
            return;

        if (disposing)
        {
            // Dispose the cached images
            image.Dispose();
            image = null;
            if (null != image1)
                image1.Dispose();
            if (null != image2)
                image2.Dispose();
            if (null != image3)
                image3.Dispose();
            image1 = null;
            image2 = null;
            image3 = null;
        }

        disposedValue = true;
    }

    /// <summary>
    /// Releases the internal resources
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    /// <summary>
    /// This is used to get a representation of the image at the requested size
    /// </summary>
    /// <param name="size">The size of the image</param>
    /// <param name="depth">The type of the image pixel elements</param>
    /// <param name="inRGBOrder">If true, the image should be in RGB order; false is BGR</param>
    /// <returns>The image</returns>
    public Mat Image(Size size, Emgu.CV.CvEnum.DepthType depth, bool inRGBOrder)
    {
        // Use the source image if the size matches
        if (image.Width == size.Width && image.Height == size.Height && image.Depth == depth && false == inRGBOrder)
            return image;
        if (size1.Width == size.Width && size1.Height == size.Height && depth1 == depth && inRGBOrder1 == inRGBOrder)
            return image1;
        if (size2.Width == size.Width && size2.Height == size.Height && depth2 == depth && inRGBOrder2 == inRGBOrder)
            return image2;
        if (size3.Width == size.Width && size3.Height == size.Height && depth3 == depth && inRGBOrder3 == inRGBOrder)
            return image3;

        // Get the resized image.  We cache the native format, as openCV may work with it
        Mat resizedImage;
        if (false == inRGBOrder)
        {
            // Resize the image to one that we work with
            // Create a new image to resize that
            resizedImage = new Mat();
            CvInvoke.Resize(
                    src: image,
                    dst: resizedImage,
                    dsize: size,
                    fx : 0,
                    fy : 0,
                    interpolation: Inter.Cubic);

            // Convert to the pixel format that is expected
            if (resizedImage.Depth != depth)
            {
                // Convert the type of image element
                resizedImage.ConvertTo(resizedImage, depth);
            }
        }
        else
        {
            // Get a cached resized version of the image
            // Note: the caching is responsible for disposing of it later
            var cachedImage = Image(size, depth, false);
            resizedImage = new Mat();
            // Swap the bytes around to make the new one
            CvInvoke.CvtColor(cachedImage, resizedImage, ColorConversion.Bgr2Rgb);
        }

        // save it in the cache
        if (null == image1)
        {
            image1 = resizedImage;
            size1  = size;
            depth1 = depth;
            inRGBOrder1 = inRGBOrder;
        }
        else if (null == image2)
        {
            image2 = resizedImage;
            size2  = size;
            depth2 = depth;
            inRGBOrder2 = inRGBOrder;
        }
        else if (null == image3)
        {
            image3 = resizedImage;
            size3  = size;
            depth3 = depth;
            inRGBOrder3 = inRGBOrder;
        }

        // return the result
        return resizedImage;
    }
}
}
