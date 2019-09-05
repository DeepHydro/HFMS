// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using SilverGlobe.Math3D;

namespace SilverGlobe
{
    /// <summary>
    /// EaseOutRotationController controls the globe by easeing out a given rotation.
    /// </summary>
    internal sealed class EaseOutRotationController
    {
        #region Members

        private DateTime _lastTick;

        private Globe _globe;

        private Vector3D _axis;
        private Double _speed;

        #endregion

        #region Constructor

        public EaseOutRotationController(Globe globe)
        {
            _globe = globe;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start controlling the globe.
        /// </summary>
        public void Init(Vector3D axis, Double initialSpeed)
        {
            _lastTick = DateTime.Now;
            _speed = initialSpeed;

            _axis = axis;
        }

        #endregion

        #region Implementation

        public void Update()
        {            
            DateTime now = DateTime.Now;
            Double dt = (now - _lastTick).TotalSeconds;
            _lastTick = now;

            Double k = 0.2 / dt;
            if (k > 1) k = 1;

            // slow down
            Double ds = _speed * 0.97 - _speed;
            _speed += ds * k;

            _globe.Orientation *= Quaternion.ForRotation(_axis, _speed);
            _globe.Orientation.Normalize();
        }

        #endregion
    }

}
