using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace TMXTranslatorConsole
{
    class Program
    {
        /// <summary>
        /// DataSet object that store source resx file contents.
        /// </summary>
        static DataSet DsSource = null;
        /// <summary>
        /// DataSet object that store TMX file contents.
        /// </summary>
        static DataSet DsTMX = null;
        /// <summary>
        /// Source language(culture) code.
        /// </summary>
        static string SrcLangCode = null;
        /// <summary>
        /// Source language(culture) name.
        /// </summary>
        static string SrcDispName = null;
        /// <summary>
        /// Request online translation or not.
        /// </summary>
        static bool ReqOnline = false;

        /// <summary>
        /// Main mathods.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Display welcome message.
            DisplayMessage(AppResources.MSG_WELCOME, false, false);
            try
            {
                //show current time
                Console.WriteLine("");
                DisplayMessage(GetCurrentDateTime(), true, true);
                Console.WriteLine("");

                //parameter check and display them
                if (!CheckParameter(args)) return;
                DisplayParameter(args);
                //read source resx file
                ReadSourceResxFile(args[0]);
                //read TMX file
                ImportTMXData(args[1], args[2]);
                //translation
                TranslateWithTM();
                if (ReqOnline)
                {
                    //online translation
                    TranslateOnline(args[1]);
                }
                DisplayMatchingResult();
                GenerateRESXFile(args[0], args[1]);
            }
            catch (ApplicationException appExp)
            {
            }
            catch (Exception e)
            {
            }
            finally
            {
                Log("--------------------------------------------------------------------------------------------------", 
                    false);
                //DisplayCurrentDateTime();
                DisplayMessage("", false, false);
                DisplayMessage(AppResources.MSG_PRESS_ANY_KEY, false, false);
                Console.ReadKey();
            }
            
        }

        /// <summary>
        /// Implement online translation.
        /// </summary>
        /// <param name="targetLangCode">Target language(culture) code</param>
        private static void TranslateOnline(string targetLangCode)
        {
            string[] msTransList = null;
            try
            {
                DisplayMessage(" Start online translation.....", true, false);
                string[] sourceList = GetTextForMSTranslator();
                MSTranslator trans = new MSTranslator();
                msTransList = trans.translate(sourceList, SrcLangCode, targetLangCode);
                //set the result in the DataSet object
                SetTextByOnlineTranslation(sourceList, msTransList, EnumDataGridInfo.MSTranslator.ToString());
            }
            catch (WebException webExp)
            {
                //show error message
                OutputExceptionMessage(webExp, AppResources.MSG_ERR_ONLINE_TRANS_REQ_FAIL);
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_ONLINE_TRANS_REQ_FAIL), false, false);
                throw new TranslationException(AppResources.MSG_ERR_ONLINE_TRANS_REQ_FAIL, webExp);
            }
            catch (Exception exp)
            {
                OutputExceptionMessage(exp, AppResources.MSG_ERR_DEFAULT);
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_ONLINE_TRANS), false, false);
                throw new TranslationException(AppResources.MSG_ERR_ONLINE_TRANS, exp);
            }
        }

        /// <summary>
        /// Set online translation result in DataSet object.
        /// </summary>
        /// <param name="sourceList">Array of source words</param>
        /// <param name="transList">Array of online translation result</param>
        /// <param name="transColName">The column name that store the result</param>
        private static void SetTextByOnlineTranslation(string[] sourceList, string[] transList, string transColName)
        {
            if (transList == null || transList.Length == 0)
            {
                throw new Exception(AppResources.MSG_ERR_ONLINE_TRANS_NO_RESULT);
            }
            string tableName = EnumSourceCulDataTable.data.ToString();
            string colNameTarget = EnumDataGridInfo.Target.ToString();
            string colNameValue = EnumDataGridInfo.value.ToString();
            string strType = EnumDataGridInfo.type.ToString();

            DataTable dtSource = DsSource.Tables[tableName];
            for (int i = 0; i < sourceList.Length; i++)
            {
                foreach (DataRow row in dtSource.Rows)
                {
                    //if "type" column has a value, ignore the record
                    if (!row.IsNull(strType) & !string.IsNullOrEmpty(row[strType].ToString())) continue;
                    if (row[colNameValue].ToString() == sourceList[i])
                    {
                        row[colNameTarget] = transList[i];
                        //set the value in Online Translation column
                        //row[transColName] = transList[i];
                    }
                }
            }
        }

        /// <summary>
        /// Get source texts for online translation.
        ///   Retrieve "No Match" words.
        /// </summary>
        /// <returns>Array of "No Match" words</returns>
        private static string[] GetTextForMSTranslator()
        {
            string tableName = EnumSourceCulDataTable.data.ToString();
            string colNameTarget = EnumDataGridInfo.Target.ToString();
            string colNameTuId = EnumTmxDataTable.tuid.ToString();
            string colNameValue = EnumDataGridInfo.value.ToString();
            string strType = EnumDataGridInfo.type.ToString();

            DataTable dtSource = DsSource.Tables[tableName];
            if (dtSource.Rows.Count == 0 || dtSource.Columns[colNameTarget] == null)
            {
                return null;
            }

            List<string> ret = new List<string>();
            foreach (DataRow row in dtSource.Rows)
            {
                //if "type" column has a value, ignore the record
                if (!row.IsNull(strType) & !string.IsNullOrEmpty(row[strType].ToString())) continue;
                //if tuid column does not have a value, that means the column did not match by TMX.
                if (!string.IsNullOrEmpty(row[colNameValue].ToString())
                    && (row.IsNull(colNameTuId) || string.IsNullOrEmpty(row[colNameTuId].ToString())))
                {
                    //set the value in value column into List object.
                    ret.Add(row[colNameValue].ToString());
                }
            }

            return ret.ToArray();
        }


        /// <summary>
        /// Get current date and time.
        /// </summary>
        /// <returns>Current date and time string</returns>
        private static string GetCurrentDateTime()
        {
            string date = null;
            try
            {
                date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToString("HH:mm:ss.fff");
            }
            catch (Exception e)
            {
                //show error message
                OutputExceptionMessage(e, AppResources.MSG_ERR_GET_DATETIME);
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_GET_DATETIME), false, false);
                throw e;
            }
            return date;
        }

        /// <summary>
        /// Get target resx file name.
        /// </summary>
        /// <param name="srcFileName">Source file name</param>
        /// <param name="targetLangCode">target language(culture) code</param>
        /// <returns>Target resx file name</returns>
        private static string GetTargetResxFileName(string srcFileName, string targetLangCode)
        {
            string temp = srcFileName.Substring(0, srcFileName.LastIndexOf("."));
            if (temp.LastIndexOf(".") > 0)
            {
                temp = temp.Substring(0, temp.LastIndexOf("."));
            }
            temp += "." + targetLangCode;
            return temp;
        }

        /// <summary>
        /// Display matching result.
        /// </summary>
        private static void DisplayMatchingResult()
        {
            string tableName = EnumSourceCulDataTable.data.ToString();
            string strType = EnumDataGridInfo.type.ToString();
            string colNameValue = EnumDataGridInfo.value.ToString();
            string colNameTarget = EnumDataGridInfo.Target.ToString();
            DataTable dtSource = DsSource.Tables[tableName];

            try
            {
                DisplayMessage("", true, true);
                DisplayMessage(" Mathcing Result:", true, false);
                DisplayMessage("     Source: <Source Word>\t\t<Matching Result>", true, false);

                foreach (DataRow row in dtSource.Rows)
                {
                    //if "type" column has a value, ignore the record
                    if (!row.IsNull(strType) & !string.IsNullOrEmpty(row[strType].ToString())) continue;
                    string value = AppResources.STR_NO_MATCH;
                    if (!row.IsNull(colNameTarget) && row[colNameTarget].ToString() != AppResources.STR_NO_MATCH)
                    {
                        value = row[colNameTarget].ToString();
                    }

                    DisplayMessage("     Source: " + row[colNameValue].ToString() + "\t\t" + value, true, false);
                }
            }
            catch (Exception e)
            {
                OutputExceptionMessage(e, null);
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_TRANSLATION), false, false);
                throw new ApplicationException(AppResources.MSG_ERR_TRANSLATION, e);
            }
            
        }

        /// <summary>
        /// Display specified message.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="writeLog">true: write a log using <para>message</para>.</param>
        private static void DisplayMessage(string message, bool writeLog, bool addTime)
        {
            Console.WriteLine(message);
            if (writeLog) Log(message, addTime);
        }

        /// <summary>
        /// Output log with specified message.
        /// </summary>
        /// <param name="msg">Log message</param>
        private static void Log(string msg, bool addDateTime)
        {
            StreamWriter writer = null;
            try
            {
                //file name format
                string strDate = string.Format("{0:dd-MM-yyyy}", DateTime.Now);
                string logFile = string.Format("{0}.log", strDate);

                if (!File.Exists(logFile)) writer = new StreamWriter(logFile, false, Encoding.UTF8);
                else writer = new StreamWriter(logFile, true, Encoding.UTF8);
                if (writer == null) return;
                //write log
                writer.WriteLine(msg);
            }
            catch (Exception e)
            {
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
            }
        }

        /// <summary>
        /// Generate target resx file.
        /// </summary>
        /// <param name="srcFileName">Source file name</param>
        /// <param name="targetLang">Target language(culture) code</param>
        private static void GenerateRESXFile(string srcFileName, string targetLang)
        {
            StreamWriter writer = null;
            try
            {
                DisplayMessage("", true, false);
                DisplayMessage(" Generating target resx file.....", true, false);
                string fileName = GetTargetResxFileName(srcFileName, targetLang);

                DataSet dsSaveData = DsSource.Copy();
                DataTable dt = dsSaveData.Tables[EnumSourceCulDataTable.data.ToString()];
                foreach (DataRow row in dt.Rows)
                {
                    //copy the values from Target column
                    if (row.IsNull(EnumDataGridInfo.type.ToString())
                        || string.IsNullOrEmpty(row[EnumDataGridInfo.type.ToString()].ToString()))
                    {
                        row[EnumDataGridInfo.value.ToString()] = row[EnumDataGridInfo.Target.ToString()];
                    }
                }

                //remove unnecessary column so that these will not be contained in target resx file
                DataColumnCollection colCollection = dsSaveData.Tables[EnumSourceCulDataTable.data.ToString()].Columns;
                colCollection.Remove(EnumDataGridInfo.Target.ToString());
                colCollection.Remove(EnumTmxDataTable.tuid.ToString());

                string formatString = @"//{0:s}";
                //get xml-string from each datatable
                string xmlMetaData = GetDataTableXmlString(
                    dsSaveData.Tables[EnumSourceCulDataTable.metadata.ToString()],
                    string.Format(formatString, EnumSourceCulDataTable.metadata.ToString()));
                string xmlAssembly = GetDataTableXmlString(
                    dsSaveData.Tables[EnumSourceCulDataTable.assembly.ToString()],
                    string.Format(formatString, EnumSourceCulDataTable.assembly.ToString()));
                //string xmlData = GetDataTableXmlString(
                //    dsSaveData.Tables[EnumSourceCulDataTable.data.ToString()],
                //    string.Format(formatString, EnumSourceCulDataTable.data.ToString()));
                string xmlData = GetDataTableXmlString(dsSaveData);
                string xmlResheader = GetDataTableXmlString(
                    dsSaveData.Tables[EnumSourceCulDataTable.resheader.ToString()],
                    string.Format(formatString, EnumSourceCulDataTable.resheader.ToString()));
                //shcema xml string
                string xmlSchema = dsSaveData.GetXmlSchema();

                StringBuilder sbRoot = new StringBuilder();
                //the order of the elements 
                StringBuilder sbContent = new StringBuilder(xmlSchema);
                sbContent.Append(xmlResheader);
                sbContent.Append(xmlAssembly);
                sbContent.Append(xmlMetaData);
                sbContent.Append(xmlData);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Encoding = Encoding.UTF8;
                settings.DoNotEscapeUriAttributes = true;
                settings.Indent = true;
                settings.IndentChars = AppResources.STR_TAB;

                //generate xml declaration and <root> element first, and then add the rest of elements.
                XmlWriter xmlWriter = XmlWriter.Create(sbRoot, settings);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement(EnumXmlElementName.root.ToString());
                xmlWriter.WriteRaw(sbContent.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.Close();
                xmlWriter.Flush();
                //add extension
                fileName += "." + AppResources.STR_EXT_RESX_FILE;

                writer = new StreamWriter(fileName, false, Encoding.UTF8);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(sbRoot.ToString());
                doc.Save(writer);

                DisplayMessage("", true, true);
                DisplayMessage(" File has been generated. File name: " + fileName, true, false);
            }
            catch (Exception exp)
            {
                OutputExceptionMessage(exp, null);
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_SAVE_TARGET_RESX_FILE), false, false);
                throw new ApplicationException(AppResources.MSG_ERR_SAVE_TARGET_RESX_FILE, exp);
            }
            finally
            {
                try
                {
                    if (writer != null)
                    {
                        writer.Close();
                        writer = null;
                    }
                }
                catch (Exception innerExp)
                {
                    OutputExceptionMessage(innerExp, null);
                    DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_SAVE_TARGET_RESX_FILE), false, false);
                    throw new ApplicationException(AppResources.MSG_ERR_SAVE_TARGET_RESX_FILE, innerExp);
                }
            }

        }


        /// <summary>
        /// Get xml string of "data" DataTable content.
        /// </summary>
        /// <param name="dsSaveData">DataSet that contains "data" DataTable</param>
        /// <returns>xml string of "data" DataTable content</returns>
        private static string GetDataTableXmlString(DataSet dsSaveData)
        {
            DataTable dt = dsSaveData.Tables[EnumSourceCulDataTable.data.ToString()];
            string colNameData = EnumSourceCulDataTable.data.ToString();
            string colNameName = EnumDataGridInfo.name.ToString();
            string colNameSpace = EnumDataGridInfo.space.ToString();
            string colNameValue = EnumDataGridInfo.value.ToString();
            string colNameType = EnumDataGridInfo.type.ToString();

            XmlDocument doc = new XmlDocument();
            XmlElement elmRoot = doc.CreateElement("root");

            foreach (DataRow row in dt.Rows)
            {
                XmlElement elmData = doc.CreateElement(colNameData);
                XmlElement elmValue = doc.CreateElement(colNameValue);
                XmlAttribute attrName = doc.CreateAttribute(colNameName);
                XmlAttribute attrSpace = doc.CreateAttribute(colNameSpace);
                XmlAttribute attrType = doc.CreateAttribute(colNameType);

                //set attribute/element values in each object
                if (!row.IsNull(colNameName)) attrName.Value = row[colNameName].ToString();
                else attrName.Value = string.Empty;
                if (!row.IsNull(colNameSpace)) attrSpace.Value = row[colNameSpace].ToString();
                else attrSpace.Value = string.Empty;
                if (!row.IsNull(colNameValue)) elmValue.InnerText = row[colNameValue].ToString();
                else elmValue.Value = string.Empty;
                if (!row.IsNull(colNameType)) attrType.Value = row[colNameType].ToString();
                else attrType.Value = string.Empty;

                elmData.SetAttributeNode(attrName);
                //if the "type" column has a value
                if (!row.IsNull(colNameType) && !string.IsNullOrEmpty(row[colNameType].ToString()))
                {
                    elmData.SetAttributeNode(attrType);
                }
                else
                {
                    elmData.SetAttributeNode(attrSpace);
                }

                elmData.AppendChild(elmValue);
                elmRoot.AppendChild(elmData);
                elmData = null;
                elmValue = null;
                attrName = null;
                attrSpace = null;
            }

            string ret = elmRoot.InnerXml;
            return ret;

        }



        /// <summary>
        /// Get xml string of specified DataTable contents
        /// </summary>
        /// <param name="dtTable">Target DataTable</param>
        /// <param name="strExtractElm">Filter string to retrieve particular element</param>
        /// <returns>xml string of the contents in the DataTable</returns>
        private static string GetDataTableXmlString(DataTable dtTable, string strExtractElm)
        {
            string ret = null;
            try
            {
                if (dtTable == null || dtTable.Rows.Count == 0
                    || dtTable.Columns.Count == 0 || string.IsNullOrEmpty(strExtractElm))
                {
                    return string.Empty;
                }

                DataTable dtTemp = dtTable.Copy();
                DataSet ds = new DataSet();
                ds.Tables.Add(dtTemp);
                ret = ds.GetXml();
                dtTemp.Clear();
                ds.Clear();
                dtTemp = null;
                ds = null;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ret);
                //doc.Load(ret);
                XmlNodeList elmList = doc.SelectNodes(strExtractElm);

                foreach (XmlNode node in elmList)
                {
                    ret = node.ParentNode.InnerXml;
                    break;
                }
            }
            catch (Exception exp)
            {
                OutputExceptionMessage(exp, null);
                throw exp;
            }

            return ret;
        }


        /// <summary>
        /// Translate using TMX.
        /// </summary>
        private static void TranslateWithTM()
        {
            try
            {
                //DisplayMessage("", true, false);
                DisplayMessage(" Start translation.....", true, false);
                string tableName = EnumSourceCulDataTable.data.ToString();
                string strType = EnumDataGridInfo.type.ToString();
                DataTable dtSource = DsSource.Tables[tableName];
                DataTable dtTargetTM = DsTMX.Tables[EnumTmxDataTable.TableTmx.ToString()];
                //lookup translation
                foreach (DataRow sourceRows in dtSource.Rows)
                {
                    if (!sourceRows.IsNull(strType) & !string.IsNullOrEmpty(sourceRows[strType].ToString()))
                    {
                        //this.culcPercentage(rowNum++);
                        //ignore if the current row has a value in type column
                        continue;
                    }

                    //find record using source language
                    string filter = EnumTmxDataTable.srcLang.ToString() + " = '"
                        + sourceRows[EnumDataGridInfo.value.ToString()].ToString() + "'";
                    DataRow[] retRows = dtTargetTM.Select(filter);

                    if (retRows == null || retRows.Length == 0)
                    {
                        sourceRows[EnumDataGridInfo.Target.ToString()] = AppResources.STR_NO_MATCH;
                    }
                    else
                    {
                        foreach (DataRow ret in retRows)
                        {
                            //copy the text into the DataRow
                            sourceRows[EnumDataGridInfo.Target.ToString()]
                                = ret[EnumTmxDataTable.targetLang.ToString()].ToString();
                            sourceRows[EnumTmxDataTable.tuid.ToString()]
                                = ret[EnumTmxDataTable.tuid.ToString()].ToString();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                OutputExceptionMessage(exp, null);
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_TRANSLATION), false, false);
                throw new ApplicationException(AppResources.MSG_ERR_TRANSLATION, exp);
            }
        }

        /// <summary>
        /// Display parameter entered by user on command line.
        /// </summary>
        /// <param name="args">Array of command line arguments</param>
        private static void DisplayParameter(string[] args)
        {
            DisplayMessage(" Parameters:", true, false);
            DisplayMessage("     Source resx file name: " + args[0], true, false);
            DisplayMessage("     Target language : " + args[1], true, false);
            DisplayMessage("     TMX file name: " + args[2], true, false);
            DisplayMessage("", true, false);
        }

        /// <summary>
        /// Load the contents of source resx file into DataSet object.
        /// </summary>
        /// <param name="fileName"></param>
        private static void ReadSourceResxFile(string fileName)
        {
            DisplayMessage(" Reading source resx file.....", true, false);

            try
            {
                string path = fileName;
                string sourceName = "dsSource";
                DsSource = new DataSet(sourceName);
                DsSource.ReadXml(path, XmlReadMode.ReadSchema);

                string tableName = EnumSourceCulDataTable.data.ToString();
                string colNameTuid = EnumTmxDataTable.tuid.ToString();
                string colNameTarget = EnumDataGridInfo.Target.ToString();

                DsSource.Tables[tableName].Columns.Add(colNameTuid, Type.GetType("System.String"));
                DsSource.Tables[tableName].Columns.Add(colNameTarget, Type.GetType("System.String"));

                SrcLangCode = GetSourceCulture(fileName, ref SrcDispName);
                //debug code
                //SrcLangCode = "en-US";

                DisplayMessage("", true, false);
                DisplayMessage(" Source culture information detected:", true, false);
                DisplayMessage("     Culture code: " + SrcLangCode, true, false);
                DisplayMessage("     Culture name: " + SrcDispName, true, false);
            }
            catch (Exception e)
            {
                OutputExceptionMessage(e, AppResources.MSG_ERR_SRC_RESOURCE_FILE_IMPORT);
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_SRC_RESOURCE_FILE_IMPORT), false, false);
                throw new ApplicationException(AppResources.MSG_ERR_SRC_RESOURCE_FILE_IMPORT, e);
            }
        }

        /// <summary>
        /// Import contents of TMX file.
        /// </summary>
        /// <param name="targetLangCode">target language code specified by user</param>
        /// <param name="fileName">TMX file name</param>
        private static void ImportTMXData(string targetLangCode, string fileName)
        {
            try
            {
                DisplayMessage("", true, false);
                DisplayMessage(" Importing TMX file.....", true, false);
                
                DsTMX = new DataSet(SrcLangCode);
                DsTMX.ReadXml(fileName);

                DataTable dtTU = DsTMX.Tables[EnumTmxDataTable.tu.ToString()];
                DataTable dtTUV = DsTMX.Tables[EnumTmxDataTable.tuv.ToString()];

                DataTable dtTmx = new DataTable(EnumTmxDataTable.TableTmx.ToString());
                dtTmx.Columns.Add(EnumTmxDataTable.tuid.ToString(), Type.GetType("System.String"));
                dtTmx.Columns.Add(EnumTmxDataTable.srcLang.ToString(), Type.GetType("System.String"));
                dtTmx.Columns.Add(EnumTmxDataTable.targetLang.ToString(), Type.GetType("System.String"));

                //create look-up table 
                DataRow newRow = null;

                //create table to store the contents
                DataRow[] sourceRows = dtTUV.Select(string.Format("lang = '{0:s}'", SrcLangCode));
                DataRow[] targetRows = dtTUV.Select(string.Format("lang = '{0:s}'", targetLangCode));
                string tempId = null;
                for (int i = 0; i < targetRows.Length; i++)
                {
                    tempId = targetRows[i][EnumTmxDataTable.tu_Id.ToString()].ToString();
                    DataRow[] drTuId = dtTU.Select(EnumTmxDataTable.tu_Id.ToString() + " = '" + tempId + "'");

                    newRow = dtTmx.NewRow();
                    if (drTuId != null && drTuId.Length > 0)
                    {
                        //set tuid
                        newRow[EnumTmxDataTable.tuid.ToString()] = drTuId[0][EnumTmxDataTable.tuid.ToString()];
                    }
                    newRow[EnumTmxDataTable.srcLang.ToString()]
                        = sourceRows[i][EnumTmxDataTable.seg.ToString()];
                    newRow[EnumTmxDataTable.targetLang.ToString()]
                        = targetRows[i][EnumTmxDataTable.seg.ToString()];
                    dtTmx.Rows.Add(newRow);
                }

                DsTMX.Tables.Add(dtTmx);
            }
            catch (Exception e)
            {
                OutputExceptionMessage(e, null);
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_TMX_FILE_IMPORT), false, false);
                throw new ApplicationException(AppResources.MSG_ERR_TMX_FILE_IMPORT, e);
            }
        }


        /// <summary>
        /// Get source culture.
        /// </summary>
        /// <param name="fileName">Source resx file</param>
        /// <param name="dispName">Source language(culture) name</param>
        /// <returns>Source language(culture)</returns>
        private static string GetSourceCulture(string fileName, ref string dispName)
        {
            string lang = string.Empty;
            try
            {
                //default language when file name does not contain language code
                string defaultText = GetDefaultCultureAndName();
                int firstPoint = fileName.LastIndexOf(".");
                string temp = fileName.Substring(0, firstPoint);
                int secondPoint = temp.LastIndexOf(".");
                if (secondPoint < 0)
                {
                    dispName = GetDefaultCultureAndName();
                    return GetDefaultCulture();
                }
                int codeLength = fileName.Length - (secondPoint + (fileName.Length - firstPoint));
                lang = temp.Substring(secondPoint + 1, (temp.Length - secondPoint) - 1);

                CultureInfo[] cultureList = AppResources.GetCultureList(true);
                foreach (CultureInfo info in cultureList)
                {
                    if (lang == info.Name)
                    {
                        dispName += " (" + info.DisplayName + ")";
                    }
                }
            }
            catch (Exception e)
            {
                OutputExceptionMessage(e, null);
                throw new ApplicationException(AppResources.MSG_ERR_SRC_LANG_SETTING, e);
            }
            return lang;
        }


        /// <summary>
        /// Get default culture code and name.
        /// </summary>
        /// <returns>Default culture code and name</returns>
        private static string GetDefaultCultureAndName()
        {
            return GetDefaultCulture() + " (" + Thread.CurrentThread.CurrentCulture.DisplayName + ")";
        }

        /// <summary>
        /// Get default culture code.
        /// </summary>
        /// <returns>Default culture code</returns>
        private static string GetDefaultCulture()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }


        /// <summary>
        /// Check parameter validity.
        /// </summary>
        /// <param name="args">command line parameter specified by user</param>
        /// <returns>true: All parameters are valid. false: At least one parameter is invalid.</returns>
        private static bool CheckParameter(string[] args)
        {
            bool ret = true;
            if (args.Length < 3 || args.Length > 4) ret = false;
            foreach (string str in args)
            {
                if (string.IsNullOrEmpty(str)) ret = false;
            }
            if (args.Length == 4 && args[3] == AppResources.STR_OPT_ONLINE)
            {
                ReqOnline = true;
            }
            else if (args.Length == 4 && args[3] != AppResources.STR_OPT_ONLINE)
            {
                ret = false;
            }

            if (!ret)
            {
                DisplayMessage(GetErrorMessage(AppResources.MSG_ERR_PARAM), false, false);
                Log(" Parameter error:", true);
                Log(" Number of parameters: " + args.Length, false);

                if (args.Length >= 1)
                {
                    if (string.IsNullOrEmpty(args[0]))
                    {
                        DisplayMessage("       param 1: null or empty", true, false);
                    }
                    else
                    {
                        DisplayMessage("       param 1: " + args[0], true, false);
                    }
                }
                if (args.Length >= 2)
                {
                    if (string.IsNullOrEmpty(args[1]))
                    {
                        DisplayMessage("       param 2: null or empty", true, false);
                    }
                    else
                    {
                        DisplayMessage("       param 2: " + args[1], true, false);
                    }
                }
                if (args.Length == 3 || args.Length == 4)
                {
                    if (string.IsNullOrEmpty(args[2]))
                    {
                        DisplayMessage("       param 3: null or empty", true, false);
                    }
                    else
                    {
                        DisplayMessage("       param 3: " + args[2], true, false);
                    }
                }
                if (args.Length == 4)
                {
                    if (string.IsNullOrEmpty(args[3]))
                    {
                        DisplayMessage("       param 4: null or empty", true, false);
                    }
                    else
                    {
                        DisplayMessage("       param 4: " + args[3], true, false);
                    }
                }
                DisplayMessage(string.Empty, false, false);
                DisplayMessage(AppResources.MSG_INST1, false, false);
                DisplayMessage(AppResources.MSG_INST2, false, false);
            }
            return ret;
        }

        /// <summary>
        /// Get error message with title.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <returns>Error message with title</returns>
        private static string GetErrorMessage(string message)
        {
            return "\n" + string.Format(AppResources.MSG_ERR_TITLE_TOP, message);
        }

        /// <summary>
        /// Output exception message.
        /// </summary>
        /// <param name="exp">Exception object</param>
        private static void OutputExceptionMessage(Exception exp, string message)
        {
            if (exp == null) return;

            Log("--------------------------------------------------------------------------------------------", false);
            Log("", true);
            Log(exp.Message, false);
            Log(exp.StackTrace, false);
            Log("---Inner Exception: ", false);
            if (exp.InnerException != null) OutputExceptionMessage(exp.InnerException, null);
            else Log("No Inner Exception.", false);
            Log("", false);

        }

    }
}
