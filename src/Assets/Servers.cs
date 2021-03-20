// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Blackwood;
using RCM;
using System.Collections.Generic;
using System.IO;


namespace Anki.Resources.SDK
{
public partial class Assets
{
        /// <summary>
        /// Provides a table of the servers and their addresses
        /// </summary>
        /// <value>
        /// Provides a table of the servers and their addresses
        /// </value>
        public IReadOnlyDictionary<string, string> Servers {get; internal set; }

    /// <summary>
    /// Loads the information from the server_cinfig file
    /// </summary>
    /// <param name="configPath">Path to the config folder</param>
    void LoadServers(string configPath)
    {
        // get the path to the server config file
        var path = Path.Combine(configPath, "server_config.json");

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get the dictionary, in a convenient form
        Servers = JSONDeserializer.Deserialize<Dictionary<string,string>>(text);
    }
}
}
