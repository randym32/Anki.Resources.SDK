// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

// This structure isn't used directly but does have some clarification
#if false
/// <summary>
/// This is a structure whose exact interpretation is not really known
/// </summary>
class Smartling
{
    /// <summary>
    /// An array of patterns that represent possible placeholder patterns.
    /// </summary>
    string[] placeholder_format_custom;

    /// <summary>
    /// “/{*}” Strings are path of a JSON key?
    /// </summary>
    string[] source_key_paths;

    /// <summary>
    /// “*/translation” Strings are path of a JSON key?
    /// </summary>
    string[] translate_paths;

    /// <summary>
    /// custom
    /// </summary>
    string translate_mode;

    /// <summary>
    /// 
    /// </summary>
    bool variants_enabled;
}
#endif

namespace Anki.Resources.SDK
{
partial class Assets
{
#if false
    /// <summary>
    /// A cache to hold the translation group
    /// </summary>
    readonly MemoryCache localizedTTSCache = new MemoryCache(new MemoryCacheOptions{SizeLimit = 65536});
#endif

    // Todo: a table of the translations

    /// <summary>
    /// Opens up the localization tables and text substitution
    /// </summary>
    /// <param name="resourceName">The name of the resouce file, with JSON suffix:
    /// e.g. BehaviorStrings.json, BlackJackStrings.json, FaceEnrollmentStrings.json </param>
    /// <param name="locale">de-DE, en-US, fr-FR, optional default is en-US</param>
    /// <returns>null on error, otherwise an object that can localize the strings</returns>
    public TextSubstitution LocalizedTextSubstitution(string resourceName, string locale="en-US")
    {
        var LRN = Path.Combine(locale, resourceName);
#if false
        // See if the localized strings are already loaded
        if (localizedTTSCache.TryGetValue(LRN, out var _cret))
            return (TextSubstitution)_cret;
#endif

        var path = Path.Combine(cozmoResourcesPath, "LocalizedStrings", LRN);

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get it in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        var translations = Util.ToDict(JsonSerializer.Deserialize<Dictionary<string,object>>(text, JSONOptions));

        // Get the smartling and its rules to match the substitutions
        var   smartling = (Dictionary<string, object>)translations["smartling"];
        Regex subst     = null;
        if (smartling.TryGetValue("placeholder_format_custom", out var fmts))
        {
            var newRegex = "";
            // Change the patterns to be the local machine
            foreach (var pattern in (string[])fmts)
            {
                // replace "\\\\" with "\\"
                newRegex += pattern.Replace("\\\\", "\\", StringComparison.InvariantCultureIgnoreCase)+"|";
            }

            // Remove the last or
            newRegex = newRegex[0..^1];

            // Create a regular expression to match the placeholder
            subst = new Regex(newRegex);
        }

        // Construct the table of localized strings
        var table = new Dictionary<string, string>();
        foreach (var kv in translations)
        {
            if ("smartling" == kv.Key)
                continue;
            // get the match and map the key to the translation
            table[kv.Key] = (string)((Dictionary<string, object>)kv.Value)["translation"];
        }

        // Create an object to handle the text substitutiosn
        var ret = new TextSubstitution(table, subst);

#if false
        // cache it
        localizedTTSCache.Set(LRN, ret);
#endif
        return ret;
    }
}
}
