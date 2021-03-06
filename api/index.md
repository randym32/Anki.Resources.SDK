# API Documentation

This file summarizes the namespaces and classes in this assembly.
There are separate namespaces to encapsulate generated code for the Cozmo and
Vector flat-buffers animation files; and variety of AudioKinetic structures.

## Anki.AudioKinetic Namespace

This namespace contains the all of the classes to access the AudioKinetic WWise sound assets.

| Component                                                          | Description                   |
|--------------------------------------------------------------------|-------------------------------|
| [AudioAssets](xref:Anki.AudioKinetic.AudioAssets)                  | This class is used to wrap the audio assets in Vector and Cozmo. These assets are Audio-Kinetic WWise soundbank files and other supporting files. |
| [BNKReader](xref:Anki.AudioKinetic.BNKReader)                      | This class is used to scan sound bank (BNK) files.   |
| [EventInfo](xref:Anki.AudioKinetic.EventInfo)                      | This is information on events that can be sent to the audio subsystem.  |
| [FileInfo](xref:Anki.AudioKinetic.FileInfo)                        | This is information on the sound files within a sound bank.     |
| [IMAWaveProvider](xref:Anki.AudioKinetic.IMAWaveProvider)          | A helper to stream the WWise encoded IMA |
| [WEMReader](xref:Anki.AudioKinetic.WEMReader)                      | This is used to open up the WEM segment in the BNK file and setup something that NAudio can access. |

### Anki.CozmoAnim Namespace

This namespace that contains all the classes used to read the binary Cozmo animation files.
The Vector animation objects reuse (or share) the classes from Cozmo animation where possible.
Most of these files are autogenerated.

| Component                                                          | Description                   |
|--------------------------------------------------------------------|-------------------------------|
| [AnimClip](xref:Anki.CozmoAnim.AnimClip)                           | The AnimClip is a named animation that can be played. |
| [AnimClips](xref:Anki.CozmoAnim.AnimClips)                         | An animation file is defined by this structure at the top level. It provides one or more animation “clips” in the file. Each clip has one or more tracks. The file is defined by this structure at the top level |
| [BackpackLights](xref:Anki.CozmoAnim.BackpackLights)               |  |
| [BodyMotion](xref:Anki.CozmoAnim.BodyMotion)                       | The BodyMotion structure is used to specify driving motions. |
| [Event](xref:Anki.CozmoAnim.Event)                                 | The Event structure is used to pause the animation at the given time code until the event is received or cancelled. When the event is received, the animation resumes the given time code. |
| [FaceAnimation](xref:Anki.CozmoAnim.FaceAnimation)                 |  |
| [HeadAngle](xref:Anki.CozmoAnim.HeadAngle)                         | The HeadAngle structure is used to specify how to move Vector’s head. The head should reach the target angle in the duration given, ramping up the movement speed smoothly (with some variability) until it reaches that that point. |
| [Keyframes](xref:Anki.CozmoAnim.Keyframes)                         | The KeyFrames structure provides separate time-coded key frames for each of the possible tracks in the animation. The tracks are optional. There tracks may have different numbers of key frames. The key frames do not need to start at the same time(s). |
| [LiftHeight](xref:Anki.CozmoAnim.LiftHeight)                       | The LiftHeight structure is used to specify how to move Vector’s lift. The lift should reach the target height in the duration given, ramping up the movement speed smoothly (with some variability) until it reaches that. |
| [ProceduralFace](xref:Anki.CozmoAnim.ProceduralFace)               |  |
| [RecordHeading](xref:Anki.CozmoAnim.RecordHeading)                 | The RecordHeading structure is used to recording the robots heading, at the start of an animation. [or possibly after a randomized body motion?] |
| [RobotAudio](xref:Anki.CozmoAnim.RobotAudio)                       | The RobotAudio structure is used to interact with the audio engine. |
| [TurnToRecordedHeading](xref:Anki.CozmoAnim.TurnToRecordedHeading) | The TurnToRecordedHeading is used to specify how Vector should turn to the previously recorded heading. The robot reach the target heading in the duration given, ramping up the movement speed smoothly until it reaches that position (within some tolerance). |

## Anki.Resources.SDK  Namespace

This namespace contains the main animation and behavior classes:

