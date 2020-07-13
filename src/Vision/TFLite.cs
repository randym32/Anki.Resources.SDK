// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.TF.Lite;
using System;
using System.Collections.Generic;
using System.Drawing;



namespace Anki.Resources.SDK
{

/// <summary>
/// This class is used to perform classification/detection based on
/// images passed to a TFLite model.
/// </summary>
public class Classifier_TFLite : IProcessImage, IDisposable
{
    /// <summary>
    /// The flat-buffer for the TF-lite model.  This needs to stay around for
    /// the life of the interpreter, otherwise you will get weird crashes
    /// </summary>
    FlatBufferModel flatBufferModel;

    /// <summary>
    /// The bridge to the TFlite interpreter that will be used to classify
    /// the images.
    /// </summary>
    Interpreter interpreter;

    /// <summary>
    /// The input to the model.
    /// </summary>
    /// <value>
    /// The input to the model.
    /// </value>
    public Tensor InputTensor { get; private set; }


    /// <summary>
    /// The size of the input image buffer that the model works with
    /// </summary>
    readonly Size inputSize;

    /// <summary>
    /// The data type of the input that the model works with
    /// </summary>
    readonly DataType inputType;

    /// <summary>
    /// How much to scale the input pixels by to get them into the range
    /// expected by the model.  Each pixel is multiplied by this number.
    /// </summary>
    internal float inputScale=1.0f/255.0f;

    /// <summary>
    /// How much to offset the input pixels by to get them into the range
    /// expected by the model
    /// </summary>
    internal float inputOfs = 0.0f;

    /// <summary>
    /// The output from the model.
    /// </summary>
    /// <value>
    /// The output from the model.
    /// </value>
    public Tensor OutputTensor { get; private set; }

    /// <summary>
    /// The labels for the potential outputs
    /// </summary>
    readonly IReadOnlyList<string> labels;

    /// <summary>
    /// The number of rows in the grid of classifications.
    /// </summary>
    readonly int numRows;

    /// <summary>
    /// The number of columns in the grid of classifications.
    /// </summary>
    readonly int numColumns;

    /// <summary>
    /// If true, this is a localization process -- producing a grid.
    /// If false, it is a classification process that may produce it's top
    /// contenders.  The MobileNet v1's highest probability item is usually
    /// wrong, but the right one is often in the top 5.
    /// </summary>
    readonly bool isLocalization;

    /// <summary>
    /// An array used to sort the categorization results.
    /// </summary>
    readonly int[] indices;

    /// <summary>
    /// The result grid of the outputs
    /// </summary>
    internal readonly Classification[,] outputGrid;

    /// <summary>
    /// Initializes the node to process the 
    /// </summary>
    /// <param name="path">The path to the model file</param>
    /// <param name="inputName">The name of the input to use</param>
    /// <param name="outputName">The name of the output to use</param>
    /// <param name="labels">The labels used for each of the classification
    /// steps.</param>
    /// <param name="numColumns">The number of columns in the grid of
    /// classifications.</param>
    /// <param name="numRows">The number of rows in the grid of classifications.</param>
    public Classifier_TFLite(string path, string inputName, string outputName,
                                 IReadOnlyList<string> labels,
                                 int numColumns=1, int numRows=1)
    {
        // Load the flatbuffer model definition for the TFlite model
        flatBufferModel = new FlatBufferModel(path);
        if (!flatBufferModel.CheckModelIdentifier())
            throw new Exception("Model identifier check failed");
        // Set up the TFLite interpreter to allow using the model
        interpreter         = new Interpreter(flatBufferModel);
        var status = interpreter.AllocateTensors();
        if (status == Status.Error)
            throw new Exception("Failed to allocate tensor");

        // Get the inputs and outputs from the model
        // Find the input
        foreach (var tensor in interpreter.Inputs)
        {
            InputTensor = tensor;
            // Does the specified input match the name of this tensor?
            if (inputName == tensor.Name)
            {
                // Yes, they match
                break;
            }
        }
        // Find the output
        foreach (var tensor in interpreter.Outputs)
        {
            OutputTensor = tensor;
            // Does the specified output match the name of this tensor?
            if (outputName == tensor.Name)
            {
                // Yes, they match
                break;
            }
        }

        // Get the size from input dimensions
        // If the number of elements in Dims is 3, they are:
        //   0: the height
        //   1: the width
        //   2: the number of channels (ie 3 for RGB)
        // If the number of elements in Dims is 4, then:
        //   0: the number of images passed
        //   1: the height
        //   2: the width
        //   3: the number of channels (ie 3 for RGB)
        var dim = InputTensor.Dims;
        var dimOfs = 0;
        if (dim.Length > 3)
            dimOfs = 1;
        var height = dim[dimOfs+0];
        var width  = dim[dimOfs+1];
        inputSize = new Size(width, height);

        // Get whether it is a float, int, etc
        inputType = InputTensor.Type;
        if (inputType != DataType.Float32 && inputType != DataType.UInt8)
        {
            throw new Exception(String.Format($"Data Type of {inputType} is not supported."));
        }

        this.labels = labels;

        isLocalization = numColumns * numRows != 1;
        if (!isLocalization)
        {
            // Create an array to be sorted
            indices = new int[labels.Count];
            for (var idx = 0; idx < indices.Length; idx++)
                indices[idx] = idx;
        }

        // Allocate the grid of regions that are classified
        this.numColumns = numColumns;
        this.numRows    = numRows;
        outputGrid = new Classification[isLocalization?numRows:(labels.Count<5?numRows:5), numColumns];
    }



