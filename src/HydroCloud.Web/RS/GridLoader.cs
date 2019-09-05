using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using HdyroCloud.Web.ODM;

namespace HdyroCloud.Web.RS
{
    public class GridLoader
    {
        public GridLoader()
        {

        }

        public RegularGrid Load(string filename)
        {
            RegularGrid grid = new RegularGrid();
            StreamReader sr = new StreamReader(filename);
            string line = sr.ReadLine();
            grid.NCell = int.Parse(line.Trim());
            line = sr.ReadLine();
            var buf = DataParser.Split<double>(line);
            grid.CellSize = buf[0];
            grid.CentroidX = new double[grid.NCell];
            grid.CentroidY = new double[grid.NCell];
            for (int i = 0; i < grid.NCell; i++)
            {
                line = sr.ReadLine();
                buf = DataParser.Split<double>(line);
                grid.CentroidX[i] = buf[0];
                grid.CentroidY[i] = buf[1];
            }
            return grid;
        }

    }
}