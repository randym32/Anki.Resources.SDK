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
    /// <summary>
    /// A cache to hold the binary animation files
    /// </summary>
    readonly Dictionary<string, WeakReference> localizedTTSCache = new Dictionary<string, WeakReference>();

    /// <summary>
    /// A collection of locales
    /// </summary>
    List<string> _Locales;

    /// <summary>
    /// A collection of locales
    /// </summary>
    List<string> _LocalizationFiles;

    /// <summary>
    /// Lists the locales that have localizations
    /// </summary>
    /// <returns>A collection of locales; the collection may be empty</returns>
    public IReadOnlyCollection<string> Locales
    {
        get
        {
            if (null != _Locales)
                return _Locales;
            var x = new List<string>();
            // Get the path to the localized strings
            var path = Path.Combine(cozmoResourcesPath, "assets/LocalizedStrings");

            // Enumerate the folders (striping the path), which are the locales
            if (Directory.Exists(path))
                foreach (var d in Directory.GetDirectories(path))
                    x.Add(Path.GetFileName(d));
            return _Locales = x;
        }
    }


    /// <summary>
    /// Lists the locales that have localizations
    /// </summary>
    /// <returns>A  collection of localization resource names; the collection
    /// may be empty</returns>
    public IReadOnlyCollection<string> LocalizationFiles
    {
        get
        {
            if (null != _LocalizationFiles)
                return _LocalizationFiles;
            // Get the path to the localized strings, using the always-present
            // US english folder
            var path = Path.Combine(cozmoResourcesPath, "assets/LocalizedStrings/en-US");
            var x = new List<string>();

            // Enumerate the folders (striping the path), which are the locales
            if (Directory.Exists(path))
                foreach (var f in Directory.GetFiles(path, "*.json"))
                    x.Add(Path.GetFileName(f));
            return _LocalizationFiles = x;
        }
    }


    /// <summary>
    /// Looks up the localization entry in the cache
    /// </summary>
    /// <param name="localeResourceName">The locale and resource name</param>
    /// <returns>null on error, otherwise an LocalizedTextSubstitution object
    /// </returns>
    TextSubstitution LocalizationCache(string localeResourceName)
    {
        // Is there an entry in the cache for this?
        if (!localizedTTSCache.TryGetValue(localeResourceName, out var wref))
            return null;
        return (TextSubstitution) wref.Target;
    }


    /// <summary>
    /// Opens up the localization tables and text substitution
    /// </summary>
    /// <param name="resourceName">The name of the resource file, with JSON suffix:
    /// e.g. BehaviorStrings.json, BlackJackStrings.json, FaceEnrollmentStrings.json </param>
    /// <param name="locale">de-DE, en-US, fr-FR, ja-JP (optional).  The default is en-US</param>
    /// <returns>null on error, otherwise an object that can localize the strings</returns>
    public TextSubstitution LocalizedTextSubstitution(string resourceName, string locale="en-US")
    {
        var LRN = Path.Combine(locale, resourceName);
        // See if the localized strings are already loaded
        var _cret = LocalizationCache(LRN);
        if (null != _cret)
            return _cret;

        // Get the path to the localized strings
        var path = Path.Combine(cozmoResourcesPath, "assets/LocalizedStrings", LRN);
        if (!File.Exists(path))
            return null;

        // Get the text for the translations file
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
            foreach (var pattern in (object[])fmts)
            {
                // replace "\\\\" with "\\"
                newRegex += ((string) pattern).Replace("\\\\", "\\", StringComparison.InvariantCultureIgnoreCase)+"|";
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

        // Add an item to the cache
        localizedTTSCache[LRN] = new WeakReference(ret);
        return ret;
    }
}
}
