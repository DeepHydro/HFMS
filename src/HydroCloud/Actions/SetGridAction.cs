using ESRI.ArcGIS.Client;
//using HeiflowOnline.SpatialTemporal;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ESRI.ArcGIS.Samples
{
    public class SetGridAction : TargetedTriggerAction<Map>
    {

        protected override void Invoke(object parameter)
        {
            //GridRender.Instance.VariableIndex = this.VariableIndex;
            //GridRender.Instance.Render(null);
        }

        /// <summary>
        /// Gets or sets the ID of layer to zoom to.
        /// </summary>
        /// <value>The layer ID.</value>
        public string LayerID
        {
            get { return (string)GetValue(LayerIDProperty); }
            set { SetValue(LayerIDProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="LayerID"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LayerIDProperty =
            DependencyProperty.Register("LayerID", typeof(string), typeof(SetGridAction), null);


        /// <summary>
        /// Gets or sets the ID of layer to zoom to.
        /// </summary>
        /// <value>The layer ID.</value>
        public int VariableIndex
        {
            get { return (int)(GetValue(VariableIndexProperty)); }
            set { SetValue(VariableIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Url"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VariableIndexProperty =
            DependencyProperty.Register("VariableIndex", typeof(int), typeof(SetGridAction), null);
    }
}
