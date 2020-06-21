// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using RCM;

namespace Anki.Resources.SDK
{

/// <summary>
/// This is a class intended to support a variety of classifiers and detectors,
/// such as those built on Open CV, TFlite, and off-boarded (bumped to an
/// external server.)
/// </summary>
public class ClassifierInfo
{
    /// <summary>
	/// The name of the classifier
    /// </summary>
	public string Name {get;internal set; }

    /// <summary>
	/// The type of classifier model -- e.g. OpenCV, TFLite, or Offboard
    /// </summary>
	public readonly string Type;

    /// <summary>
	/// The full path to the classifier file
    /// </summary>
	public readonly string FullPath;

    /// <summary>
	/// The path (relative to the cozmo resources) to the model file
    /// </summary>
    internal string FilePath;

    /// <summary>
    /// The default parameters for a TF lite node that isn't fully configured right
    /// </summary>
    static readonly IReadOnlyDictionary<string, object> TFLite_defaults = new Dictionary<string,object>()
    {
        {"inputLayerName"  , "input_1" },
        {"outputLayerNames", "output_node0" },
        // The mobilenet seems to work with a few different values, but it both
        // works well with this, and these are typical values
        {"inputScale"      , 127.5 }, // When input is float, data is first divided by scale and then shifted
        {"inputShift"      , -1 },    // I.e.:  float_input = data / inputScale + inputShift
        {"minScore"        , 0.5 }
    };

    /// <summary>
	/// The parameters for interpreting the model.
	/// As there is a variety of models the kinds of parameters can vary a lot
    /// </summary>	
	public IReadOnlyDictionary<string, object> Parameters {get; internal set; }

    /// <summary>
    /// 
    /// </summary>
	public IReadOnlyDictionary<string, object> Definition {get; internal set; }
	
    /// <summary>
	/// The path to the labels file
    /// </summary>
	internal string LabelsPath;

    /// <summary>
    /// The labels for the potential outputs.  This is shared with the
    /// classifier as well.
    /// </summary>
    internal string[] labels;

    /// <summary>
    /// The server address, in the case of the off-boarded vision
    /// </summary>
    internal string serverAddress;

    /// <summary>
    /// The labels for the potential outputs.  This is shared with the
    /// classifier as well.
    /// </summary>
    public IReadOnlyList<string> Labels
    { get {
        if (null != labels|| null == LabelsPath)
            return labels;

        // Read the labels
        // But strip off "###:" from them
        var pat = new Regex("^\\d+:\\s*");
        var tmp = new List<string>();
        foreach (var line in File.ReadAllLines(LabelsPath))
        {
            var m = pat.Match(line);
            if (!m.Success)
                tmp.Add(line);
            else
                tmp.Add(line.Substring(m.Value.Length));
        }
        return labels = tmp.ToArray();
    } }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name">The name of the classifier</param>
    /// <param name="Type">The type of classifier model -- e.g. OpenCV, TFLite, or Offboard</param>
    /// <param name="FilePath">The relative path to the model file</param>
    /// <param name="FullPath">The full path to the classifier file</param>
    internal ClassifierInfo(string Name, string Type, string FilePath=null, string FullPath=null)
    {
        this.Name    = Name;
        this.FilePath= FilePath;
        this.Type    = Type;
        this.FullPath= FullPath;
    }

