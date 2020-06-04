// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.IO;

namespace Anki.Resources.SDK
{
partial class Assets
{
    /// <summary>
    /// The path to the folder of text to speech voices
    /// </summary>
    /// <returns></returns>
    string VoicesPath()
    {
        return Path.Combine(cozmoResourcesPath, "tts");
    }


    /// <summary>
    /// This provides a path to the folder of the specified voice
    /// </summary>
    /// <param name="voice">The name of the voice</param>
    /// <returns>The path to the voice folder</returns>
    /// <remarks>
    /// The voices can be
    ///     co-French-Bruno-22khz
    ///     co-German-Klaus-22khz
    ///     co-Japanese-Sakura-22khz
    ///     co-USEnglish-Bendnn-22khz
    /// </remarks>
    string VoicePath(string voice)
    {
        return Path.Combine(VoicesPath(), voice);
    }
}
}