
namespace ESRI.ArcGIS.Samples.Extensions
{
	/// <summary>
	/// Geometry extension methods
	/// </summary>
	public static class Geometry
	{
		public static bool Contains(this ESRI.ArcGIS.Client.Geometry.Polygon polygon, ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint)
		{
			return mapPoint.IsWithin(polygon);
		}
		/// <summary>
		/// Utility function to determine whether a map point is inside of a given polygon
		/// </summary>
		/// <param name="polygon"></param>
		/// <param name="mapPoint"></param>
		/// <returns></returns>
		public static bool IsWithin(this ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint, ESRI.ArcGIS.Client.Geometry.Polygon polygon)
		{
			foreach (ESRI.ArcGIS.Client.Geometry.PointCollection points in polygon.Rings)
			{
				int j = points.Count - 1;
				bool inPoly = false;

				for (int i = 0; i < points.Count; i++)
				{
					if (points[i].X < mapPoint.X && points[j].X >= mapPoint.X ||
						points[j].X < mapPoint.X && points[i].X >= mapPoint.X)
					{
						if (points[i].Y + (mapPoint.X - points[i].X) / (points[j].X - points[i].X) * (points[j].Y - points[i].Y) < mapPoint.Y)
						{
							inPoly = !inPoly;
						}
					}
					j = i;
				}

				if (inPoly)
					return true;
			}

			return false;
		}

	}
}
