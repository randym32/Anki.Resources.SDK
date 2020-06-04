// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.Drawing;


namespace Anki.Resources.SDK
{
/// <summary>
/// This is used to desribe the illumination of a single light
/// </summary>
public class LightFrame
{
    /// <summary>
    /// The color for the light.
    /// Alpha is always 1.
    /// </summary>
    public Color color;

    /// <summary>
    /// The duration before a transition to the next light setting may begin.
    /// During this time the light should be illuminated with the above color;
    /// after this the color may transition to the next colors.
    /// </summary>
    public uint duration_ms;

    /// <summary>
    /// The time at which the light should reach this color
    /// </summary>
    public uint triggerTime_ms;

    /// <summary>
    /// Sets up the frame
    /// </summary>
    /// <param name="triggerTime_ms">The time at which the light should reach this color</param>
    /// <param name="duration_ms"> The duration before a transition to the next light setting may begin.</param>
    /// <param name="color">The color for the light</param>
    internal LightFrame(uint triggerTime_ms, uint duration_ms, Color color)
    {
        this.triggerTime_ms = triggerTime_ms;
        this.duration_ms    = duration_ms;
        this.color          = color;
    }
}



/// <summary>
/// This describes the pattern for all of the lights on the item
/// </summary>
public partial class LightsPattern
{
    /// <summary>
    /// A text name that is associated with this structure. Optional.
    /// </summary>
    public string name {get; internal set;}

    /// <summary>
    /// Default is true. Optional.
    /// </summary>
    public bool canBeOverridden {get; internal set;} = true;

    /// <summary>
    /// if zero, do this until told to stop, otherwise perform the animation
    /// for this period and proceed to next structure or stop.
    /// </summary>
    public float duration_ms {get; internal set;}

    /// <summary>
    /// A structure describing the light patterns for each of the lgihts
    /// </summary>
    public IReadOnlyList<IReadOnlyList<LightFrame>> lightKeyFrames {get; internal set;}
}


/// <summary>
/// This is a tool used to emulate the light patterns
/// </summary>
class LightState
{
//    LightFrame next() { return null; }

    /// <summary>
    /// When the transition started
    /// </summary>
    public int transitionStart_ms;

    /// <summary>
    /// These are transition scalars
    /// </summary>
    float dR, dG, dB;

    /// <summary>
    /// The current frame to display
    /// </summary>
    LightFrame currentFrame;

    /// <summary>
    /// The next frame to display
    /// </summary>
    LightFrame nextFrame;

    /// <summary>
    /// This sets up the structure to transition from one color to the next in the time frame
    /// </summary>
    /// <param name="from">The starting color frame</param>
    /// <param name="to">The light frame to transtion to</param>
    public void Make(LightFrame from, LightFrame to)
    {
        // The amount of time it will take to go from the current light
        // to the next one
        float transitionDuration_ms = to.triggerTime_ms - (from.triggerTime_ms + from.duration_ms);
        if (transitionDuration_ms < 1)
            return;

        // Compute the change in color by millisecond steps
        dR = (to.color.R - from.color.R) / transitionDuration_ms;
        dG = (to.color.G - from.color.G) / transitionDuration_ms;
        dB = (to.color.B - from.color.B) / transitionDuration_ms;
    }

    /// <summary>
    /// This is used to fetch the color for the light the given time
    /// </summary>
    /// <param name="timeNow">The current time</param>
    /// <param name="nextUpdateTime">The time that the light will next change</param>
    /// <returns>The color</returns>
    public Color Update(int timeNow, ref int nextUpdateTime)
    {
        for (; ; )
        {
            // Keep on with the current color until the transition
            if (timeNow <= transitionStart_ms)
            {
                nextUpdateTime = transitionStart_ms;
                return currentFrame.color;
            }

            // Of we are into the next frame
            if (timeNow >= nextFrame.triggerTime_ms)
            {
                currentFrame = nextFrame;
                nextFrame = null;
                // make transition;
                continue;
            }

            // Compute the transitory color
            var dT = timeNow - transitionStart_ms;
            var color = currentFrame.color;
            nextUpdateTime = timeNow + 30;
            return Color.FromArgb(255, (byte)(color.R + dR * dT), (byte)(color.G + dG * dT), (byte)(color.B + dB * dT));
        }
    }
}
}

