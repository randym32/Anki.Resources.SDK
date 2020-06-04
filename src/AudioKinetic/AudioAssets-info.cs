// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Anki.AudioKinetic;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Anki.AudioKinetic
{


public partial class AudioAssets
{
     /// <summary>
    /// Map a sound bank name to a record about the sound bank (mostly its path)
    /// </summary>
    readonly internal Dictionary<string,SoundBankInfo> soundBank2Info = new Dictionary<string, SoundBankInfo>();

    /// <summary>
    /// Loads the bundle information about the sound banks
    /// </summary>
    void SoundbankBundleInfo()
    {
        // Create the path to the configuration file
        var cfgStream = folderWrapper.Stream("SoundbankBundleInfo.json");
        if (null == cfgStream)
            return;

        // Open the sound configuration JSON file'
        string text;
        using (var rdr = new StreamReader(cfgStream))
        {
            text = rdr.ReadToEnd();
        }

        // Lets convert the JSON basic object types
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        var cfg = JsonSerializer.Deserialize<SoundBankInfo[]>(text, JSONOptions);

        // Go thru and convert the configuration into a bank look up table
        foreach (var entry in cfg)
        {
            // Map the soundbank name to the entry
            soundBank2Info[entry.Name.ToUpper()] = entry;
            // Cache the bank name
            IDForString(entry.Name);
        }

        // Try reading the initialization file
        // SoundBank("Init");
    }


    /// <summary>
    /// The languages within the audio assets system
    /// </summary>
    /// <remarks>These are similar to locales, but the names are different;
    /// it also incldues a special effects language (SFX)</remarks>
    public IReadOnlyCollection<string> Languages
    {
        get
        {
            // Build a list of the unique names
            var ret = new Dictionary<string,string>();
            foreach (var rec in soundBank2Info.Values)
                if (null != rec.Language)
                    ret[rec.Language]=rec.Language;
            return ret.Keys;
        }
    }

    /// <summary>
    /// Retrieves a list of the sound banks
    /// </summary>
    /// <remarks>The banks can be localized, but there isn't any reason here</remarks>
    public IReadOnlyCollection<string> SoundBankNames
    {
        get
        {
            // Build a list of the unique names
            var ret = new Dictionary<string,string>();
            foreach (var rec in soundBank2Info.Values)
            {
                var bankName = rec.Name;
                if (null == bankName || "Init" == bankName) continue;
                ret[bankName] = bankName;
            }
            return ret.Keys;
        }
    }

}
}
