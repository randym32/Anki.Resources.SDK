// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Anki.Resources.SDK
{
/// <summary>
/// Provide a localized strings with substituted matches
/// </summary>
public class TextSubstitution
{
    /// <summary>
    /// This is used to match placeholders that are populated later by the
    /// context information
    /// </summary>
    readonly Regex placeholderRegex;


    /// <summary>
    /// This maps the text key to the local translation of the text,
    /// including placeholders
    /// 
    /// </summary>
    readonly IReadOnlyDictionary<string, string> translatedStrings;

    /// <summary>
    /// Constructs method to look up translated strings and fill in placeholders
    /// </summary>
    /// <param name="translatedStrings">The table of tranaslated strings</param>
    /// <param name="placeholderPattern">An optional regular expression to match placeholders</param>
    internal TextSubstitution(IReadOnlyDictionary<string, string> translatedStrings, Regex placeholderPattern)
    {
        this.translatedStrings = translatedStrings;
        this.placeholderRegex = placeholderPattern;
    }


    /// <summary>
    /// Constructs method to look up translated strings and fill in placeholders
    /// </summary>
    /// <param name="translatedStrings">The table of translated strings</param>
    internal TextSubstitution(IReadOnlyDictionary<string, string> translatedStrings)
        : this(translatedStrings, null)
    {
    }


    /// <summary>
    /// Enumerates over the set of localization keys.
    /// </summary>
    /// <value>
    /// Enumerates over the set of localization keys.
    /// </value>
    public IEnumerable<string> Keys
    {
        get
        {
            return translatedStrings.Keys;
        }
    }


    /// <summary>
    /// This procedure looks up the localized string without substitution
    /// strings.
    /// </summary>
    /// <param name="key">The localization key</param>
    /// <returns>null on error, otherwise the string with placeholder substitutions</returns>
    public string LocalizedTextWithoutSubstitutions(string key)
    {
        // Look up the string
        translatedStrings.TryGetValue(key, out var localString);

        // Return the formatted string
        return localString;
    }

    /// <summary>
    /// This procedure looks up the localized string with possible substitution
    /// strings.
    /// </summary>
    /// <param name="key">The localization key</param>
    /// <param name="substitutions">A table of text substitutions</param>
    /// <returns>null on error, otherwise the string with placeholder substitutions</returns>
    public string LocalizedText(string key, IReadOnlyDictionary<string, string> substitutions)
    {
        // Look up the string
        if (!translatedStrings.TryGetValue(key, out var localString))
            return null;

        // Skip the remaining steps if there is no placeholder matching regular
        // expression
        if (null == placeholderRegex)
            return localString;

        // A buffer to hold the substituted
        var SB = new StringBuilder();

        // The last point in the source string that we've copied
        var lastIdx = 0;

        // Scan over the string for place holders to add more
        foreach (Match match in placeholderRegex.Matches(localString))
        {
            // Copy the string from the last point to the start of the match
            SB.Append(localString[lastIdx..match.Index]);
            lastIdx = match.Index + match.Value.Length;

            // Look up the substitution
            if (substitutions.TryGetValue(match.Value, out var tmp))
                SB.Append(tmp);
        }

        // Copy the rest of the string
        SB.AppendFormat(localString.Substring(lastIdx));

        // Return the formatted string
        return SB.ToString();
    }
}
}
