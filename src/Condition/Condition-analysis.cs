// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

using System;
using System.Collections.Generic;
using System.Text;

namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// The condition node schema
    /// </summary>
    static readonly ConditionSchema conditionSchema;

    /// <summary>
    /// This maps a field in a condition node to its condition type
    /// </summary>
    /// <remarks>
    /// Note: the condition type is the analog to a behavior node's class.
    /// </remarks>
    private static readonly Dictionary<string, string> conditionField2Type = new Dictionary<string, string>();


    /// <summary>
    /// This is used to help analyze a condition with in a behavior node
    /// </summary>
    /// <param name="errs">The errors found within the condition tree</param>
    /// <param name="errKey">The label to help identify which condition this
    /// error is referring to</param>
    /// <param name="cond">The condition structure</param>
    public void AnalyzeCondition(StringBuilder errs, string errKey, Dictionary<string, object> cond)
    {
        // See if it contains nested conditions
        foreach (var keyName in (ICollection<object>)conditionSchema.conditionKeys)
            if (cond.TryGetValue((string)keyName, out var x))
            {
                // Typically the item should refer to a single condition, but can be
                // multiple with 'and' and 'or'
                if (x is Dictionary<string, object> z)
                    AnalyzeCondition(errs, errKey, z);
                else if (x is IEnumerable<object> y)
                    foreach (var b in y)
                        AnalyzeCondition(errs, errKey, (Dictionary<string, object>)b);
                else
                    errs.AppendLine($"{errKey}: '{keyName}' has wrong type");
            }

        // get the conditionType against which we'll cross reference everything
        if (!cond.TryGetValue(k_conditionType, out var _conditionType))
        {
            // Missing condition type
            errs.Append($"{errKey}: missing conditionType \n");
            return;
        }
        var conditionType = (string)_conditionType;

        // Make a note of which condition types have which fields
        foreach (var kv in cond)
        {
            // See if the key is already in there
            if (conditionField2Type.TryGetValue(kv.Key, out var prevClass))
            {
                // If it is already generic or the same class, skip
                if (0 == prevClass.Length) continue;
                if (conditionType == prevClass)
                    continue;

                // It is in more than one class, assume it is generic for now
                conditionField2Type[kv.Key] = "";
                continue;
            }

            // Make a note of the condition type
            conditionField2Type[kv.Key] = conditionType;
        }

        // Got through each of the keys and check that it is allowed with this condition type
        var type2keys = conditionSchema.type2keys;
        if (!type2keys.TryGetValue(conditionType, out var _acceptableKeys))
        {
            errs.AppendLine($"{errKey}: condition '{conditionType}' is not a recognized conditionType");
            return;
        }

        var acceptableKeys = (object[])_acceptableKeys;
        foreach (var key in cond.Keys)
        {
            if (Array.IndexOf(acceptableKeys, key) < 0)
                errs.AppendLine($"{errKey}: condition '{conditionType}' has an unexpected key '{key}'");
        }

        // check the rules on the condition type
        // - only one and, or, or not with compound
        int compoundKeys = 0;
        if (cond.TryGetValue(k_not, out var _not))
        {
            compoundKeys++;
            if (!(_not is Dictionary<string, object>))
                errs.AppendLine($"{errKey}: 'not' has wrong value type");
        }
        if (cond.TryGetValue("and", out var _and))
        {
            compoundKeys++;
            if (!(_and is ICollection<object>))
                errs.AppendLine($"{errKey}: 'and' has wrong value type");
            if (((ICollection<object>)_and).Count < 2)
                errs.AppendLine($"{errKey}: 'and' has the wrong number of arguments");
        }
        if (cond.TryGetValue("or", out var _or))
        {
            compoundKeys++;
            if (!(_or is ICollection<object>))
                errs.AppendLine($"{errKey}: 'or' has wrong value type");
            if (((ICollection<object>)_or).Count < 2)
                errs.AppendLine($"{errKey}: 'or' has the wrong number of arguments");
        }
        // Check that there is only a single and/or/not
        if (compoundKeys > 1)
        {
            errs.AppendLine($"{errKey}: 'compound' node has too many and/or/not clauses");
        }

        // Check some specifics on feature gates
        if ("FeatureGate" == conditionType)
        {
            // Check that the feature is known
            var feature = (string) cond["feature"];
            if (!featureToggle.ContainsKey(feature))
            {
                errs.AppendLine($"{errKey}: feature '{feature}' is not defined feature");
            }
        }
        else if ("Emotion" == conditionType)
        {
            // Check that emotion is within bound
            var emotionType = (string) cond["emotion"];
            var minV = (float) cond["emotion"];

            if (!emotionRanges.TryGetValue(emotionType, out var mm))
            {
                errs.AppendLine($"{errKey}: '{emotionType}' is not a defined emotion type.");
            }
            else if (minV < mm.min || minV > mm.max)
            {
                errs.AppendLine($"{errKey}: value is out of range for emotion type '{emotionType}'");
            }
        }
    }
}
}
