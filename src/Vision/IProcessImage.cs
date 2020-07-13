// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

using System;

namespace Anki.Resources.SDK
{

/// <summary>
/// The classification in each grid cell.
/// </summary>
public struct Classification:IEquatable<Classification>
{
    /// <summary>
    /// The label associated with the classification.
    /// </summary>
    /// <value>
    /// The label associated with the classification.
    /// </value>
    public string Label { get; internal set; }

    /// <summary>
    /// The probability associated with the classification.
    /// </summary>
    /// <value>
    /// The probability associated with the classification.
    /// </value>
    public float Probability { get; internal set; }

    public override bool Equals(object obj)
    {
        if (obj is Classification b)
            return Equals(b);
        return false;
    }
    public bool Equals(Classification b)
    {
        return Label == b.Label && Probability == b.Probability;
    }
    public static bool operator ==(Classification a, Classification b)
    {
        return a.Label == b.Label && a.Probability==b.Probability;
    }
    public static bool operator !=(Classification a, Classification b)
    {
        return a.Label != b.Label || a.Probability != b.Probability;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Probability);
    }
}


/// <summary>
/// This interface allows us to work with many different kinds of image
/// processing steps.
/// </summary>
public interface IProcessImage
{
    /// <summary>
    /// This returns true if the results are a list of categories, false if
    /// it is a localization
    /// </summary>
    /// <returns>true if a categorization, false if a localization</returns>
    bool IsCategorization();

    /// <summary>
    /// Processes the input image to determine the classification
    /// </summary>
    /// <param name="imageContainer">The image to process</param>
    /// <returns>If two dimensional, it is a grid of the image and the
    /// classification of each cell within it.  If one dimensional, it is list
    /// of classifications for the whole image. </returns>
    Classification[,] ProcessImage(ImageContainer imageContainer);
}
}
