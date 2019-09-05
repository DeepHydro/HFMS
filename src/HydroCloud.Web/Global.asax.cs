using Heiflow.Core.Data.ODM;
using HydroCloud.Web.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace HydroCloud.Web
{
    public class Global : System.Web.HttpApplication
    {
        public static ODMSource DBSource { get; private set; }
        protected void Application_Start(object sender, EventArgs e)
        {
            string msg = "";
            DBSource = new ODMSource();
            DBSource.Open(Settings.Default.DBPath, ref msg);
            HdyroCloud.Web.RS.RSConfig.Load();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}