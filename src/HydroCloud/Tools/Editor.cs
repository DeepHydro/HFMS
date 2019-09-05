using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcGIS.Samples
{
	/// <summary>
	/// GraphicLayer editor
	/// </summary>
	public class Editor
	{

		public enum Action
		{
			VertexAdded,
			VertexRemoved,
			VextedMoved,
			EditCompleted,
			EditStarted
		}
		public sealed class GeometryEditEventArgs : EventArgs
		{
			internal GeometryEditEventArgs(Graphic g, MapPoint newItem, MapPoint oldItem, Action action)
				: this(g, newItem == null ? null : new MapPoint[] { newItem },
					oldItem == null ? null : new MapPoint[] { oldItem }, action)
			{ }
			internal GeometryEditEventArgs(Graphic g, MapPoint[] newItems, MapPoint[] oldItems, Action action)
			{
				Graphic = g;
				NewItems = newItems;
				OldItems = oldItems;
				Action = action;
			}
			
			public Graphic Graphic { get; private set; }
			public MapPoint[] NewItems { get; private set; }
			public MapPoint[] OldItems { get; private set; }
			public Action Action { get; private set; }
		}

		#region Private fields
		/// <summary>Invisible line symbol for tragging mouse move on polygon edges and polylines</summary>
		SimpleLineSymbol hoverLine = new SimpleLineSymbol(System.Windows.Media.Colors.Transparent, 10);

		/// <summary>Vertex symbol while dragging the polygon</summary>
		MarkerSymbol draggingVertexSymbol = new SimpleMarkerSymbol() { Size = 7, Color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow) };
		/// <summary>Snap Symbol when hovering on the line segments</summary>
		MarkerSymbol snapSymbol = new SimpleMarkerSymbol()
		{
			Size = 8,
			Color = new System.Windows.Media.SolidColorBrush(
				System.Windows.Media.Color.FromArgb(200, 255, 255, 0))
		};

		private Graphic snapVertex;
		private Graphic DraggingVertex;
		private Graphic activeGraphic;
		private GraphicsLayer vertexLayer;
		private Map MyMap;
		private GraphicsLayer editLayer;
		private bool enabled;
		#endregion

		public event EventHandler<GeometryEditEventArgs> GeometryEdit;

		/// <summary>Symbol for each vertex in a polygon or polyline</summary>
		public MarkerSymbol VertexSymbol { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Editor"/> class.
		/// </summary>
		/// <param name="map">The map you're editing on.</param>
		/// <param name="layer">The layer you want to edit.</param>
		public Editor(Map map, GraphicsLayer layer)
		{
			MyMap = map;
			editLayer = layer;

			VertexSymbol = new SimpleMarkerSymbol()
			{
				Size = 7,
				Style = SimpleMarkerSymbol.SimpleMarkerStyle.Square,
				Color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
			};
		}

		/// <summary>
		/// Gets or sets a value indicating whether editing is enabled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if editing is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool IsEnabled
		{
			get { return enabled; }
			set
			{
				if (enabled != value)
				{
					enabled = value;
					if (enabled) Enable();
					else Disable();
				}
			}
		}

		private void Disable()
		{
			StopEdit();
			editLayer.MouseLeftButtonDown -= GraphicsLayer_MouseLeftButtonDown;
		}

		private void Enable()
		{
			editLayer.MouseLeftButtonDown += GraphicsLayer_MouseLeftButtonDown;
		}


		/// <summary>
		/// Called when a feature is clicked. Makes the clicked feature "active" for editing,
		/// by creating a set of vertices in an edit layer
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="graphic">The graphic.</param>
		/// <param name="args">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void GraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs args)
		{
            Graphic graphic = args.Graphic;
			if (graphic.Geometry is MapPoint)
				args.Handled = true;
			if (activeGraphic == graphic)
			{
				StopEdit();
			}
			else StartEdit(graphic);
		}

		/// <summary>
		/// Starts editing a graphic.
		/// </summary>
		/// <param name="graphic">The graphic.</param>
		public void StartEdit(ESRI.ArcGIS.Client.Graphic graphic)
		{
			StartEdit(graphic, false);
		}
		private void StartEdit(ESRI.ArcGIS.Client.Graphic graphic, bool suppressEvent)
		{
			if (activeGraphic == graphic)
			{
				return;
			}
			StopEdit();
			//Create vertex graphics layer with all the vertices of the clicked graphic
			vertexLayer = new GraphicsLayer();
			if (graphic.Geometry is MapPoint) //Points are a little special. We start tracking immediately
			{
				DraggingVertex = AddVertexToEditLayer(graphic, vertexLayer, graphic.Geometry as MapPoint);
				StartTracking();
			}
			else
			{
				if (graphic.Geometry is Polyline)
				{
					Polyline line = graphic.Geometry as Polyline;
					foreach (PointCollection ps in line.Paths)
					{
						BuildHoverLines(graphic, ps, true);
					}
				}
				else if (graphic.Geometry is Polygon)
				{
					Polygon poly = graphic.Geometry as Polygon;
					foreach (PointCollection ps in poly.Rings)
					{
						BuildHoverLines(graphic, ps, false);
					}
				}
                else if (graphic.Geometry is Envelope)
                {
                    Envelope poly = graphic.Geometry as Envelope;
                    Graphic g = AddVertexToEditLayer(graphic, vertexLayer, new MapPoint(poly.XMin, poly.YMin));
                    g.Attributes.Add("corner", 0);
                    g = AddVertexToEditLayer(graphic, vertexLayer, new MapPoint(poly.XMax, poly.YMax));
                    g.Attributes.Add("corner", 1);
                }
			}
			activeGraphic = graphic;
			MyMap.Layers.Add(vertexLayer);
			//Start listening for mouse down events
			vertexLayer.MouseLeftButtonDown += vertexLayer_MouseLeftButtonDown;
			vertexLayer.MouseLeave += vertexLayer_MouseLeave;
			vertexLayer.MouseMove += vertexLayer_MouseMove;
			if (!suppressEvent)
				OnGeometryEdit(activeGraphic, null, null, Action.EditStarted);
		}
		/// <summary>
		/// Stops editing a graphic.
		/// </summary>
		public void StopEdit()
		{
			StopEdit(false);
		}
		private void StopEdit(bool suppressEvent)
		{
			if (vertexLayer != null)
			{
				vertexLayer.ClearGraphics();
				MyMap.Layers.Remove(vertexLayer);
				vertexLayer.MouseLeftButtonDown -= GraphicsLayer_MouseLeftButtonDown;
				vertexLayer = null;
			}
			StopTracking();
			if (activeGraphic != null && activeGraphic.Geometry is Envelope) //if Envelope correct min/max
			{
				Envelope env = activeGraphic.Geometry as Envelope;
				double x1 = env.XMin;
				double x2 = env.XMax;
				double y1 = env.YMin;
				double y2 = env.YMax;
				env.XMin = Math.Min(x1, x2);
				env.XMax = Math.Max(x1, x2);
				env.YMin = Math.Min(y1, y2);
				env.YMax = Math.Max(y1, y2);
			}
			if (!suppressEvent && activeGraphic != null)
				OnGeometryEdit(activeGraphic, null, null, Action.EditCompleted);
			activeGraphic = null;
		}

		/// <summary>
		/// Adds hover lines and vertices to track mouseover on the polygon outline and line segments.
		/// </summary>
		/// <param name="ps">The ps.</param>
		/// <param name="includeLastPoint">set to <c>true</c> for lines and <c>false</c> for polygons.</param>
		private void BuildHoverLines(Graphic graphic, PointCollection ps, bool includeLastPoint)
		{
			Graphic first = null;
			for (int i = 0; i < ps.Count - 1; i++)
			{
				Graphic g = AddVertexToEditLayer(graphic, vertexLayer, ps[i]);
				if (i == 0) first = g;
				MapPoint p0 = ps[i];
				MapPoint p1 = ps[i + 1];
				AddHoverLineSegment(ps, p0, p1, graphic);
				g.Attributes.Add("PointCollection", ps);
			}
			if (ps.Count > 2 && !includeLastPoint)
			{
				MapPoint p0 = ps[0];
				MapPoint pn = ps[ps.Count - 1];
				if (p0.X == pn.X && p0.Y == pn.Y)
				{
					first.Attributes.Add("lastPnt", pn);
				}
			}
			if (includeLastPoint)
			{
				Graphic g = AddVertexToEditLayer(graphic, vertexLayer, ps[ps.Count - 1]);
				g.Attributes.Add("PointCollection", ps);
			}
		}

		private void AddHoverLineSegment(PointCollection ps, MapPoint p0, MapPoint p1, Graphic graphic)
		{
			Polyline linesegment = new Polyline();
			linesegment.Paths.Add(new PointCollection());
			linesegment.Paths[0].Add(p0);
			linesegment.Paths[0].Add(p1);
			Graphic segment = new Graphic() { Geometry = linesegment, Symbol = hoverLine };
			segment.SetZIndex(1);
			vertexLayer.Graphics.Add(segment);
			segment.Attributes.Add("PointCollection", ps);
			segment.Attributes.Add("Feature", graphic);
		}

		/// <summary>
		///Removes the snap vertex.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="graphic">The graphic.</param>
		/// <param name="args">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void vertexLayer_MouseLeave(object sender, GraphicMouseEventArgs args)
		{
			if (snapVertex != null)
			{
				vertexLayer.Graphics.Remove(snapVertex);
				snapVertex = null;
			}
		}
		/// <summary>
		/// Shows and moves the snap vertex.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="graphic">The graphic.</param>
		/// <param name="args">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void vertexLayer_MouseMove(object sender, GraphicMouseEventArgs args)
		{
            var graphic = args.Graphic;
			if (DraggingVertex != null) return;
			if (graphic.Geometry is Polyline) //We are over a hover line
			{
				Polyline line = graphic.Geometry as Polyline;
				Point pScreen = args.GetPosition(MyMap);
				MapPoint pMap = MyMap.ScreenToMap(pScreen);
				MapPoint snap = FindPointOnLineClosestToPoint(line.Paths[0][0], line.Paths[0][1], pMap);

				if (snapVertex == null)
				{
					snapVertex = new Graphic() { Symbol = snapSymbol, Geometry = snap };
					vertexLayer.Graphics.Add(snapVertex);
				}
				else
				{
					snapVertex.Geometry = snap;
				}
			}
		}

		/// <summary>
		/// Finds the point on a line closest to point (used for line snapping).
		/// </summary>
		/// <param name="p0">Start of line segment.</param>
		/// <param name="p1">End of line segment.</param>
		/// <param name="p">Point to snap.</param>
		/// <returns></returns>
		private MapPoint FindPointOnLineClosestToPoint(MapPoint p0, MapPoint p1, MapPoint p)
		{
			MapPoint p0p = new MapPoint(p.X - p0.X, p.Y - p0.Y);
			MapPoint p1p = new MapPoint(p1.X - p0.X, p1.Y - p0.Y);
			double p0p1sq = p1p.X * p1p.X + p1p.Y * p1p.Y;
			double p0p_p0p1 = p0p.X * p1p.X + p0p.Y * p1p.Y;
			double t = p0p_p0p1 / p0p1sq;
			if (t < 0.0) t = 0.0;
			else if (t > 1.0) t = 1.0;
			return new MapPoint(p0.X + p1p.X * t, p0.Y + p1p.Y * t);
		}

		private Graphic AddVertexToEditLayer(Graphic graphic, GraphicsLayer layer, MapPoint p)
		{
			Graphic g = new Graphic()
			{
				Geometry = p,
				Symbol = VertexSymbol
			};
			g.SetZIndex(2);
			layer.Graphics.Add(g);
			g.Attributes.Add("Feature", graphic);
			g.AddDoubleClick(vertex_doubleClick);
			return g;
		}


		/// <summary>
		/// Handles the Double click event extension on Graphic and deletes the vertex.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		private void vertex_doubleClick(object sender, MouseButtonEventArgs e)
		{
			Graphic g = sender as Graphic;
            Graphic parent = g.Attributes["Feature"] as Graphic;
            if (parent.Geometry is Envelope) return; //Cannot delete
            PointCollection p = g.Attributes["PointCollection"] as PointCollection;
			if (parent.Geometry is Polygon && p.Count < 5) return; //Cannot delete
			if (parent.Geometry is Polyline && p.Count < 3) return; //Cannot delete
			int index = p.IndexOf(g.Geometry as MapPoint);
			if (parent.Geometry is Polygon && index == 0) //Maintain closing point
			{
				p.RemoveAt(p.Count - 1); //remove last point
				p.Add(new MapPoint(p[1].X, p[1].Y)); //close poly
			}
			MapPoint removeItem = p[index];
			p.RemoveAt(index);
			//Restart edit to rebuild vertices and hover lines
			StopEdit(true);
			StartEdit(parent, true);
			OnGeometryEdit(g, null, removeItem, Action.VertexRemoved);
		}
		private MapPoint fromPoint;
        private void vertexLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs args)
        {
            Graphic graphic = args.Graphic;
			if (graphic.Geometry is MapPoint) //Moving point
			{
				DraggingVertex = graphic;
				args.Handled = true; //Prevent map from reacting to mouse event
				DraggingVertex.Select();
				StartTracking();
				fromPoint = new MapPoint((graphic.Geometry as MapPoint).X, (graphic.Geometry as MapPoint).Y);
			}
			else if (graphic.Geometry is Polyline) //Adding vertex
			{
				Polyline line = graphic.Geometry as Polyline;
				Point pScreen = args.GetPosition(MyMap);
				MapPoint pMap = MyMap.ScreenToMap(pScreen);
				MapPoint snap = FindPointOnLineClosestToPoint(line.Paths[0][0], line.Paths[0][1], pMap);
				args.Handled = true;
				PointCollection pnts = graphic.Attributes["PointCollection"] as PointCollection;
				Graphic parent = graphic.Attributes["Feature"] as Graphic;
				//Add new vertex and immediately start tracking it
				if (snapVertex != null)
					vertexLayer.Graphics.Remove(snapVertex);
				DraggingVertex = AddVertexToEditLayer(parent, vertexLayer, snap);
				DraggingVertex.Select();
				args.Handled = true; //Prevent map from reacting to mouse event
				StartTracking();
				DraggingVertex.Attributes.Add("PointCollection", pnts);
				int index = pnts.IndexOf(line.Paths[0][0]);
				pnts.Insert(index + 1, snap);
				vertexLayer.Graphics.Remove(graphic);
				AddHoverLineSegment(pnts, line.Paths[0][0], snap, parent);
				AddHoverLineSegment(pnts, snap, line.Paths[0][1], parent);
				OnGeometryEdit(graphic, snap, null, Action.VertexAdded);
				fromPoint = new MapPoint(snap.X, snap.Y);
			}	
		}
		/// <summary>
		/// Hooks up mouse events for moving the geometry/vertex
		/// </summary>
		private void StartTracking()
		{
			MyMap.MouseMove += Map_MouseMove;
			MyMap.MouseLeftButtonUp += Map_MouseLeftButtonUp;
			MyMap.MouseLeave += MyMap_MouseLeave;
			MyMap.KeyDown += MyMap_KeyDown;
			MyMap.Cursor = Cursors.None;
			MyMap.Focus();
		}

		private void MyMap_KeyDown(object sender, KeyEventArgs e)
		{
			//if (e.Key == Key.Delete && DraggingVertex != null && activeGraphic != null)
			//{
			//    if (activeGraphic.Geometry is MapPoint)
			//    {
			//        this.editLayer.Graphics.Remove(activeGraphic);
			//        StopTracking();
			//    }
			//    else
			//    {
			//        PointCollection coll = DraggingVertex.Attributes["PointCollection"] as PointCollection;
			//        coll.Remove(DraggingVertex.Geometry as MapPoint);
			//    }

			//    vertexLayer.Graphics.Remove(DraggingVertex);
			//    DraggingVertex = null;
			//}
		}


		/// <summary>
		/// Unhooks mouse events and stops tracking vertex dragging
		/// </summary>
		private void StopTracking()
		{
			MyMap.MouseLeave -= MyMap_MouseLeave;
			MyMap.MouseMove -= Map_MouseMove;
			MyMap.MouseLeftButtonUp -= Map_MouseLeftButtonUp;
			MyMap.KeyDown -= MyMap_KeyDown;
			MyMap.Cursor = Cursors.Arrow;
			fromPoint = null;
			if (DraggingVertex == null) return;
			//DraggingVertex.Symbol = VertexSymbol;
			DraggingVertex.UnSelect();
			DraggingVertex = null;
			if (activeGraphic != null && activeGraphic.Geometry is MapPoint)
			{
				StopEdit();
			}
		}

		/// <summary>
		/// Handles the MouseLeave event of the MyMap control (cancels vertex drag tracking).
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
		private void MyMap_MouseLeave(object sender, MouseEventArgs e)
		{
			StopTracking();
		}

		/// <summary>
		/// Handles the MouseLeftButtonUp event of the Map control (cancels vertex drag tracking).
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		private void Map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (activeGraphic != null && DraggingVertex != null &&
				fromPoint!=null && (fromPoint.X != (DraggingVertex.Geometry as MapPoint).X || 
				fromPoint.Y != (DraggingVertex.Geometry as MapPoint).Y))
				OnGeometryEdit(activeGraphic, DraggingVertex.Geometry as MapPoint, fromPoint, Action.VextedMoved);
			StopTracking();
		}

		/// <summary>
		/// Handles the MouseMove event of the Map control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
		private void Map_MouseMove(object sender, MouseEventArgs e)
		{
			if (DraggingVertex == null ||
				DraggingVertex.Geometry == null ||
				!(DraggingVertex.Geometry is MapPoint)) return;
			//Update the point geometry based on the map coordinate
			Point p = e.GetPosition(MyMap);
			ESRI.ArcGIS.Client.Geometry.MapPoint pnt = MyMap.ScreenToMap(p);
			MapPoint pEdit = DraggingVertex.Geometry as MapPoint;
			pEdit.X = pnt.X;
			pEdit.Y = pnt.Y;
			if (DraggingVertex.Attributes.ContainsKey("lastPnt")) //Polygon first vertex - also update closing vertex
			{
				pEdit = DraggingVertex.Attributes["lastPnt"] as MapPoint;
				pEdit.X = pnt.X;
				pEdit.Y = pnt.Y;
			}
            else if (DraggingVertex.Attributes.ContainsKey("corner")) //Envelope
            {
				Graphic g = DraggingVertex.Attributes["Feature"] as Graphic;
				Envelope env = g.Geometry as Envelope;
				if ((int)DraggingVertex.Attributes["corner"] == 0) //lower left
				{
					env.XMin = pnt.X;
					env.YMin = pnt.Y;

				}
				else //upper right
				{
					env.XMax = pnt.X;
					env.YMax = pnt.Y;
				}
            }
		}

		protected void OnGeometryEdit(Graphic g, MapPoint newItem, MapPoint oldItem, Action action)
		{
			if (GeometryEdit != null)
				GeometryEdit(this, new GeometryEditEventArgs(g, newItem, oldItem, action));
		}
	}

	/// <summary>
	/// Extension method for tracking double clicking on graphics
	/// </summary>
	internal static class MouseExtensions
	{
		private const int doubleClickInterval = 200;
		private static readonly DependencyProperty DoubleClickTimerProperty = DependencyProperty.RegisterAttached("DoubleClickTimer", typeof(DispatcherTimer), typeof(UIElement), null);
		private static readonly DependencyProperty DoubleClickHandlersProperty = DependencyProperty.RegisterAttached("DoubleClickHandlers", typeof(List<MouseButtonEventHandler>), typeof(UIElement), null);
		private static readonly DependencyProperty DoubleClickPositionProperty = DependencyProperty.RegisterAttached("DoubleClickPosition", typeof(Point), typeof(UIElement), null);
		/// <summary>
		/// Adds a double click event handler.
		/// </summary>
		/// <param name="element">The Element to listen for double clicks on.</param>
		/// <param name="handler">The handler.</param>
		public static void AddDoubleClick(this Graphic element, MouseButtonEventHandler handler)
		{
			element.MouseLeftButtonDown += element_MouseLeftButtonDown;
			List<MouseButtonEventHandler> handlers;
			handlers = element.GetValue(DoubleClickHandlersProperty) as List<MouseButtonEventHandler>;
			if (handlers == null)
			{
				handlers = new List<MouseButtonEventHandler>();
				element.SetValue(DoubleClickHandlersProperty, handlers);
			}
			handlers.Add(handler);
		}
		/// <summary>
		/// Removes a double click event handler.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="handler">The handler.</param>
		public static void RemoveDoubleClick(this Graphic element, MouseButtonEventHandler handler)
		{
			element.MouseLeftButtonDown -= element_MouseLeftButtonDown;
			List<MouseButtonEventHandler> handlers = element.GetValue(DoubleClickHandlersProperty) as List<MouseButtonEventHandler>;
			if (handlers != null)
			{
				handlers.Remove(handler);
				if (handlers.Count == 0)
					element.ClearValue(DoubleClickHandlersProperty);
			}
		}
		private static void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Graphic element = sender as Graphic;
			Point position = e.GetPosition(Application.Current.RootVisual);
			DispatcherTimer timer = element.GetValue(DoubleClickTimerProperty) as DispatcherTimer;
			if (timer != null) //DblClick
			{
				timer.Stop();
				Point oldPosition = (Point)element.GetValue(DoubleClickPositionProperty);
				element.ClearValue(DoubleClickTimerProperty);
				element.ClearValue(DoubleClickPositionProperty);
				if (Math.Abs(oldPosition.X - position.X) < 1 && Math.Abs(oldPosition.Y - position.Y) < 1) //mouse didn't move => Valid double click
				{
					List<MouseButtonEventHandler> handlers = element.GetValue(DoubleClickHandlersProperty) as List<MouseButtonEventHandler>;
					if (handlers != null)
					{
						foreach (MouseButtonEventHandler handler in handlers)
						{
							handler(sender, e);
						}
					}
					return;
				}
			}
			//First click or mouse moved. Start a new timer
			timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(doubleClickInterval) };
			timer.Tick += new EventHandler((s, args) =>
			{  //DblClick timed out
				(s as DispatcherTimer).Stop();
				element.ClearValue(DoubleClickTimerProperty); //clear timer
				element.ClearValue(DoubleClickPositionProperty); //clear first click position
			});
			element.SetValue(DoubleClickTimerProperty, timer);
			element.SetValue(DoubleClickPositionProperty, position);
			timer.Start();
		}
	}
}