| Component                                                              | Description                   |
|------------------------------------------------------------------------|-------------------------------|
| [AnimationGroupItem](xref:Anki.Resources.SDK.AnimationGroupItem) | The AnimationGroupItem structure describes the specific animation clip to use. It may also specify some head movement, with some variability; this is optional. If enabled, the head is to move to some angle (between the given min and max) while the animation plays. |
| [Assets](xref:Anki.Resources.SDK.Assets)                         | A class to access the resources in the Cozmo_Resources folder for Vector (And maybe some of Cozmo).   |
| [BehaviorCoordinator](xref:Anki.Resources.SDK.BehaviorCoordinator)| A helper for behaviors like games and weather.        |
| [BlackJack](xref:Anki.Resources.SDK.BlackJack)                    | The behavior to support the Black Jack game.        |
| [Clock](xref:Anki.Resources.SDK.Clock)                     | The behavior to support the Clock / Timer behaviors.        |
| [CompositeImage](xref:Anki.Resources.SDK.CompositeImage)   | A composite image defines layers on the display with rectangular areas where images and sprite sequences will be drawn.        |
| [DecayGraph](xref:Anki.Resources.SDK.DecayGraph)           | This describes how an emotion decays over time.        |
| [EmotionAffector](xref:Anki.Resources.SDK.EmotionAffector) | The EmotionAffector describes how an emotion dimension should be modified. |
| [EmotionEvent](xref:Anki.Resources.SDK.EmotionEvent)       | The EmotionEvent describes how the emotions respond to a given event. |
| [LightFrame](xref:Anki.Resources.SDK.LightFrame)                 | This is used to desribe the illumination of a single light.     |
| [LightsPattern](xref:Anki.Resources.SDK.LightsPattern)           | This describes the pattern for all of the lights on the item.   |
| [MinMax](xref:Anki.Resources.SDK.MinMax)                  | A helper class to hold the allow minimum and maximum.              |
| [SimpleVoiceResponse](xref:Anki.Resources.SDK.SimpleVoiceResponse)| The animation and emotion changes that should occur.              |
| [SpriteBox](xref:Anki.Resources.SDK.SpriteBox)                    | A sprite box defines a rectangular area on the display to draw an image of sprite sequence. |
| [SpriteSequence](xref:Anki.Resources.SDK.SpriteSequence)          | This is a wrapper around an sprite sequence.          |
| [TextSubstitution](xref:Anki.Resources.SDK.TextSubstitution)      | Provide a localized strings with substituted matches. |
| [UserIntentMap](xref:Anki.Resources.SDK.UserIntentMap)            | A table that maps the intent received by the cloud or application to the intent name used internally. This includes renaming the parameters.              |
| [Weather](xref:Anki.Resources.SDK.Weather)                        | The Weather behavior.              |
| [XY](xref:Anki.Resources.SDK.XY)                                  | The XY structure is used to define how a value (often the value associated with an emotion dimension) should decay with time..              |


### Anki.VectorAnim Namespace

This namespace that contains all the classes used to read the binary Vector animation files.
Where possible it reuses the Cozmo classes.
Most of these files are autogenerated.


| Component                                                          | Description                   |
|--------------------------------------------------------------------|-------------------------------|
| [AnimClip](xref:Anki.VectorAnim.AnimClip)                          | The AnimClip is a named animation that can be played. |
| [AnimClips](xref:Anki.VectorAnim.AnimClips)                        | An animation file is defined by this structure at the top level. It provides one or more animation “clips” in the file. Each clip has one or more tracks. The file is defined by this structure at the top level |
| [AudioEventGroup](xref:Anki.VectorAnim.AudioEventGroup)            | The AudioEventGroup structure is used to randomly select an audio event (and volume), and send it to the audio subsystem. |
| [AudioParameter](xref:Anki.VectorAnim.AudioParameter)              | The AudioParameter structure is used to set one of the sound parameters in the AudioKinetic Wwise subsystem. |
| [AudioState](xref:Anki.VectorAnim.AudioState)                      | The AudioState structure is used to put the audio system into a particular state. |
| [AudioSwitch](xref:Anki.VectorAnim.AudioSwitch)                    | The AudioSwitch structure is used to put an audio switch into a particular setting. |
| [BackpackLights](xref:Anki.VectorAnim.BackpackLights)              | The BackpackLights structure is used to animate the lights on Vector’s back. |
| [FaceAnimation](xref:Anki.VectorAnim.FaceAnimation)                | The FaceAnimation structure is used to specify the JSON file to animation Vector’s display. |
| [Keyframes](xref:Anki.VectorAnim.Keyframes)                        | The KeyFrames structure provides separate time-coded key frames for each of the possible tracks in the animation. The tracks are optional. There tracks may have different numbers of key frames. The key frames do not need to start at the same time(s). |
| [ProceduralFace](xref:Anki.VectorAnim.ProceduralFace)              | The ProceduralFace structure is used to direct how the eyes should be drawn. |
| [RobotAudio](xref:Anki.VectorAnim.RobotAudio)                      | The RobotAudio structure is used to interact with the audio engine. |
| [SpriteBox](xref:Anki.VectorAnim.SpriteBox)                        | The SpriteBox structure defines a rectangular region on the display to draw an image from a file. |



### RCM Namespace

This namespace is used to contain extra helper procedures and classes.

| Component                                                         | Description                                    |
|-------------------------------------------------------------------|------------------------------------------------|
| [FolderWrapper](xref:RCM.FolderWrapper)                           | This is a class to allow access to resources within a folder. It is a sibling to ZipWrapper that can access resources within a zip file.  |
| [IFolderWrapper](xref:RCM.IFolderWrapper)                         | This is an interface to allow access to resources within a folder or archive.  |
| [Util](xref:RCM.Util)                                             | A class to hold a variety of helper utilities.       |
| [ZipWrapper](xref:RCM.Util)                                       | This is a class to allow access to resources within an archive. It is a sibling to FolderWrapper that can access resources within a folder. |


##
some [gripes](gripes.html)