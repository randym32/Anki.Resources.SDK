// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using RCM;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Anki.Resources.SDK
{
/// <summary>
/// Schema information about behaviors
/// </summary>
class BehaviorSchema:ConditionSchema
{
    /// <summary>
    /// The keys that refer to other behaviors (via a behavior ID)
    /// </summary>
    public IReadOnlyList<string> behaviorRefKeys {get;set; }
}

#if false
/// <summary>
/// A mode in the behavior tree
/// </summary>
class BehaviorNode
{
    /// <summary>
    /// The name of behavior node
    /// </summary>
	string behaviorID;
    /// <summary>
    /// The class of the behavior
    /// </summary>
	string behaviorClass;
	Dictionary<string,object> fields;
}
#endif

partial class Assets
{
    /// <summary>
    /// The beahvior node schema
    /// </summary>
    static readonly BehaviorSchema behaviorSchema;

    /// <summary>
    /// The Weather
    /// </summary>
    public Weather Weather     {get;internal set;}

    /// <summary>
    /// The blackjack game
    /// </summary>
    public BlackJack BlackJack {get;internal set;}

#if false
    /// <summary>
    /// This maps a field in a behavior node to the classes of node it appears in
    /// </summary>
    Dictionary<string, Dictionary<string, string>> behaviorField2Class =
        new Dictionary<string, Dictionary<string, string>>();

    /// <summary>
    /// This maps a behaviorID to the nodes that called it.
    /// This is used as part of analyzing the behavior tree system
    /// </summary>
    Dictionary<string, Dictionary<string, string>> behaviorTreeTopo = 
        new Dictionary<string, Dictionary<string, string>>();
#endif
		
    /// <summary>
    /// </summary>
	readonly Dictionary<string,Dictionary<string,object>> behaviorNodes = new Dictionary<string,Dictionary<string,object>>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configPath"></param>
    void LoadBehaviors(string configPath)
    {
        // First the behavior coordinators
        // Load the weather
        Weather = new Weather(Path.Combine(configPath,"weather"));
        // Load the clock
        // Clock = new Clock();
        // Load the blackjack game
        BlackJack = new BlackJack();
        // Load the cube spinner game
        // Load the behavior config
        var path = Path.Combine(configPath,"victor_behavior_config.json");
        // Get the text for the file
        var text = File.ReadAllText(path);

        // Load all of the behavior nodes, and sort out the tree
        // It's just easier rather than trying to on demand load them
        // The JSON parsing options
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        var behaviorsPath = Path.Combine(configPath,"behaviors");
        var files = Directory.EnumerateFiles(behaviorsPath, "*.json", SearchOption.AllDirectories);
        foreach (string currentFile in files)
        {
            // Get the text file
            text = File.ReadAllText(currentFile);
            // Get the dictionary
            var d = Util.ToDict(JsonSerializer.Deserialize<Dictionary<string,object>>(text, JSONOptions));

            // Store in tree
            behaviorNodes[(string) d["behaviorID"]] = d;
        }
    }
	
#if false
    /// <summary>
    /// Look up the behavior node for the id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
	BehaviorNode BehaviorNode(string id)
	{
		// See if it is already known
		if (behaviorNodes.TryGetValue(id, out var node))
			return node;
		
		// Load all files?
		// Try finding one with the right name?

		// Otherwise, look stuff up
		behaviorNodes[id] = node;
		return node;
	}
#endif
}

}
