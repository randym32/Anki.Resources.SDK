# Packages

This library employs the following nuget packages:

| Package         | Purpose                                  |
|-----------------|------------------------------------------|
|NAudio           | Used for sound playback of decoded audio |
|NVorbis          | Used to decode Vorbis audio              |
|NAudio.Vorbis    | Used to complete the bridging of Vorbis to the audio playback. |
|System.Text.Json | Used to read JSON files; the JSON files are heavily used in the resources folder |
|System . ? Display| Used for the bitmap images |
|EMGU.CV          |Used to provide OpenCV to C# by wrapping it.  This is used by the vision classifiers. |
|EMGU.TF.Lite     |Used to provide TensorFlow Lite to C# by wrapping it.  This is used by the vision classifiers. |


Applications may have to apply some of the other packages in to be fully realized:

| Package           | Purpose                                  |
|-------------------|------------------------------------------|
|Emgu.CV.runtime.windows | Used to provide the windows-specific openCV binaries. |
|Emgu.TF.Lite.runtime.windows|Used to provide the windows-specific TensorFlow Lite binaries. |

if you use another operating system, you'll want to use packages for that OS.