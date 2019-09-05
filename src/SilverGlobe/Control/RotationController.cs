// Silver.Globe, version 0.11 for Silverlight 1.1 Alpha
// Copyright © Florian Krüsch (xaml-kru.com)
// xaml-kru.com/silverglobe
// This source is subject to the Microsoft Public License (Ms-PL).
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx.
// All other rights reserved.

using System;
using System.Windows.Threading;
using SilverGlobe.Data;

namespace SilverGlobe
{
    /// <summary>
    /// RotationController .
    /// </summary>
    internal sealed class RotationController
    {
        #region Members

        private RotationMode _currentMode = RotationMode.None;
		private DispatcherTimer _timer;
        private EaseOutRotationController _easeOutController;
        private MouseRotationController _mouseController;
        private SlerpController _slerpController;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true, if currently a quaternion slerp is performed on the globe.
        /// </summary>
        public Boolean DoesLocationAnimation
        {
            get { return _currentMode == RotationMode.Slerp; }
        }

        #endregion

        #region Constructor

        public RotationController(Globe globe, DispatcherTimer timer)
        {
            _easeOutController = new EaseOutRotationController(globe);
            _mouseController = new MouseRotationController(globe);
            _slerpController = new SlerpController(globe);
			_slerpController.SlerpCompleted += Slerp_Completed;
			_timer = timer;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Let the current rotation controller update its values
        /// </summary>
        public void Update()
        {
            switch (_currentMode)
            {
                case RotationMode.EaseOut:
                    _easeOutController.Update();
                    break;

                case RotationMode.Slerp:
                    _slerpController.Update();
                    break;
            }
        }

        /// <summary>
        /// Animate the globe to a given geographical position.
        /// </summary>
        public void AnimateTo(GeoPosition position)
        {
            _currentMode = RotationMode.Slerp;
            _mouseController.End();
            _slerpController.Init(position.GetQuaternion());
        }

        /// <summary>
        /// Bind the globe's rotation to the mouse.
        /// </summary>
        public void BindToMousePosition()
        {
            _currentMode = RotationMode.Mouse;

            _mouseController.Init();
        }

        /// <summary>
        /// Ease out the current globe rotation
        /// </summary>
        public void EaseOut()
        {
            _currentMode = RotationMode.EaseOut;
                        
            _mouseController.End();
            _easeOutController.Init(_mouseController.Axis, _mouseController.GetSpeed());
        }

        /// <summary>
        /// Stop all rotations.
        /// </summary>
        public void Stop()
        {
            _currentMode = RotationMode.None;
            _mouseController.End();
			_timer.Stop();
        }

        #endregion

        #region Implementation

        private void Slerp_Completed(Object sender, EventArgs e)
        {
            Stop();
        }

        #endregion   
    }
}
