# Some lessons
Some lessons I've learned in doing this:
* There are several areas that require scanning all of the files to find a
  resource.  This should not be done on an
  embedded device like Vector.  This uses limited memory, and battery life
  (to do all the accesses and processing).  For example, there is no way to
  know which .bin file to load to get an animation (clip); the names can vary.

* Images shouldn't be reshaped/resized to fit on the display.  That again takes
  time and battery life unneccessarily.

* I've had a wonderful time with C# and the .NET framework for decades.  But
  I've had endless problems with .NET core and .NET standard.  For instance, it
  will spontaneously announce that it can't load a standard .dll,

* Namespaces are an awful mess, creating unneccessary indentation blocks,
  and generally more confusion.  They clearly are a bad solution to a problem
  that should exist.  What is wrong with putting:
```
	namespace MyHackyNameToWorkAroundPoorLanguageDesign;
```
  instead?  (If I wanted to hurt myself, I'd write in LISP.)  It doesn't help
  that Visual Studio is a less-than great editor: sluggish, wasteful, and distracting.

* System.Text.Json has a lot of random, hidden "rules" lest hard-to-debug
  exceptions be thrown.  It doesn't let one deserialize to private fields

* XML is the same

* VisualStudio is not a great environment.  It (esp the devenv.exe) regularly
  spikes the CPU and is unresponsive.  The tooltip popups are noisy.
* VisualStudio doesn't  support forms in .NET core, and and requires a "preview"
  check box to turn this on... But the designer toolbox doesn't show any UI
  controls...

* Dear universe,  we do not need 9000 variants of ADPCM  encoding for the same
  basic bit depth & rate?  Knock it off.

* NAudio requires Windows.Forms.  This shouldn't be the case.
