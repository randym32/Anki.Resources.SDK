// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Blackwood;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// This is an internal table of which features are enabled/disabled in
    /// this resource folder
    /// </summary>
    readonly Dictionary<string, bool> featureToggle = new Dictionary<string, bool>();

    /// <summary>
    /// Provides a table of the feature flags and whether it is enabled or disabled.
    /// </summary>
    /// <value>
    /// Provides a table of the feature flags and whether it is enabled or disabled.
    /// </value>
    public IReadOnlyDictionary<string, bool> Features => featureToggle;

    /// <summary>
    /// Loads the information from the features file
    /// </summary>
    /// <param name="configPath">Path to the config folder</param>
    void LoadFeatures(string configPath)
    {
        // get the path to the features file
        var path = Path.Combine(configPath, "features.json");

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get the dictionary, in a convenient form
        var items = (object[])JSONDeserializer.JsonToNormal(JSONDeserializer.Deserialize<JsonElement>(text));

        // internalize each of the items
        foreach (var _item in items)
        {
            var item = (Dictionary<string, object>)_item;
            featureToggle[(string)item["feature"]] = (bool) item["enabled"];
        }
    }
}
}
