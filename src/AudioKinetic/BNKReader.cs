// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Anki.AudioKinetic
{
public partial class BNKReader:IDisposable
{
    /// <summary>
    /// Bank file header section tag
    /// </summary>
    static readonly uint BKHD = Util.Tag("BKHD");
    /// <summary>
    /// Data index section tag
    /// </summary>
    static readonly uint DIDX = Util.Tag("DIDX");
    /// <summary>
    /// Data section tag
    /// </summary>
    static readonly uint DATA = Util.Tag("DATA");
    /// <summary>
    /// Data index section tag
    /// </summary>
    static readonly uint HIRC = Util.Tag("HIRC");
    /// <summary>
    /// Sound type identifier section tag
    /// </summary>
    static readonly uint STID = Util.Tag("STID");
#if false
    static readonly uint STMG = Util.Tag("STMG");
    static readonly uint FXPR = Util.Tag("FXPR");
    static readonly uint ENVS = Util.Tag("ENVS");
    static readonly uint INIT = Util.Tag("INIT");
    static readonly uint PLAT = Util.Tag("PLAT");
#endif


    // TODO: add enumeration for the HIRC section types?
    /// <summary>
    /// The binary reader used to access the data in the file
    /// </summary>
    BinaryReader binaryReader;

    /// <summary>
    /// The size of the data index section
    /// </summary>
    uint DIDX_size;

    /// <summary>
    /// The offset to the data section
    /// </summary>
    long DATA_ofs;


    /// <summary>
    /// The size of the data section
    /// </summary>
    uint DATA_size;


    /// <summary>
    /// Opens the file and checks out its basic format
    /// </summary>
    internal void Open()
    {
        // Do the initial scan of the file
        while (true)
        {
            // Stop the music if we've reached the end
            if (binaryReader.BaseStream.Length <= binaryReader.BaseStream.Position) break;
            try
            {
                // Read the section
                var newPos = ReadSection();
                if (binaryReader.BaseStream.Length <= newPos) break;
                binaryReader.BaseStream.Position = newPos;
            }
            catch (Exception ex)
            {
                break;
            }
        }

        // Done with the binary reader
        binaryReader.Dispose();
        binaryReader=null;
    }


    /// <summary>
    /// Reads the current section
    /// </summary>
    /// <returns>The position of the next section</returns>
    long ReadSection()
    {
        // Read the section tag and size
        var tag         = binaryReader.ReadUInt32();
        var sectionSize = binaryReader.ReadUInt32();
        // Make a note of the end of the section
        var nextSection = binaryReader.BaseStream.Position + sectionSize;

        // Dispatch to the reader
        // - we ignore sections with nothing interesting in them
        if (BKHD == tag)
        {
            // The file version number
            var versionNumber = binaryReader.ReadUInt32();
            // The ID of this soundbank
            Id = binaryReader.ReadUInt32();
        }
        else if (DIDX == tag)
        {
            // Make a note of the data index section information so that we can
            // scan it later.
            DIDX_size = sectionSize;
            ReadDIDX();
        }
        else if (DATA == tag)
        {
            // Make a note of the location and size of the Data section
            DATA_ofs = binaryReader.BaseStream.Position;
            DATA_size = sectionSize;
        }
        else if (STID == tag)
        {
            // Read the sound types
            ReadSTID();
        }
        else if (HIRC == tag)
        {
            // Read the sound types
            ReadHIRC();
        }

        //INIT==magic||STMG==magic||HIRC==magic||ENVS==magic||PLAT==magic
        // Return the offset to the next position
        return nextSection;
    }

    /// <summary>
    /// The Sound Type ID section, with a list of other Sound Banks.  This
    /// includes a series of file names (without their extension)
    /// </summary>
    void ReadSTID()
    {
        // Skip an unknown 32-bit number
        binaryReader.ReadUInt32();

        // The number of sound types described
        var numSounds = binaryReader.ReadUInt32();

        // Read each of the sound identifiers and file names
        for (var idx = 0; idx < numSounds; idx++)
        {
            var soundId = binaryReader.ReadUInt32();
            // Read the string length
            var strLen = binaryReader.ReadByte();
            var strBytes = binaryReader.ReadBytes(strLen);
            var fileName = System.Text.Encoding.UTF8.GetString(strBytes);
            // Store a mapping of the sound Id to the file to read
            Add(soundId, fileName);
        }
    }


    /// <summary>
    /// Scans the data index section to build up a table of some of the WEM file
    /// Ids.  Other sections may have added to that set of file ids as well.
    /// </summary>
    void ReadDIDX()
    {
        // Get the start of the data index section
        var DIDX_ofs = binaryReader.BaseStream.Position;

        // Compute the number of WEM file entries
        var numEntries = DIDX_size / 12;

        // scan thru the list in the file to find the WEM id that matches
        // the one there
        for (var idx = 0; idx < numEntries; idx++)
        {
            // Skip to the start if the record
            binaryReader.BaseStream.Position = DIDX_ofs + idx * 12;

            // See if the tables WEM file id matches
            var fileId = binaryReader.ReadUInt32();

            // This WEM file may be embedded in this sound bank
            // offset from start of DATA section
            var ofs = binaryReader.ReadUInt32();
            // The size of the WEM segment 
            var size = binaryReader.ReadUInt32();
            if (0 == fileId)
                return;

            // Check to see if the file exists; if so, use that over whatever is in the file
            var WEMPath = fileId + ".wem";
            if (folderWrapper.Exists(WEMPath))
            {
                // The file exists, so map this id to the external file
                // Note: there is a bit of a difference between some of the
                // WEM header fields here and in the external file.  They
                // are not being used but could be in some cases
                var info = InfoFor(fileId);
                if (null == info)
                {
                    // Create a record so that we can look it up later
                    Add(fileId, new FileInfo {SoundBankName=soundBankName,ID=fileId,Offset=ofs,PrefetchSize=size});
                }
            }
            else
            {
                // This WEM file is embedded in this sound bank
                var info = InfoFor(fileId);
                if (null == info)
                {
                    // Create a record so that we can look it up later
                    Add(fileId, new FileInfo {SoundBankName=soundBankName,ID=fileId,Offset=ofs,Size=size});
                }
                else
                {
                    // Update the record
                    var fileInfo = (FileInfo) info;
                    fileInfo.Offset=ofs;
                    fileInfo.Size  =size;
                }
            }
        }
    }




    /// <summary>
    /// Read the HIRC section "The HIRC section contains all the Wwise objects,
    /// including the events, the containers to group sounds, and the references
    /// to the sound files."
    /// </summary>
    void ReadHIRC()
    {
        // Fetch each of objects in the section
        var numObjects = binaryReader.ReadUInt32();
        for (var idx = 0; idx < numObjects; idx++)
        {
            // The type of object
            var objType = binaryReader.ReadByte();
            // The length of the remaining area
            var objSize = binaryReader.ReadUInt32();
            var here = binaryReader.BaseStream.Position;
            // The identifier for the objects
            var objId = binaryReader.ReadUInt32();

            // Todo read stuff
            // Based on object type, decode it 
            switch (objType)
            {
                case 1:
                    ReadHIRCSettings(objId);
                    break;
                case 2:
                    ReadHIRCSfx(objId);
                    break;
                case 3:
                    ReadHIRCEventAction(objId);
                    break;
                case 4:
                    ReadHIRCEvent(objId, here+objSize);
                    break;
                case 5:
                    //readHIRCRandomContainer(objId);
                    break;
                case 6:
                    //readHIRCSwitchContainer(objId);
                    break;
                case 7:
                    //readHIRCActorMixer(objId);
                    break;
                case 8:
                    //readHIRCAudioBus(objId);
                    break;
                case 9:
                    //readHIRCBlendContainer(objId);
                    break;
                case 15:
                    //readHIRCDialogEvent(objId);
                    break;
                case 16:
                    //readHIRCMotionBus(objId);
                    break;
                case 17:
                    //readHIRCMotionFx(objId);
                    break;
                case 18:
                    //readHIRCEffect(objId);
                    break;
                //No one knows what section 21 is
                default:
                    //Console.WriteLine($"  objId:{objId} type:{objType} size:{objSize}");
                    break;
            }

            // Go to the start of the next object
            binaryReader.BaseStream.Position = here + objSize;
        }
    }


    /// <summary>
    /// Read the settings subsection in the HIRC section.
    /// </summary>
    /// <param name="objId">the ID of the settings object</param>
    /// <remarks>
    /// For no particular reason I kept the HIRC in the name of the procedure
    /// for reading the object definitions in them
    /// </remarks>
    void ReadHIRCSettings(uint objId)
    {
        // Fetch each of settings in the section
        var numObjects = binaryReader.ReadByte();
        for (var idx = 0; idx < numObjects; idx++)
        {
            // The type of setting
            var type = binaryReader.ReadByte();
        }
        for (var idx = 0; idx < numObjects; idx++)
        {
            // The value for each
            var value = binaryReader.ReadSingle();
        }
    }


    /// <summary>
    /// Read the special effects subsection in the HIRC section.
    /// </summary>
    /// <param name="objId">the ID of the SFX object</param>
    void ReadHIRCSfx(uint objId)
    {
        // Ignore an unknown 4 bytes
        var b = binaryReader.ReadUInt32();

        // Whether the sound is embedded or "streamed"
        var isEmbedded = binaryReader.ReadByte();

        // The id of the Audio file used by the sound effect (the number to use for the WEM file)
        var audioId = binaryReader.ReadUInt32();

        // The id of the sound bank that holds it
        var sourceId = binaryReader.ReadUInt32();
        // Skip empty records (which a couple of sound banks have)
        if (0 == audioId)
            return;

#if false
            // Get the remaining info and store a record for this item
            if (0 == isEmbedded)
        {
            // The offset to where the WEM sound file can be found
            var ofs = binaryReader.ReadUInt32();
            // the length
            var size = binaryReader.ReadUInt32();

            // Create a record so that we can look it up later
            var info = InfoFor(audioId);
            if (null == info)
            {
                // Create a record so that we can look it up later
                Add(audioId, new FileInfo {SoundBankName=soundBankName,ID=audioId,Offset=ofs, Size=size});
            }
            else
            {
                // Update the record so that we can look it up later
                var s = (FileInfo) info;
                s.Offset=ofs;
                s.Size=size;
            }
        }
        else
        {
            // Create a record so that we can look it up later
            var info = InfoFor(audioId);
            // Note: the Dev_Debug sound bank has some id's that are very small not in the text tables,
            // and don't map to WEM files... so skip them.
            if (null == info && audioId > 64)
            {
                // Create a record so that we can look it up later
                Add(audioId, new FileInfo {SoundBankName=soundBankName,ID=audioId,PrefetchSize=(uint)(2 == isEmbedded?1:0)});
            }
        }

        // The type of sound  object
        binaryReader.ReadByte();
        // Bytes for the sound structure
        ReadSoundStructure();

        // Note: a lot of this seems redundant with the data-index section
#endif

        // map the id to this info
        Add(objId, new SFX{ SoundBank = this, AudioId= audioId });
    }


    /// <summary>
    /// This reads in an event action
    /// </summary>
    /// <param name="objId">the ID of the event-action object</param>
    void ReadHIRCEventAction(uint objId)
    {
        // The scope
        binaryReader.ReadByte();
        // The action type
        binaryReader.ReadByte();
        // The id of the thing that is referencing.
        // This refers to an SFX object
        var refId = binaryReader.ReadUInt32();
        // A padding byte
        // Each parameters
        // paramter type
        // parameter value
        // A padding byte
        // optional state group
        // optional switch group info

        // map the id to this info
        Add(objId, new EventAction{ SoundBank = this, SFXObjectId= refId });
    }


    /// <summary>
    /// This reads a event to event action mapping
    /// </summary>
    /// <param name="eventId">the ID of the event that maps to an action</param>
    /// <remarks>
    /// The action Ids refer to objects created in the Event Action 
    /// </remarks>
    void ReadHIRCEvent(uint eventId, long endPos)
    {
        // Create a record so that we can look it up later
        var eventActions = new List<uint>();

        // Fetch each of event actions
        // Note: this is a byte, not a uint32 as reported elsewhere
        var numObjects = binaryReader.ReadByte();
        for (var idx = 0; idx < numObjects; idx++)
        {
            // Sanity check, since there is some weirdness with some files
            if (binaryReader.BaseStream.Position+4 > endPos) break;

            // Append the event action id to the list
            eventActions.Add(binaryReader.ReadUInt32());
        }

        // Look up the record
        var info = (EventInfo) InfoFor(eventId);
        if (null != info)
            info.EventActionIds = eventActions;
        else
        {
            // map the id to this info
            var eventName = AudioAssets.StringForID(eventId);
            Add(eventId, new EventInfo{SoundBank=this,Name=eventName??(object)eventId,EventActionIds=eventActions });
        }
    }

    /// <summary>
    /// Read a common sound structure
    /// </summary>
    void ReadSoundStructure()
    {
    }


}

}