    /// <summary>
    /// Creates a classifier to process images
    /// </summary>
    /// <returns>null on error, otherwise the classifier</returns>
    /// <remarks>The object may be disposable as well</remarks>
    public IProcessImage CreateClassifier()
    {
        // Is this a TFLite based classifier?
        if ("TFLite" ==  Type && null != FullPath)
        {
            // Create a TF lite recognizer
            // First, get all of the configuration information
            // Use a default if there are no parameters at all in the config file.
            // This is used with the mobilenet that is included, but not configured
            var Parms = Parameters ?? TFLite_defaults;
            object inputName=null, outputName=null;
            object inputScale=1.0f, inputShift=0.0f;
            Parms.TryGetValue("inputLayerName"  , out inputName);
            Parms.TryGetValue("outputLayerNames", out outputName);
            if (!Parms.TryGetValue("numGridRows", out var numGridRows)) numGridRows=1;
            if (!Parms.TryGetValue("numGridCols", out var numGridCols)) numGridCols=1;
            Parms.TryGetValue("inputScale"      , out inputScale);
            Parms.TryGetValue("inputShift"      , out inputShift);
            // The JSON can be interpreted with a lot of different potential
            // types... clean that up
            if (inputScale is double d)
                inputScale = (float)d;
            if (inputScale is int i)
                inputScale = (float)i;
            if (inputShift is double d1)
                inputShift = (float)d1;
            if (inputShift is int i1)
                inputShift = (float)i1;
            if (numGridRows is double d2)
                numGridRows = (int)d2;
            if (numGridCols is double d3)
                numGridCols = (int)d3;

            // Create the classifier
            var classifier = new Classifier_TFLite(
                path      : FullPath,
                inputName : (string) inputName,
                outputName: (string) outputName,
                labels    : Labels,
                numColumns: (int)numGridCols,
                numRows   : (int)numGridRows
            );
            // Adjust the last remaining bits
			if (null != inputScale)
				classifier.inputScale = 1.0f/(float) inputScale;
            classifier.inputOfs   = (float) inputShift;
            return classifier;
        }

        // Is this a OpenCV based classifier?
        if ("OpenCV" ==  Type && null != FullPath)
            return new Classifier_OpenCV(
                path : FullPath,
                label: labels?[0]);

        // Didn't know how to create the object
        return null;
    }
}


partial class Assets
{
    /// <summary>
    /// A table of the classifiers (and detectors) included or configured in
    /// the resources.
    /// </summary>
    public IReadOnlyDictionary<string,ClassifierInfo> Classifiers {get; internal set; }

    /// <summary>
    /// The folder holding the vision processing resources
    /// </summary>
    const string visionFolder = "config/engine/vision";

    /// <summary>
    /// This gets the information about the vision classifiers/detectors
    /// </summary>
    void LoadClassifierInfo()
    {
        // We'll create a table of each of the classifier/detectors in the
        // resources.
        var _classifiers      = new Dictionary<string,ClassifierInfo>();

        // Scan over the vision config stuff
        // First a catalog the open CV classifiers 
        var openCVClassifiers = Util.BuildNameToRelativePathXref(Path.Combine(cozmoResourcesPath,visionFolder), "yaml");

        // Look up any configuration for those
        // Start with the Ground plane classifier
        if (visionConfig.TryGetValue("GroundPlaneClassifier", out var _gp))
        {
            var gp = (Dictionary<string,object>) _gp;
            // Catch whether or not an item is added
            bool addedGP = false;

            // See if there ia file name we can use to match ours
            if (gp.TryGetValue("FileOrDirName", out var path))
            {
                // See which, of any, files match
                var name = Path.GetFileNameWithoutExtension((string)path);
                if (openCVClassifiers.TryGetValue(name, out var cvPath))
                {
                    // Update the record with it
                    // Create a classifier info record for it
                    var classifier = new ClassifierInfo(
                               Name    : name,
                               Type    : "OpenCV",
                               FilePath: "GroundPlaneClassifier",
                               FullPath: cvPath);
                    classifier.Parameters= gp;
                    _classifiers[classifier.Name] = classifier;
                    openCVClassifiers.Remove(name);
                    addedGP = true;
                }
            }

            // If there wasn't a file found, create a dummy entry
            if (!addedGP)
            {
                var classifier = new ClassifierInfo(
                            Name    : "GroundPlaneClassifier",
                            Type    : "OpenCV",
                            FilePath: (string)path);
                _classifiers["GroundPlaneClassifier"] = classifier;
                classifier.Parameters=gp;
            }
        }
        // Transfer the rest of the open CV classifiers to our table
        foreach (var classifier in openCVClassifiers)
        {
            // For now, default the name to be the file name without the extension
            var name = Path.GetFileNameWithoutExtension(classifier.Key);

            // Create a classifier info record for it
            _classifiers[name] = new ClassifierInfo(
                        Name    : name,
                        Type    : "OpenCV",
                        FilePath: classifier.Key,
                        FullPath: classifier.Value);
            _classifiers[name].labels = new string[]{"groundPlane"};
        }

        // Load the illumination classifier info
        if (visionConfig.TryGetValue("IlluminationDetector", out var illd))
        {
            // Set up the structure
            var illumDetect = (Dictionary<string,object>) illd;
            illumDetect.TryGetValue("ClassifierConfigPath", out var path);
            var classifier = new ClassifierInfo(
                        Name    : "IlluminationDetector",
                        Type    : "LinearClassifier",
                        FilePath: (string) path,
                        FullPath: null!=path?Path.Combine(cozmoResourcesPath,(string)path):null);
            _classifiers["IlluminationDetector"] = classifier;
            classifier.Parameters=illumDetect;

            // Read the illumination classifier
            if (null != classifier.FullPath)
            {
                // Get the text for the file
                var text = File.ReadAllText(classifier.FullPath);
                // The conversion options
                var JSONOptions = new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true,
                        IgnoreNullValues    = true
                    };
                classifier.Definition = Util.ToDict(JsonSerializer.Deserialize<Dictionary<string,object>>(text, JSONOptions));
            }
        }

