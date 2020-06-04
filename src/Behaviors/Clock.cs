// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

using System.Collections.Generic;

namespace Anki.Resources.SDK
{
/// <summary>
/// The behavior to support the Clock / Timer behaviors
/// </summary>
public partial class Clock : BehaviorCoordinator
{
    /// <summary>
    /// 
    /// </summary>
    internal Clock() : base(new List<string>()
        {
        "clock_00",
        "clock_01",
        "clock_02",
        "clock_03",
        "clock_04",
        "clock_05",
        "clock_06",
        "clock_07",
        "clock_08",
        "clock_09",
        "clock_colon",
        "clock_empty_grid"})
    {
    }
}
}
