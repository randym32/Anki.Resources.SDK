// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Anki.Resources.SDK
{

/// <summary>
/// Inherits from PictureBox; adds Interpolation Mode Setting
/// </summary>
/// <remarks>
/// https://stackoverflow.com/questions/29157/how-do-i-make-a-picturebox-use-nearest-neighbor-resampling
/// </remarks>
public class PictureBoxWithInterpolationMode : PictureBox
{
    /// <summary>
    /// The interpolation method to employ
    /// </summary>
    public InterpolationMode InterpolationMode { get; set; }

    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {
        paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
        base.OnPaint(paintEventArgs);
    }
}
}
