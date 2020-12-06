// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;

namespace Anki.AudioKinetic
{

/// <summary>
/// This is information on action associated with an event.
/// </summary>
public class EventAction
{
    /// <summary>
    /// The sound bank that it is part of
    /// </summary>
    /// <value>The sound bank that this event info is part of.</value>
    public BNKReader SoundBank { get; internal set; }

    /// <summary>
    /// The special-effect for this action.
    /// </summary>
    /// <value>The special effect id for this event action.</value>
    internal uint SFXObjectId  {get; set; }

    /// <summary>
    /// The special-effect for this action.
    /// </summary>
    /// <value>The special effect id for this event action.</value>
    public SFX SFXObject
    {
        get
        {
            var obj = SoundBank.InfoFor(SFXObjectId);
            if (obj is SFX info)
            {
                // Return the event action
                return info;
            }
            return null;
        }
    }
}

}
