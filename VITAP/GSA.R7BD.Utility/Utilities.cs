using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using GSA.R7BD.Utility;
using GSAImageNow;  
 

// Change history:
// 8/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility {
    public class Utilities {
        //This routine will format a string for inclusion in a SQL statement
        public static string SqlEncode(string Parameter) {
            //Null strings are just empty strings as for as a SQL statement is concerned
            if (Parameter == null) {
                Parameter = "";
            }
            return Parameter.Trim().Replace("'", "''");
        }

        //		//This routine will build an Oracle connection string from the shared database password file
        public static string GetConnectionString(string databaseSetting, string userSetting) {
            string databaseName = "";
            string userID = "";
            string password = "";
            string connectionString;
            databaseName = ConfigurationManager.AppSettings.Get(databaseSetting);
            userID = ConfigurationManager.AppSettings.Get(userSetting);    //The user id is stored in the configuration file
            password = SharedPassword.GetPassword(databaseName, userID); //Retreive the password from the shared password database
            //this connectionstring is only for OracleClient style
            connectionString = "Data Source=" + databaseName + ";User ID=" + userID + ";Password=" + password;
            //connectionString="Provider=OraOLEDB.Oracle.1;Password="+ password+";"+"Persist Security Info=True;User ID="+userID+";"+"Data Source="+databaseName+";Extended Properties=";  //Create the connection string dynamically
            return connectionString.ToString();
        } //end of this method

        //		//This routine will return a unique file name for the given extension in the folder passed
        public static string GetNewFileName(string Path, string FileExt) {
            string RandomNo;
            string NewFileName = "OK Man";
            string NewExcelName;
            if (!FileExt.StartsWith(".")) {
                FileExt = "." + FileExt;
            }
            if (!Path.EndsWith("\\")) {
                Path = Path + "\\";
            }
            try {
                // Initialize random-number generator.
                Random RandomNumber = new Random();
                RandomNo = RandomNumber.Next(1, 9999).ToString(); // Generate random value between 1 and 9999.
                NewExcelName = RandomNo.Trim();  //
                RandomNo = RandomNumber.Next(1, 999).ToString(); // Generate random value between 1 and 999.
                NewFileName = "T" + NewExcelName + RandomNo.Trim() + FileExt;

                if (System.IO.File.Exists(Path + NewFileName)) {  //if this file exists create with a different name
                    RandomNo = RandomNumber.Next(1, 9999).ToString(); // Generate random value between 1 and 9999.
                    NewExcelName = RandomNo.Trim();  //
                    RandomNo = RandomNumber.Next(1, 999).ToString(); // Generate random value between 1 and 999.
                    NewFileName = "I" + NewExcelName + RandomNo.Trim() + FileExt;
                }
            }
            catch (System.Exception exp) 
            {
                Console.Write(exp.Message);
                EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "GetNewFileName", exp.Message);

            }
            return NewFileName;
        } //end of this method  


        /// <summary>
        /// This method return the drive path
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public static string getDrivePath(string driveLetter, string configFile)
        {
            string drivepath = "";
            try
            {
                if (!driveLetter.Trim().EndsWith(":")) { driveLetter = driveLetter.Trim() + ":"; }
                //string FilePath = System.Environment.GetEnvironmentVariable("FINANCECONFIG");  //Read from Env variable
                //if (FilePath == null) { FilePath = "C:\\Finance\\"; }

                string FilePath = ConfigurationManager.AppSettings["FINANCECONFIG"];
                if (FilePath == null) { FilePath = "D:\\financeconfig\\"; }
                if (!FilePath.EndsWith("\\")) { FilePath += "\\"; }
                FilePath += configFile.Trim();
                DSPATH myDataSet = new DSPATH();
                myDataSet.ReadXml(FilePath.Trim());
                if (myDataSet.NetworkMappings.Rows.Count > 0)
                {
                    DataRow[] rowsFound = myDataSet.NetworkMappings.Select("DriveLetter ='" + driveLetter.Trim().ToUpper() + "' OR DriveLetter ='" + driveLetter.Trim().ToLower() + "'");
                    if (rowsFound.Length > 0)
                    {
                        drivepath = rowsFound[0]["DrivePath"].ToString().Trim();
                    }
                    else 
                    { 
                        drivepath = "ERROR: Path not found for '" + driveLetter + "'";
                        EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "getDrivePath", drivepath);
                    }
                }
            }
            catch (Exception exp) 
            { 
                drivepath = "ERROR:" + exp.Message;
                EventLog.AddWebErrors("GGSA.R7BD.Utility", "Utilities", "getDrivePath", exp.Message);
            }
            return drivepath;

        }//end of method

        /// <summary>
        /// This method return the drive path
        /// </summary>
        /// <param name="driveLetter"></param>       
        /// <returns></returns>
        public static string getDrivePathByLetter(string driveLetter)
        {
            string drivepath = "";
            try
            {
                if (!driveLetter.Trim().EndsWith(":")) { driveLetter = driveLetter.Trim() + ":"; }
                //string configFilePath = System.Environment.GetEnvironmentVariable("FINANCECONFIG");  //Read from Env variable
                //if (configFilePath == null) { configFilePath = "C:\\Finance\\"; }

                string configFilePath = ConfigurationManager.AppSettings["FINANCECONFIG"].ToString();
                if (configFilePath == null) { configFilePath = "D:\\financeconfig\\"; }
                if (!configFilePath.EndsWith("\\")) { configFilePath += "\\"; }
                configFilePath = configFilePath.Trim() + "DriveMappings.xml";
                DSPATH myDataSet = new DSPATH();
                myDataSet.ReadXml(configFilePath.Trim());
                if (myDataSet.NetworkMappings.Rows.Count > 0)
                {
                    DataRow[] rowsFound = myDataSet.NetworkMappings.Select("DriveLetter ='" + driveLetter.Trim().ToUpper() + "' OR DriveLetter ='" + driveLetter.Trim().ToLower() + "'");
                    if (rowsFound.Length > 0)
                    {
                        drivepath = rowsFound[0]["DrivePath"].ToString().Trim();
                    }
                    else
                    {
                        drivepath = "ERROR: Path not found for '" + driveLetter + "'";
                        EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "getDrivePathByLetter", drivepath);
                    }
                }
            }
            catch (Exception exp)
            {
                drivepath = "ERROR:" + exp.Message;
                EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "getDrivePathByLetter", exp.Message);
            }
            return drivepath;

        }//end of method 

        public static string getDBConfigValue(string driveLetter, string configFile)
        {
            string drivepath = "";
            try
            {
                if (!driveLetter.Trim().EndsWith(":")) { driveLetter = driveLetter.Trim() + ":"; }
                //string FilePath = System.Environment.GetEnvironmentVariable("FINANCECONFIG");  //Read from Env variable
                //if (FilePath == null) { FilePath = "C:\\Finance\\"; }

                string FilePath = ConfigurationManager.AppSettings["FINANCECONFIG"];
                if (FilePath == null) { FilePath = "D:\\financeconfig\\"; }
                if (!FilePath.EndsWith("\\")) { FilePath += "\\"; }
                FilePath += configFile.Trim();
                DSPATH myDataSet = new DSPATH();
                myDataSet.ReadXml(FilePath.Trim());
                if (myDataSet.NetworkMappings.Rows.Count > 0)
                {
                    DataRow[] rowsFound = myDataSet.NetworkMappings.Select("DriveLetter ='" + driveLetter.Trim().ToUpper() + "' OR DriveLetter ='" + driveLetter.Trim().ToLower() + "'");
                    if (rowsFound.Length > 0)
                    {
                        drivepath = rowsFound[0]["DrivePath"].ToString().Trim();
                    }
                    else
                    {
                        drivepath = "ERROR: Path not found for '" + driveLetter + "'";
                        EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "getDBConfigValue", drivepath);
                    }
                }
            }
            catch (Exception exp)
            {
                drivepath = "ERROR:" + exp.Message;
                EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "getDBConfigValue", exp.Message);
            }
            return drivepath;

        }//end of method 

        public static string getDBConfigValueByLetter(string driveLetter)
        {
            string drivepath = "";
            try
            {
                if (!driveLetter.Trim().EndsWith(":")) { driveLetter = driveLetter.Trim() + ":"; }
                //string configFilePath = System.Environment.GetEnvironmentVariable("FINANCECONFIG");  //Read from Env variable
                //if (configFilePath == null) { configFilePath = "C:\\Finance\\"; }

                string configFilePath = ConfigurationManager.AppSettings["FINANCECONFIG"];
                if (configFilePath == null) { configFilePath = "D:\\financeconfig\\"; }
                if (!configFilePath.EndsWith("\\")) { configFilePath += "\\"; }
                configFilePath = configFilePath.Trim() + "Databases.xml";
                DSPATH myDataSet = new DSPATH();
                myDataSet.ReadXml(configFilePath.Trim());
                if (myDataSet.NetworkMappings.Rows.Count > 0)
                {
                    DataRow[] rowsFound = myDataSet.NetworkMappings.Select("DriveLetter ='" + driveLetter.Trim().ToUpper() + "' OR DriveLetter ='" + driveLetter.Trim().ToLower() + "'");
                    if (rowsFound.Length > 0)
                    {
                        drivepath = rowsFound[0]["DrivePath"].ToString().Trim();
                    }
                    else
                    {
                        drivepath = "ERROR: Path not found for '" + driveLetter + "'";
                        EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "getDBConfigValueByLetter", drivepath);
                    }
                }
            }
            catch (Exception exp)
            {
                drivepath = "ERROR:" + exp.Message;
                EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "getDBConfigValueByLetter", exp.Message);
            }
            return drivepath;

        }//end of method

        public static string SetImagePath(string imageId)
        {
            string strImageIPath = string.Empty;
            string strImageVPath = string.Empty;
            string uncPath = string.Empty;
            try
            {
                strImageIPath = getDrivePathByLetter("IMAGEIDRIVEMAPPINGDIR:");
                strImageVPath = getDrivePathByLetter("IMAGEVDRIVEMAPPINGDIR:");
                if (imageId.LastIndexOf(":") > 0)
                {
                    string firstChar = imageId.Substring(0, 1).ToUpper();
                    switch (firstChar)
                    {
                        case "I":
                            imageId = strImageIPath + imageId.Substring(imageId.LastIndexOf(":\\") + 2).Trim();
                            break;
                        case "V":
                            imageId = strImageVPath + imageId.Substring(imageId.LastIndexOf(":\\") + 2).Trim();
                            break;
                    }
                }
                if (imageId.Substring(0, 2) == "\\\\" && !imageId.Substring(0, 12).Equals("\\\\e07bds-san", StringComparison.CurrentCultureIgnoreCase))
                {
                    //code for IP
                    if (imageId.IndexOf("\\images", StringComparison.CurrentCultureIgnoreCase) > 0)
                    {
                        imageId = imageId.ToLower().Replace(imageId.ToLower().Substring(0, imageId.ToLower().IndexOf("\\images") + 7), strImageIPath);
                    }
                    if(imageId.IndexOf("\\vitap", StringComparison.CurrentCultureIgnoreCase) > 0)
                    {
                        imageId = imageId.ToLower().Replace(imageId.ToLower().Substring(0, imageId.ToLower().IndexOf("\\vitap") + 6), strImageVPath);
                    }
                }
                imageId = imageId.ToLower().Replace("\\\\e07bds-san\\images\\", strImageIPath.ToLower());
                imageId = imageId.ToLower().Replace("\\\\e07bds-san.r7bc.int\\images\\", strImageIPath.ToLower());

                imageId = imageId.ToLower().Replace("\\\\e07bds-san\\vitap\\", strImageVPath.ToLower());
                imageId = imageId.ToLower().Replace("\\\\e07bds-san.r7bc.int\\vitap\\", strImageVPath.ToLower());

                return imageId;
            }
            catch (Exception exp)
            {
                EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "SetImagePath", exp.Message.ToString());
                return imageId;
            }            
        }

        public static string CopyAndReturnTempImg(string imageId)
        {
            string strImageFile = string.Empty;
            string fileExtension = string.Empty;            
            strImageFile = imageId.Trim();
            fileExtension = System.IO.Path.GetExtension(strImageFile).Substring(1);
            string strNewFile = GetNewFileName(getDrivePathByLetter("FILELOG:"), fileExtension);
            if (!string.IsNullOrEmpty(strImageFile))
            {
                try
                {
                    System.IO.File.Copy(strImageFile, getDrivePathByLetter("FILELOG:") + strNewFile, true);
                    return strNewFile;
                }
                catch (Exception exp)
                {
                    EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "CopyAndReturnImgPath", exp.Message.ToString());
                    return strNewFile;
                }
            }
            else
            {
                return string.Empty;
            }            
        }

        //public static string GetWebNowUrl()
        //{
        //    string hostUrl = string.Empty;
        //    string username = string.Empty;
        //    string password = string.Empty;
        //    string database = string.Empty;

        //    try
        //    {
        //        hostUrl = getDrivePathByLetter("GSAWEBNOW:");
        //        database = getDBConfigValueByLetter("IMAGENOWDB:");
        //        username = getDBConfigValueByLetter("IMAGENOWUSER:");
        //        password = SharedPassword.GetPassword(database, username);
        //        if (!string.IsNullOrEmpty(hostUrl) && !string.IsNullOrEmpty(database) && !string.IsNullOrEmpty(password))
        //        {
        //            hostUrl += "&username=" + username + "&password=" + password;
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        GSA.R7BD.Utility.EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "GetWebNowUrl", exp.Message.ToString());
        //    }
        //    return hostUrl;
        //}
        
        public static Dictionary<string, string> ImageNowServiceHelper(string dbName, string dbUser)
        {
            Dictionary<string, string> imageNowServiceHelper = new Dictionary<string, string>();
            string password = string.Empty;
            string database = string.Empty;
            string userName = string.Empty;            
            try
            {
                database = getDBConfigValueByLetter(dbName);
                userName = getDBConfigValueByLetter(dbUser);
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(database))
                {
                    password = SharedPassword.GetPassword(database, userName);                    
                }
                imageNowServiceHelper.Add(userName, password);
            }
            catch (Exception exp)
            {
                GSA.R7BD.Utility.EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "ImageNowServiceHelper", exp.Message.ToString());
            }
            return imageNowServiceHelper; 
        }

        public static string GetImageNowPath(string docId)
        {
            INCopyFile inCopyFile = new INCopyFile(); 
            string fileName = string.Empty;
            try
            {
                if(string.IsNullOrEmpty(docId))
                {
                    return "Image id is empty";
                }
                else
                {
                    fileName = inCopyFile.INDocCopy(docId);
                }                
            }
            catch (Exception exp)
            {
                GSA.R7BD.Utility.EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "GetImageNowPath", exp.Message.ToString());
            }
            return fileName;
        }

        public static string GetUrl(string driveLetter, string appName)
        {
            string drivepath = "";
            try
            {
                if (!driveLetter.Trim().EndsWith(":")) { driveLetter = driveLetter.Trim() + ":"; }


                string configFilePath = ConfigurationManager.AppSettings["FINANCECONFIG"].ToString();
                if (configFilePath == null) { configFilePath = "D:\\financeconfig\\"; }
                if (!configFilePath.EndsWith("\\")) { configFilePath += "\\"; }
                configFilePath = configFilePath.Trim() + "urls.xml";

                DSPATH myDataSet = new DSPATH();
                myDataSet.ReadXml(configFilePath.Trim());
                if (myDataSet.NetworkMappings.Rows.Count > 0)
                {
                    DataRow[] rowsFound = myDataSet.NetworkMappings.Select("DriveLetter ='" + driveLetter.Trim().ToUpper() + "' OR DriveLetter ='" + driveLetter.Trim().ToLower() + "'");
                    if (rowsFound.Length > 0)
                    {
                        drivepath = rowsFound[0]["DrivePath"].ToString().Trim();
                    }
                    else
                    {
                        EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "GetUrl", " AppName:" + appName + "  DriveLetter:" + driveLetter);
                    }
                }
            }
            catch (Exception exp)
            {
                EventLog.AddWebErrors("GSA.R7BD.Utility", "Utilities", "GetUrl", exp.Message + " AppName:" + appName + "  DriveLetter:" + driveLetter);
            }
            return drivepath;
        }
    }
}
