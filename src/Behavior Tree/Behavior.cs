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
public class BehaviorSchema:ConditionSchema
{
    /// <summary>
    /// The keys that refer to other behaviors (via a behavior ID)
    /// </summary>
    /// <value>The names of fields in a behavior configuration</value>
    public IReadOnlyList<string> behaviorRefKeys {get;set; }

    /// <summary>
    /// The keys that refer to other animations (via a animation trigger)
    /// </summary>
    /// <value>The names of fields in a behavior configuration</value>
    public IReadOnlyList<string> animationTriggerKeys { get; set; }
    
}

/// <summary>
/// A node in the behavior tree.   This node can have conditions that trigger
/// it, and behaviours (if the conditions are satisfied) that it carries out.
/// </summary>
public class BehaviorNode
{
    /// <summary>
    ///   This is used to
    /// allow other behavior tree nodes (and top level behaviors) to refer to
    /// it.
    /// </summary>
    /// <value>The name (or identifier) of a behavior tree node.</value>
	public string behaviorID
    {
        get
        {
            // There are two possible identifiers for the behavior.
            // The global behaviors have a behavior ID
            if (fields.TryGetValue("behaviorID", out var ret))
                return (string) ret;

            // Anonymous behaviors have behaviour name
            return (string)fields["behaviorName"];
        }
    }

    /// <summary>
    /// The class of the behavior.  This is used by Vector behavior engine to
    /// link to the software that implements the functions/actions of the behavior.
    /// </summary>
    /// <value>The class name of the behavior.</value>
    public string behaviorClass
    {
        get { return (string)fields["behaviorClass"]; }
    }

    /// <summary>
    /// The fields and values used to configure this behaviour node
    /// </summary>
	public readonly IReadOnlyDictionary<string,object> fields;

    /// <summary>
    /// Constructs a behavior node
    /// </summary>
    /// <param name="fields">A table of the key-value pairs of parameters</param>
    internal BehaviorNode(IReadOnlyDictionary<string, object> fields)
    {
        this.fields = fields;
    }

    /// <summary>
    /// Provides an enumeration of anonymous behavior nodes contained within
    /// this behavior node.
    /// </summary>
    public IEnumerable<BehaviorNode> anonymousBehaviors
    {
        get
        {
            // See if it contains nested behaviors
            if (!fields.TryGetValue("anonymousBehaviors", out var x))
            {
                yield break;
            }
            // Return each of the anonymous modes
            foreach (var b in (IEnumerable<object>)x)
            {
                yield return new BehaviorNode((Dictionary<string, object>)b);
            }
        }
    }
}


/// <summary>
/// This is a container of the behavior tree nodes in the behavior tree
/// </summary>
public class BehaviorTree
{
    /// <summary>
    /// The table of behavior nodes, mapping an id to the behavior node.
    /// </summary>
    readonly IReadOnlyDictionary<string, BehaviorNode> behaviorNodes;

    /// <summary>
    /// Constructor for the container of behavior tree nodes
    /// </summary>
    /// <param name="behaviorNodes">The table of behavior nodes, mapping an id to the behavior node.</param>
    internal BehaviorTree(IReadOnlyDictionary<string, BehaviorNode> behaviorNodes)
    {
        this.behaviorNodes = behaviorNodes;
    }

    /// <summary>
    /// Constructor for the container of behavior tree nodes
    /// </summary>
    /// <param name="behaviorsPath">The path to the behavior tree</param>
    internal BehaviorTree(string behaviorsPath)
    {
        // Load all of the behavior nodes, and sort out the tree
        // It's just easier rather than trying to on demand load them

        // The JSON parsing options
        var JSONOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            IgnoreNullValues    = true
        };

