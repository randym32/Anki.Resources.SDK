// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Emgu.CV;
using Emgu.CV.ML;
using System;

namespace Anki.Resources.SDK
{
/// <summary>
/// This class is used to perform classification/detection based on
/// images passed to an OpenCV model
/// </summary>
/// <remarks>
/// The reflectiveMazeClassifier seems to be missing the first line of the
/// file and won't load.
/// </remarks>
class Classifier_OpenCV : IProcessImage, IDisposable
{
    /// <summary>
    /// The bridge to the OpenCV interpreter that will be used to classify the
    /// image frame.
    /// </summary>
    DTrees decisionTree;

    /// <summary>
    /// The output from the classification process
    /// </summary>
    Mat output = new Mat();

    /// <summary>
    /// The result grid of the outputs
    /// </summary>
    readonly Classification[,] outputGrid = new Classification[1,1];



    /// <summary>
    /// Opens the decision tree used to classify the image
    /// </summary>
    /// <param name="path">The path to the decision tree file.</param>
    /// <param name="label">The label for what the tree recognizes.</param>
    internal Classifier_OpenCV(string path, string label=null)
    {
        // Open the classifier decision tree
        decisionTree = new DTrees();

        // Read the decision tree in from the file
        using var fs = new FileStorage(path, FileStorage.Mode.Read);
        decisionTree.Read(fs.GetFirstTopLevelNode());
        
        // Set the output label
        outputGrid[0,0].Label=label;
    }

    /// <summary>
    /// Release the IDiposable resources
    /// </summary>
    /// <param name="disposing">True if called from Dispose()</param>
    protected virtual void Dispose(bool disposing)
    {
        if (null == decisionTree)
            return;

        if (disposing)
        {
            decisionTree.Dispose();
            output.Dispose();
        }

        decisionTree = null;
        output       = null;
    }

    /// <summary>
    /// Release the IDiposable resources
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
	

    /// <summary>
    /// This returns true if the results are a list of categories, false if
    /// it is a localization
    /// </summary>
    /// <returns>true if a categorization, false if a localization</returns>
    public bool IsCategorization()
    {
        return true;
    }


    /// <summary>
    /// Processes the input image to determine the classification
    /// </summary>
    /// <param name="imageContainer">The image to process</param>
    /// <returns>If two dimensional, it is a grid of the image and the
    /// classification of each cell within it.  If one dimensional, it is list
    /// of classifications for the whole image. </returns>
    public Classification[,] ProcessImage(ImageContainer imageContainer)
    {
        // Convert the image to a shape that openCV can work with
        using var f = imageContainer.image.Reshape(1, 1); // flatten to a single row
        f.ConvertTo(f, Emgu.CV.CvEnum.DepthType.Cv32F);

        // Apply the decision tree to the input image
        var g = decisionTree.Predict(f, output);

        var d = (float[,]) output.GetData();
        outputGrid[0, 0].Probability = d[0,0];

        // Return the classification
        return outputGrid;
    }
}
}
