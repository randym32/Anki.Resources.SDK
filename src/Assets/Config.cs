// Copyright © 2020 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

namespace Anki.Resources.SDK
{

/// <summary>
/// The file that is used to configure the relative file locations
/// </summary>
class Platform_config
{
    /// <summary>
    /// The path to where the software cacahes items
    /// </summary>
    public string DataPlatformCachePath {get;set; }

    /// <summary>
    /// The path to where 
    /// </summary>
    public string DataPlatformPersistentPath {get;set; }

    /// <summary>
    /// The path to most configuration files and assets
    /// </summary>
    public string DataPlatformResourcesPath {get;set; }
}


/// <summary>
/// A helper class to make loading configuration easier
/// </summary>
class AssetsConfig
{
    /// <summary>
    /// The paths to search thru (in order) for the independent sprites
    /// </summary>
    public string[] independentSpritesSearchPaths  {get;set; }

    /// <summary>
    /// The paths to search thru (in order) for the sprite sequences
    /// </summary>
    public string[] spriteSequencesSearchPaths  {get;set; }
}
}



