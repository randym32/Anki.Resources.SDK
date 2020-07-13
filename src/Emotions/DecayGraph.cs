// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;


namespace Anki.Resources.SDK
{

/// <summary>
/// This describes how an emotion decays over time.
/// </summary>
public class DecayGraph
{
    /// <summary>
    /// The dimension or type of emotion ("Happy", "Confident", "Stimulated",
    /// "Social", or "Trust").   "default" also matches.
    /// </summary>
    /// <value>
    /// The dimension or type of emotion ("Happy", "Confident", "Stimulated",
    /// "Social", or "Trust").   "default" also matches.
    /// </value>
    public string emotionType {get; set;}="default";

    /// <summary>
    /// How to interpret the graph: "TimeRatio" or "ValueSlope"
    /// </summary>
    /// <value>
    /// "TimeRatio" or "ValueSlope"
    /// </value>
    public string graphType {get; set;} = "TimeRatio";

    /// <summary>
    /// This is a "time ratio" describing how the value decays. 
    /// </summary>
    /// <value>
    /// This is a "time ratio" describing how the value decays. 
    /// </value>
    public IReadOnlyList<XY> nodes {get; set; }
}



/// <summary>
/// The XY structure is used to define how a value (often the value associated
/// with an emotion dimension) should decay with time.  
/// </summary>
public class XY
{
    /// <summary>
    /// With time graphs, this is "the time in seconds since the most recent
    /// event (which changed the emotion by more than some delta)."
    /// 
    /// With value slopes, this is "the emotion value."
    /// </summary>
    /// <value>The time (in seconds) or the emotion value</value>
    public float x {get; set; }

    /// <summary>
    /// With time graphs, this is "the ratio of the original value that should
    /// be reached by the given time."
    /// 
    /// With value slopes, this is "the amount it decays (towards zero) per
    /// minute as a fixed amount (not a ratio)."  The value never goes below
    /// zero.
    /// </summary>
    /// <value>The ratio of the original value or amount of decay per minute.</value>
    public float y {get; set; }
}
}
