// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using SilverGlobe.Math3D;
using SilverGlobe.Data;

namespace SilverGlobe
{
    /// <summary>
    /// Provide methods for projecting geographical positions.
    /// </summary>
    public interface IPositionProvider
    {
        Point3D ProjectPosition(GeoPosition pos);
    }
}
