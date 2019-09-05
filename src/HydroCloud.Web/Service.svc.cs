using HdyroCloud.Web;
using HdyroCloud.Web.RS;
using HydroCloud.Web.Properties;
using HydroCloud.Web.RS;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace HydroCloud.Web
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service
    {
        public void DoWork()
        {

        }
        [OperationContract]
        public string[] GetKeyWords()
        {
            return Global.DBSource.GetKeyWords();
        }
        [OperationContract]
        public Station GetSite(int id)
        {
            string sql = "select * from Sites where SiteID=" + id;
            var dt = Global.DBSource.Execute(sql);
            if (dt != null)
            {
                var sites = from r in dt.AsEnumerable()
                            select new Station()
                            {
                                ID = r.Field<int>("SiteID"),
                                Name = r.Field<string>("SiteName"),
                                Latitude = r.Field<double>("Latitude"),
                                Longitude = r.Field<double>("Longitude"),
                                SiteType = r.Field<string>("SiteType")
                            };
                return sites.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
        [OperationContract]
        public Station[] GetAllSites()
        {
            string sql = "select * from Sites";
            var dt = Global.DBSource.Execute(sql);
            if (dt != null)
            {
                var sites = from r in dt.AsEnumerable()
                            select new Station()
                            {
                                ID = r.Field<int>("SiteID"),
                                Name = r.Field<string>("SiteName"),
                                Latitude = r.Field<double>("Latitude"),
                                Longitude = r.Field<double>("Longitude"),
                                SiteType = r.Field<string>("SiteType")
                            };
                return sites.ToArray();
            }
            else
            {
                return null;
            }
        }
        [OperationContract]
        public Variable GetVariableByName(string varName)
        {
            string sql = string.Format("select * from Variables where variableName='{0}'", varName);
            var dt = Global.DBSource.Execute(sql);
            if (dt != null)
            {
                var dr = dt.Rows[0];
                var vb = new Variable()
                {
                    ID = (int)dr["variableID"],
                    Name = dr["VariableName"].ToString(),
                };
                return vb;
            }
            else
            {
                return null;
            }
        }
        [OperationContract]
        public Variable GetVariable(int varID)
        {
            string sql = string.Format("select * from Variables where variableID={0}", varID);
            var dt = Global.DBSource.Execute(sql);
            if (dt != null)
            {
                var dr = dt.Rows[0];
                var vb = new Variable()
                {
                    ID = (int)dr["variableID"],
                    Name = dr["VariableName"].ToString(),
                };
                return vb;
            }
            else
            {
                return null;
            }
        }
        [OperationContract]
        public Station[] GetSites(QueryCriteria qc)
        {
            var varb = GetVariableByName(qc.VariableName);
            if (varb != null)
            {
                string sql = string.Format("select * from SeriesCatalog where  VariableID={0} and (BeginDateTime <= #{1}# and EndDateTime >= #{2}#)",
                     varb.ID, qc.End.ToString("yyyy/MM/dd"), qc.Start.ToString("yyyy/MM/dd"));
                var dt = Global.DBSource.Execute(sql);
                if (dt != null)
                {
                    List<Station> result = new List<Station>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        int id = int.Parse(dr["SiteID"].ToString());
                        var site = GetSite(id);
                        if (site.Latitude <= qc.BBox.North && site.Latitude >= qc.BBox.South && site.Longitude <= qc.BBox.East
                             && site.Longitude >= qc.BBox.West)
                            result.Add(site);
                    }
                    return result.ToArray();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        [OperationContract]
        public DoubleTimeSeries GetDoubleTimeSeries(QueryCriteria qc)
        {
            if (qc.VariableID == 0)
            {
                var variable = GetVariableByName(qc.VariableName);
                if (variable != null)
                    qc.VariableID = variable.ID;
            }
            string sql = "";
            if (qc.MaximumRecord <= 0)
            {
                sql = string.Format("select DateTimeUTC, DataValue from DataValues where SiteID={0} and VariableID={1} order by DateTimeUTC",
                    qc.SiteID, qc.VariableID);
            }
            else
            {
                sql = string.Format("select TOP {0} DateTimeUTC, DataValue from DataValues where SiteID={1} and VariableID={2} and DateTimeUTC >=#{3}# and DateTimeUTC <=#{4}# order by DateTimeUTC",
                   qc.MaximumRecord, qc.SiteID, qc.VariableID, qc.Start.ToString("yyyy/MM/dd"), qc.End.ToString("yyyy/MM/dd"));
            }
            DataTable dt = Global.DBSource.Execute(sql);
            if (dt != null)
            {
                var dates = from dr in dt.AsEnumerable() select dr.Field<DateTime>("DateTimeUTC");
                var values = from dr in dt.AsEnumerable() select dr.Field<double>("DataValue");
                return new DoubleTimeSeries()
                {
                    Values = values.ToArray(),
                    DateTimes = dates.ToArray()
                };
            }
            else
            {
                return null;
            }
        }
        [OperationContract]
        public RegularGrid GetGrid()
        {
            GridLoader loader = new GridLoader();
            return loader.Load(RSConfig.GridFile);
        }
        [OperationContract]
        public double[] GetTimeRange()
        {
            return new double[] { RSConfig.Start.ToOADate(), RSConfig.End.ToOADate() };
        }
        [OperationContract]
        public double[] GetSlice(int var_index, int time_index)
        {
            return RSConfig.DataSource[var_index][time_index];
        }
        [OperationContract]
        public double[] GetPointProfile(int var_index, int cell_index)
        {
            var vec = new double[RSConfig.NTime];
            for (int i = 0; i < RSConfig.NTime; i++)
                vec[i] = RSConfig.DataSource[var_index][i][cell_index];
            return vec;
        }
        [OperationContract]
        public string DownloadImage(string filename)
        {
            FileStream fileStream = null;
            if (filename != null)
            {
                string filePath = Path.Combine(Settings.Default.BaseFilePath, filename);
                var result = string.Empty;
                byte[] filebytes = null;
                try
                {
                    if (File.Exists(filePath))
                    {
                        fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        int length = (int)fileStream.Length;
                        filebytes = new byte[length];
                        fileStream.Read(filebytes, 0, length);
                        result = Convert.ToBase64String(filebytes);
                    }
                    return result;
                }
                catch (Exception)
                {
                    return result;
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }
            }
            else
            {
                return null;
            }
        }
        [OperationContract]
        public string DownloadLegend(string filename)
        {
            FileStream fileStream = null;
            if (filename != null)
            {
                string filePath = Path.Combine(Settings.Default.BaseFilePath, filename);
                var result = string.Empty;
                byte[] filebytes = null;
                try
                {
                    if (File.Exists(filePath))
                    {
                        fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        int length = (int)fileStream.Length;
                        filebytes = new byte[length];
                        fileStream.Read(filebytes, 0, length);
                        result = Convert.ToBase64String(filebytes);
                    }
                    return result;
                }
                catch (Exception)
                {
                    return result;
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }
            }
            else
            {
                return null;
            }
        }
        [OperationContract]
        public SensorImageRecord[] GetSensorImageRecord()
        {
            ImageBaseSource source = new ImageBaseSource();
            var catafile = Settings.Default.RSImageCatalog;
            return source.GetSensorCatalog(catafile);
        }
    }
}