        // Put in the other classifiers in as well.
        // Catalog the TFLite classifiers 
        var tfliteFiles = Util.BuildNameToRelativePathXref(Path.Combine(cozmoResourcesPath,visionFolder), "tflite");
        if (  visionConfig.TryGetValue("NeuralNets", out var NN)
           && ((Dictionary<string,object>)NN).TryGetValue("Models", out var models))
        {
            // Add each of these models to the internal table;
            foreach (var _model in (object[]) models)
            {
                // Convert to the type we can work with
                var model = (Dictionary<string,object>)_model;

                // Get some of the basic info
                model.TryGetValue("modelType"  , out var modelType);
                model.TryGetValue("networkName", out var networkName);
                ClassifierInfo classifier;

                // Look up the TFLite record in the table
                if (model.TryGetValue("graphFile"  , out var graphFile))
                {
                    // Convert the graph file name into the name used to look
                    // up the full path
                    var name     = Path.GetFileNameWithoutExtension((string)graphFile);
                    tfliteFiles.TryGetValue(name, out var fullPath);

                    // Create the record to hold it
                    classifier = new ClassifierInfo(
                                Name    : (string) networkName,
                                Type    : (string) modelType,
                                FilePath: (string) graphFile,
                                FullPath: fullPath);
                    // Add in the labels files
                    if (model.TryGetValue("labelsFile", out var labelsFile))
                        classifier.LabelsPath = Path.Combine(cozmoResourcesPath,visionFolder,"dnn_models",(string) labelsFile);

                    // Remove it from the table of TFlite models, so we can add
                    // the rest in later
                    if (null != fullPath)
                        tfliteFiles.Remove(name);
                }
                else
                {
                    // This is perhaps an off-board classifier/detector reference
                    // Create the record to hold this
                    classifier = new ClassifierInfo(
                                Name  : (string) modelType,
                                Type  : (string) networkName);

                    // Is this classifier on a remote server?
                    if ("offboard" == (string) modelType)
                    {
                        // Look up the address of the server for the vision processing
                        Servers?.TryGetValue("offboard_vision", out classifier.serverAddress);
                    }

                }

                // Add in the rest of the info and put it in our table of classifiers
                classifier.Parameters = model;
                _classifiers[(string) networkName] = classifier;
            }
        }

        // Add in any of the remaining models.
        // This can include a mobilenet that is not configured, and other hand detectors
        var mobilenetLabels = Path.Combine(cozmoResourcesPath,visionFolder,"dnn_models","mobilenet_labels.txt");
        if (!File.Exists(mobilenetLabels))
            mobilenetLabels=null;
        foreach (var kv in tfliteFiles)
        {
            var name       = Path.GetFileNameWithoutExtension(kv.Key);
            var classifier = new ClassifierInfo(
                        Name    : name,
                        Type    : "TFLite", 
                        FilePath: kv.Key,
                        FullPath: kv.Value);
            // Guess the configuration of the label files
            if (0 == kv.Key.IndexOf("mobilenet", StringComparison.OrdinalIgnoreCase))
               classifier.LabelsPath = mobilenetLabels;

            // And add it to the table
            _classifiers[name] = classifier;
        }

        // And make the classifiers accessible
        Classifiers = _classifiers;
    }
}
}
