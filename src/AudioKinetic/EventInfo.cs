// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;

namespace Anki.AudioKinetic
{

/// <summary>
/// This is information on events that can be sent to the audio subsystem.
///   Audio events are sent to "apply actions to the different sound objects or
///   object groups..  The actions you select specify whether the Wwise objects
///   will play, stop, pause, .. mute, set volume, enable effect bypass, and so on.”
/// </summary>
public class EventInfo
{
    /// <summary>
    /// The name of the sound bank that it is part of
    /// </summary>
    public string SoundBankName {get; internal set; }

    /// <summary>
    /// This is the event name or id; a string if it is known, otherwise a string
    /// </summary>
    public object Name {get; internal set; }

    /// <summary>
    /// This is a path within the workspace to the audio file
    /// </summary>
    /// <remarks>This is left from the audio editing workspace</remarks>
    public string ObjectPath {get; internal set; }

    /// <summary>
    /// This maps the event to the set of actions for this event
    /// </summary>
    public IReadOnlyList<uint> EventActions  {get; internal set; }
}

}
