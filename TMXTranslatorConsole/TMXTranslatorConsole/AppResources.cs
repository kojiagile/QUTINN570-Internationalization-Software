using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXTranslatorConsole
{
    enum EnumTargetCulDataTable
    {
        TargetSource,
        index,
        sourceLang,
        targetLang,
        langName,
        DispName,
        filePath,
        sortValue,
    }

    enum EnumTmxDataTable
    {
        TableTmx,
        tu,
        tuv,
        seg,
        tuid,
        srcLang,
        targetLang,
        tu_Id,
    }

    enum EnumXmlElementName
    {
        tu,
        tuv,
        tuid,
        root,
        header,
        srclang,// this is the attribute name in a TMX file to detect source language.
    }

    /// <summary>
    /// enum for source data (culture, data grid)
    /// </summary>
    enum EnumDataGridInfo
    {
        type,
        name,
        value,
        StringID,
        Source,
        Target,
        //QS,
        MSTranslator,
        space,
    }

    enum EnumSourceCulDataTable
    {
        metadata,
        assembly,
        data,
        resheader
    }
    
    
    class AppResources
    {

        public const string MSG_WELCOME = "======================================================================\n"
                        + "\n"
                        + "                     Welcome to TMX Translator\n"
                        + "\n"
                        + "======================================================================\n";

        public const string MSG_INST1 = " 1. Place your source resx file and TMX file"
                        + " in the folder that the application is stored.\n";
        public const string MSG_INST2 = "\n"
                        + " 2. Enter source resx file names, target language code and TMX file name,\n"
                        + " and then press Enter key to start translation.\n"
                        + " If you want to use Online translation, enter \"-o\" after TMX file name.\n"
                        + " (Online transration is optional.)\n"
                        + "  Example:\n"
                        + "     app.resx de-DE English-German.tmx -o\n\n"
                        + " Target resx file will be generated in the same directory.\n";

        public const string MSG_PRESS_ANY_KEY = "Press any key to end.";
        public const string MSG_ERR_PARAM = "Invalid Parameter.\n";

        public const string MSG_ERR_TITLE_TOP = "********************************************************************\n"
                                            + " ERROR: {0}"
                                            + "********************************************************************\n";
        public const string MSG_ERR_INVALID_PARAM = " Invalid parameter.";

        public const string TESTCODE_SOURCE_LANG_CODE = "en-US";
        public const string STR_EXT_TMX_FILE = ".tmx";
        public const string STR_EXT_RESX_FILE = "resx";

        public const string STR_OPT_ONLINE = "-o";

        public const string MSG_ERR_CALL_ADMIN = "Call system administrator.";
        public const string MSG_ERR_DEFAULT = "Error has occured. ";
        public const string MSG_ERR_GET_DATETIME = "Failed to get current DateTime. ";
        public const string MSG_ERR_SRC_RESOURCE_FILE_IMPORT = "Soure resource file import error has occured.\n";
        public const string MSG_ERR_TMX_FILE_IMPORT = "TMX file import error has occured.\n";
        public const string MSG_ERR_TRANSLATION = "Translation error has occured.\n";
        public const string MSG_ERR_ONLINE_TRANS = "Online translation error has occured.\n";
        public const string MSG_ERR_ONLINE_TRANS_REQ_FAIL = "Online translation request could not reach or time out.\n";
        public const string MSG_ERR_SAVE_TARGET_RESX_FILE = "File save error has occured.\n";
        public const string MSG_ERR_SRC_LANG_SETTING = "Source culture setting error has occured.\n";
        public const string MSG_ERR_IMPORT_TMX_DATA = "TMX file import error has occured.\n";
        public const string MSG_ERR_ONLINE_TRANS_NO_RESULT = "Online Translation Error. The result is null or the length is 0";

        public const string STR_NO_MATCH = "No Match";
        public const string STR_TAB = @"\t";






        /// <summary>
        /// get cultureInfo array
        /// </summary>
        /// <param name="isAllCulture"></param>
        /// <returns></returns>
        public static CultureInfo[] GetCultureList(bool isAllCulture)
        {
            CultureInfo[] ret = null;
            //Get a list of cultures - either all or installed
            if (isAllCulture)
            {
                ret = CultureInfo.GetCultures(CultureTypes.AllCultures);
            }
            else
            {
                ret = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures);
            }

            return ret;
        }

    }
}
