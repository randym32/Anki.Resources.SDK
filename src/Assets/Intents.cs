// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Anki.Resources.SDK
{
/// <summary>
/// Maps the cloud and application intents to the user intent
/// </summary>
class UserIntentsMap
{
    /// <summary>
    /// A table that maps the intent received from the cloud intent to animation
    /// and emotion responses.
    /// </summary>
    public IReadOnlyCollection<SimpleVoiceResponseMap> simple_voice_responses {get; set;}

    /// <summary>
    /// A table that maps the intent received by the cloud or application to
    /// the intent name used internally.    This includes renaming the parameters.
    /// </summary>
    public IReadOnlyCollection<UserIntentMap> user_intent_map {get; set;}

    /// <summary>
    /// The intent to employ if cloud’s intent cannot be found in the table
    /// above. Default: “unmatched_intent”
    /// </summary>
    public string unmatched_intent {get; set;}
}

/// <summary>
/// This is a structure that maps the intent received from the cloud intent to
/// animation and emotion responses.
/// </summary>
class SimpleVoiceResponseMap
{
    /// <summary>
    /// The intent name returned by the cloud.
    /// </summary>
    public string cloud_intent {get; set;}

    /// <summary>
    /// The animation and emotion changes that should occur in response to the
    /// intent.
    /// </summary>
    public SimpleVoiceResponse response {get; set;}
}

/// <summary>
/// The animation and emotion changes that should occur.
/// </summary>
public class SimpleVoiceResponse
{
    /// <summary>
    /// The AI behavior feature that should be activated.
    /// </summary>
    public string active_feature {get; set;}

    /// <summary>
    /// The trigger name of the animation to play.
    /// </summary>
    public string anim_group {get; set;}

    /// <summary>
    /// 
    /// </summary>
    public bool   disable_wakeword_turn {get; set;} = false;

    /// <summary>
    /// The name of the emotion event, describing how this intent affects
    /// Vector's current mood.
    /// </summary>
    public string emotion_event {get;set;}


}


/// <summary>
/// A table that maps the intent received by the cloud or application to
/// the intent name used internally.  This includes renaming the parameters.
/// </summary>
public class UserIntentMap
{
    /// <summary>
    /// The intent name sent by the SDK application.  Optional.
    /// </summary>
    public string app_intent {get; set;}

    /// <summary>
    /// A dictionary whose keys are the keys provided by the application's
    /// intent structure, and maps to the keys used internally.  Optional.
    /// </summary>
    public Dictionary<string,string> app_substitutions {get; set;}

    /// <summary>
    /// The intent name returned by the cloud.
    /// </summary>
    public string cloud_intent {get; set;}


    /// <summary>
    /// Names of keys that used as parameter values by the behaviour..??
    /// Optional.
    /// </summary>
    public IReadOnlyCollection<string> cloud_numerics {get; set;}

    /// <summary>
    /// A dictionary whose keys are the keys provided by the cloud’s intent
    /// structure, and maps to the keys used internally.  Optional.
    /// </summary>
    public IReadOnlyDictionary<string,string> cloud_substitutions {get; set;}

    /// <summary>
    /// The name of the feature that must be enabled before this intent can be
    /// processed.  Optional.
    /// </summary>
    public string feature_gate {get; set;}

    /// <summary>
    /// Default: true.  Optional.
    /// </summary>
    public bool test_parsing {get; set;} = true;

    /// <summary>
    /// The name of the intent used internally within Vector's engine.
    /// </summary>
    public string user_intent {get; set;}

}

partial class Assets
{
    /// <summary>
    /// The intent to employ if cloud’s intent cannot be found in the table
    /// above. Default: “unmatched_intent”
    /// </summary>
    public string unmatched_intent {get;set; }

    /// <summary>
    /// Maps the cloud intent to the user intent
    /// </summary>
    public IReadOnlyDictionary<string, UserIntentMap> Cloud2UserIntent;

    /// <summary>
    /// Maps the cloud intent to the response
    /// </summary>
    public IReadOnlyDictionary<string, SimpleVoiceResponse> Cloud2Response;

    /// <summary>
    /// Maps the application intent to the user intent.
    /// </summary>
    public IReadOnlyDictionary<string, UserIntentMap> App2UserIntent;
    
    /// <summary>
    /// Loads the user intents configuration folder
    /// </summary>
    void LoadIntents()
    {
        // get the path to the animation file
        var path = Path.Combine(cozmoResourcesPath, "config/engine/behaviorComponent/user_intent_maps.json");
        // Skip it if the file doesn't exist
        if (!File.Exists(path))
            return;

        // Get the text for the file
        var text = File.ReadAllText(path);

        // Get it in a convenient form
        var JSONOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                IgnoreNullValues=true
            };
        var userIntentMap = JsonSerializer.Deserialize<UserIntentsMap>(text, JSONOptions);

        // Map the cloud intents and the app intents to the user intents
        var cld = new Dictionary<string, UserIntentMap>();
        var app = new Dictionary<string, UserIntentMap>();
        foreach (var item in userIntentMap.user_intent_map)
        {
            if (null != item.cloud_intent)
                cld[item.cloud_intent]= item;
            if (null != item.app_intent)
                app[item.app_intent]= item;
        }
        Cloud2UserIntent= cld;
        App2UserIntent = app;
        // apply the more responses
        var clr = new Dictionary<string, SimpleVoiceResponse>();
        foreach (var item in userIntentMap.simple_voice_responses)
        {
            if (null != item.cloud_intent)
                clr[item.cloud_intent]= item.response;
        }
        Cloud2Response = clr;


    }
}
}
