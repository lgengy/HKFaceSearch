using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;

namespace HKFaceSearch.utils
{
    public class Utils
    {
        /// <summary>
        /// 读XML文件
        /// </summary>
        /// <param name="XMLNodeName"></param>
        /// <param name="XMLElementName"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="XMLFileName"></param>
        /// <returns></returns>
        public static string ReadXMLString(string XMLNodeName, string XMLElementName, string DefaultValue, string XMLFileName)
        {
            try
            {
                XmlDocument XMLData = new XmlDocument();
                XMLData.LoadXml(XMLFileName);

                XmlNode xnUser = XMLData.SelectSingleNode(XMLNodeName);
                string strValue = DefaultValue;
                if (xnUser[XMLElementName] != null)
                {
                    strValue = xnUser[XMLElementName].InnerText;
                }
                return strValue;
            }
            catch (Exception)
            {
                return DefaultValue;
            }
        }

        /// <summary>
        /// 读取配置文件，将节点值保存在类中
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="XMLNodeName">节点路径</param>
        /// <param name="elements">类list</param>
        /// <param name="XMLFileName">xml文件（名）</param>
        public static void ReadXMLString<T>(string XMLNodeName, List<T> elements, string XMLFileName)
        {
            XmlDocument XMLData = new XmlDocument();
            XMLData.LoadXml(XMLFileName);

            T t1 = Activator.CreateInstance<T>();
            Type type = t1.GetType();
            PropertyInfo[] propertyInfo = type.GetProperties();

            XmlNodeList xmlNodeList = XMLData.SelectNodes(XMLNodeName);

            if (xmlNodeList.Count > 0)
            {
                foreach (XmlNode node in xmlNodeList)
                {
                    T t2 = Activator.CreateInstance<T>();
                    foreach (PropertyInfo p in propertyInfo)
                    {
                        p.SetValue(t2, node[p.Name].InnerText, null);
                    }
                    elements.Add(t2);
                }
            }
        }

        /// <summary>
        /// 生成指定长度随机数
        /// </summary>
        /// <param name="iLength"></param>
        /// <returns></returns>
        public static string GetRandomString(int iLength)
        {
            string buffer = "0123456789";// 随机字符中也可以为汉字（任何）
            StringBuilder sb = new StringBuilder();
            Random r = new Random();
            int range = buffer.Length;
            for (int i = 0; i < iLength; i++)
            {
                sb.Append(buffer.Substring(r.Next(range), 1));
            }
            return sb.ToString();
        }

        /// <summary>
        /// image转byte数组
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ChangeImageToByteArray(Image image)
        {
            try
            {
                Bitmap bm = new Bitmap(image);
                MemoryStream ms = new MemoryStream();
                bm.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                bm.Dispose();

                return arr;
            }
            catch (Exception)
            {
                //return "Fail to change bitmap to string!";
                return null;
            }
        }
    }
}
