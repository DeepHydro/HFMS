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
    internal delegate Quaternion SlerpMethod(Double n);

    /// <summary>
    /// SlerpController controls the globe by rotating it towards another quaternion
    /// performing a spherical linear interpolation.
    /// </summary>
    internal sealed class SlerpController
    {
        #region Members

        private DateTime _startTime;

        private Globe _globe;
        public Globe Globe
        {
            get { return _globe; }
            set { _globe = value; }
        }

        private Quaternion _start;
        private Quaternion _end;

        private Double _timeFactor;

        private const Double ThresholdToLinear = 0.9995;

        private SlerpMethod _doSlerp;

        #endregion

        #region Events

        public event EventHandler SlerpCompleted;

        #endregion

        #region Constructor

        public SlerpController(Globe globe)
        {
            _globe = globe;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start controlling the globe.
        /// </summary>
        public void Init(Quaternion target)
        {
            _start = _globe.Orientation;
            _end = target;

            _startTime = DateTime.Now;

            // inner product
            Double dotProduct = _start.Dot(_end).Clamp(-1, 1);

            // is linear sufficient?
            if (dotProduct > ThresholdToLinear)
            {
                _timeFactor = 1;

                // set strategy to linear
                _doSlerp = (Double u) =>
                {
                    if (u >= 1) return _end;

                    Quaternion q = _start + (_end - _start) * u;
                    q.Normalize();

                    return q;
                };
            }
            else
            {
                Double angle = Math.Acos(dotProduct);
                Quaternion q;

                if (angle > (Math.PI / 2))
                {
                    // left
                    angle = Math.PI - angle;
                    q = _start * dotProduct - _end;
                }
                else
                {
                    // right
                    q = _end - _start * dotProduct;
                }

                q.Normalize();

                _timeFactor = Math.Sqrt((Math.PI / 2) / angle);

                _doSlerp = (Double u) =>
                {
                    if (u >= 1) return _end;

                    Double t = angle * u;
                    return _start * Math.Cos(t) + q * Math.Sin(t);
                };
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Perform update calculations
        /// </summary>
        public void Update()
        {
            // calc time factor 
            TimeSpan ts = DateTime.Now - _startTime;
            Double n = (ts.TotalSeconds) * 0.6 * _timeFactor;
            if (n > 1) n = 1;

            n = 0.5 + Math.Sin((n - 0.5) * Math.PI) * 0.5;
            n = Math.Sqrt(n);

            // perform slerp
            _globe.Orientation = _doSlerp(n);

            if (n == 1)
            {
                // fire completed event
                if (SlerpCompleted != null)
                {
                    EventHandler h = SlerpCompleted;
                    h(this, EventArgs.Empty);
                }
            }
        }

        #endregion
    }

}
