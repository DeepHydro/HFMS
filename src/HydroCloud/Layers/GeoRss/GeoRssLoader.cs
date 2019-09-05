using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Samples.GeoRss
{
	public class GeoRssLoader
	{
		public class RssLoadedEventArgs : EventArgs
		{
			public object UserState { get; set; }
			public System.Collections.Generic.IEnumerable<Graphic> Graphics { get; set; }
		}
		public class RssLoadFailedEventArgs : EventArgs
		{
			public object UserState { get; set; }
			public Exception ex { get; set; }
		}
		public event EventHandler<RssLoadedEventArgs> LoadCompleted;
		public event EventHandler<RssLoadFailedEventArgs> LoadFailed;

		public void LoadRss(Uri feedUri, object userToken)
		{
			WebClient wc = new WebClient();
			wc.OpenReadCompleted += new OpenReadCompletedEventHandler(wc_OpenReadCompleted);
			wc.OpenReadAsync(feedUri, userToken);
		}

		//Adding symbols from each entry read from the feed to the graphics object of the layer
		private void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				if(LoadFailed!=null)
					LoadFailed(this, new RssLoadFailedEventArgs()
					{
						ex = new Exception("Error in Reading the RSS feed. Try Again later!", e.Error),
						UserState = e.UserState
					});
				return;
			}

			ESRI.ArcGIS.Client.GraphicCollection graphics = new ESRI.ArcGIS.Client.GraphicCollection();

			using (Stream s = e.Result)
			{
				SyndicationFeed feed;
				List<SyndicationItem> feedItems = new List<SyndicationItem>();

				using (XmlReader reader = XmlReader.Create(s))
				{
					feed = SyndicationFeed.Load(reader);
					foreach (SyndicationItem feedItem in feed.Items)
					{
						SyndicationElementExtensionCollection ec = feedItem.ElementExtensions;

						string x = "";
						string y = "";

						foreach (SyndicationElementExtension ee in ec)
						{
							XmlReader xr = ee.GetReader();
							switch (ee.OuterName)
							{
								case ("lat"):
									{
										y = xr.ReadElementContentAsString();
										break;
									}
								case ("long"):
									{
										x = xr.ReadElementContentAsString();
										break;
									}
								case ("point"):
									{
										string sp = xr.ReadElementContentAsString();
										string[] sxsy = sp.Split(new char[] { ' ' });
										x = sxsy[1];
										y = sxsy[0];
										break;
									}
							}
						}

						if (!string.IsNullOrEmpty(x))
						{
							Graphic graphic = new Graphic()
							{
								Geometry = new MapPoint(Convert.ToDouble(x), Convert.ToDouble(y))
							};

							graphic.Attributes.Add("Title", feedItem.Title.Text);
							graphic.Attributes.Add("Summary", feedItem.Summary.Text);
							graphic.Attributes.Add("PublishDate", feedItem.PublishDate);
							graphic.Attributes.Add("Id", feedItem.Id);

							graphics.Add(graphic);
						}
					}
				}
			}
			
			//Invoking the initialize method of the base class to finish the initialization of the graphics layer:
			if (LoadCompleted != null)
				LoadCompleted(this, new RssLoadedEventArgs()
				{
					Graphics = graphics,
					UserState = e.UserState
				}
				);
		}
	}
}
