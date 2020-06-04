// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.Text;

namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// Analyzes an animation clip, starting from the key frames
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="animationName">The animation name (the name of the clip not the trigger name)</param>
    public void AnalyzeAnimation(StringBuilder errs, string animationName)
    {
        // See if the animation was defined in more than one file
        if (animMultiplyDefined.TryGetValue(animationName, out var errStr))
            errs.AppendLine($"{animationName}: ${errStr}");

        // Get the animation 
        var anim = AnimationForName(animationName);
        if (anim is Anki.CozmoAnim.Keyframes cozmoAnim)
        {
            // Process the Cozmo animation
            // TODO
        }
        else if (anim is Anki.VectorAnim.Keyframes vectAnim)
        {
            // Process the Vector animation
            Analyze(errs, $"clip '{animationName}'", vectAnim);
        }
        else if (anim is Dictionary<string,object>[] Anim)
        {
            // Process the JSON form of a Vector animation
            // TODO
        }
        else
        {
            ;
        }
    }

    /// <summary>
    /// Analyzes an animation clip, starting from the key frames
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframes">The keyframes object to check</param>
    void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.Keyframes keyframes)
    {
        // Check each of the key frames
        // Check the order of time codes
        long triggerTime_ms = -1;

        // Check the backpack lights keyframes
        var L = keyframes.BackpackLightsKeyFrameLength;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.BackpackLightsKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (VectorAnim.BackpackLights) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:BackpackLightsKeyFrame[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:BackpackLightsKeyFrame[{idx}]", frames);
        }

        // Check the body motion keyframes
        L = keyframes.BodyMotionKeyFrameLength ;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.BodyMotionKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (CozmoAnim.BodyMotion) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:BodyMotion[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:BodyMotion[{idx}]", frames);
        }

        // Check the event keyframes
        L = keyframes.EventKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.EventKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (CozmoAnim.Event) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:Event[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:Event[{idx}]", frames);
        }

        // Check the event keyframes
        L = keyframes.FaceAnimationKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.FaceAnimationKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (VectorAnim.FaceAnimation) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:FaceAnimation[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:FaceAnimation[{idx}]", frames);
        }

        // Check the head angle keyframes
        L = keyframes.HeadAngleKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.HeadAngleKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (CozmoAnim.HeadAngle) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:HeadAngle[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:HeadAngle[{idx}]", frames);
        }

        // Check the lift height keyframes
        L = keyframes.LiftHeightKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.LiftHeightKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (CozmoAnim.LiftHeight) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:LiftHeight[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:LiftHeight[{idx}]", frames);
        }

        // Check the procedural face keyframes
        L = keyframes.ProceduralFaceKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.ProceduralFaceKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (VectorAnim.ProceduralFace) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:ProceduralFace[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:ProceduralFace[{idx}]", frames);
        }

        // Check the record heading keyframes
        L = keyframes.RecordHeadingKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.RecordHeadingKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (CozmoAnim.RecordHeading) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:RecordHeading[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // There is nothing else to check in the specific structure
        }

        // Check the audio keyframes
        L = keyframes.RobotAudioKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.RobotAudioKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (VectorAnim.RobotAudio) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:RobotAudio[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:RobotAudio[{idx}]", frames);
        }

        // Check the spritebox keyframes
        L = keyframes.SpriteBoxKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.SpriteBoxKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (VectorAnim.SpriteBox) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:SpriteBox[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:SpriteBox[{idx}]", frames);
        }

        // Check the turn to recorded heading keyframes
        L = keyframes.TurnToRecordedHeadingKeyFrameLength;
        triggerTime_ms = -1;
        for (var idx = 0; idx < L; idx++)
        {
            // Get the keyframes
            var _frames = keyframes.TurnToRecordedHeadingKeyFrame(idx);
            // skip if there are no keyframes
            if (null != _frames)
                continue;
            var frames = (CozmoAnim.TurnToRecordedHeading) _frames;
            var tt = frames.TriggerTimeMs;
            if (tt < triggerTime_ms)
                errs.AppendLine($"{errKey}:TurnToRecordedHeading[{idx}]: the timecodes must advance.");
            triggerTime_ms=tt;
            // Check the specific structure
            Analyze(errs, $"{errKey}:TurnToRecordedHeading[{idx}]", frames);
        }
    }


    /// <summary>
    /// Analyzes the backpack lights keyframes
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The backpack light to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.BackpackLights keyframe)
    {
        // Check that each of the lights has 4 colors
        if (4 != keyframe.FrontLength)
            errs.AppendLine($"{errKey}:Front: the RGBA float array has the wrong length; should be 4.");
        if (4 != keyframe.MiddleLength)
            errs.AppendLine($"{errKey}:Middle: the RGBA float array has the wrong length; should be 4.");
        if (4 != keyframe.BackLength)
            errs.AppendLine($"{errKey}:Back: the RGBA float array has the wrong length; should be 4.");

        // Check that the values are in range.
        for (var idx = 0; idx < 4; idx++)
        {
            var f = keyframe.Front(idx);
            if (f < 0.0f || f > 1.0f)
                errs.AppendLine($"{errKey}:Front[idx]: the value is out of range; should be in the range 0.0 ... 1.0.");
            f = keyframe.Middle(idx);
            if (f < 0.0f || f > 1.0f)
                errs.AppendLine($"{errKey}:Middle[idx]: the value is out of range; should be in the range 0.0 ... 1.0.");
            f = keyframe.Back(idx);
            if (f < 0.0f || f > 1.0f)
                errs.AppendLine($"{errKey}:Back[idx]: the value is out of range; should be in the range 0.0 ... 1.0.");
        }
    }


    /// <summary>
    /// Analyzes the body motion keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The body motion to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.CozmoAnim.BodyMotion keyframe)
    {
        // radius is weird... what should that be?
        // speed is that radians/sec? mm/s?
    }

    static bool IsEventIdValid(string eventId)
    {
            // todo
        return true;
    }


    /// <summary>
    /// Analyzes the event keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The event to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.CozmoAnim.Event keyframe)
    {
        // check that the event is handled someplace
        var eventId = keyframe.EventId;
        if (!IsEventIdValid(eventId))
            errs.AppendLine($"{errKey}: the event id ('{eventId}') is not recognized by any behavior.");
    }


    /// <summary>
    /// Analyzes the face animation keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The face animation to check</param>
    void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.FaceAnimation keyframe)
    {
        // Check the sprite sequence out
        var spriteSeqName= keyframe.AnimName;
        var spriteSeq = SpriteSequence(spriteSeqName);
        if (null == spriteSeq)
        {
            errs.AppendLine($"{errKey}: the sprite sequence '{spriteSeqName}' could not be found");
        }
        else
        {
            // check that its size makes sense
            Analyze(errs, spriteSeq, displayWidth, displayHeight);
        }

        // The opacity
        var opacity = keyframe.ScanlineOpacity;
        if (opacity < 0.0f || opacity > 1.0f)
            errs.AppendLine($"{errKey}: the opacity (opacity) must be in the range 0.0 .. 1.0.");
    }


    /// <summary>
    /// Analyzes the head angle keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The head angle to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.CozmoAnim.HeadAngle keyframe)
    {
        // Check the head angle rangge
        var headAngle = keyframe.AngleDeg;
        if (headAngle < -25.0f || headAngle > 45.0f)
        {
            errs.AppendLine($"{errKey}: The head angle is {headAngle}°, outside of the allowed range.  It should be in the range -22.0° to 45.0°, and must be > -25°.");
        }
    }


    /// <summary>
    /// Analyzes the lift height keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The lift height to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.CozmoAnim.LiftHeight keyframe)
    {
        // todo: figure out range of heights and check that
    }


    /// <summary>
    /// Analyzes the procedural face keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The procedural face to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.ProceduralFace keyframe)
    {
        // todo: figure out the procedural face settings
    }


    /// <summary>
    /// Analyzes the audio keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The audio to check</param>
    void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.RobotAudio keyframe)
    {
        // Check the audio events out 
        var L = keyframe.EventGroupsLength;
        for (var idx = 0; idx < L; idx++)
        {
            // Fetch the event group
            var _eventGroup = keyframe.EventGroups(idx);
            if (null == _eventGroup)
                continue;
            var eventGroup = (Anki.VectorAnim.AudioEventGroup)_eventGroup;

            // Check the audio state
            Analyze(errs, $"{errKey}:eventGroups[{idx}]", eventGroup);
        }

        // Check the audio states out 
        L = keyframe.StatesLength;
        for (var idx = 0; idx < L; idx++)
        {
            // Fetch the state
            var _state = keyframe.States(idx);
            if (null == _state)
                continue;
            var state = (Anki.VectorAnim.AudioState)_state;

            // Check the audio state
            Analyze(errs, $"{errKey}:states[{idx}]", state);
        }

        // Check the audio switches out 
        L = keyframe.SwitchesLength;
        for (var idx = 0; idx < L; idx++)
        {
            // Fetch the switch
            var _switch = keyframe.Switches(idx);
            if (null == _switch)
                continue;
            var audioSwitch = (Anki.VectorAnim.AudioSwitch)_switch;

            // Check the audio switch
            Analyze(errs, $"{errKey}:switches[{idx}]", audioSwitch);
        }

        // Check the audio parameters out 
        L = keyframe.ParametersLength;
        for (var idx = 0; idx < L; idx++)
        {
            // Fetch the parameter
            var _parameter = keyframe.Parameters(idx);
            if (null == _parameter)
                continue;
            var parameter = (Anki.VectorAnim.AudioParameter)_parameter;

            // Check the audio parameter
            Analyze(errs, $"{errKey}:parameters[{idx}]", parameter);
        }
    }


    /// <summary>
    /// Analyzes the turn to recorded heading keyframe
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this
    /// error is referring to</param>
    /// <param name="keyframe">The turn to recorded heading to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.CozmoAnim.TurnToRecordedHeading keyframe)
    {
        // TODO: check a lot of the other values
        var offsetDeg = keyframe.OffsetDeg;
        if (offsetDeg < -359 || offsetDeg > 359)
            errs.AppendLine($"{errKey}: offset_deg ({offsetDeg}) is outside of the range -359..359.");
    }


    /// <summary>
    /// Checks the audio event group
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this</param>
    /// <param name="eventGroup">The audio event group structure to check</param>
    void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.AudioEventGroup eventGroup)
    {
        // Check that the items all have the same length
        var L = eventGroup.EventIdsLength;
        if (L != eventGroup.VolumesLength || L != eventGroup.ProbabilitiesLength)
        {
            errs.AppendLine($"{errKey}: the eventIds, volumes, and probabilites arrays must have the same length.");
        }

        // Check that the probabilities sum to zero
        var probSum = 0.0f;
        for (var idx = 0; idx < L; idx++)
        {
            var prob = eventGroup.Probabilities(idx);
            if (prob <= 0.0f)
                errs.AppendLine($"{errKey}: the probability[{idx}] {prob} is too small; the value should be in the range (0.0..1.0].");
            else if (prob > 1.0f)
                errs.AppendLine($"{errKey}: the probability[{idx}] {prob} is too large; the value should be in the range (0.0..1.0].");
            probSum+= prob;
        }
        if (probSum > 1.0f)
            errs.AppendLine($"{errKey}: the probabilities summed to greater than 1.0.");

        // Check that the event id is valid
        var audioAssets = AudioAssets;
        foreach (var eventId in eventGroup.GetEventIdsArray())
        {
            // Count the number of matches for the event
            var matches = 0;
            foreach (var bankNames in audioAssets.SoundBankNames)
            {
                // Todo: analyze the bank that all of the name works, etc
                var bank = audioAssets.SoundBank(bankNames);
                if (null == bank) continue;
                foreach (var eventInfo in bank.Events)
                    if (eventInfo.Name is string name)
                    {
                        if (eventId == Anki.AudioKinetic.AudioAssets.IDForString(name))
                            matches++;
                    }
                    else if (eventId == (uint) eventInfo.Name)
                        matches++;
            }

            // Is the event recognized?
            if (0 == matches)
            {
                // The event was not recognized, report
                var name = Anki.AudioKinetic.AudioAssets.StringForID(eventId);
                if (null != name)
                    errs.AppendLine($"{errKey}: the event {name} (eventId) is not present in the audio asset sound banks.");
                else
                    errs.AppendLine($"{errKey}: the event eventId is not present in the audio asset sound banks.");
            }
        }

        // Check that the property values are sensible todo 
        // Check that the volume values are sensible todo 
    }


    /// <summary>
    /// Checks the audio parameters
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this</param>
    /// <param name="audioParameter">The audio parameter structure to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.AudioParameter audioParameter)
    {
        // TODO
        // Check that the parameter id is valid
        // Check that the values are sensible
    }


    /// <summary>
    /// Checks the audio state
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this</param>
    /// <param name="audioState">The audio state structure to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.AudioState audioState)
    {
        // TODO
        // Check that the state group id is valid
        // Check that the state id is valid
    }


    /// <summary>
    /// Checks the audio switch
    /// </summary>
    /// <param name="errs">The errors found within the animation clip</param>
    /// <param name="errKey">The label to help identify which animation clip this</param>
    /// <param name="audioSwitch">The audio switch structure to check</param>
    static void Analyze(StringBuilder errs, string errKey, Anki.VectorAnim.AudioSwitch audioSwitch)
    {
        // TODO
        // Check that the switch group id is valid
        // Check that the switch id is valid
    }
}
}
