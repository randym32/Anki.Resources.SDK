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
    /// A list of internally referenced the independent sprites.
    /// </summary>
    /// <value>The identifier (or name) of the independent sprites used by the behavior.</value>
    public readonly IReadOnlyList<string> IndependentSpritesUsed;

    /// <summary>
    /// A list of the internally referenced composite images.
    /// </summary>
    /// <value>The identifier (or name) of the composite images used by the behavior.</value>
    public readonly IReadOnlyList<string> CompositeImagesUsed;

    /// <summary>
    /// Constructor for the object
    /// </summary>
    /// <param name="independentSpritesUsed">A list of the internally referenced independent sprites.</param>
    /// <param name="compositeImagesUsed">A list of the internally referenced composite images.</param>
    internal BehaviorCoordinator(IReadOnlyList<string> independentSpritesUsed=null,
                               IReadOnlyList<string> compositeImagesUsed=null)
    {
        this . IndependentSpritesUsed = independentSpritesUsed;
        this . CompositeImagesUsed    = compositeImagesUsed;
    }
}
}
