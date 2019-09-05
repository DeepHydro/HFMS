﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace HdyroCloud.Web.ODM
{
    public static class DataParser
    {
        public static char[] DefualtSplitor = new char[] { '\t', ',' };
        public static char[] Enter = new char[] { '\n' };
        /// <summary>
        /// ',' 
        /// </summary>
        public static char[] Comma = new char[] { ',' };
        public static char[] Table = new char[] { '\t' };
        public static char[] Sharp = new char[] { '#' };
        public static char[] DoubleQuotationMark = new char[] { '"' };
        public static char[] SquareLeftBracket = new char[] { '[' };

        public static T ChangeType<T>(object obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static T[] ChangeType<T>(object[] obj)
        {
            T[] changed = new T[obj.Length];
            for (int i = 0; i < obj.Length; i++)
            {
                changed[i] = (T)Convert.ChangeType(obj[i], typeof(T));
            }
            return changed;
        }

        public static T2[] ChangeType<T1, T2>(T1[] obj)
        {
            T2[] changed = new T2[obj.Length];
            for (int i = 0; i < obj.Length; i++)
            {
                changed[i] = (T2)Convert.ChangeType(obj[i], typeof(T2));
            }
            return changed;
        }

        public static void ChangeType<T>(object[] source, T[] target)
        {
            if (target == null)
                target = new T[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                target[i] = (T)Convert.ChangeType(source[i], typeof(T));
            }
        }

        public static T[] ChangeType<T>(string[] obj, int start, int end)
        {
            T[] changed = new T[end - start + 1];
            for (int i = 0; i < changed.Length; i++)
            {
                changed[i] = (T)Convert.ChangeType(obj[i + start], typeof(T));
            }
            return changed;
        }

        public static void ChangeType<T>(string[] source, int start, int end, T[] target)
        {
            if (target == null)
                target = new T[end - start + 1];
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = (T)Convert.ChangeType(source[i + start], typeof(T));
            }
        }
        /// <summary>
        /// splitor include '\t',','
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="line"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static T[] Split<T>(string line, int num = -1)
        {
            line = line.Trim().Replace('\t', ' ');
            line = line.Trim().Replace(',', ' ');
            line = line.Split(Sharp)[0];

            string[] strs = Regex.Split(line.Trim(), @"[ ]+");
            if (num < 0)
                num = strs.Length;
            T[] values = new T[num];
            for (int i = 0; i < num; i++)
            {
                values[i] = (T)Convert.ChangeType(strs[i], typeof(T));
            }
            return values;
        }

        /// <summary>
        /// split strings
        /// </summary>
        /// <typeparam name="T">output type</typeparam>
        /// <param name="line">string</param>
        /// <param name="start">index of starting</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static T[] SkipSplit<T>(string line, int start = 0, int length = -1)
        {
            line = line.Trim().Replace('\t', ' ');
            line = line.Trim().Replace(',', ' ');
            line = line.Split(new char[] { '#' })[0];

            string[] strs = Regex.Split(line.Trim(), @"[ ]+");
            if (length < 0)
                length = strs.Length;
            T[] values = new T[length - start];
            for (int i = start; i < length; i++)
            {
                if (strs[i] != "NaN")
                    values[i - start] = (T)Convert.ChangeType(strs[i], typeof(T));
            }
            return values;
        }

        public static float[] SkipSplit(string line, int start = 0, int length = -1, float scalor = 1)
        {
            line = line.Trim().Replace('\t', ' ');
            line = line.Trim().Replace(',', ' ');
            line = line.Split(new char[] { '#' })[0];

            string[] strs = Regex.Split(line.Trim(), @"[ ]+");
            if (length < 0)
                length = strs.Length;
            float[] values = new float[length - start];
            for (int i = start; i < length; i++)
            {
                values[i - start] = float.Parse(strs[i]) * scalor;
            }
            return values;
        }

        public static T[] Split<T>(string line, char[] splitor, int num = -1)
        {
            string[] strs = line.Split(splitor);
            if (num < 0)
                num = strs.Length;
            T[] values = new T[num];
            for (int i = 0; i < num; i++)
            {
                values[i] = (T)Convert.ChangeType(strs[i], typeof(T));
            }
            return values;
        }

        public static string Vector2String<T>(T[] vector)
        {
            return string.Join<T>("\t", vector);
        }

        public static string[] ToStringVector<T>(T[] vector)
        {
            return (from v in vector select v.ToString()).ToArray();
        }

        public static bool IsNull(string str)
        {
            return (str == "" || str == null);
        }
        public static bool IsNotNull(string str)
        {
            return str != "" && str != null;
        }
        public static bool IsNumeric(System.Object Expression)
        {
            if (Expression == null || Expression is DateTime)
                return false;

            if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;

            try
            {
                if (Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                return true;
            }
            catch { } // just dismiss errors but return false
            return false;
        }

    }
}