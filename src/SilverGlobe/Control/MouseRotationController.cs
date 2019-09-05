// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using System.Windows;
using SilverGlobe.Math3D;
using System.Windows.Input;

namespace SilverGlobe
{
    /// <summary>
    /// MouseRotationController controls the globe by rotating it towards the current mouse position.
    /// </summary>
    internal sealed class MouseRotationController
    {
        #region Members

        private Boolean _start = false;
        private Vector _position;
        private DateTime _lastTime;

        private Double _speed = 0d;
        private Vector3D _rotationAxis = Vector3D.YAxis * -1;

        private Globe _globe;
        private FrameworkElement _mouseEventSource;

        #endregion

        #region Properties

        public Vector3D Axis
        {
            get { return _rotationAxis; }
        }

        #endregion

        #region Constructor

        public MouseRotationController(Globe globe)
        {
            _globe = globe;
            _mouseEventSource = (FrameworkElement)Application.Current.RootVisual;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start controlling the globe.
        /// </summary>
        public void Init()
        {
            _lastTime = DateTime.Now;
            _position = Vector.Empty;
            _start = true;
            _speed = 0d;

            _mouseEventSource.MouseMove -= MouseMove_Rotate;
            _mouseEventSource.MouseMove += MouseMove_Rotate;
        }

        /// <summary>
        /// Stop controlling the globe.
        /// </summary>
        public void End()
        {
            _mouseEventSource.MouseMove -= MouseMove_Rotate;
        }

        /// <summary>
        /// Get the current rotation speed.
        /// </summary>
        public Double GetSpeed()
        {
            Double speed = _speed;
            _speed = 0d;

            return speed;
        }

        #endregion

        #region Implementation

        private void MouseMove_Rotate(object sender, MouseEventArgs e)
        {
            // mouse position relativ to Globe's center
            Vector mousePos = new Vector(e.GetPosition(_globe)) - new Vector(_globe.Width / 2, _globe.Height / 2);

            if (_start)
            {
                _start = false;
                _position = mousePos;
                return;
            }

            // position
            Vector oldPos = _position;                        

            // time difference
            DateTime now = DateTime.Now;
            Double dt = (now - _lastTime).TotalSeconds;
            _lastTime = now;

            // smoothed position
            _position = mousePos * 0.7 + _position * 0.3;

            // direction
            Vector dir = _position - oldPos;
            if (dir == Vector.Empty) return;

            // polar coordinates of mouse position
            Double angle = -Math.Atan2(_position.Y, _position.X);
            Double radius = _position.Length;

            Double dx;
            dx = radius / _globe.ScreenRadius;
            dx = dx.Clamp(0, 1) * 0.75;
            dx = dx * dx;

            Vector v = dir.Normalized * (1d - dx);

            _rotationAxis = new Vector3D(v.Y, -v.X, v.Rotate(angle).Y * dx);

            Double k = 0.2 / dt;
            if (k > 1) k = 1;
            _speed = dir.Length * k * 0.3;

            _globe.Orientation *= Quaternion.ForRotation(_rotationAxis, _speed);
            _globe.Orientation.Normalize();
        }

        #endregion
    }
}
