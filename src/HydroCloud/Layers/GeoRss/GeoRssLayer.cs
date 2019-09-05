using System;
using System.Windows;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcGIS.Samples
{
    public class GeoRssLayer : GraphicsLayer
    {
		ESRI.ArcGIS.Samples.GeoRss.GeoRssLoader loader;

        #region Constructor:
        public GeoRssLayer() : base()
        {
			loader = new ESRI.ArcGIS.Samples.GeoRss.GeoRssLoader();
			loader.LoadCompleted += loader_LoadCompleted;
			loader.LoadFailed += loader_LoadFailed;
        }

		private void loader_LoadFailed(object sender, ESRI.ArcGIS.Samples.GeoRss.GeoRssLoader.RssLoadFailedEventArgs e)
		{
			this.InitializationFailure = e.ex;
			if (!IsInitialized)
				base.Initialize();
		}

		private void loader_LoadCompleted(object sender, ESRI.ArcGIS.Samples.GeoRss.GeoRssLoader.RssLoadedEventArgs e)
		{
			this.Graphics.Clear();
			foreach (Graphic g in e.Graphics)
			{
				g.Symbol = Symbol;
				this.Graphics.Add(g);
			}
			if(!IsInitialized)
				base.Initialize();
		}
        #endregion

        #region Overriden Methods:
        //Overriding the Initialize method to fetch the RSS feed first
        public override void Initialize()
        {
            Update();
        }
        #endregion

        #region Dependency Properties:
        //The ESRI picture marker symbol used while rendering the layer
        public MarkerSymbol Symbol
        {
			get { return (MarkerSymbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }
        public static readonly DependencyProperty SymbolProperty =
			DependencyProperty.Register("Symbol", typeof(MarkerSymbol), typeof(GeoRssLayer), null);

        //The URL of the RSS feed
		public Uri Source
        {
			get { return ((Uri)GetValue(SourceProperty)); }
			set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register("Source", typeof(Uri), typeof(GeoRssLayer), null);
        
        #endregion

		public void Update()
		{
			if (Source != null)
			{
				loader.LoadRss(Source, null);
			}
		}
	}
}
