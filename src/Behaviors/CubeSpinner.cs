// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
namespace Anki.Resources.SDK
{
/// <summary>
/// This is used to map events to the light states
/// </summary>
class LightMap
{
    /// <summary>
    /// This structure maps event to animation trigger names appropriate for
    /// the backpack light animation.
    /// </summary>
    public IReadOnlyDictionary<string, string> backpackLights {get;set; }

    /// <summary>
    /// This structure maps event to animation trigger names appropriate for
    /// the cube light animation.
    /// </summary>
    public IReadOnlyDictionary<string, string> cubeLights  {get;set; }

    /// <summary>
    /// A name like “blue”.  This likely is used to provide the active mapping
    /// to a tool during development.
    /// </summary>
    public string debugColorName  {get;set; }


    #if false
    /// <summary>
    /// Look up the cube lights animation for the game event
    /// </summary>
    /// <param name="assets">The resource manager</param>
    /// <param name="eventName">The name of the event to look up</param>
    /// <returns>null on erorr, otherwise, a sequence of light patterns to display</returns>
    internal IReadOnlyList<LightsPattern> CubeLightsForTrigger(Assets assets, string eventName)
    {
        // Map the event name to the animation trigger name
        if (cubeLights.TryGetValue(eventName, out var triggerName))
        {
            // Look up the cube lights animation for the trigger name
            return assets.CubeLightsForTrigger(triggerName);
        }

        // Couldn't find any
        return null;
    }

    /// <summary>
    /// Look up the backpack lights animation for the game event
    /// </summary>
    /// <param name="assets">The resource manager</param>
    /// <param name="eventName">The name of the event to look up</param>
    /// <returns>null on erorr, otherwise, a sequence of light patterns to display</returns>
    internal IReadOnlyList<LightsPattern> BackpackLightsForTrigger(Assets assets, string eventName)
    {
        // Map the event name to the animation trigger name
        if (cubeLights.TryGetValue(eventName, out var triggerName))
        {
            // Look up the backpack lights animation for the trigger name
            return assets.BackpackLightsForTrigger(triggerName);
        }

        // Couldn't find any
        return null;
    }
    #endif
}

class C
{
    /// <summary>
    /// This is an array of alternatives mappings from events to the animation triggers.
    /// </summary>
    public IReadOnlyList<LightMap> lightMap {get;set; }

    /// <summary>
    /// The animation trigger name for the playerErrorCubeLights event.
    /// </summary>
    public string playerErrorCubeLights {get;set;}

    /// <summary>
    /// The animation trigger name for the startGameCubeLights event.
    /// </summary>
    public string startGameCubeLights {get;set;}
}

 
/// <summary>
/// The structure of the configuration file for the cube spinner game.  It is
/// used to map game events to animation triggers.  
/// </summary>
class CubeSpinner:BehaviorCoordinator
{
    CubeSpinner():base(null)
    { }

    /// <summary>
    /// This is an array of alternatives mappings from events to the animation triggers.
    /// </summary>
    public LightMap[] lightMap {get;set;}

#if false
    int SelectName(string name)
    {
        for (var idx = 0; idx < lightMap.Length; idx++)
            if (name == lightMap[idx].debugColorName)
            {
                return idx;
            }
        return 0;
    }


    /// <summary>
    /// Look up the cube lights animation for the game event
    /// </summary>
    /// <param name="assets">The resource manager</param>
    /// <param name="eventName">The name of the event to look up</param>
    /// <param name="idx">The index int he list</param>
    /// <returns>null on erorr, otherwise, a sequence of light patterns to display</returns>
    public IReadOnlyList<LightsPattern> CubeLightsForTrigger(Assets assets, string eventName, int idx)
    {
        // Look up the animation given the name
        if ("startGame" == eventName)
            return assets.CubeLightsForTrigger(startGameCubeLights);
        if ("playerError" == eventName)
            return assets.CubeLightsForTrigger(playerErrorCubeLights);
        return lightMap[idx].CubeLightsForTrigger(assets, eventName);
    }


    /// <summary>
    /// Look up the backpack lights animation for the game event
    /// </summary>
    /// <param name="assets">The resource manager</param>
    /// <param name="eventName">The name of the event to look up</param>
    /// <returns>null on erorr, otherwise, a sequence of light patterns to display</returns>
    IReadOnlyList<LightsPattern> BackpackLightsForTrigger(Assets assets, string eventName, int idx)
    {
        // Look up the animation given the name
        return lightMap[idx].BackpackLightsForTrigger(assets, eventName);
    }
        #endif
}

}
