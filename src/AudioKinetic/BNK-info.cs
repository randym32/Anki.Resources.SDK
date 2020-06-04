// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;

namespace Anki.AudioKinetic
{
public partial class BNKReader
{
    /// <summary>
    /// Maps the id to information about the object
    /// </summary>
    readonly Dictionary<object, object> objectId2Info = new Dictionary<object, object>();

    /// <summary>
    /// Add information about an id
    /// </summary>
    /// <param name="id">The internal 32-bit id form for events, sounds, etc.</param>
    /// <param name="info">Arbitrary extra information associated with the </param>
    void Add(uint id, object info)
    {
        // First, map the id to a string, if possible
        object objId;
        var name = AudioAssets.StringForID(id);
        if (null != name)
            objId = name.ToUpper();
        else
            objId = id;
        // First, check that there is not already an ojbect in there with this
        // information
        if (objectId2Info.TryGetValue(objId, out _))
            throw new System.Exception("dang, there should not be two.");
        objectId2Info[objId] = info;
    }

    /// <summary>
    /// Looks up the records for the given id
    /// </summary>
    /// <param name="id">The id of the file, event, etc</param>
    /// <returns>null if no information, otherwise, the record</returns>
    object InfoFor(uint id)
    {
        // First, map the id to a string, if possible
        object objId;
        var name = AudioAssets.StringForID(id);
        if (null != name)
            objId = name.ToUpper();
        else
            objId = id;

        // Try looking up the record
        objectId2Info.TryGetValue(objId, out var ret);
        return ret;
    }


    /// <summary>
    /// This is used to enumerate the names and id of sounds used in the sound bank
    /// file
    /// </summary>
    /// <returns>A descriptor of the file, including its ids</returns>
    public IEnumerable<FileInfo> Sounds
    {
        get
        {
        // scan thru the table to find the ids/names of sound related things
        foreach (var kv in objectId2Info)
        {
            if (kv.Value is FileInfo info)
            {
                // Return the id/name descriptor
                yield return info;
            }
        }
        }
    }

    /// <summary>
    /// This is used to enumerate the names and id of events used in the sound bank
    /// file.
    ///   Audio events are sent to "apply actions to the different sound objects or
    ///   object groups..  The actions you select specify whether the Wwise objects
    ///   will play, stop, pause, .. mute, set volume, enable effect bypass, and so on.”
    /// </summary>
    /// <returns>A descriptor of the events</returns>
    public IEnumerable<EventInfo> Events
    {
        get
        {
        // scan thru the table to find the ids/names
        foreach (var kv in objectId2Info)
        {
            if (kv.Value is EventInfo info)
            {
                // Return the id/name
                yield return info;
            }
        }
        }
    }

}
}