using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using HydroCloud.Web.Properties;


namespace HdyroCloud.Web.RS
{
    public static class RSConfig
    {
        public static string GridFile { get; private set; }
        public static int NVar { get; private set; }
        public static string[] VariableNames { get; private set; }
        public static string[] VariableFiles { get; private set; }
        public static DateTime Start { get; private set; }
        public static DateTime End { get; private set; }
        public static double[][][] DataSource { get; private set; }
        public static int  NCell { get; private set; }
        public static int NTime { get; private set; }

        public static void Load()
        {
            StreamReader sr = new StreamReader(Settings.Default.RSConfigFile);
            var line = sr.ReadLine();
            GridFile = sr.ReadLine().Trim();
            sr.ReadLine();
            line = sr.ReadLine();
            NVar = int.Parse(line.Trim());
            sr.ReadLine();
            VariableNames = new string[NVar];
            VariableFiles = new string[NVar];
            for (int i = 0; i < NVar; i++)
            {
                VariableNames[i] = sr.ReadLine().Trim();
            }
            sr.ReadLine();
            for (int i = 0; i < NVar; i++)
            {
                VariableFiles[i] = sr.ReadLine().Trim();
            }     
            sr.ReadLine();
            line = sr.ReadLine();
            Start = DateTime.Parse(line.Trim());
            sr.ReadLine();
            line = sr.ReadLine();
            End = DateTime.Parse(line.Trim());
            sr.Close();

            LoadDataCube();
        }

        private static void LoadDataCube()
        {
            int feaNum = 0;
            int varnum = 0;
            int nstep = (End - Start).Days + 1;
            NTime = nstep;
            DataSource = new double[NVar][][];
            for (int i = 0; i < NVar; i++)
            {
                DataSource[i] = new double[nstep][];

                FileStream fs = new FileStream(VariableFiles[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(fs);
                varnum = br.ReadInt32();
                int varname_len = br.ReadInt32();
                var varname = new string(br.ReadChars(varname_len)).Trim();
                feaNum = br.ReadInt32();
                NCell = feaNum;
                for (int j = 0; j < nstep; j++)
                {
                    DataSource[i][j] = new double[feaNum];
                }
                for (int t = 0; t < nstep; t++)
                {
                    for (int s = 0; s < feaNum; s++)
                    {
                            DataSource[i][t][s] = br.ReadSingle();
                    }
                }
                br.Close();
                fs.Close();
            }
        }
    }
}