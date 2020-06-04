// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Resource = Anki.Resources.SDK.Properties.Resources;
using System.Collections.Generic;
using System.Text;


namespace Anki.AudioKinetic
{
public partial class AudioAssets
{
    /// <summary>
    /// This provides a cache of the known names for a given WWise ID.
    /// It is used to look up the names for the id number
    /// </summary>
    static internal Dictionary<uint, string> id2Name = new Dictionary<uint, string>();

    /// <summary>
    /// This loads the pre-known things for the audio assets engine 
    /// </summary>
    static AudioAssets()
    {
        // Get the text file
        var text = Resource.ResourceManager.GetString("audioEvents");
        // Convert the known events into some ID's
        foreach (var line in text.Split(new [] { '\r', '\n' }))
        {
            id2Name[IDForString(line)] = line;
        }
    }

    /// <summary>
    /// Compute the AudioKinetic WWise ID from the string
    /// </summary>
    /// <param name="str">The string</param>
    /// <returns>The 32-bit ID for the string</returns>
    public static uint IDForString(string str)
    {
        // Convert the bytes to lower case
        byte[] bytes = Encoding.ASCII.GetBytes(str.ToLower());

        // Start with the FNV offset
        uint hash = 2166136261;

        // Apply each of the characters
        foreach (byte ch in bytes)
        {
            // Multiply it by the prime
            hash *= 16777619;

            // Xor in the charcter
            hash ^= ch;
        }
        
        // Store the hash
        id2Name[hash] = str;

        // Return the result
        return hash;
    }

    /// <summary>
    /// Looks up a string for an AudioKinetic WWise ID.
    /// (This only works if the name was previously used to compute the ID).
    /// </summary>
    /// <param name="ID">A 32-bit ID</param>
    /// <returns>null, if the string is not known; otherwis the string that produces the ID</returns>
    public static string StringForID(uint ID)
    {
        // Look up the previously cached
        if (id2Name.TryGetValue(ID, out var name))
            return name;
        return null;
    }
}
}
