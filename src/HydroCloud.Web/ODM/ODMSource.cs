//
// The Visual HEIFLOW License
//
// Copyright (c) 2015-2018 Yong Tian, SUSTech, Shenzhen, China. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// Note:  The software also contains contributed files, which may have their own 
// copyright notices. If not, the GNU General Public License holds for them, too, 
// but so that the author(s) of the file have the Copyright.
//

using HdyroCloud.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml.Serialization;

namespace Heiflow.Core.Data.ODM
{
    [Serializable]
    public class ODMSource 
    {

        public ODMSource()
        {
     
        }

        [XmlElement]
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// relative file path
        /// </summary>
        [XmlElement]
        public string DatabaseFilePath
        {
            get;
            set;
        }

        [XmlIgnore]
        public IDBase ODMDB { get; private set; }


        public bool Open(string dbpath, ref string msg)
        {
            try
            {
                DBaseFactory factory = new DBaseFactory();
                DBConnectInfo info = new DBConnectInfo()
                {
                    DataSource = dbpath
                };
                ODMDB = factory.CreateInitialDbClass(DBkind.Access2013, DbOptKind.Oledb, info);
                if (ODMDB.ConnectionState == ConnectionState.Closed)
                    ODMDB.DbConnection.Open();
                DatabaseFilePath = dbpath;
                Name = Path.GetFileNameWithoutExtension(dbpath);
                msg = "successful";
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public void Close()
        {
            if (ODMDB != null && ODMDB.DbConnection != null)
                ODMDB.DbConnection.Close();
        }

        [OperationContract]
        public string[] GetKeyWords()
        {
            var vars = GetVariablesFromSeriesCatalog();
            if (vars != null)
            {
                var buf = from vv in vars select vv.Name;
                return buf.ToArray();
            }
            else
            {
                return null;
            }
        }

        public DataTable Execute(string sql)
        {
            DataTable dt = ODMDB.QueryDataTable(sql);
            return dt;
        }
        [OperationContract]
        public Variable[] GetVariablesFromSeriesCatalog()
        {
            string sql = "SELECT distinct variableid FROM SeriesCatalog";
            var dt = ODMDB.QueryDataTable(sql);
            if (dt != null)
            {
                int nvar = dt.Rows.Count;
                var vars = new Variable[nvar];
                for (int i = 0; i < nvar; i++)
                {
                    vars[i] = GetVariable(int.Parse(dt.Rows[i][0].ToString()));
                }
                return vars;
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
            var dt = ODMDB.QueryDataTable(sql);
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
    }
}
