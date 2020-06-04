// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Anki.AudioKinetic
{

/// <summary>
/// This is information about the sound bank file
/// </summary>
class SoundBankInfo
{
    /// <summary>
    /// The language that is assigned to this sound bank
    /// </summary>
    [JsonPropertyName("language")]
    public string Language {get;  set; }

    /// <summary>
    /// The name of the sound bank
    /// </summary>
    [JsonPropertyName("soundbank_name")]
    public string Name {get;  set; }
    /// <summary>
    /// The relative path within the assets folder
    /// </summary>
    [JsonPropertyName("path")]
    public string Path {get;  set; }

    /// <summary>
    /// This is information on the sound files within the sound bank
    /// </summary>
    internal IReadOnlyList<FileInfo> Files;

    /// <summary>
    /// This is information on events that can be sent to the audio subsystem
    /// that are defined in this sound bank
    /// </summary>
    internal IReadOnlyList<EventInfo> Events;
}

}
