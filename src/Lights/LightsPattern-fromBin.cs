// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.Drawing;
using Anki.VectorAnim;

namespace Anki.Resources.SDK
{
partial class LightsPattern
{
    /// <summary>
    /// Initializes the light pattern from the keyframes in the animation file
    /// </summary>
    /// <param name="k"></param>
    void FromVectorAnim(Keyframes k)
    {
        // Allocate lists to hold the LEDs
        var FrontSeq  = new List<LightFrame>();
        var MiddleSeq = new List<LightFrame>();
        var BackSeq   = new List<LightFrame>();
        uint lastTime = 0;

        // Convert each of the animation files backpack lights into a
        // single internal format
        for (var I = 0; I < k.BackpackLightsKeyFrameLength; I++)
        {
            var l = k.BackpackLightsKeyFrame(I);
            if (null == l) continue;

            // Convert the struct to frames for each of the LEDs
            lastTime = From((Anki.VectorAnim.BackpackLights)l, out var front, out var middle, out var back);

            // Append these to the list
            FrontSeq .Add(front);
            MiddleSeq.Add(middle);
            BackSeq  .Add(back);
        }

        if (FrontSeq.Count == 0 && MiddleSeq.Count == 0 && BackSeq.Count == 0)
            return;

        // Now put that into something to hold all of them
        lightKeyFrames = new IReadOnlyList<LightFrame>[3] { FrontSeq, MiddleSeq, BackSeq };
        duration_ms = lastTime - FrontSeq[0].triggerTime_ms;
    }


    /// <summary>
    /// This is used to translate a backpack lights animation struct into
    /// the internal format used for animation
    /// </summary>
    /// <param name="bl">The backpack lights struct</param>
    /// <param name="Front">The front LED animation frame</param>
    /// <param name="Middle">The middle LED animation frame</param>
    /// <param name="Back">The back LED animation frame</param>
    /// <returns>duration</returns>
    static uint From(Anki.VectorAnim.BackpackLights bl, out LightFrame Front, out LightFrame Middle, out LightFrame Back)
    {
        // Ignore alpha
        var frontColor = Color.FromArgb(255, (byte)(255 * bl.Front (0)), (byte)(255 * bl.Front (1)), (byte)(255 * bl.Front (2)));
        var middleColor= Color.FromArgb(255, (byte)(255 * bl.Middle(0)), (byte)(255 * bl.Middle(1)), (byte)(255 * bl.Middle(2)));
        var backColor  = Color.FromArgb(255, (byte)(255 * bl.Back  (0)), (byte)(255 * bl.Back  (1)), (byte)(255 * bl.Back  (2)));
        var triggerTime_ms  = bl.TriggerTimeMs;
        var durationTime_ms = bl.DurationTimeMs;

        // Convert each of these to their own frame
        Front  = new LightFrame(triggerTime_ms, durationTime_ms, frontColor);
        Middle = new LightFrame(triggerTime_ms, durationTime_ms, middleColor);
        Back   = new LightFrame(triggerTime_ms, durationTime_ms, backColor);
        return triggerTime_ms+durationTime_ms;
    }
}
}
