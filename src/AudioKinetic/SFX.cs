// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;

namespace Anki.AudioKinetic
{

/// <summary>
/// This is information on special-effect.
/// </summary>
public class SFX
{
    /// <summary>
    /// The sound bank that it is part of
    /// </summary>
    /// <value>The sound bank that this event info is part of.</value>
    public BNKReader SoundBank { get; internal set; }

    /// <summary>
    /// The audio file associated with this.
    /// </summary>
    /// <value>The audio file Id.</value>
    internal uint AudioId { get; set; }

    /// <summary>
    /// The audio file for this special-effect.
    /// </summary>
    /// <value>The audio file for this special effect.</value>
    public WEMReader AudioFile
    {
        get
        {
            return SoundBank.WEM(AudioId);
        }
    }
}

}