        // Scan over the behavior tree path and load all of the json files
        var files = Directory.EnumerateFiles(behaviorsPath, "*.json", SearchOption.AllDirectories);
        var _behaviorNodes = new Dictionary<string, BehaviorNode>();
        foreach (string currentFile in files)
        {
            // Get the text file
            var text = File.ReadAllText(currentFile);

            // Get the dictionary
            var d = Util.ToDict(JsonSerializer.Deserialize<Dictionary<string, object>>(text, JSONOptions));

            // Store in tree
            var node = new BehaviorNode(d);
            _behaviorNodes[node.behaviorID] = node;
        }
        this.behaviorNodes = _behaviorNodes;
    }


    /// <summary>
    /// Returns the ids for all of the behavior nodes in the behavior tree.
    /// Condition nodes are returned here.
    /// </summary>
    /// <value>An enumeration of the ids (keys) for each of behavior nodes in
    /// the behavior tree.</value>
    public IEnumerable<string> Ids => behaviorNodes.Keys;

    /// <summary>
    /// Returns the ids for all of the behavior nodes in the behavior tree.
    /// Condition nodes are returned here.
    /// </summary>
    /// <value>An enumeration of the ids (keys) for each of behavior nodes in
    /// the behavior tree.</value>
    public IEnumerable<string> Keys => behaviorNodes.Keys;


    /// <summary>
    /// Look up the behavior node for the id
    /// </summary>
    /// <param name="key">The ID for the behavior node</param>
    /// <returns>null on error, otherwise the node associated with the ID</returns>
	public BehaviorNode this[string key]
    {
        get
        {
            // See if it is already known
            // /All/ of the nodes are loaded at the start since their id doesn't
            // always match the file name
            behaviorNodes.TryGetValue(key, out var node);
            return node;
        }
    }
}

partial class Assets
{
    /// <summary>
    /// The beahvior node schema
    /// </summary>
    public static readonly BehaviorSchema behaviorSchema;

    /// <summary>
    /// The Weather behavior.
    /// </summary>
    /// <value>
    /// The Weather behavior
    /// </value>
    public Weather Weather     {get;internal set;}

    /// <summary>
    /// The blackjack game behavior.
    /// </summary>
    /// <value>
    /// The blackjack game behavior.
    /// </value>
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
    /// A table mapping a behavior to a behavior node id
    /// </summary>
	IReadOnlyDictionary<string, string> behavior2BehaviorNodeId;

    /// <summary>
    /// The container of the behavior tree nodes
    /// </summary>
    BehaviorTree behaviorTree;

    /// <summary>
    /// Load the behavior tree for Cozmo
    /// </summary>
    /// <param name="configPath">The path to Cozmo's behavior system</param>
    void LoadCozmoBehaviors(string configPath)
    {
        // Load the activities [todo]
        // Load the reactions[todo]
        // Load the behavior trees
        behaviorTree = new BehaviorTree(Path.Combine(configPath, "behaviors"));
    }

    /// <summary>
    /// Load the behavior tree for Vector
    /// </summary>
    /// <param name="configPath">The path to Vector's behavior system</param>
    void LoadVectorBehaviors(string configPath)
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

        // The JSON parsing options
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues    = true
            };
        // Get the dictionary mapping the main behavior to the behavior node id
        behavior2BehaviorNodeId = JsonSerializer.Deserialize<Dictionary<string, string>>(text, JSONOptions);


        // Load the behavior trees
        behaviorTree = new BehaviorTree(Path.Combine(configPath, "behaviors"));
    }


    /// <summary>
    /// Provides the container of behavior tree nodes.
    /// </summary>
    /// <value>The behavior tree and its nodes</value>
    public BehaviorTree BehaviorTree
    {
        get { return behaviorTree; }
    }

    /// <summary>
    /// Returns a mapping of a high level behavior name to the id node at the
    /// root of that behavior tree.
    /// </summary>
    /// <value>A mapping of a high-level behavior name to an identifier of a
    /// node within the behavior tree.</value>
    /// <remarks>
    /// The following are built-in to the Vector software and are not
    /// configured:
    ///  * PR demo
    ///  * factory behavior
    ///  * acoustic testing behavior
    /// </remarks>
    public IReadOnlyDictionary<string, string> BehaviorRootToNodeId
    {
        get
        {
            return behavior2BehaviorNodeId;
        }
    }

}

}
