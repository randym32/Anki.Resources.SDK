// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.Text.Json;

namespace RCM
{
public static partial class Util
{
    /// <summary>
    /// Converts the JSON element to a C# object
    /// </summary>
    /// <param name="item">The JSON element</param>
    /// <returns>The new thing</returns>
    static public object JsonToNormal(JsonElement item)
    {
        switch (item.ValueKind)
        {
            case JsonValueKind.Array : return ToArray(item);
            case JsonValueKind.Object: return ToDict(item);
            case JsonValueKind.String:
                    var s = item.GetString();
                    if ("True" == s) return true;
                    if ("False" == s) return false;
                    return s;
            case JsonValueKind.True  : return true;
            case JsonValueKind.False : return false;
            case JsonValueKind.Number: return item.GetDouble();
        }
        return null;
    }

    /// <summary>
    /// Convert the JSON to a dictionary to an array of strings
    /// </summary>
    /// <param name="jsonDictionary">The dictionary of JSON elements</param>
    /// <returns>The dictionary</returns>
    static public Dictionary<string, object> ToDict(Dictionary<string, object> jsonDictionary)
    {
        if (null == jsonDictionary)
            return null;
        var ret = new Dictionary<string, object>();
        foreach (var item in jsonDictionary)
        {
            if (null != item.Value)
                ret[item.Key] = JsonToNormal((JsonElement)item.Value);
        }
        return ret;
    }

    /// <summary>
    /// Convert the JSON element to an a dictionary
    /// </summary>
    /// <param name="jsonDictionary">The JSON element</param>
    /// <returns>The dictionary</returns>
    static public Dictionary<string, object> ToDict(JsonElement jsonDictionary)
    {
        var ret = new Dictionary<string, object>();
        foreach (var item in jsonDictionary.EnumerateObject())
        {
            ret[item.Name] = JsonToNormal(item.Value);
        }
        return ret;
    }


    /// <summary>
    /// Convert the JSON element to an array
    /// </summary>
    /// <param name="v">The JSON object to make into a an array</param>
    /// <returns>The array</returns>
    static object[] ToArray(JsonElement v)
    {
        var ret = new List<object>();
        foreach (var item in v.EnumerateArray())
            ret.Add(JsonToNormal(item));
        return ret.ToArray();
    }
}
}