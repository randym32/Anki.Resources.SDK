// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Drawing;

namespace Anki.Resources.SDK
{
public partial class Assets
{
    /// <summary>
    /// The size of the LCD.
    /// </summary>
    /// <value>
    /// The size of the LCD.
    /// </value>
    public readonly Size DisplaySize;

    /// <summary>
    /// Vector's display size
    /// </summary>
    const int VectorDisplayWidth  = 184;
    const int VectorDisplayHeight = 96;

    /// <summary>
    /// Cozmo's display size
    /// </summary>
    const int CozmoDisplayWidth   = 128;
    const int CozmoDisplayHeight  = 64;
}
}
