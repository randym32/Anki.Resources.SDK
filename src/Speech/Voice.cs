// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.IO;

namespace Anki.Resources.SDK
{
partial class Assets
{
    /// <summary>
    /// A collection of voices
    /// </summary>
    List<string> _Voices;

    /// <summary>
    /// The path to the folder of text to speech voices
    /// </summary>
    /// <returns></returns>
    string VoicesPath()
    {
        return Path.Combine(cozmoResourcesPath, "tts");
    }

    /// <summary>
    /// Lists the voices available
    /// </summary>
    /// <returns>A collection of voices; the collection may be empty</returns>
    public IReadOnlyCollection<string> VoiceNames
    {
        get
        {
            if (null != _Voices)
                return _Voices;
            var x = new List<string>();
            // Get the path to the localized strings
            var path = VoicesPath();

            // Enumerate the folders (striping the path), which are the voices
            if (Directory.Exists(path))
                foreach (var d in Directory.GetDirectories(path))
                    x.Add(Path.GetFileName(d));
            return _Voices = x;
        }
    }

    /// <summary>
    /// This provides a path to the folder of the specified voice
    /// </summary>
    /// <param name="voiceName">The name of the voice</param>
    /// <returns>The path to the voice folder</returns>
    /// <remarks>
    /// The voices can be any listed from VoiceNames.  These often are:
    ///     co-French-Bruno-22khz
    ///     co-German-Klaus-22khz
    ///     co-Japanese-Sakura-22khz
    ///     co-USEnglish-Bendnn-22khz
    /// </remarks>
    string VoicePath(string voiceName)
    {
        return Path.Combine(VoicesPath(), voiceName);
    }
}
}