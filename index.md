# Anki Vector/Cozmo Resources SDK  (.NET)

This SDK allows manipulation of a resources folder from an "over the air update"
OTA file for [Anki Vector](https://www.anki.com/en-us/vector) and the APK files
for [Anki Cozmo](https://www.anki.com/en-us/cozmo), in any .NET language
(C#, VB.NET, F#) or language that interoperates (python) on Windows, Mac, and
Linux.  This requires that you know how to one of these files, and how extract
the contents.

* You can play the audio sounds on your computer
* Examine the sprite-sequences, and other animations on your computer,

* It has lint-like checking to help catch flaws in the resource bundle, so that
   they can be eliminated before shipping, reducing the test effort

There is some support for Cozmo resources.

Note: This SDK is not a product of Anki or Digital Dream Labs, and is not
supported by them.

## Getting Started

### Download Microsoft development tools

If you working on Windows, download Visual Studio 2019 Community Edition to get started.  This version is free for personal use.
* [Download Visual Studio 2019 Community Edition](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community)

To get started on Mac and Linux, you can download .NET Core 3.0.  
* [Download .NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)

To play audio you will need to add the following packages to your project:
* [NAudio](https://www.nuget.org/packages/NAudio/)
* [NVorbis](https://www.nuget.org/packages/NVorbis/)
* [NAudio.Vorbis](https://www.nuget.org/packages/NAudio.Vorbis/)

To play with the sprites you will need to add the following packages to your project:
* [System.Drawing.Common](https://www.nuget.org/packages/System.Drawing.Common/)

### Download
* [Github](https://github.com/randym32/Anki.Resources.SDK)
* [Nuget](https://www.nuget.org/packages/Anki.Resources.SDK/)

### SDK Example Code  / Tutorial Programs

Some examples of how to use the SDK can be found at

* [Anki.Resources.Samples GitHub project](https://github.com/randym/Anki.Resouces.Samples)


## Documentation
The documentation can be found at [randym32.github.io/Anki.Resources.SDK](https://randym32.github.io/Anki.Resources.SDK)

### Browser configuration notes
The documentation, when browsed from a local filesystem, may require tweaking
the browser.

#### Firefox
Go to about:config and make change to the following:
* security.fileuri.strict_origin_policy set to false
* privacy.file_unique_origin  set to false

#### Chrome
You can not use Chrome with a local file system.  You can, however, use an extension
like "Web Server for Chrome"

## Getting help
Being a new library, there is not yet an established usage pattern, clear place
and patterns for getting help.  Try

* **Official Anki developer forums**: https://forums.anki.com/
* **Anki Vector developer subreddit**: https://www.reddit.com/r/ankivectordevelopers
* **Anki robots Discord chat**: https://discord.gg/FT8EYwu 

## Contributing
View the [Anki.Resources.SDK GitHub Project](https://github.com/randym32/Anki.Resources.SDK)
for information on contributing.

## Grateful Acknowledgements and Special Thanks

Thank-you to the [vgmstream](https://github.com/losnoco/vgmstream/) and
[ww2ogg](https://github.com/hcs64/ww2ogg) projects, whose efforts allowed
decoding of the audio files.

Wayne Venables, you set a very high bar with [Anki.Vector.SDK](https://github.com/codaris/Anki.Vector.SDK) 
I lifted some of your index.md getting started information for here.

[Packages used by this SDK](api/packages.html)