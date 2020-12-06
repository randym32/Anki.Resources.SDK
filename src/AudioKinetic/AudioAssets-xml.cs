// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Anki.AudioKinetic.XML;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Anki.AudioKinetic
{
partial class AudioAssets
{
    /// <summary>
    /// A list of the audio plug-ins in the sound processing pipeline
    /// </summary>
    IReadOnlyList<string> plugins;
    /// <summary>
    /// A list of the audio plug-ins in the sound processing pipeline
    /// </summary>
    /// <value>A list of the audio plug-ins.</value>
    public IReadOnlyList<string> Plugins => plugins;

    /// <summary>
    /// Loads the plug-in information file
    /// </summary>
    /// <returns>The plug-in information tree</returns>
    /// <remarks>The plug-in file is only in the Cozmo AudioAssets zip file</remarks>
    IReadOnlyList<string> PluginInfo()
    {
        // There is only one file, just load that
        return plugins= PluginInfo("PluginInfo.xml");
    }

    /// <summary>
    /// Loads the plug-in information file
    /// </summary>
    /// <param name="relativePath">The path, within the assets container, to the file</param>
    /// <returns>The plug-in information tree</returns>
    /// <remarks>The plug-in file is only in the Cozmo AudioAssets zip file</remarks>
    IReadOnlyList<string> PluginInfo(string relativePath)
    {
        var ser = new XmlSerializer(typeof(PluginInfo));
        // Get the file stream
        using var stream = folderWrapper.Stream(relativePath);
        if (null == stream)
            return null;
        using var reader = XmlReader.Create(stream);
        var X = new List<string>();
        // Deserialize the XML info
        foreach (var p1 in ((PluginInfo)ser.Deserialize(reader)).Plugins)
        {
            X.Add(p1.Name);
        }
        X.Sort();
        return X.Count>0?X:null;
    }


    /// <summary>
    /// Loads the sound banks information file
    /// </summary>
    /// <returns>The soundbanks information tree</returns>
    /// <remarks>This sound bank information file is only in the Cozmo AudioAssets zip file</remarks>
    SoundBanksInfo SoundBanksInfo()
    {
        // There is only one file, just load that
        return SoundBanksInfo("SoundbanksInfo.xml");
    }

    /// <summary>
    /// Loads the sound banks information file
    /// </summary>
    /// <param name="relativePath">The path, within the assets container, to the file</param>
    /// <returns>The soundbanks information tree</returns>
    /// <remarks>This sound bank information file is only in the Cozmo AudioAssets zip file</remarks>
    SoundBanksInfo SoundBanksInfo(string relativePath)
    {
        var ser = new XmlSerializer(typeof(SoundBanksInfo));
        // Get the file stream
        using var stream = folderWrapper.Stream(relativePath);
        if (null == stream)
            return null;
        using var reader = XmlReader.Create(stream);
        // Deserialize the XML info
        var ret = (SoundBanksInfo)ser.Deserialize(reader);
        // Grab the data that we use elsewhere
        foreach (var bnk in ret.SoundBanks)
        {
            // Gather the events
            var events = new List<EventInfo>();
            if (null != bnk.IncludedEvents)
            foreach (var evt in bnk.IncludedEvents)
            {
                events.Add(new EventInfo{/*SoundBank=bnk.ShortName,*/ Name=evt.Name,ObjectPath=evt.ObjectPath });
                // Also, add a cross ference
                IDForString(evt.Name);
            }

            // Gather the files
            var files = new List<FileInfo>();
            if (null != bnk.IncludedMemoryFiles)
            foreach (var evt in bnk.IncludedMemoryFiles)
            {
                files.Add(new FileInfo
                {
                    SoundBankName=bnk.ShortName,ID=evt.Id,
                    ShortName=evt.ShortName,Path=evt.Path,
                    PrefetchSize=evt.PrefetchSize
                });
                IDForString(evt.ShortName);
            }

            // Store the record
            soundBank2Info[bnk.ShortName.ToUpper()] = new SoundBankInfo
            {
                Language=bnk.Language,
                Name=bnk.ShortName,
                Path=bnk.Path,
                Events=events.Count>0?events:null,
                Files=files.Count>0?files:null
            };
        }
        return ret;
    }
}
}
