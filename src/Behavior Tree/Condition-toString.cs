// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using Resource = Anki.Resources.SDK.Properties.Resources;
using System;
using System.Collections.Generic;

namespace Anki.Resources.SDK
{
/// <summary>
/// Schema information about conditions
/// </summary>
public class ConditionSchema
{
    /// <summary>
    /// The keys that refer to conditions.
    /// </summary>
    /// <value>The keys that refer to conditions.</value>
    public IReadOnlyList<string> conditionKeys {get;set; }

    /// <summary>
    /// This maps a condition type to the keys that are acceptable.
    /// </summary>
    /// <value>This maps a condition type to the keys that are acceptable.</value>
    public IReadOnlyDictionary<string,IReadOnlyList<string>> type2keys {get;set; }
}

/// <summary>
/// This is used to merge some of the clauses to be more elegant
/// </summary>
enum Grouping
{
    none,
    comparison,
    or,
    and
}


public partial class Assets
{
    /// <summary>
    /// Some common strings used by keys
    /// </summary>
    const string k_not = "not";
    const string k_and = "and";
    const string k_conditionType = "conditionType";
    const string k_emotion  = "emotion";

    /// <summary>
    /// The condition node schema.
    /// </summary>
    /// <value>
    /// The condition node schema.
    /// </value>
    public static readonly ConditionSchema ConditionSchema;


    /// <summary>
    /// Convert the condition JSON structure to a pretty-printed string
    /// </summary>
    /// <param name="cond">The condition JSON structure </param>
    /// <returns>The pretty-printed string</returns>
    public static string ConditionToString(IReadOnlyDictionary<string, object> cond)
    {
        // Punt to the helper, but throw out the extra
        return ConditionToString(cond, out _);
    }

    /// <summary>
    /// Looks up the localized string for the token
    /// </summary>
    /// <param name="token">The token to look up the localize string</param>
    /// <returns>The localized string</returns>
    static string Localized(string token)
    {
        return Resource.ResourceManager.GetString(token, Resource.Culture);
    }