    /// <summary>
    /// Release the IDiposable resources
    /// </summary>
    /// <param name="disposing">True if called from Dispose()</param>
    protected virtual void Dispose(bool disposing)
    {
        if (null == interpreter)
            return;

		// Dispose of the unmanaged resouces
        if (disposing)
        {
            InputTensor.Dispose();
            OutputTensor.Dispose();
            interpreter.Dispose();
            flatBufferModel.Dispose();
        }

        InputTensor    = null;
        OutputTensor   = null;
        interpreter    = null;
        flatBufferModel= null;
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
        return !isLocalization;
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
        // Get a version of the image that is the size and depth that we work with
        // and then pass the image to the input tensor for the model

        // We will key off of the type of the tensor
        if (inputType == DataType.Float32)
        {
            // Get the image of the size and types, in TFLite's RGB
            var image = imageContainer.Image(inputSize, DepthType.Cv32F, true);

            // Rescale, and offset the image
            using Mat matF = new Mat(
                size: inputSize,
                type: Emgu.CV.CvEnum.DepthType.Cv32F,
                channels:3,
                data: InputTensor.DataPointer,
                step: sizeof(float) * 3 * inputSize.Width);
            // This is to put the pixels in the right type and range;  It scales
			// each and then offsets them, usually putting them into the range
			// 0..1 or -1..1
            image.ConvertTo(matF, Emgu.CV.CvEnum.DepthType.Cv32F, inputScale, inputOfs);
        }
        else if (inputType == DataType.UInt8)
        {
            // Get the image of the size and types, in TFLite's RGB
            var image = imageContainer.Image(inputSize, DepthType.Cv8U, true);
            using Mat matB = new Mat(
                size: inputSize,
                type: Emgu.CV.CvEnum.DepthType.Cv8U,
                channels: 3,
                data: InputTensor.DataPointer,
                step: sizeof(byte) * 3 * inputSize.Width);
            image.CopyTo(matB);
        }

        // And have the model be interpreted
        var status = interpreter.Invoke();
        if (status == Status.Error)
            throw new Exception("TF lite invocation failed.");

        // Form the output grid of the classification results
        var data = (float[])OutputTensor.Data;
        if (!isLocalization)
        {
            // Find the "best" classification for this cell
            // Sort the items from most to least
            Array.Sort(indices, (a, b) => { return data[a] < data[b] ? 1 : data[a] > data[b] ? -1 : 0; });
            // Keep the best item(s)
            var num = indices.Length < 5 ? 1 : 5;
            for (var idx = 0; idx < num; idx++)
            {
                // Set the classification and probability
                outputGrid[idx,0].Label       = labels[indices[idx]];
                outputGrid[idx,0].Probability = data  [indices[idx]];
            }
        }
        else
        {
            // The output is a grid, row, column order
            var idx = 0;
            for (var row = 0; row < numRows; row++)
                for (var col = 0; col < numColumns; col++, idx++)
                {
                    outputGrid[row, col].Label       = labels[0];
                    outputGrid[row, col].Probability = data[idx];
                }
        }

        // Return the classification
        return outputGrid;
    }
}
}
