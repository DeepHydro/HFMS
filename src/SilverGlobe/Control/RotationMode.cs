// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.


namespace SilverGlobe
{
    /// <summary>
    /// Possible control modes for 3D rotations
    /// </summary>
    public enum RotationMode
    {
        None = 0,   
        /// <summary>
        /// Mouse controlled
        /// </summary>
        Mouse = 1,  
        /// <summary>
        /// Quaternion slerp
        /// </summary>
        Slerp = 2,  
        /// <summary>
        /// Eas out rotation around a fixed axis
        /// </summary>
        EaseOut = 3 
    }
}
