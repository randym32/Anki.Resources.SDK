// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Collections.Generic;
using System.IO;

namespace Anki.Resources.SDK
{
/// <summary>
/// A helper for behaviors like games and weather
/// </summary>
public partial class BehaviorCoordinator
{
    /// <summary>
    /// A list of the independent sprites that are internally referenced
    /// </summary>
    public readonly IReadOnlyList<string> IndependentSpritesUsed;

    /// <summary>
    /// A list of the composite images internally referenced
    /// </summary>
    public readonly IReadOnlyList<string> CompositeImagesUsed;

    /// <summary>
    /// Constructor for the object
    /// </summary>
    /// <param name="independentSpritesUsed">A list of the independent sprites that are internally referenced</param>
    /// <param name="compositeImagesUsed">A list of the composite images that are internally referenced</param>
    internal BehaviorCoordinator(IReadOnlyList<string> independentSpritesUsed=null,
                               IReadOnlyList<string> compositeImagesUsed=null)
    {
        this . IndependentSpritesUsed = independentSpritesUsed;
        this . CompositeImagesUsed    = compositeImagesUsed;
    }
}
}