    /// <summary>
    /// Convert the condition JSON structure to a pretty-printed string
    /// </summary>
    /// <param name="cond">The condition JSON structure </param>
    /// <param name="parenGrouping">The internal grouping used for the parenthesis</param>
    /// <returns>The pretty-printed string</returns>
    static string ConditionToString(IReadOnlyDictionary<string, object> cond, out Grouping parenGrouping)
    {
        parenGrouping = Grouping.none;

        // get the conditionType against which we'll cross reference everything
        if (!cond.TryGetValue(k_conditionType, out var _conditionType))
        {
            // Missing condition type
            return null;
        }
        var conditionType = (string)_conditionType;

        // Try the simple compounds
        if ("Compound" == conditionType)
        {
            // not, and, or
            if (cond.TryGetValue(k_not, out var _not))
            {
                var s = ConditionToString((Dictionary<string, object>)_not, out var needsParen2);
                if (Grouping.none != needsParen2)
                    s = "(" + s + ")";
                return "not " + s;
            }
            if (cond.TryGetValue(k_and, out var _and))
            {
                parenGrouping = Grouping.and;
                return ExprJoin("&&", (ICollection<object>)_and, Grouping.and);
            }
            if (cond.TryGetValue("or", out var _or))
            {
                parenGrouping = Grouping.or;
                return ExprJoin("||", (ICollection<object>)_or, Grouping.or);
            }
            return "???";
        }

        // Handle emotion dimensions
        if ("Emotion" == conditionType)
        {
            parenGrouping = Grouping.comparison;
            return cond[k_emotion] + " >= " + cond["min"];
        }

        // Handle timers
        if ("BehaviorTimer" == conditionType)
        {
            return "Timer " + cond["timerName"] + " expired" + ConditionParams(cond, new string[] { "cooldown_s" });
        }
        if ("TimedDedup" == conditionType)
        {
            return "Timer Dedup " + ConditionParams(cond, new string[] { "dedupInterval_ms" }) +
                "{" + ConditionToString((Dictionary<string, object>)cond["subCondition"], out _) + "}";
        }

        // Handle on / off treads
        if ("OffTreadsState" == conditionType)
        {
            var targetState = (string)cond["targetState"];
            var s = Localized(targetState);

            if (null == s)
                s = Localized("OffTreads");
            return  s+ ConditionParams(cond, new string[] { "minTimeSinceChange_ms", "maxTimeSinceChange_ms" });
        }

        // Handle battery level
        if ("BatteryLevel" == conditionType)
        {
            var targetBatteryLevel = (string)cond["targetBatteryLevel"];
            return string.Format(Localized(conditionType), targetBatteryLevel);
        }

        // Handle being held
        if ("BeingHeld" == conditionType)
        {
            return (bool)cond["shouldBeHeld"] ? "being held" : "not being held";
        }
        if ("RobotHeldInPalm" == conditionType)
        {
            var s = (bool)cond["shouldBeHeldInPalm"] ? "being held in palm" : "not being held in palm";
            return s + ConditionParams(cond, new string[] { "minDuration_s" });
        }

        // Handle feature gate
        if ("FeatureGate" == conditionType)
        {
            var feature = (string)cond["feature"];
            var expected = true;
            if (cond.TryGetValue("expected", out var _expected))
                expected = (bool)_expected;
            return $"{feature} is " + (expected ? "enabled" : "disabled");
        }
        if ("TrueCondition" == conditionType)
        {
            return "true";
        }

        // Look up the short description for the condition type
        var desc = Localized(conditionType);

        // Look up the parameters and add them i
        var type2keys = ConditionSchema.type2keys;
        if (type2keys.TryGetValue(conditionType, out var _acceptableKeys))
        {
            // Append the parameters to the condition
            desc += ConditionParams(cond, _acceptableKeys);
        }

        // Return the result
        return desc;
    }


    /// <summary>
    /// Convert the sub expression into a string
    /// </summary>
    /// <param name="op">The operator joining the subexpression</param>
    /// <param name="exprs">The list of clauses</param>
    /// <param name="mergePrec">How the operators in the expression should be grouped for prosody</param>
    /// <returns>The new string</returns>
    static string ExprJoin(string op, ICollection<object> exprs, Grouping mergePrec)
    {
        // First, convert each of the sub expressions into a string
        var lst = new List<string>();
        foreach (var expr in exprs)
        {
            // Convert the sub exprssion into a string
            var condStr = ConditionToString((Dictionary<string, object>)expr, out var needParen);

            // Parenthesis for clarity, as needed
            lst.Add(needParen > Grouping.comparison && mergePrec != needParen ? $"({condStr})" : condStr);
        }

        // Join it all together
        return String.Join($" {op} ", lst);
    }


    /// <summary>
    /// Pretty print the parameters of the expression
    /// </summary>
    /// <param name="cond">The condition JSON structure </param>
    /// <param name="paramNames">The list of potential parameters</param>
    /// <returns>The pretty printed parameters</returns>
    static string ConditionParams(IReadOnlyDictionary<string, object> cond, IReadOnlyCollection<string> paramNames)
    {
        // This will hold each of the strings for the parameters
        var condParams = new List<string>();

        // Scan over the parameter names to build up the parameter list
        foreach (var a in paramNames)
        {
            var k = (string)a;
            if (k_conditionType == k) continue;
            if (cond.TryGetValue(k, out var tmp1))
            {
                var tmpS = tmp1;
                if (tmp1 is ICollection<object> tmpI)
                    tmpS = "[" + String.Join(", ", tmpI) + "]";
                condParams.Add($"{k}: {tmpS}");
            }
        }

        // Append the parameters to the condition
        if (condParams.Count > 0)
            return " (" + String.Join(", ", condParams) + ")";
        return "";
    }
}
}
