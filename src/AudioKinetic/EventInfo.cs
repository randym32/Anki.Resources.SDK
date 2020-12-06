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
    /// The sound bank that it is part of
    /// </summary>
    /// <value>The sound bank that this event info is part of.</value>
    public BNKReader SoundBank {get; internal set; }

    /// <summary>
    /// This is the event name or id; a string if it is known, otherwise a string
    /// </summary>
    /// <value>
    /// This is the event name or id; a string if it is known, otherwise a string
    /// </value>
    public object Name {get; internal set; }

    /// <summary>
    /// This is a path within the workspace to the audio file
    /// </summary>
    /// <value>his is a path within the workspace to the audio file</value>
    /// <remarks>This is left from the audio editing workspace</remarks>
    public string ObjectPath {get; internal set; }

    /// <summary>
    /// The set of action ids for this event.
    /// </summary>
    /// <value>The set of action ids for this event.</value>
    internal IReadOnlyList<uint> EventActionIds  {get; set; }

    /// <summary>
    /// The set of actions for this event.
    /// </summary>
    /// <value>The set of action ids for this event.</value>
    public IEnumerable<EventAction> EventActions
    {
        get
        {
            // scan thru the table to find the ids/names of sound related things
            foreach (var kv in EventActionIds)
            {
                var obj = SoundBank.InfoFor(kv);
                if (obj is EventAction info)
                {
                    // Return the event action
                    yield return info;
                }
            }
        }
    }
}

}
