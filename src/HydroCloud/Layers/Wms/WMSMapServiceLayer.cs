using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Samples
{
	/// <summary>
	/// A WMS service layer for the ArcGIS API for Microsoft Silverlight/WPF
	/// </summary>
	public class WMSMapServiceLayer : DynamicMapServiceLayer
	{
		string[] _layersArray;
		string _proxyUrl;
		BoundingExtent _boundingExtent = new BoundingExtent();

		// Coordinate system WKIDs in WMS 1.3 where X,Y (Long,Lat) is switched to Y,X (Lat,Long)
		private int[,] LatLongCRSRanges = new int[,] { { 4001, 4999 },
						{2044, 2045},   {2081, 2083},   {2085, 2086},   {2093, 2093},
						{2096, 2098},   {2105, 2132},   {2169, 2170},   {2176, 2180},
						{2193, 2193},   {2200, 2200},   {2206, 2212},   {2319, 2319},
						{2320, 2462},   {2523, 2549},   {2551, 2735},   {2738, 2758},
						{2935, 2941},   {2953, 2953},   {3006, 3030},   {3034, 3035},
						{3058, 3059},   {3068, 3068},   {3114, 3118},   {3126, 3138},
						{3300, 3301},   {3328, 3335},   {3346, 3346},   {3350, 3352},
						{3366, 3366},   {3416, 3416},   {20004, 20032}, {20064, 20092},
						{21413, 21423}, {21473, 21483}, {21896, 21899}, {22171, 22177},
						{22181, 22187}, {22191, 22197}, {25884, 25884}, {27205, 27232},
						{27391, 27398}, {27492, 27492}, {28402, 28432}, {28462, 28492},
						{30161, 30179}, {30800, 30800}, {31251, 31259}, {31275, 31279},
						{31281, 31290}, {31466, 31700} };

		public WMSMapServiceLayer()
			: base()
		{
			LayerList = new ObservableCollection<LayerInfo>();
		}

		#region Public Properties

		/// <summary>
		/// Required.  Gets or sets the URL to a WMS service endpoint.  
		/// For example, 
		/// http://sampleserver1.arcgisonline.com/ArcGIS/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/WMSServer,
		/// http://mesonet.agron.iastate.edu/cgi-bin/wms/nexrad/n0r.cgi.
		/// </summary>
		/// <value>The URL.</value>
		public string Url { get; set; }

		/// <summary>
		/// Required. Gets or sets the unique layer ids in a WMS service.  
		/// Each id is a string value.  At least one layer id must be defined.   
		/// </summary>
		/// <value>A string array of layer ids.</value>
		[System.ComponentModel.TypeConverter(typeof(StringToStringArrayConverter))]
		public string[] Layers
		{
			get { return _layersArray; }
			set
			{
				_layersArray = value;
				OnLayerChanged();
			}
		}

		/// <summary>
		/// Optional. Gets or sets the URL to a proxy service that brokers Web requests between the Silverlight 
		/// client and a WMS service.  Use a proxy service when the WMS service is not hosted on a site that provides
		/// a cross domain policy file (clientaccesspolicy.xml or crossdomain.xml).  You can also use a proxy to
		/// convert png images to a bit-depth that supports transparency in Silverlight.  
		/// </summary>
		/// <value>The proxy URL string.</value>
		public string ProxyUrl
		{
			get { return _proxyUrl; }
			set { _proxyUrl = value; }
		}
		/// <summary>
		/// Optional. Gets or sets the WMS version.  If SkipGetCapabilities property is set to true, this value determines version requested.  
		/// If SkipGetCapabilities is false, this value determines version to retrieve.  If no value specified, default value returned from 
		/// the site will be used.
		/// </summary>
		/// <value>The version string.</value>
		public string Version { get; set; }
		/// <summary>
		/// Optional. Gets or sets a value indicating whether to skip a request to get capabilities. 
		/// Default value is false.  Set SkipGetCapabilities if the site hosting the WMS service does not provide a
		/// cross domain policy file and you do not have a proxy page.  In this case, you must set the WMS service version.
		/// If true, the initial and full extent of the WMS Silverlight layer will not be defined.
		/// </summary>
		public bool SkipGetCapabilities { get; set; }
		/// <summary>
		/// Gets the title metadata for this service.
		/// </summary>
		public string Title { get; private set; }
		/// <summary>
		/// Gets the abstract metadata for this service.
		/// </summary>
		public string Abstract { get; private set; }
		/// <summary>
		/// Gets a list of layers available in this service.
		/// </summary>
		public ObservableCollection<LayerInfo> LayerList { get; private set; }
		
		#endregion

		private bool initializing;
		/// <summary>
		/// Initializes this a WMS layer.  Calls GetCapabilities if SkipGetCapabilities is false. 
		/// </summary>
		public override void Initialize()
		{
			if (initializing || IsInitialized) return;
			initializing = true;

			if (SkipGetCapabilities)
			{
				base.Initialize();
			}
			else
			{
				string wmsUrl = string.Format("{0}{1}{2}",
					Url,
					"?service=WMS&request=GetCapabilities&version=",
					Version);

				WebClient client = new WebClient();
				client.DownloadStringCompleted += client_DownloadStringCompleted;
				client.DownloadStringAsync(PrefixProxy(wmsUrl));
			}
		}

		private Uri PrefixProxy(string url)
		{
			if (string.IsNullOrEmpty(ProxyUrl))
				return new Uri(url, UriKind.RelativeOrAbsolute);
			string proxyUrl = ProxyUrl;
			if (!proxyUrl.Contains("?"))
			{
				if (!proxyUrl.EndsWith("?"))
					proxyUrl = ProxyUrl + "?";
			}
			else
			{
				if (!proxyUrl.EndsWith("&"))
					proxyUrl = ProxyUrl + "&";
			}
			if (ProxyUrl.StartsWith("~") || ProxyUrl.StartsWith("../")) //relative to xap root
			{
				string uri = Application.Current.Host.Source.AbsoluteUri;
				int count = proxyUrl.Split(new string[] { "../" }, StringSplitOptions.None).Length;
				for (int i = 0; i < count; i++)
				{
					uri = uri.Substring(0, uri.LastIndexOf("/"));
				}
				if (!uri.EndsWith("/"))
					uri += "/";
				proxyUrl = uri + proxyUrl.Replace("~", "").Replace("../", "");
			}
			else if (ProxyUrl.StartsWith("/")) //relative to domain root
			{
				proxyUrl = ProxyUrl.Replace("/", string.Format("{0}://{1}:{2}",
					Application.Current.Host.Source.Scheme,
					Application.Current.Host.Source.Host,
					Application.Current.Host.Source.Port));
			}
			UriBuilder b = new UriBuilder(proxyUrl);
			b.Query = url;
			return b.Uri;
		}
		private string httpGetResource;
		
		public class LayerInfo
		{
			public string Name { get; set; }
			public string Title { get; set; }
			public string Abstract { get; set; }
			public Envelope Extent { get; set; }
			public IList<LayerInfo> ChildLayers { get; set; }
		}

		internal void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			if (!CheckForError(e))
			{
				// Process capabilities file
				XDocument xDoc = XDocument.Parse(e.Result);
				ParseCapabilities(xDoc);
			}

			// Call initialize regardless of error
			base.Initialize();
		}

		private void ParseCapabilities(XDocument xDoc)
		{
			//Get service info
			var info = (from Service in xDoc.Descendants("Service")
					   select new
					   {
						   Title = Service.Element("Title") == null ? null : Service.Element("Title").Value,
						   Abstract = Service.Element("Abstract") == null ? null : Service.Element("Abstract").Value
					   }).First();
			if (info != null)
			{
				this.Title = info.Title;
				//OnPropertyChanged("Title");
				this.Abstract = info.Abstract;
				//OnPropertyChanged("Abstract");
			}
			//Get a list of layers
			var layerList =  from Layers in xDoc.Descendants("Layer")
						 where Layers.Descendants("Layer").Count() == 0
						 select new LayerInfo()
						 {
							 Name = Layers.Element("Name") == null ? null : Layers.Element("Name").Value,
							 Title = Layers.Element("Title") == null ? null : Layers.Element("Title").Value,
							 Abstract = Layers.Element("Abstract") == null ? null : Layers.Element("Abstract").Value
						 };
			foreach (LayerInfo i in layerList)
				this.LayerList.Add(i);
			//OnPropertyChanged("LayerList");

			try 
			{
				//Get endpoint for GetMap requests
				//I wish SL supported XPath...
				var capabilities = (from c in xDoc.Descendants("Capability") select c).First();
				var request = (from c in capabilities.Descendants("Request") select c).First();
				var GetMap = (from c in request.Descendants("GetMap") select c).First();
				var DCPType = (from c in request.Descendants("DCPType") select c).First();
				var HTTP = (from c in request.Descendants("HTTP") select c).First();
				var Get = (from c in request.Descendants("Get") select c).First();
				var OnlineResource = (from c in request.Descendants("OnlineResource") select c).First();
				var href = OnlineResource.Attribute(XName.Get("href", "http://www.w3.org/1999/xlink"));
				httpGetResource = href.Value;
			}
			catch
			{   //Default to WMS url
				httpGetResource = this.Url;
			}
			//Get the full extent of all layers
			var box = (from Layers in xDoc.Descendants("LatLonBoundingBox")
					   	 select new
						 {
							 minx = double.Parse(Layers.Attribute("minx").Value),
							 miny = double.Parse(Layers.Attribute("miny").Value),
							 maxx = double.Parse(Layers.Attribute("maxx").Value),
							 maxy = double.Parse(Layers.Attribute("maxy").Value)
						 }).ToList();
			var minx = (from minmax in box select minmax.minx).Min();
			var miny = (from minmax in box select minmax.miny).Min();
			var maxx = (from minmax in box select minmax.maxx).Max();
			var maxy = (from minmax in box select minmax.maxy).Max();
			this.FullExtent = new Envelope(minx, miny, maxx, maxy) { SpatialReference = new SpatialReference(4326) };
		}

		private bool CheckForError(DownloadStringCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				InitializationFailure = new Exception("Request Cancelled");
				return true;
			}
			if (e.Error != null)
			{
				Exception ex = e.Error;
				if (ex is System.Security.SecurityException)
				{
					ex = new System.Security.SecurityException(
						@"A security exception occured while trying to connect to the WMS service. 
                        Make sure you have a cross domain policy file available at the root for your server that allows for requests from this application.  
                        If not, use a proxy page (handler) to broker communication.",
						ex);
				}
				InitializationFailure = ex;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Gets the URL. Override from DynamicMapServiceLayer
		/// </summary>
		/// <param name="extent">The extent.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="onComplete">OnUrlComplete delegate.</param>
		/// <remarks>
		/// The Map has a private method loadLayerInView which calls Layer.Draw.   
		/// The DynamicMapServiceLayer abstract class overrides the Draw method and calls 
		/// DynamicMapServiceLayer.GetUrl which must be implemented in a subclass.   
		/// The last parameter is the OnUrlComplete delegate, which is used to pass the appropriate values 
		/// (url, width, height, envelope) to the private DynamicMapServiceLayer.getUrlComplete method.
		/// </remarks>
		public  void GetUrl(ESRI.ArcGIS.Client.Geometry.Envelope extent, int width, int height,
			DynamicMapServiceLayer.OnUrlComplete onComplete)
		{
			int extentWKID = extent.SpatialReference.WKID;
			string baseUrl = httpGetResource ?? Url;
			StringBuilder mapURL = new StringBuilder(baseUrl);

			if (!baseUrl.Contains("?"))
				mapURL.Append("?");
			else if (!baseUrl.EndsWith("&"))
				mapURL.Append("&");
			mapURL.Append("SERVICE=WMS");
			mapURL.Append("&REQUEST=GetMap");
			mapURL.AppendFormat("&WIDTH={0}", width);
			mapURL.AppendFormat("&HEIGHT={0}", height);
			mapURL.AppendFormat("&FORMAT={0}", "image/png");
			mapURL.AppendFormat("&LAYERS={0}", String.Join(",", Layers));
			mapURL.Append("&STYLE=");
			mapURL.AppendFormat("&BGCOLOR={0}", "0xFFFFFF");
			mapURL.AppendFormat("&TRANSPARENT={0}", "true");

			mapURL.AppendFormat("&VERSION={0}", Version);

			switch (Version)
			{
				case ("1.1.1"):
					mapURL.AppendFormat("&SRS=EPSG:{0}", extentWKID);
					mapURL.AppendFormat("&bbox={0},{1},{2},{3}", extent.XMin, extent.YMin, extent.XMax, extent.YMax);
					break;
				case ("1.3"):
				case ("1.3.0"):
					mapURL.AppendFormat("&CRS=EPSG:{0}", extentWKID);

					bool useLatLong = false;
					int length = LatLongCRSRanges.Length / 2;
					for (int count = 0; count < length; count++)
					{
						if (extentWKID >= LatLongCRSRanges[count, 0] && extentWKID <= LatLongCRSRanges[count, 1])
						{
							useLatLong = true;
							break;
						}
					}

					if (useLatLong)
						mapURL.AppendFormat(CultureInfo.InvariantCulture,
							"&BBOX={0},{1},{2},{3}", extent.YMin, extent.XMin, extent.YMax, extent.XMax);
					else
						mapURL.AppendFormat(CultureInfo.InvariantCulture, 
							"&BBOX={0},{1},{2},{3}", extent.XMin, extent.YMin, extent.XMax, extent.YMax);
					break;
			}

            onComplete(PrefixProxy(mapURL.ToString()).AbsoluteUri, new ImageResult(new ESRI.ArcGIS.Client.Geometry.Envelope()
            {
                XMin = extent.XMin,
                YMin = extent.YMin,
                XMax = extent.XMax,
                YMax = extent.YMax
            }));
		}

		internal class BoundingExtent
		{
			double _xmin = double.MaxValue;
			double _ymin = double.MaxValue;
			double _xmax = double.MinValue;
			double _ymax = double.MinValue;

			public double XMin
			{
				get { return _xmin; }
				set { if (value < _xmin) _xmin = value; }
			}

			public double YMin
			{
				get { return _ymin; }
				set { if (value < _ymin) _ymin = value; }
			}
			public double XMax
			{
				get { return _xmax; }
				set { if (value > _xmax) _xmax = value; }
			}

			public double YMax
			{
				get { return _ymax; }
				set { if (value > _ymax) _ymax = value; }
			}
		}

		/// <summary>
		/// String To String Array Converter
		/// </summary>
		public sealed class StringToStringArrayConverter : System.ComponentModel.TypeConverter
		{
			public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
			{
				return (sourceType == typeof(string));
			}

			public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				if (value == null)
					return null;
				if (value is string)
				{
					string[] values = (value as string).Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					if (values.Length == 0)
						return null;
					return values;
				}
				throw new NotSupportedException("Cannot convert to string array");
			}
		}

        public override void GetUrl(DynamicLayer.ImageParameters properties, DynamicMapServiceLayer.OnUrlComplete onComplete)
        {
            throw new NotImplementedException();
        }
    }
}
