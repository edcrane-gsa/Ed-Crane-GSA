using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.Data.OracleClient;  
using System.Xml;
using System.IO;
using System.Text;
//using System.Runtime.InteropServices;
using GSA.R7BD.Utility;
using System.Configuration;


// Change history:
// 8/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility {
    public partial class DataAccess : Component {
        //These enums are used in all the conectionStrings below		
        public enum OracleDriver { OraOleDB = 1, OleDB = 2 };
        //public enum DatabaseType { Live = 1, Test = 2, Reports = 3, Development = 4 };
        //public enum DatabaseEnvironment { Development = 1, Test = 2, Production = 3 };
        //public enum FinsOraType { Live = 1, Test = 2 };
        public enum User { iuser_vitap = 1, 
                            iuser_vendors = 2, 
                            r7bcquery = 3, 
                            iuser_vcadj = 4, 
                            //var = 5, 
                            iuser_hava = 6, 
                            iuser_poir = 7, 
                            iuser_pwdapp = 8, 
                            fmis_U7B0223 = 9, 
                            iuser_audit = 10, 
                            iuser_bart = 11, 
                            iuser_formdelete = 12, 
                            iuser_expaccr = 13, 
                            iuser_pmt = 14, 
                            iuser_dodaac = 15, 
                            iuser_webadmin = 16,
                            //user = 17,
                            iuser_wsa = 18,
                            iuser_leaseea = 19,
                            iuser_rwa = 20,
                            iuser_acorn = 21,
                            iuser_external = 22,
                            iuser_wvendor = 23,
                            poldr = 24,
                            var = 25};
   

        public enum FileSource { XML = 1, SAN_BOX = 2 };
        public static string filePath = "";
        internal static string OpenAppDate = "";
        internal static string CloseAppDate = "";
        internal static string CloseAppStartDate = "";
        internal static string CloseAppEndDate = "";
	
        public DataAccess() {
            InitializeComponent();
        }
        public DataAccess(IContainer container) {
            container.Add(this);

            InitializeComponent();
        }

        // This method return the Closed Message or "Open"
        public static string GetAppClosed(string appname)
        {
            CloseAppStartDate = "";
            CloseAppEndDate = "";
            string returnval = "";
            try
            {
                //string FilePath = System.Environment.GetEnvironmentVariable("FINANCECONFIG");  //Read from Env variable
                //if (FilePath == null) { FilePath = "C:\\Finance\\"; }

                string FilePath = ConfigurationManager.AppSettings["FINANCECONFIG"];

                if (FilePath == null) { FilePath = "D:\\financeconfig\\"; }
                if (!FilePath.EndsWith("\\")) { FilePath += "\\"; }
                FilePath += "appstatus.xml";
                APPSTATXML myDataSet = new APPSTATXML();
                myDataSet.ReadXml(FilePath.Trim());
                if (myDataSet.AppStatus.Rows.Count > 0)
                {
                    //DataRow[] rowsFound = myDataSet.AppStatus.Select("AppName ='NEAR FILES'");
                    DataRow[] rowsFound = myDataSet.AppStatus.Select("AppName ='" + appname.Trim().ToUpper() + "' OR AppName ='" + appname.Trim().ToLower() + "'");
                    if (rowsFound.Length > 0)
                    {
                        string strCloseMsg = rowsFound[0]["CloseMsg"].ToString().Trim();
                        string strDate = rowsFound[0]["CloseStartDate"].ToString().Trim();
                        if (strDate == "") { strDate = "12/31/9999 12:00:01 AM"; }
                        DateTime objCloseStartDate = DateTime.Parse(strDate);
                        OpenAppDate = DateTime.Parse(strDate).ToShortDateString();
                        strDate = rowsFound[0]["CloseEndDate"].ToString().Trim();
                        if (strDate == "") { strDate = "12/31/9999 12:00:01 AM"; }
                        DateTime objCloseEndDate = DateTime.Parse(strDate);
                        CloseAppDate = DateTime.Parse(strDate).ToShortDateString();
                        if (DateTime.Now >= objCloseStartDate && DateTime.Now <= objCloseEndDate)
                        {
                            returnval = strCloseMsg;
                        }
                        else { returnval = "OPEN"; }
                    }
                    else 
                    { 
                        returnval = "ERROR: Entry not found for '" + appname + "'";
                        EventLog.AddWebErrors("GGSA.R7BD.Utility", "DataAccess", "GetAppClosed", returnval);
                    }
                }
                //returnval = FilePath;
            }
            catch (Exception exp) 
            { 
                returnval = "ERROR:" + exp.Message;
                EventLog.AddWebErrors("GGSA.R7BD.Utility", "DataAccess", "GetAppClosed", returnval);
            }
            return returnval;

        }//end of method

        // This method return the Warning Message or "Open"
        public static string GetAppWarning(string appname)
        {
            string returnval = "";
            try
            {
                //string FilePath = System.Environment.GetEnvironmentVariable("FINANCECONFIG");  //Read from Env variable
                string FilePath = ConfigurationManager.AppSettings["FINANCECONFIG"].ToString();
                
                //if (FilePath == null) { FilePath = "C:\\Finance\\"; }
                if (FilePath == null) { FilePath = "D:\\financeconfig\\"; }
                if (!FilePath.EndsWith("\\")) { FilePath += "\\"; }
                FilePath += "appstatus.xml";
                APPSTATXML myDataSet = new APPSTATXML();
                myDataSet.ReadXml(FilePath.Trim());
                if (myDataSet.AppStatus.Rows.Count > 0)
                {
                    //DataRow[] rowsFound = myDataSet.AppStatus.Select("AppName ='NEAR FILES'");
                    DataRow[] rowsFound = myDataSet.AppStatus.Select("AppName ='" + appname.Trim().ToUpper() + "' OR AppName ='" + appname.Trim().ToLower() + "'");
                    if (rowsFound.Length > 0)
                    {
                        string strWarningMsg = rowsFound[0]["WarningMsg"].ToString().Trim();
                        string strDate = rowsFound[0]["WarningStartDate"].ToString().Trim();
                        if (strDate == "") { strDate = "12/31/9999 12:00:01 AM"; }
                        DateTime objWarningStartDate = DateTime.Parse(strDate);
                        strDate = rowsFound[0]["WarningEndDate"].ToString().Trim();
                        if (strDate == "") { strDate = "12/31/9999 12:00:01 AM"; }
                        DateTime objWarningEndDate = DateTime.Parse(strDate);
                        if (DateTime.Now >= objWarningStartDate && DateTime.Now <= objWarningEndDate)
                        {
                            returnval = strWarningMsg;
                        }
                        else { returnval = "OPEN"; }
                    }
                  else 
                    { 
                        returnval = "ERROR: Entry not found for '" + appname + "'";
                        EventLog.AddWebErrors("GGSA.R7BD.Utility", "DataAccess", "GetAppWarning", returnval);
                    }                }
                //returnval = FilePath;
            }
            catch (Exception exp) 
            { 
                returnval = "ERROR:" + exp.Message;
                EventLog.AddWebErrors("GGSA.R7BD.Utility", "DataAccess", "GetAppWarning", returnval);
            }
            return returnval;
        }//end of method

        //GetRole gets all the roles for a user associated with the application
        //Passes in the User Name and Application to the bdfapps.user_auth_pkg.GET_USER_ACCESS
        //Returns a Datatable of information related to user and roles
        public static DataTable GetRole(string userName, string appName)
        {
            OracleConnection conBDFAPPS = new OracleConnection();
            OracleCommand cmdBDFAPPS = new OracleCommand();
            DataTable dtRole = new DataTable();

            conBDFAPPS.ConnectionString = ConnStrBDFApps();
            cmdBDFAPPS.Connection = conBDFAPPS;
            cmdBDFAPPS.CommandText = "bdfapps.user_auth_pkg.GET_USER_ACCESS";
            cmdBDFAPPS.CommandType = CommandType.StoredProcedure;
            cmdBDFAPPS.Parameters.Clear();

            cmdBDFAPPS.Parameters.Add(new OracleParameter("in_User_Name", OracleType.VarChar, 35)).Value = userName;
            cmdBDFAPPS.Parameters.Add(new OracleParameter("in_App_Name", OracleType.VarChar, 50)).Value = appName;
            cmdBDFAPPS.Parameters.Add(new OracleParameter("out_App_Cursor", OracleType.Cursor)).Direction = ParameterDirection.ReturnValue;

            OracleDataAdapter daRole = new OracleDataAdapter(cmdBDFAPPS);

            try
            {
                conBDFAPPS.Open();
                daRole.Fill(dtRole);
            }
            catch (Exception exp)
            {
                GSA.R7BD.Utility.EventLog.AddWebErrors("Utility Dll", "DataAccess.cs", "GetRole", exp.Message);
            }
            finally
            {
                conBDFAPPS.Close();
            }
            return dtRole;
        }

        public static string getFMISMessage()
        {
            return (GetAppClosed("FMISDB"));
        }
        //Method added 09/09/04 by Madan - Will Provide us with info if FMIS Database is terminated
        //public static string getFMISMessage() {
        //    string pegResult = "";
        //    StringBuilder FilePath = new StringBuilder();
        //    FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
        //    FilePath.Append("\\MSCTL64.tlb"); 
        //    //string Filepath = "d:\\Components\\MSCTL64.tlb";
        //    try {
        //        DataSet dsXML = new DataSet();
        //        if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
        //            try {
        //                FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
        //                dsXML.ReadXml(fsXML);
        //                fsXML.Close();
        //            }
        //            catch (Exception e) {
        //                Console.Write(e.Message);
        //            }
        //        }
        //        else {  //send this only when this file is not available
        //            pegResult = "We are experiencing technical difficulties on web server. Please check back later.";
        //        }
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0) {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] FMISDR = dsXML.Tables["Msg"].Select("App='FMIS' and Term='True'");
        //            if (FMISDR.Length == 1) {
        //                pegResult = FMISDR[0]["Message"].ToString();
        //            }
        //        }
        //    }
        //    catch (System.Exception e) {
        //        Console.Write(e.Message);
        //    }

        //    return pegResult;
        //} //end of method				

        public static string getSanBoxMessage()
        {
            return (GetAppClosed("R7 SAN"));
        }

        public static string getPegDBMessage()
        {
            return (GetAppClosed("PEGDB"));
        }

        public static string getVITAPMessage()
        {
            return (GetAppClosed("VITAPDB"));
        }

        //Method added 09/09/04 by Madan - Will Provide us with info if VITAP Database is terminated
        //public static string getVITAPMessage() {
        //    string pegResult = "";
        //    //string Filepath = "d:\\Components\\MSCTL64.tlb";
        //    StringBuilder FilePath = new StringBuilder();
        //    FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
        //    FilePath.Append("\\MSCTL64.tlb"); 
        //    try {
        //        DataSet dsXML = new DataSet();
        //        if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
        //            try {
        //                FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
        //                dsXML.ReadXml(fsXML);
        //                fsXML.Close();
        //            }
        //            catch (Exception e) {
        //                Console.Write(e.Message);
        //            }
        //        }
        //        else {  //send this only when this file is not available
        //            pegResult = "We are experiencing technical difficulties on web server. Please check back later.";
        //        }
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0) {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] VitapDR = dsXML.Tables["Msg"].Select("App='VORA' and Term='True'");
        //            if (VitapDR.Length == 1) {
        //                pegResult = VitapDR[0]["Message"].ToString();
        //            }
        //        }
        //    }
        //    catch (System.Exception e) {
        //        Console.Write(e.Message);
        //    }

        //    return pegResult;
        //} //end of method		

        //Method added 06/30/03 by Madan
        //public static string getAppsMessage(string ServerName, string AppName)
        //{
        //    string pegResult = "";
        //    string strSAN = "";	    // Message for SAN box
        //    string strOracle = "";   // Message for Oracle
        //    string strApp = "";      //Message for Individual App
        //    string Filepath = "\\\\" + ServerName.Trim() + "\\components\\MSCTL64.tlb";
        //    try
        //    {
        //        DataSet dsXML = new DataSet();
        //        if (System.IO.File.Exists(Filepath.Trim()))
        //        {	//file exists)				
        //            try
        //            {
        //                FileStream fsXML = new FileStream(Filepath.Trim(), FileMode.Open, FileAccess.Read);
        //                dsXML.ReadXml(fsXML);
        //                fsXML.Close();
        //            }
        //            catch (Exception e)
        //            {
        //                Console.Write(e.Message);
        //            }
        //        }
        //        else
        //        {  //send this only when this file is not available
        //            pegResult = "We are experiencing technical difficulties on web server. Please check back later.";
        //        }
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0)
        //        {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] sanDR = dsXML.Tables["Msg"].Select("App='SANB' and Term='True'");
        //            if (sanDR.Length == 1)
        //            {
        //                strSAN = sanDR[0]["Message"].ToString();
        //            }
        //            //check for oracle entry if term =true
        //            System.Data.DataRow[] oraDR = dsXML.Tables["Msg"].Select("App='ORAC' and Term='True'");
        //            if (oraDR.Length == 1)
        //            {
        //                strOracle = oraDR[0]["Message"].ToString();
        //            }
        //            //check for Individual App entry if term =true
        //            System.Data.DataRow[] appDR = dsXML.Tables["Msg"].Select("App='" + AppName.Trim().ToUpper() + "' and Term='True'");
        //            if (appDR.Length == 1)
        //            {
        //                strApp = appDR[0]["Message"].ToString();
        //            }
        //            //decide on the message
        //            if (strSAN == "" && strOracle == "" && strApp == "")
        //            { //everything is ok
        //                pegResult = "";
        //            }
        //            if (strSAN != "")
        //            { //Something wrong with SAN BOX
        //                pegResult = strSAN;
        //            }
        //            if (strOracle != "")
        //            { //Something wrong with Oracle
        //                pegResult = strOracle;
        //            }
        //            if (strApp != "" && strSAN == "" && strOracle == "")
        //            { //Something wrong with Individual App
        //                pegResult = strApp;
        //            }
        //        }
        //    }
        //    catch (System.Exception e)
        //    {
        //        Console.Write(e.Message);
        //    }

        //    return pegResult;
        //} //end of method		

        //Method added 07/10/03 by Madan - overloading the previou method
        //public static string getAppsMessage(string AppName) {
        //    string pegResult = "";
        //    string strSAN = "";	    // Message for SAN box
        //    string strOracle = "";   // Message for Oracle
        //    string strApp = "";      //Message for Individual App
        //    //string Filepath = "d:\\Components\\MSCTL64.tlb";
        //    StringBuilder FilePath = new StringBuilder();
        //    FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
        //    FilePath.Append("\\MSCTL64.tlb"); 
        //    try {
        //        DataSet dsXML = new DataSet();
        //        if (System.IO.File.Exists(FilePath.ToString() )) {	//file exists)				
        //            try {
        //                FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
        //                dsXML.ReadXml(fsXML);
        //                fsXML.Close();
        //            }
        //            catch (Exception e) {
        //                Console.Write(e.Message);
        //            }
        //        }
        //        else {  //send this only when this file is not available
        //            pegResult = "We are experiencing technical difficulties on web server. Please check back later.";
        //        }
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0) {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] sanDR = dsXML.Tables["Msg"].Select("App='SANB' and Term='True'");
        //            if (sanDR.Length == 1) {
        //                strSAN = sanDR[0]["Message"].ToString();
        //            }
        //            //check for oracle entry if term =true
        //            System.Data.DataRow[] oraDR = dsXML.Tables["Msg"].Select("App='ORAC' and Term='True'");
        //            if (oraDR.Length == 1) {
        //                strOracle = oraDR[0]["Message"].ToString();
        //            }
        //            //check for Individual App entry if term =true
        //            System.Data.DataRow[] appDR = dsXML.Tables["Msg"].Select("App='" + AppName.Trim().ToUpper() + "' and Term='True'");
        //            if (appDR.Length == 1) {
        //                strApp = appDR[0]["Message"].ToString();
        //            }
        //            //decide on the message
        //            if (strSAN == "" && strOracle == "" && strApp == "") { //everything is ok
        //                pegResult = "";
        //            }
        //            if (strSAN != "") { //Something wrong with SAN BOX
        //                pegResult = strSAN;
        //            }
        //            if (strOracle != "") { //Something wrong with Oracle
        //                pegResult = strOracle;
        //            }
        //            if (strApp != "" && strSAN == "" && strOracle == "") { //Something wrong with Individual App
        //                pegResult = strApp;
        //            }
        //        }
        //    }
        //    catch (System.Exception e) {
        //        GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetAppsMessage", e.Message);
        //    }

        //    return pegResult;
        //} //end of method

        //public static string getAppsMessage() {
        //    string pegResult = "";
        //    string strSAN = "";	    // Message for SAN box
        //    string strOracle = "";   // Message for Oracle			
        //    //string Filepath = "d:\\Components\\MSCTL64.tlb";
        //    StringBuilder FilePath = new StringBuilder();
        //    FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
        //    FilePath.Append("\\MSCTL64.tlb"); 

        //    try {
        //        DataSet dsXML = new DataSet();
        //        if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
        //            try {
        //                FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
        //                dsXML.ReadXml(fsXML);
        //                fsXML.Close();
        //            }
        //            catch (Exception e) {
        //                Console.Write(e.Message);
        //            }
        //        }
        //        else {  //send this only when this file is not available
        //            pegResult = "We are experiencing technical difficulties on web server. Please check back later.";
        //        }
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0) {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] sanDR = dsXML.Tables["Msg"].Select("App='SANB' and Term='True'");
        //            if (sanDR.Length == 1) {
        //                strSAN = sanDR[0]["Message"].ToString();
        //            }
        //            //check for oracle entry if term =true
        //            System.Data.DataRow[] oraDR = dsXML.Tables["Msg"].Select("App='ORAC' and Term='True'");
        //            if (oraDR.Length == 1) {
        //                strOracle = oraDR[0]["Message"].ToString();
        //            }

        //            //decide on the message
        //            if (strSAN == "" && strOracle == "") { //everything is ok
        //                pegResult = "";
        //            }
        //            if (strSAN != "") { //Something wrong with SAN BOX
        //                pegResult = strSAN;
        //            }
        //            if (strOracle != "") { //Something wrong with Oracle
        //                pegResult = strOracle;
        //            }
        //        }
        //    }
        //    catch (System.Exception e) {
        //        GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetAppsMessage", e.Message);
        //    }

        //    return pegResult;
        //} //end of method		

        //Method added 11/15/2007 by Madan - For getting Invoice Search Login Page Instructions
        public static string getInvSearchMsg(string AppName)
        {
            string pegResult = "";
            string strApp = "";      //Message for Individual App
            //string Filepath = "d:\\Components\\MSCTL64.tlb";
            StringBuilder FilePath = new StringBuilder();
            FilePath.Append(Utilities.getDrivePathByLetter("MSCTL64:"));
            FilePath.Append("\\MSCTL64.tlb");
            try
            {
                DataSet dsXML = new DataSet();
                if (System.IO.File.Exists(FilePath.ToString()))
                {	//file exists)				
                    try
                    {
                        FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
                        dsXML.ReadXml(fsXML);
                        fsXML.Close();
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }
                }
                else
                {  //send this only when this file is not available
                    pegResult = "We are experiencing technical difficulties on web server. Please check back later.";
                }
                //Now lets read the value from dataset
                if (dsXML.Tables["Msg"].Rows.Count > 0)
                {
                    //check for Individual App entry
                    System.Data.DataRow[] appDR = dsXML.Tables["Msg"].Select("App='" + AppName.Trim().ToUpper() + "'");
                    if (appDR.Length == 1)
                    {
                        strApp = appDR[0]["Message"].ToString();
                        pegResult = strApp;
                    }
                }
            }
            catch (System.Exception e)
            {
                EventLog.AddWebErrors("GSA.R7BD.Utility", "DataAccess", "getInvSearchMsg", e.Message);
            }
            return pegResult;
        } //end of method   

        public static string GetUPPSYearUnitName()
        {
            System.Data.OracleClient.OracleConnection ConnBDFApps;
            System.Data.OracleClient.OracleCommand CmdBDFApps;

            string strConfigValue = "";
            try
            {
                ConnBDFApps = new System.Data.OracleClient.OracleConnection();
                CmdBDFApps = new System.Data.OracleClient.OracleCommand();

                ConnBDFApps.ConnectionString = ConnStrBDFApps();
                CmdBDFApps.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='UPPSYEAREND' and CONFIG_NAME='UNITNAME'";

                CmdBDFApps.Connection = ConnBDFApps;
                CmdBDFApps.Connection.Open();
                OracleDataReader myReader = CmdBDFApps.ExecuteReader();
                while (myReader.Read())
                {
                    strConfigValue = myReader["CONFIG_VALUE"].ToString().Trim();
                }
                ConnBDFApps.Close();
            }
            catch (Exception exp) { strConfigValue = "ERROR:" + exp.Message; }
            return strConfigValue;
        }

        //public static string GetIREPData(string Drawer)
        //{
        //    OracleConnection ConnBDFApps = new OracleConnection(); ;
        //    OracleCommand CmdBDFApps1 = new OracleCommand();
        //    OracleCommand CmdBDFApps2 = new OracleCommand();

        //    OracleConnection ConnIREP = new OracleConnection();
        //    OracleCommand CmdIREP = new OracleCommand();
        //    OracleDataAdapter adpIREP = new OracleDataAdapter();

        //    //DataSet dsData = new DataSet();
            
        //    string strDocType = "", strDrawerType = "", strSql="";

        //    try
        //    {
        //        ConnBDFApps.ConnectionString = ConnStrBDFApps();
        //        CmdBDFApps1.CommandText = "select bdfapps.f_get_config_value('" + Drawer.ToString() + "', 'IMAGENOW_DOC_TYPE_LIST') cval from DUAL";
        //        CmdBDFApps1.Connection = ConnBDFApps;
        //        CmdBDFApps1.Connection.Open();

        //        OracleDataReader myReader1 = CmdBDFApps1.ExecuteReader();
        //        while (myReader1.Read())
        //        {
        //            strDocType = myReader1["CVAL"].ToString().Trim();
        //        }

        //        CmdBDFApps2.CommandText = "select bdfapps.f_get_config_value('" + Drawer.ToString() + "', 'IMAGENOW_DRAWER_TYPE') cval from DUAL";
        //        CmdBDFApps2.Connection = ConnBDFApps;

        //        OracleDataReader myReader2 = CmdBDFApps2.ExecuteReader();
        //        while (myReader2.Read())
        //        {
        //            strDrawerType = myReader2["CVAL"].ToString().Trim();
        //        }

        //        //ConnIREP.ConnectionString = ConnStrIREP();
        //        strSql = "Select a.* from IN_TRANS.TRANS_DOC a, IN_TRANS.V_DOC_TYPE b, IN_TRANS.V_DRAWER c ";
        //        strSql = strSql + "where a.doc_type_id = b.doc_type_id and a.drawer_id = c.drawer_id and ";
        //        strSql = strSql + "lower(b.doc_type_name) in lower(" + strDocType.ToString() + ") and ";
        //        strSql = strSql + "lower(c.drawer_name) = lower('" + strDrawerType.ToString() + "') and a.doc_status = 0";

        //        //adpIREP.SelectCommand = new OracleCommand(strSql, ConnIREP);
        //        //adpIREP.Fill(dsData);
        //        //adpIREP.Dispose();
        //    }
        //    catch(Exception exp)
        //    {
        //        EventLog.AddWebErrors("Utility", "DataAccess.cs", "IREPData", exp.Message.ToString());
        //    }

        //    //return dsData;

        //    return strSql;

        //}

        /// <summary>
        /// Check if the UPPSYE application is open by checking the following:
        /// If the current date is greater or equal then start date and
        /// if current date is less then or equal to the end date and
        /// if the UPPSOPENFlag is set to true
        /// UPPSOPENFLAG set to true - indicates the back end processes have run 
        /// and data is available for the application
        /// </summary>
        /// <returns></returns>
        public static string IsUPPSYearEndOpen()
        {
            System.Data.OracleClient.OracleConnection ConnBDFApps;
            System.Data.OracleClient.OracleCommand CmdBDFApps1;
            System.Data.OracleClient.OracleCommand CmdBDFApps2;
            System.Data.OracleClient.OracleCommand CmdBDFApps3;

            string strStartDate = "", strEndDate = "", strConfigValue="", strFlag="";
            try
            {
                ConnBDFApps = new System.Data.OracleClient.OracleConnection();
                CmdBDFApps1 = new System.Data.OracleClient.OracleCommand();
                CmdBDFApps2 = new System.Data.OracleClient.OracleCommand();
                CmdBDFApps3 = new System.Data.OracleClient.OracleCommand();

                ConnBDFApps.ConnectionString = ConnStrBDFApps();
                CmdBDFApps1.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='UPPSYEAREND' and CONFIG_NAME='STARTDATE'";

                CmdBDFApps1.Connection = ConnBDFApps;
                CmdBDFApps1.Connection.Open();
                OracleDataReader myReader1 = CmdBDFApps1.ExecuteReader();
                while (myReader1.Read())
                {
                    strStartDate = myReader1["CONFIG_VALUE"].ToString().Trim();
                }

                CmdBDFApps2.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='UPPSYEAREND' and CONFIG_NAME='ENDDATE'";

                CmdBDFApps2.Connection = ConnBDFApps;
                OracleDataReader myReader2 = CmdBDFApps2.ExecuteReader();
                while (myReader2.Read())
                {
                    strEndDate = myReader2["CONFIG_VALUE"].ToString().Trim();
                }

                CmdBDFApps3.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='UPPSYEAREND' and CONFIG_NAME='UPPSOPENFLAG'";

                CmdBDFApps3.Connection = ConnBDFApps;
                OracleDataReader myReader3 = CmdBDFApps3.ExecuteReader();
                while (myReader3.Read())
                {
                    strFlag = myReader3["CONFIG_VALUE"].ToString().Trim();
                }

                ConnBDFApps.Close();

                DateTime dt = DateTime.Now;
                
                strStartDate = strStartDate.Trim() + "/" + dt.Year.ToString();
                strEndDate = strEndDate.Trim() + "/" + dt.Year.ToString();


            }
            catch (Exception exp) { strConfigValue = "ERROR:" + exp.Message; }

            try
            {
                DateTime dtStartDate = DateTime.Parse(strStartDate);
                DateTime dtEndDate = DateTime.Parse(strEndDate);

                if (DateTime.Now >= dtStartDate && DateTime.Now <= dtEndDate && strFlag.Equals("T"))
                {
                    strConfigValue = "OPEN";
                }
                else
                {
                    strConfigValue = "The New FY Profile Updates application is open between " + strStartDate + " and " + strEndDate;
                }
                return strConfigValue;
            }
            catch (Exception exp)
            {
                strConfigValue = "OPEN";
                return strConfigValue;
            }
        }

        public static string GetOIReviewFinanceCutoffDate()
        {
            System.Data.OracleClient.OracleConnection ConnBDFApps;
            System.Data.OracleClient.OracleCommand CmdBDFApps;

            string strConfigValue = "";
            try
            {
                ConnBDFApps = new System.Data.OracleClient.OracleConnection();
                CmdBDFApps = new System.Data.OracleClient.OracleCommand();

                ConnBDFApps.ConnectionString = ConnStrBDFApps();
                CmdBDFApps.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='OIReview' and CONFIG_NAME='FinanceCutoff'";

                CmdBDFApps.Connection = ConnBDFApps;
                CmdBDFApps.Connection.Open();
                OracleDataReader myReader = CmdBDFApps.ExecuteReader();
                while (myReader.Read())
                {
                    strConfigValue = myReader["CONFIG_VALUE"].ToString().Trim();
                }
            }
            catch (Exception exp) { strConfigValue = "ERROR:" + exp.Message; }
            return strConfigValue;
        }

        public static string GetOIReviewServiceCutoffDate()
        {
            System.Data.OracleClient.OracleConnection ConnBDFApps;
            System.Data.OracleClient.OracleCommand CmdBDFApps;

            string strConfigValue = "";
            try
            {
                ConnBDFApps = new System.Data.OracleClient.OracleConnection();
                CmdBDFApps = new System.Data.OracleClient.OracleCommand();

                ConnBDFApps.ConnectionString = ConnStrBDFApps();
                CmdBDFApps.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='OIReview' and CONFIG_NAME='ServiceOfficeCutoff'";

                CmdBDFApps.Connection = ConnBDFApps;
                CmdBDFApps.Connection.Open();
                OracleDataReader myReader = CmdBDFApps.ExecuteReader();
                while (myReader.Read())
                {
                    strConfigValue = myReader["CONFIG_VALUE"].ToString().Trim();
                }
            }
            catch (Exception exp) { strConfigValue = "ERROR:" + exp.Message; }
            return strConfigValue;
        }

        // Method to check if PegasysOpen Item Review - Finance is open.
        // The app is open if today's date is past the service cutoff date
        // but before the Finance cutoff date, otherwise it is closed
        public static string IsPoirFinanceOpen()
        {
            string strServiceCutoffDate = "";
            string strFinanceCutoffDate = "";
            string returnval = "";
            int iServiceCutoffDate = 0;
            int iFinanceCutoffDate = 0;
            string strFromDate = "";
            string strToDate = "";

            try
            {
                strServiceCutoffDate = GetOIReviewServiceCutoffDate();
                if (strServiceCutoffDate != "")
                    iServiceCutoffDate = int.Parse(strServiceCutoffDate);

                strFinanceCutoffDate = GetOIReviewFinanceCutoffDate();
                if (strFinanceCutoffDate != "")
                    iFinanceCutoffDate = int.Parse(strFinanceCutoffDate);

                if (strFinanceCutoffDate == "")
                {
                    EventLog.AddWebErrors("R7BD.Utility", "DataAccess", "IsPoirFinanceOpen", "Finance cutoff date is blank");
                    returnval = "Pegasys Open Items Review - Finance application is unavailable now. Please try again later.";
                }
                else if (strServiceCutoffDate == "")
                {
                    EventLog.AddWebErrors("R7BD.Utility", "DataAccess", "IsPoirFinanceOpen", "Service cutoff date is blank");
                    returnval = "Pegasys Open Items Review - Finance application is unavailable now. Please try again later.";
                }
                    else if (DateTime.Now.Day <= iServiceCutoffDate || DateTime.Now.Day > iFinanceCutoffDate)
                {
                    strFromDate = DateTime.Now.Month.ToString() + "/" + (iServiceCutoffDate + 1).ToString();
                    strToDate = DateTime.Now.Month.ToString() + "/" + iFinanceCutoffDate;
                    returnval = "Pegasys Open Items Review - Finance application is only available from " + strFromDate + " to " + strToDate;
                }
                else
                    returnval = "OPEN";
                //Here we assume the Service cutoff date is before the Finance cutoff date
                //else if ((DateTime.Now.Day > iFinanceCutoffDate) ||
                //    (DateTime.Now.Day <= iServiceCutoffDate)) 
                //else if (1 == 2)  // use this line for TEST ONLY
                //{
                //    strFromDate = DateTime.Now.Month.ToString() + "/" + (iServiceCutoffDate + 1).ToString();
                //    strToDate = DateTime.Now.Month.ToString() + "/" + strFinanceCutoffDate;
                //    returnval = "This application is only available from " + strFromDate + " to " + strToDate;
                //}
                //else returnval = "OPEN";
            }
            catch (Exception exp) { returnval = "ERROR:" + exp.Message; }
            return returnval;
        }//end of method

        // Method to check if PegasysOpen Item Review - Service is open.
        // The app is open if today's date is at least the first of the month, 
        // but not after the Service Cutoff date, otherwise it is closed.
        public static string IsPoirServiceOpen()
        {
            string strServiceCutoffDate = "";
            string returnval = "";
            int iServiceCutoffDate = 0;
            string strFromDate = "";
            string strToDate = "";

            try
            {
                strServiceCutoffDate = GetOIReviewServiceCutoffDate();
                if (strServiceCutoffDate != "")
                    iServiceCutoffDate = int.Parse(strServiceCutoffDate);

                if (strServiceCutoffDate == "")
                {
                    EventLog.AddWebErrors("R7BD.Utility", "DataAccess", "IsPoirServiceOpen", "Service cutoff date is blank");
                    returnval = "Pegasys Open Items Review - Service Office application is unavailable now. Please try again later.";
                }
                else if (DateTime.Now.Day > iServiceCutoffDate)
                {
                    strFromDate = DateTime.Now.Month.ToString() + "/1";
                    strToDate = DateTime.Now.Month.ToString() + "/" + strServiceCutoffDate;
                    returnval = "Pegasys Open Items Review - Service Office application is only available from " + strFromDate + " to " + strToDate;
                }
                else
                    returnval = "OPEN";

            }
            catch (Exception exp) { returnval = "ERROR:" + exp.Message; }
            return returnval;
        }//end of method

        //public static string getUPPSFiscalYear()
        //{
        //    System.Data.OracleClient.OracleConnection ConnBDFApps;
        //    System.Data.OracleClient.OracleCommand CmdBDFApps;

        //    string strConfigValue = "";
        //    try
        //    {
        //        ConnBDFApps = new System.Data.OracleClient.OracleConnection();
        //        CmdBDFApps = new System.Data.OracleClient.OracleCommand();

        //        ConnBDFApps.ConnectionString = ConnStrBDFApps();
        //        CmdBDFApps.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='UPPSYEAREND' and CONFIG_NAME='FISCALYEAR'";

        //        CmdBDFApps.Connection = ConnBDFApps;
        //        CmdBDFApps.Connection.Open();
        //        OracleDataReader myReader = CmdBDFApps.ExecuteReader();
        //        while (myReader.Read())
        //        {
        //            strConfigValue = myReader["CONFIG_VALUE"].ToString().Trim();
        //        }
        //    }
        //    catch (Exception exp) { strConfigValue = "ERROR:" + exp.Message; }
        //    return strConfigValue;
        //}

        ////New method for getting IREP connection string
        ////Added by RupaD - 03/21/2013
        //public static string ConnStrIREP()
        //{
        //    //string ConnStr = "";
        //    string strDBName = "";
        //    string strUser = "";
        //    string strPW = "";
        //    StringBuilder ConnStr = new StringBuilder();

        //    //Get the name of the Database to use
        //    strDBName = GSA.R7BD.Utility.Utilities.getDBConfigValueByLetter("IMAGENOWVITAPDB:");

        //    //Get the name of the User account to use
        //    strUser = GSA.R7BD.Utility.Utilities.getDBConfigValueByLetter("IMAGENOWVITAPUSER:");

        //    //get the password for the User account
        //    strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

        //    //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
        //    ConnStr.Append("Data Source=");
        //    ConnStr.Append(strDBName);
        //    ConnStr.Append(";User ID=");
        //    ConnStr.Append(strUser);
        //    ConnStr.Append(";Password=");
        //    ConnStr.Append(strPW);
        //    ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

        //    return ConnStr.ToString();
        //}

        //New method for getting SysApps connection string
        //Added by Ken Sickler - 08/27/2010
        public static string ConnStrBDFApps()
        {
            //string ConnStr = "";
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = GSA.R7BD.Utility.Utilities.getDBConfigValueByLetter("BDRPTDB:");

            //Get the name of the User account to use
            strUser = GSA.R7BD.Utility.Utilities.getDBConfigValueByLetter("BDRPTUSER:");

            //get the password for the User account
            strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            ConnStr.Append(";Password=");
            ConnStr.Append(strPW);
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }

        //New method for getting SysApps connection string
        //Added by Ron Sele - 02/18/2011
        public static string ConnStrWSAApps()
        {
            //string ConnStr = "";
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = GSA.R7BD.Utility.Utilities.getDBConfigValueByLetter("BDRPTDB:");

            //Get the name of the User account to use
            strUser = GSA.R7BD.Utility.Utilities.getDBConfigValueByLetter("WSAUSER:");

            //get the password for the User account
            strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            ConnStr.Append(";Password=");
            ConnStr.Append(strPW);
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }

        //New method for getting BD Reports DB connection string
        //Added by Ken Sickler - 08/27/2010
        public static string ConnStrBDRPT(User user)
        {
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = Utilities.getDBConfigValueByLetter("BDRPTDB:");

            //Get the name of the User account to use
            switch (user)
            {
                case User.iuser_bart:
                    strUser = Utilities.getDBConfigValueByLetter("BDRPTBART:");
                    break;
                case User.var:
                    strUser = Utilities.getDBConfigValueByLetter("BDRPTVARUSER:");
                    break;
            }

            //get the password for the User account
            strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            ConnStr.Append(";Password=");
            ConnStr.Append(strPW);
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }

        //New method for getting BD Reports DB connection string
        //Added by Ken Sickler - 08/27/2010
        public static string ConnStrDW(User user)
        {
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = Utilities.getDBConfigValueByLetter("DWRPTDB:");

            //Get the name of the User account to use
            switch (user)
            {
                case User.iuser_wsa:
                    strUser = Utilities.getDBConfigValueByLetter("DWRPTWSA:");
                    break;
            }

            //get the password for the User account
            strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            ConnStr.Append(";Password=");
            ConnStr.Append(strPW);
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }

        //New method for getting FMIS connection string
        //Added by Ken Sickler - 08/27/2010
        public static string ConnStrFMIS(User user)
        {
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = Utilities.getDBConfigValueByLetter("FMISDB:");

            //Get the name of the User account to use
            switch (user)
            {
                case User.fmis_U7B0223:
                    strUser = Utilities.getDBConfigValueByLetter("FMIS_U7B0223"); 
                    break;

                case User.r7bcquery:
                    strUser = Utilities.getDBConfigValueByLetter("FMISR7BCQUERY:");
                    break;

                //case User.audit:
                //    strUser = Utilities.getDBConfigValueByLetter("VITAPAUDITUSER:");
                //    break;
            }

            //get the password for the User account
            strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            ConnStr.Append(";Password=");
            ConnStr.Append(strPW);
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }
  
        //New method for getting Pegasys connection string
        //Added by Ken Sickler - 08/27/2010
        public static string ConnStrPegasysLive(User user)
        {
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = Utilities.getDBConfigValueByLetter("PEGLIVEDB:");

            //Get the name of the User account to use
            switch (user)
            {
                case User.r7bcquery:
                    strUser = Utilities.getDBConfigValueByLetter("PEGLIVE_R7BCQUERY:");
                    break;
            }

            //get the password for the User account
            strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            ConnStr.Append(";Password=");
            ConnStr.Append(strPW);
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }

        //Get the name of the pegasys DB we are using
        public static string GetCurrentPegDBName()
        {
            //Get the name of the Pegasys Database 
            return (Utilities.getDBConfigValueByLetter("PEGDB:"));
        }

        //New method for getting Vitap connection string
        //Added by Ken Sickler - 08/27/2010
        public static string ConnStrVitapOracle(User user)
        {
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = Utilities.getDBConfigValueByLetter("VITAPDB:");

            //Get the name of the User account to use
            switch (user)
            {
                case User.iuser_audit:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPAUDITUSER:");
                    break;

                case User.iuser_dodaac:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPDODAAC:");
                    break;

                case User.iuser_expaccr:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPEXPACCR:");
                    break;

                case User.iuser_external:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPEXTERNAL:");
                    break;

                case User.iuser_formdelete:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPFORMDELETE:");
                    break;

                case User.iuser_leaseea:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPLEASEEA:");
                    break;

                case User.iuser_poir:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPPOIR:");
                    break;

                case User.iuser_pmt:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPPMT:");
                    break;

                case User.iuser_pwdapp:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPPWDAPP:");
                    break;

                case User.iuser_vitap:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPVITAP:");
                    break;

                case User.iuser_webadmin:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPWEBADMIN:");
                    break;

                case User.iuser_wvendor:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPWVENDOR:");
                    break;

                case User.poldr:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPPOLDR:");
                    break;

                case User.var:
                    strUser = Utilities.getDBConfigValueByLetter("VITAPVARUSER:");
                    break;
            }

            //get the password for the User account
            //if (user == User.iuser_wvendor)
            //    strPW = "wvendors_2011";
            //else
                strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            ConnStr.Append(";Password=");
            ConnStr.Append(strPW);
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }

        //public static String ConnStrOrabill(User user)
        //{
        //    string strDBName = "";
        //    string strUser = "";
        //    string strPW = "";
        //    StringBuilder ConnStr = new StringBuilder();

        //    //Get the name of the Database to use
        //    strDBName = Utilities.getDBConfigValueByLetter("BDRPTDB:");

        //    //Get the name of the User account to use
        //    switch (user)
        //    {
        //        case User.user:
        //            strUser = Utilities.getDBConfigValueByLetter("BDRPTUSER:");
        //            break;

        //        case User.iuser_bart:
        //            strUser = Utilities.getDBConfigValueByLetter("BDRPTBART:");
        //            break;
        //    }

        //    //get the password for the User account
        //    strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

        //    //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
        //    ConnStr.Append("Data Source=");
        //    ConnStr.Append(strDBName);
        //    ConnStr.Append(";User ID=");
        //    ConnStr.Append(strUser);
        //    ConnStr.Append(";Password=");
        //    ConnStr.Append(strPW);
        //    ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

        //    return ConnStr.ToString();
        //}

        public static string ConnStrFinsOra8(User user)
        {
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = Utilities.getDBConfigValueByLetter("FINANCEDB:");

            //Get the name of the User account to use
            switch (user)
            {
                case User.iuser_acorn:
                    strUser = Utilities.getDBConfigValueByLetter("FINANCEACORN:");
                    break;

                case User.iuser_pmt: 
                    strUser = Utilities.getDBConfigValueByLetter("FINANCEPMT:");
                    break;

                case User.iuser_rwa:
                    strUser = Utilities.getDBConfigValueByLetter("FINANCERWA:");
                    break;

                case User.iuser_vitap:
                    strUser = Utilities.getDBConfigValueByLetter("FINANCEVITAP:");
                    break;

            }

            //get the password for the User account
            strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            ConnStr.Append(";Password=");
            ConnStr.Append(strPW);
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }

        public static string ConnStrPegasysReports(User user)
        {
            string strDBName = "";
            string strUser = "";
            string strPW = "";
            StringBuilder ConnStr = new StringBuilder();

            //Get the name of the Database to use
            strDBName = Utilities.getDBConfigValueByLetter("PEGREPORTSDB:");

            //Get the name of the User account to use
            switch (user)
            {
                case User.r7bcquery:
                    strUser = Utilities.getDBConfigValueByLetter("PEGREPORTS_R7BCQUERY:");
                    break;

            }

            //get the password for the User account
            strPW = GSA.R7BD.Utility.SharedPassword.GetPassword(strDBName, strUser);

            //ConnStr = "Data Source=bdrptdev;User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
            ConnStr.Append("Data Source=");
            ConnStr.Append(strDBName);
            ConnStr.Append(";User ID=");
            ConnStr.Append(strUser);
            //if (user != User.r7bcquery)
            //{
                ConnStr.Append(";Password=");
                ConnStr.Append(strPW);
            //}
            ConnStr.Append(";Pooling=true;Connection Lifetime=1200");

            return ConnStr.ToString();
        }
       
        //New method for getting Reports DB Name
        //Added by Ken Sickler - 08/27/2010
        public static string getReportsDBName()
        {
            return (Utilities.getDBConfigValueByLetter("PEGREPORTSDB:"));
        }

        //New method for getting Pegasys DB Name
        //Added by Ken Sickler - 08/27/2010
        public static string getPegasysDBName()
        {
            return (Utilities.getDBConfigValueByLetter("PEGDB:"));
        }

        //start adding methods here - for getting pegasys connection strings
        #region public static String ConnStrPegasys(User User_ID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse)
        //public static String ConnStrPegasys(User User_ID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    string UserID = "";
        //    string TestDBName = getTestDBName();
        //    if (TestDBName == "ERROR") { TestDBName = "test6.world"; }
        //    string LiveDBName = getLiveDBName();
        //    if (LiveDBName == "ERROR") { TestDBName = "newpegasys.world"; }
        //    string ReportsDBName = getReportsDBName();
        //    if (ReportsDBName == "ERROR") { TestDBName = "newpegoltp.world"; }
        //    UserID = User_ID.ToString();

        //    if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID);
        //            ConnStr = "Data Source=" + LiveDBName + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID);
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + LiveDBName + ";Extended Properties=;Pooling=true;Connection Lifetime=1200"; //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Test) {
        //        pwd = SharedPassword.GetPassword(TestDBName.Trim(), UserID);

        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=" + TestDBName + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";	//Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Reports) {
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID);
        //            ConnStr = "Data Source=" + ReportsDBName + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID);
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + ReportsDBName + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";	 //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr;
        //}  //end of this method
        #endregion

        //Added by Madan - To give the choice to choose between source of the password. XML file or SAN Box
        #region public static String ConnStrPegasys(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN, string FileLocation)
        //public static String ConnStrPegasys(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN, string FileLocation) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    string TestDBName = getTestDBName();
        //    if (TestDBName == "ERROR") { TestDBName = "test6.world"; }
        //    string LiveDBName = getLiveDBName();
        //    if (LiveDBName == "ERROR") { TestDBName = "newpegasys.world"; }
        //    string ReportsDBName = getReportsDBName();
        //    if (ReportsDBName == "ERROR") { TestDBName = "newpegoltp.world"; }

        //    if (XmlOrSAN == FileSource.XML) {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID, FileLocation.Trim());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";	//Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword(TestDBName, UserID, FileLocation.Trim());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + TestDBName + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Reports) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID, FileLocation.Trim());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + ReportsDBName + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + ReportsDBName + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";	  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    else {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword(TestDBName.Trim(), UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + TestDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Reports) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + ReportsDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + ReportsDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";	  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    return ConnStr;
        //}  //end of this method
        #endregion

        //Overloaded. Added by Madan - To give the choice to choose between source of the password. XML file or SAN Box
        #region public static String ConnStrPegasys(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN)
        //public static String ConnStrPegasys(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    string TestDBName = getTestDBName();
        //    if (TestDBName == "ERROR") { TestDBName = "test6.world"; }
        //    string LiveDBName = getLiveDBName();
        //    if (LiveDBName == "ERROR") { TestDBName = "newpegasys.world"; }
        //    string ReportsDBName = getReportsDBName();
        //    if (ReportsDBName == "ERROR") { TestDBName = "newpegoltp.world"; }
        //    //string FileLocation = "d:\\Components\\MSCTL32.tlb";
        //    StringBuilder FileLocation = new StringBuilder();
        //    FileLocation.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32:"));
        //    FileLocation.Append("\\MSCTL32.tlb"); 

        //    if (XmlOrSAN == FileSource.XML) {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID, FileLocation.ToString());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword(TestDBName, UserID);

        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + TestDBName + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Reports) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID, FileLocation.ToString());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + ReportsDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + ReportsDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    else {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword(TestDBName, UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + TestDBName + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Reports) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + ReportsDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + ReportsDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    return ConnStr;
        //}  //end of this method
        #endregion

        //Added by Madan 7/1/03 - to call this method based on User ID.		
        #region public static String ConnStrPegasys(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse)
        //public static String ConnStrPegasys(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    string TestDBName = getTestDBName();
        //    if (TestDBName == "ERROR") { TestDBName = "test6.world"; }
        //    string LiveDBName = getLiveDBName();
        //    if (LiveDBName == "ERROR") { TestDBName = "newpegasys.world"; }
        //    string ReportsDBName = getReportsDBName();
        //    if (ReportsDBName == "ERROR") { TestDBName = "newpegoltp.world"; }

        //    if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID.Trim());
        //            ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID.Trim() + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID.Trim());
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Test) {
        //        pwd = SharedPassword.GetPassword(TestDBName, UserID);
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=" + TestDBName + ";User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Reports) {
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID.Trim());
        //            ConnStr = "Data Source=" + ReportsDBName.Trim() + ";User ID=" + UserID.Trim() + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID.Trim());
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=" + ReportsDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr + ";Pooling=true;Connection Lifetime=1200";
        //}  //end of this method 
        #endregion

        //for getting pegasys connection strings by supplying App Name 
        #region public static String ConnStrPegasys(User User_ID, string AppName, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse)
        //public static String ConnStrPegasys(User User_ID, string AppName, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    string UserID = "";
        //    UserID = User_ID.ToString();
        //    string TestDBName = getTestDBName();
        //    if (TestDBName == "ERROR") { TestDBName = "test6.world"; }
        //    string LiveDBName = getLiveDBName();
        //    if (LiveDBName == "ERROR") { TestDBName = "newpegasys.world"; }
        //    string ReportsDBName = getReportsDBName();
        //    if (ReportsDBName == "ERROR") { TestDBName = "newpegoltp.world"; }

        //    if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //        pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID);
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Test) {
        //        pwd = SharedPassword.GetPassword(TestDBName.Trim(), UserID);

        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=" + TestDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Reports) {
        //        pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID);

        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=" + ReportsDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + ReportsDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr + ";Pooling=true;Connection Lifetime=1200";
        //}  //end of this method
        #endregion

        //Added by Madan 12/15/2005 - This method will give the connstr for Pegasys 6.1 version 	
        #region public static String ConnStrPegasys61(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse)
        //public static String ConnStrPegasys61(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    //Rupa Duthulur 02/17/2006 Changing the Pegasys 6.1 test database
        //    //string TestDBName = "pegasys6.world";
        //    string TestDBName = "tstmtm3";
        //    string LiveDBName = getLiveDBName();
        //    if (LiveDBName == "ERROR") { TestDBName = "newpegasys.world"; }
        //    string ReportsDBName = getReportsDBName();
        //    if (ReportsDBName == "ERROR") { TestDBName = "newpegoltp.world"; }

        //    if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID.Trim());
        //            ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID.Trim() + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID.Trim());
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Test) {
        //        pwd = SharedPassword.GetPassword(TestDBName, UserID);
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=" + TestDBName + ";User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Reports) {
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID.Trim());
        //            ConnStr = "Data Source=" + ReportsDBName.Trim() + ";User ID=" + UserID.Trim() + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword(ReportsDBName.Trim(), UserID.Trim());
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=" + ReportsDBName.Trim() + ";Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr + ";Pooling=true;Connection Lifetime=1200";
        //}  //end of this method 
        #endregion

        #region public static String ConnStrVitapOracle(User User_ID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse)
        //*******************************************************************************************************************
        //public static String ConnStrVitapOracle(User User_ID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    string UserID = "";
        //    UserID = User_ID.ToString();
        //    if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword("vitap.world", UserID);
        //            ConnStr = "Data Source=vitap.world;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword("vitap.world", UserID);
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=vitap.world;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Test) {
        //        pwd = SharedPassword.GetPassword("vitaptst", UserID);
        //        ConnStr = "Data Source=vitaptst;User ID=" + UserID + ";Password=" + pwd;
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword("vitaptst", UserID);
        //            ConnStr = "Data Source=vitaptst;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword("vitaptst", UserID);
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=vitaptst;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }            
        //    else if (ChooseDatabaseType == DatabaseType.Development) {
        //        pwd = SharedPassword.GetPassword("tstvitap.world", UserID);
        //        ConnStr = "Data Source=tstvitap;User ID=" + UserID + ";Password=" + pwd;
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            pwd = SharedPassword.GetPassword("tstvitap.world", UserID);
        //            ConnStr = "Data Source=tstvitap.world;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            pwd = SharedPassword.GetPassword("tstvitap.world", UserID);
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=tstvitap.world;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr + ";Pooling=true;Connection Lifetime=1200";
        //}  //end of this method
        #endregion

        #region public static String ConnStrVitapOracle(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse)
        //public static String ConnStrVitapOracle(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //        pwd = SharedPassword.GetPassword("vitap.world", UserID);
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=vitap.world;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=vitap.world;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Test) {
        //        pwd = SharedPassword.GetPassword("vitaptst", UserID);
        //        ConnStr = "Data Source=vitaptst;User ID=" + UserID + ";Password=" + pwd;
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=vitaptst;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=vitaptst;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Development) {
        //        pwd = SharedPassword.GetPassword("tstvitap.world", UserID);
        //        ConnStr = "Data Source=tstvitap;User ID=" + UserID + ";Password=" + pwd;
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=tstvitap.world;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=tstvitap.world;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr + ";Pooling=true;Connection Lifetime=1200";
        //}  //end of this method
        #endregion

        //Madan 06/30/2008 For DWDEV and DWPROD Databases   
        #region public static String ConnStrDW(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse)
        //public static String ConnStrDW(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //        pwd = SharedPassword.GetPassword("dwprod.gsa.gov", UserID);
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=dwprod.gsa.gov;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=dwprod.gsa.gov;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Test) {
        //        pwd = SharedPassword.GetPassword("dwdev.gsa.gov", UserID);
        //        ConnStr = "Data Source=dwdev.gsa.gov;User ID=" + UserID + ";Password=" + pwd;
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=dwdev.gsa.gov;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=dwdev.gsa.gov;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Reports) {
        //        pwd = SharedPassword.GetPassword("dwtest.gsa.gov", UserID);
        //        ConnStr = "Data Source=dwtest.gsa.gov;User ID=" + UserID + ";Password=" + pwd;
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=dwtest.gsa.gov;User ID=" + UserID + ";Password=" + pwd;
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=dwtest.gsa.gov;Extended Properties=";  //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr + ";Pooling=true;Connection Lifetime=1200";
        //}  //end of this method
        #endregion

        #region public static String ConnStrVitapOracle(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN, string FileLocation)
        //public static String ConnStrVitapOracle(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN, string FileLocation) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    if (XmlOrSAN == FileSource.XML) {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword("vitap.world", UserID.Trim(), FileLocation.Trim());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=vitap.world;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd.Trim() + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=vitap.world;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword("vitaptst", UserID.Trim(), FileLocation.Trim());
        //            ConnStr = "Data Source=vitaptst;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=vitaptst;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd.Trim() + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=vitaptst;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Development) {
        //            pwd = SharedPassword.GetPassword("tstvitap.world", UserID.Trim(), FileLocation.Trim());
        //            ConnStr = "Data Source=tstvitap;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=tstvitap.world;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd.Trim() + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=tstvitap.world;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    else {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword("vitap.world", UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=vitap.world;User ID=" + UserID + ";Password=" + pwd;
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=vitap.world;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword("vitaptst", UserID);
        //            ConnStr = "Data Source=vitaptst;User ID=" + UserID + ";Password=" + pwd;
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=vitaptst;User ID=" + UserID + ";Password=" + pwd;
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=vitaptst;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Development) {
        //            pwd = SharedPassword.GetPassword("tstvitap.world", UserID);
        //            ConnStr = "Data Source=tstvitap;User ID=" + UserID + ";Password=" + pwd;
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=tstvitap.world;User ID=" + UserID + ";Password=" + pwd;
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=tstvitap.world;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    return ConnStr + ";Pooling=true;Connection Lifetime=1200";
        //}  //end of this method
        #endregion

        //Overloaded.
        #region public static String ConnStrVitapOracle(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN)
        //public static String ConnStrVitapOracle(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    //string FileLocation = "d:\\Components\\MSCTL32.tlb";
        //    StringBuilder FileLocation = new StringBuilder();
        //    FileLocation.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32:"));
        //    FileLocation.Append("\\MSCTL32.tlb");

        //    if (XmlOrSAN == FileSource.XML) {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword("vitap.world", UserID.Trim(), FileLocation.ToString());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=vitap.world;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd.Trim() + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=vitap.world;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword("vitaptst", UserID.Trim(), FileLocation.ToString());
        //            ConnStr = "Data Source=vitaptst;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=vitaptst;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd.Trim() + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=vitaptst;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Development) {
        //            pwd = SharedPassword.GetPassword("tstvitap.world", UserID.Trim(), FileLocation.ToString());
        //            ConnStr = "Data Source=tstvitap;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=tstvitap.world;User ID=" + UserID.Trim() + ";Password=" + pwd.Trim();
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd.Trim() + ";" + "Persist Security Info=True;User ID=" + UserID.Trim() + ";" + "Data Source=tstvitap.world;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    else {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword("vitap.world", UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=vitap.world;User ID=" + UserID + ";Password=" + pwd;
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=vitap.world;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword("vitaptst", UserID);
        //            ConnStr = "Data Source=vitaptst;User ID=" + UserID + ";Password=" + pwd;
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=vitaptst;User ID=" + UserID + ";Password=" + pwd;
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=vitaptst;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Development) {
        //            pwd = SharedPassword.GetPassword("tstvitap.world", UserID);
        //            ConnStr = "Data Source=tstvitap;User ID=" + UserID + ";Password=" + pwd;
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=tstvitap.world;User ID=" + UserID + ";Password=" + pwd;
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=tstvitap.world;Extended Properties=";  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    return ConnStr + ";Pooling=true;Connection Lifetime=1200";
        //}  //end of this method
        #endregion

        #region public static String ConnStrVitapFoxPro(DatabaseType ChooseDatabaseType)
        //**************************************************************************************************************************
        //public static String ConnStrVitapFoxPro(DatabaseType ChooseDatabaseType) {
        //    string ConnStr = "";
        //    if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //        string dts = "\\\\e07bds-san\\vitap\\vitap.dbc";
        //        ConnStr = "Provider=VFPOLEDB.1;Extended Properties='';Data Source=" + dts + ";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE";
        //    }
        //    else if (ChooseDatabaseType == DatabaseType.Test) {
        //        string dts = "\\\\e07bds-san\\vitapapp\\vitap\\vitap.dbc";
        //        ConnStr = "Provider=VFPOLEDB.1;Extended Properties='';Data Source=" + dts + ";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE";
        //    }
        //    return ConnStr;
        //}  //end of this method 		
        #endregion

        //Method added 07/10/03 by Madan : Modifying so that we have the option of querying XML file
        #region public static string GetCurrentPegDBName()
        //public static string GetCurrentPegDBName() {
        //    //string Filepath = "d:\\Components\\MSCTL64.tlb";
        //    StringBuilder FilePath = new StringBuilder();
        //    FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
        //    FilePath.Append("\\MSCTL64.tlb"); 
        //    string pegResult = "";

        //    DataSet dsXML = new DataSet();
        //    if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
        //        try {
        //            FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
        //            dsXML.ReadXml(fsXML);
        //            fsXML.Close();
        //        }
        //        catch (Exception e) {
        //            GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetCurrentPegDBName", e.Message);
        //        }
        //    }
        //    else {  //File not there. Take it from SAN BOX
        //        System.Data.OleDb.OleDbDataReader pegReader;
        //        System.Data.OleDb.OleDbConnection pegConnection = new System.Data.OleDb.OleDbConnection();
        //        System.Data.OleDb.OleDbCommand pegCommand = new System.Data.OleDb.OleDbCommand();
        //        pegConnection.ConnectionString = ConnectionString.CSforPasswordsTable();
        //        pegCommand.CommandText = "select appname from message where app='PORA'";
        //        //Open and execute the command
        //        try {
        //            pegCommand.Connection = pegConnection;
        //            pegCommand.Connection.Open();
        //            pegReader = pegCommand.ExecuteReader();
        //            if (pegReader.Read()) {
        //                pegResult = pegReader.GetString(0);
        //            }
        //            pegCommand.Connection.Close();
        //        }
        //        catch (System.Exception e) {
        //            GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetCurrentPegDBName", e.Message);
        //        }
        //    }
        //    try {
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0) {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] sanDR = dsXML.Tables["Msg"].Select("App='PORA'");
        //            if (sanDR.Length == 1) {
        //                string strSAN = sanDR[0]["Appname"].ToString();
        //                pegResult = strSAN;
        //            }
        //        }
        //    }
        //    catch (System.Exception excp) {
        //        GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetCurrentPegDBName", excp.Message);
        //    }
        //    return pegResult;
        //} //end of method
        #endregion

        //Method added 06/30/03 by Madan - for checking the connection status of Pegasys
        #region public static string CheckConnStatus(string DBName)
        public static string CheckConnStatus(string DBName) {
            System.Data.OracleClient.OracleDataReader pegReader;
            System.Data.OracleClient.OracleConnection pegConnection = new System.Data.OracleClient.OracleConnection();
            System.Data.OracleClient.OracleCommand pegCommand = new System.Data.OracleClient.OracleCommand();
            string pegResult = "";
            switch (DBName.Trim().ToLower()) {
                case "pdcreports":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasys(GSA.R7BD.Utility.DataAccess.User.r7bcquery, GSA.R7BD.Utility.DataAccess.DatabaseType.Reports, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasysReports(GSA.R7BD.Utility.DataAccess.User.r7bcquery);
                    pegCommand.CommandText = "select uidy from mf_io where rownum <2";
                    break;
                case "pegasys.world":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasys(GSA.R7BD.Utility.DataAccess.User.r7bcquery, GSA.R7BD.Utility.DataAccess.DatabaseType.Live, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasysReports(GSA.R7BD.Utility.DataAccess.User.r7bcquery);
                    pegCommand.CommandText = "select uidy from mf_po where rownum <2";
                    break;
                case "newpegoltp.world":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasys(GSA.R7BD.Utility.DataAccess.User.r7bcquery, GSA.R7BD.Utility.DataAccess.DatabaseType.Reports, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasysReports(GSA.R7BD.Utility.DataAccess.User.r7bcquery);
                    pegCommand.CommandText = "select doc_num from mf_po where rownum <2";
                    break;
                case "test4.world":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasys(GSA.R7BD.Utility.DataAccess.User.r7bcquery, GSA.R7BD.Utility.DataAccess.DatabaseType.Test, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasysReports(GSA.R7BD.Utility.DataAccess.User.r7bcquery);
                    pegCommand.CommandText = "select uidy from mf_po where rownum <2";
                    break;
                case "newpegasys.world":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasys(GSA.R7BD.Utility.DataAccess.User.r7bcquery, GSA.R7BD.Utility.DataAccess.DatabaseType.Live, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrPegasysReports(GSA.R7BD.Utility.DataAccess.User.r7bcquery);
                    pegCommand.CommandText = "select uidy from mf_po where rownum <2";
                    break;
                case "vitap.world":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrVitapOracle(GSA.R7BD.Utility.DataAccess.User.var, GSA.R7BD.Utility.DataAccess.DatabaseType.Live, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrVitapOracle(User.iuser_vitap);
                    pegCommand.CommandText = "select po_id from pegasyspo where rownum <2";
                    break;
                case "tstvitap.world":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrVitapOracle(GSA.R7BD.Utility.DataAccess.User.var, GSA.R7BD.Utility.DataAccess.DatabaseType.Test, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrVitapOracle(User.iuser_vitap);
                    pegCommand.CommandText = "select po_id from pegasyspo where rownum <2";
                    break;
                case "fmistest.world":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrFMIS("u7b0223", GSA.R7BD.Utility.DataAccess.DatabaseType.Test, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB, GSA.R7BD.Utility.DataAccess.FileSource.XML);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrFMIS(User.fmis_U7B0223);
                    pegCommand.CommandText = "select ord_doc_num  from fmisdba.peg_open_items_Summary where ord_doc_num is not null and rownum < 2";
                    break;
                case "fmis.world":
                    //pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrFMIS("u7b0223", GSA.R7BD.Utility.DataAccess.DatabaseType.Live, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB, GSA.R7BD.Utility.DataAccess.FileSource.XML);
                    pegConnection.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrFMIS(User.fmis_U7B0223);
                    pegCommand.CommandText = "select ord_doc_num  from fmisdba.peg_open_items_Summary where ord_doc_num is not null and rownum < 2";
                    break;
            }

            //Open and execute the command
            try {
                pegCommand.Connection = pegConnection;
                if (pegCommand.Connection.State != System.Data.ConnectionState.Open) {
                    pegCommand.Connection.Open();
                }
                pegReader = pegCommand.ExecuteReader();
                if (pegReader.Read()) {
                    pegResult = pegReader.GetString(0);
                }
                pegCommand.Connection.Close();
            }
            catch (System.Exception e) {
                pegResult = "ERROR";
                GSA.R7BD.Utility.EventLog.WriteEntry("Utility", "DataAccess", "CheckConnStatus", e.Message, EventLog.LogType.XMLFile);
            }
            return pegResult;

        }//end of method
        #endregion

        //Method added 07/10/03 by Madan - overloading the previous method
        #region internal static string getTestDBName()
        //internal static string getTestDBName() {
        //    string DBName = "";
        //    //string Filepath = "d:\\Components\\MSCTL64.tlb";
        //    StringBuilder FilePath = new StringBuilder();
        //    FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
        //    FilePath.Append("\\MSCTL64.tlb");
        //    try {
        //        DataSet dsXML = new DataSet();
        //        if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
        //            try {
        //                FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
        //                dsXML.ReadXml(fsXML);
        //                fsXML.Close();
        //            }
        //            catch (Exception e) {
        //                GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetCurrentPegDBName", e.Message);
        //                DBName = "ERROR";
        //            }
        //        }
        //        else {  //send this only when this file is not available
        //            DBName = "ERROR";
        //        }
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0) {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] sanDR = dsXML.Tables["Msg"].Select("App='6ORA'");
        //            if (sanDR.Length == 1) {
        //                DBName = sanDR[0]["Appname"].ToString();
        //            }
        //        }
        //    }
        //    catch (System.Exception e) {
        //        GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetCurrentPegDBName", e.Message);
        //        DBName = "ERROR";
        //    }

        //    return DBName;
        //} //end of method	
        #endregion

        //Method added 07/10/03 by Madan - overloading the previous method
        #region internal static getLiveDBName()
        //internal static string getLiveDBName() {
        //    string DBName = "";
        //    //string Filepath = "d:\\Components\\MSCTL64.tlb";
        //    StringBuilder FilePath = new StringBuilder();
        //    FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
        //    FilePath.Append("\\MSCTL64.tlb");

        //    try {
        //        DataSet dsXML = new DataSet();
        //        if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
        //            try {
        //                FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
        //                dsXML.ReadXml(fsXML);
        //                fsXML.Close();
        //            }
        //            catch (Exception e) {
        //                GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetLiveDBName", e.Message);
        //            }
        //        }
        //        else {  //send this only when this file is not available
        //            DBName = "ERROR";
        //        }
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0) {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] sanDR = dsXML.Tables["Msg"].Select("App='PPRD'");
        //            if (sanDR.Length == 1) {
        //                DBName = sanDR[0]["Appname"].ToString();
        //            }
        //        }
        //    }
        //    catch (System.Exception e) {
        //        GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetLiveDBName", e.Message);
        //    }

        //    return DBName;
        //} //end of method		
#endregion

        //Method added 07/10/03 by Madan - overloading the previous method
        #region internal static string getReportsDBName()
        //internal static string getReportsDBName() {
        //    string DBName = "";
        //    //string Filepath = "d:\\Components\\MSCTL64.tlb";
        //    StringBuilder FilePath = new StringBuilder();
        //    FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
        //    FilePath.Append("\\MSCTL64.tlb");

        //    try {
        //        DataSet dsXML = new DataSet();
        //        if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
        //            try {
        //                FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
        //                dsXML.ReadXml(fsXML);
        //                fsXML.Close();
        //            }
        //            catch (Exception e) {
        //                GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetReportsDBName", e.Message);
        //            }
        //        }
        //        else {  //send this only when this file is not available
        //            DBName = "ERROR";
        //        }
        //        //Now lets read the value from dataset
        //        if (dsXML.Tables["Msg"].Rows.Count > 0) {
        //            //check for san box entry if term =true
        //            System.Data.DataRow[] sanDR = dsXML.Tables["Msg"].Select("App='PRPT'");
        //            if (sanDR.Length == 1) {
        //                DBName = sanDR[0]["Appname"].ToString();
        //            }
        //        }
        //    }
        //    catch (System.Exception e) {
        //        GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "DataAccess", "GetReportsDBName", e.Message);
        //    }

        //    return DBName;
        //} //end of method		
        #endregion

        /**************************************************************************/
        public static string StreamExcelFile(DataTable datTable) {
            StringBuilder strExcelData = new StringBuilder();
            try {
                // Build header
                foreach (DataColumn tblCol in datTable.Columns) strExcelData.Append(tblCol.ColumnName.ToString() + "\t");
                strExcelData.Append("\r\n");
                // Build body
                foreach (DataRow tblRow in datTable.Rows) {
                    foreach (DataColumn tblCol in datTable.Columns) {
                        if (Convert.IsDBNull(tblRow[tblCol]) || tblRow[tblCol].ToString() == "") strExcelData.Append("--\t");
                        else strExcelData.Append(tblRow[tblCol].ToString() + "\t");
                        //Ron - 5/16/11 - Changed back to previous line code to display cells correctly
                        //else strExcelData.Append("='" + tblRow[tblCol].ToString() + "'\t");
                    }
                    strExcelData.Append("\r\n");
                }
            }
            catch { }
            return strExcelData.ToString();
        }
        /**************************************************************************/
        public static string StreamExcelFile(DataTable datTable, string[] straryColumnNames) {
            /***********************************************************************
                Company		: GSA
                Author		: James Bortters
                Date		: 03/2004
                Purpose		: Convert the records within into an excel file stream
                Parameters	: datTable			: Recordset
                            : straryColumnNames	: Column names to display
                Return		: Excel file stream
                Notes		: 
                Revisions	:
            ***********************************************************************/
            StringBuilder strExcelData = new StringBuilder();
            // Build header
            foreach (string strCol in straryColumnNames) strExcelData.Append(strCol + "\t");
            strExcelData.Append("\r\n");
            // Build body
            foreach (DataRow tblRow in datTable.Rows) {
                foreach (string strCol in straryColumnNames) {
                    if (Convert.IsDBNull(tblRow[strCol]) || tblRow[strCol].ToString() == "") strExcelData.Append("--\t");
                    else strExcelData.Append(tblRow[strCol].ToString() + "\t");
                    //Ron - 5/16/11 - Changed back to previous line code to display cells correctly
                    //else strExcelData.Append("='" + tblRow[strCol].ToString() + "'\t");
                }
                strExcelData.Append("\r\n");
            }
            return strExcelData.ToString();
        }

        //Madan Saini 06/28/2004 - This method should be used for reading images and returning binary data
        public static byte[] ReadBinaryFile(string strSourceFile) {
            System.IO.Stream stmFS = File.OpenRead(strSourceFile);
            byte[] bytBuffer = new byte[stmFS.Length];
            stmFS.Read(bytBuffer, 0, (int)stmFS.Length);
            stmFS.Close();
            return bytBuffer;
        }
        /**************************************************************************/
 
        public static string StreamExcelFile(DataSet datSet) {
            /***********************************************************************
                Company		: GSA
                Author		: James Bortters
                Date		: 03/2004
                Purpose		: Convert the records within into an excel file stream
                Parameters	: datSet			: Recordset
                Return		: Excel file stream
                Notes		: 
                Revisions	:
            ***********************************************************************/
            StringBuilder strExcelData = new StringBuilder();
            try {
                // Build header
                foreach (DataColumn tblCol in datSet.Tables[0].Columns) strExcelData.Append(tblCol.ColumnName.ToString() + "\t");
                strExcelData.Append("\r\n");
                // Build body
                foreach (DataRow tblRow in datSet.Tables[0].Rows) {
                    foreach (DataColumn tblCol in datSet.Tables[0].Columns) 
                        strExcelData.Append("='" + tblRow[tblCol].ToString() + "'\t");
                    strExcelData.Append("\r\n");
                }
            }
            catch { }
            return strExcelData.ToString();
        }
        /**************************************************************************/

        //Madan Saini 03/29/2004
        #region public static String ConnStrFinsOra8(string UserID, FinsOraType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN)
        //public static String ConnStrFinsOra8(string UserID, FinsOraType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN) {
        //    string ConnStr = "";
        //    string pwd = "";

        //    //Commented by Rupa Duthulur 02/06/06 The TestDBName changed as per James to tstora.test for testing purposes
        //    //string TestDBName = "tstfinsora.world";
        //    string TestDBName = "tstora.test";
        //    string LiveDBName = "finsora8.gsa.gov";
        //    //string FileLocation = "d:\\Components\\MSCTL32.tlb";
        //    StringBuilder FileLocation = new StringBuilder();
        //    FileLocation.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32:"));
        //    FileLocation.Append("\\MSCTL32.tlb");

        //    if (XmlOrSAN == FileSource.XML) {
        //        if (ChooseDatabaseType == FinsOraType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID, FileLocation.ToString());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";	//Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == FinsOraType.Test) {
        //            pwd = SharedPassword.GetPassword(TestDBName, UserID, FileLocation.ToString());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + TestDBName + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    else {
        //        if (ChooseDatabaseType == FinsOraType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword(LiveDBName.Trim(), UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + LiveDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + LiveDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == FinsOraType.Test) {
        //            pwd = SharedPassword.GetPassword(TestDBName.Trim(), UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=" + TestDBName.Trim() + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=" + TestDBName.Trim() + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    return ConnStr;
        //}  //end of this method	
        #endregion

        #region public static String ConnStrFMIS(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN)
        //Overloaded. Added by Madan - To give the choice to choose between source of the password. XML file or SAN Box
        //public static String ConnStrFMIS(string UserID, DatabaseType ChooseDatabaseType, OracleDriver OracleDriverToUse, FileSource XmlOrSAN) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    //string FileLocation = "d:\\Components\\MSCTL32.tlb";
        //    StringBuilder FileLocation = new StringBuilder();
        //    FileLocation.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32:"));
        //    FileLocation.Append("\\MSCTL32.tlb");

        //    if (XmlOrSAN == FileSource.XML) {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword("fmis.world", UserID.ToUpper(), FileLocation.ToString());
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=fmis.world" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=fmis.world" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword("fmistest.world", UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=fmistest.world" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=fmistest.world" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    else {
        //        if (ChooseDatabaseType == DatabaseType.Live) { //get password and form connection string
        //            pwd = SharedPassword.GetPassword("fmis.world", UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=fmis.world" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=fmis.world" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //        else if (ChooseDatabaseType == DatabaseType.Test) {
        //            pwd = SharedPassword.GetPassword("fmistest.world", UserID);
        //            if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //                ConnStr = "Data Source=fmistest.world" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //            }
        //            else if (OracleDriverToUse == OracleDriver.OleDB) {
        //                ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=fmistest.world" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //            }
        //        }
        //    }
        //    return ConnStr;
        //}  //end of this method  
#endregion

        #region public static String ConnStrOrabill(string UserID, OracleDriver OracleDriverToUse, FileSource XmlOrSAN)
        //public static String ConnStrOrabill(string UserID, OracleDriver OracleDriverToUse, FileSource XmlOrSAN) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    //string FileLocation = "d:\\Components\\MSCTL32.tlb";
        //    StringBuilder FileLocation = new StringBuilder();
        //    FileLocation.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32:"));
        //    FileLocation.Append("\\MSCTL32.tlb");

        //    if (XmlOrSAN == FileSource.XML) {
        //        pwd = SharedPassword.GetPassword("bdrptprd", UserID.ToLower(), FileLocation.ToString());
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=bdrpt" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=bdrpt" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //        }
        //    }
        //    else {
        //        pwd = SharedPassword.GetPassword("bdrptprd", UserID.ToLower());
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=bdrpt" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=bdrpt" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr;
        //}  //end of this method
        #endregion

        #region public static String ConnStrOrabillTest(string UserID, OracleDriver OracleDriverToUse, FileSource XmlOrSAN)
        //public static String ConnStrOrabillTest(string UserID, OracleDriver OracleDriverToUse, FileSource XmlOrSAN) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    //string FileLocation = "d:\\Components\\MSCTL32.tlb";
        //    StringBuilder FileLocation = new StringBuilder();
        //    FileLocation.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32:"));
        //    FileLocation.Append("\\MSCTL32.tlb");

        //    if (XmlOrSAN == FileSource.XML) {
        //        pwd = SharedPassword.GetPassword("bdrpttst", UserID.ToLower(), FileLocation.ToString());
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=bdrpttst" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=bdrpttst" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //        }
        //    }
        //    else {
        //        pwd = SharedPassword.GetPassword("bdrpttst", UserID.ToLower());
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=bdrpttst" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=bdrpttst" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //        }
        //    }
        //    return ConnStr;
        //}  //end of this method
        #endregion

        #region public static String ConnStrBDRPT(string UserID, DatabaseEnvironment dbEnv, OracleDriver OracleDriverToUse)
        //public static String ConnStrBDRPT(string UserID, DatabaseEnvironment dbEnv, OracleDriver OracleDriverToUse) {
        //    string ConnStr = "";
        //    string pwd = "";
        //    //string FileLocation = "d:\\Components\\MSCTL32.tlb";
        //    StringBuilder FileLocation = new StringBuilder();
        //    FileLocation.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32:"));
        //    FileLocation.Append("\\MSCTL32.tlb");

        //    //Production
        //    if (dbEnv == DatabaseEnvironment.Production) {
        //        pwd = SharedPassword.GetPassword("bdrptprd", UserID.ToLower(), FileLocation.ToString());
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=bdrpt" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=bdrpt" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //        }
        //    }
        //    //Test
        //    if (dbEnv == DatabaseEnvironment.Test) {
        //        pwd = SharedPassword.GetPassword("bdrpttst", UserID.ToLower(), FileLocation.ToString());
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=bdrpttst" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=bdrpttst" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //        }
        //    }
        //    //Development
        //    if (dbEnv == DatabaseEnvironment.Development) {
        //        pwd = SharedPassword.GetPassword("bdrptdev", UserID.ToLower(), FileLocation.ToString());
        //        if (OracleDriverToUse == OracleDriver.OraOleDB) {
        //            ConnStr = "Data Source=bdrptdev" + ";User ID=" + UserID + ";Password=" + pwd + ";Pooling=true;Connection Lifetime=1200";
        //        }
        //        else if (OracleDriverToUse == OracleDriver.OleDB) {
        //            ConnStr = "Provider=OraOLEDB.Oracle.1;Password=" + pwd + ";" + "Persist Security Info=True;User ID=" + UserID + ";" + "Data Source=bdrptdev" + ";Extended Properties=;Pooling=true;Connection Lifetime=1200";			  //Create the connection string dynamically
        //        }
        //    }            
        //    return ConnStr;
        //}  //end of this method
        #endregion

        //Madan Saini- 08/24/2004. When an App is using Open and Close Dates from Message table
        //use the following methods.
        public static bool IsOpenDateRange(string AppName) {
            bool IsOpen = false;
            //string Filepath = "d:\\Components\\MSCTL64.tlb";
            StringBuilder FilePath = new StringBuilder();
            FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL64:"));
            FilePath.Append("\\MSCTL64.tlb");

            try {
                DataSet dsXML = new DataSet();
                if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
                    FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
                    dsXML.ReadXml(fsXML);
                    fsXML.Close();
                    //Now lets read the value from dataset
                    if (dsXML.Tables["Msg"].Rows.Count > 0) {
                        //check for san box entry if term =true
                        System.Data.DataRow[] sanDR = dsXML.Tables["Msg"].Select("App='" + AppName.Trim().ToUpper() + "'");
                        if (sanDR.Length == 1) {
                            OpenAppDate = sanDR[0]["startdate"].ToString().Trim();
                            CloseAppDate = sanDR[0]["enddate"].ToString().Trim();
                            //Lets compare it to today
                            if (OpenAppDate.Length > 4 && CloseAppDate.Length > 4) {
                                if (System.DateTime.Today >= System.Convert.ToDateTime(OpenAppDate) && System.DateTime.Today <= System.Convert.ToDateTime(CloseAppDate)) {
                                    IsOpen = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception exp) { IsOpen = false; }

            return IsOpen;
        }
        
        // A read-only static property for Open and close dates
        public static string OpenDate {
            get {
                return OpenAppDate;
            }
        }//end of this property

        public static string CloseDate {
            get {
                return CloseAppDate;
            }
        }//end of this property

        // A read-only static property for CloseStartDate and CloseEndDate
        public static string CloseStartDate
        {
            get
            {
                return CloseAppStartDate;
            }
        }// end of this property

        public static string CloseEndDate
        {
            get
            {
                return CloseAppEndDate;
            }
        }// end of this property

        //Madan 08/10/2009 - This method will be used to provide list of fiscal years
        public static DataTable getFiscalYears(int LowestYear) {
            DataTable listFY = new DataTable();
            int currFY;            
            try {
                listFY.Columns.Add("FYText",String.Empty.GetType());
                listFY.Columns.Add("FYVal", String.Empty.GetType());
                //find out the current Fiscal Year
                if (DateTime.Now.Month > 9) { currFY = DateTime.Now.Year + 1; }
                else { currFY = DateTime.Now.Year; }
                listFY.Rows.Add("Current FY", currFY);
                for (int i = currFY-1; i >= LowestYear; i--) {
                    listFY.Rows.Add("FY "+ i, i);                    
                }
                listFY.AcceptChanges();            
            }
            catch (Exception exp) { }
            return listFY;        
        }

        //Guru 04/13/2011 - Get Application Configuration values
        //Parameters - Application Name, Config Name
        public static string getConfigValue(string p_app_name, string p_config_name)
        {
            //Use Oracle function to get the value
            //Oracle Connection Settings
            OracleConnection myBDFAConn = new OracleConnection();
            OracleCommand myBDFACmd = new OracleCommand();

            OracleParameter return_val = new OracleParameter("ret_val", OracleType.VarChar, 200);
            OracleParameter p_appname = new OracleParameter("p_app_name", OracleType.VarChar, 50);
            OracleParameter p_configname = new OracleParameter("p_config_name", OracleType.VarChar, 100);

            string cvalue = null;

            try
            {
                myBDFAConn.ConnectionString = ConnStrBDFApps();

                p_appname.Value = p_app_name.Trim();
                p_configname.Value = p_config_name.Trim();
                return_val.Direction = ParameterDirection.ReturnValue;

                myBDFACmd.Parameters.Clear();
                myBDFACmd.Connection = myBDFAConn;
                myBDFACmd.CommandText = "BDFAPPS.f_get_config_value";
                myBDFACmd.CommandType = CommandType.StoredProcedure;
                myBDFACmd.Parameters.Clear();
                myBDFACmd.Parameters.Add(p_appname);
                myBDFACmd.Parameters.Add(p_configname);
                myBDFACmd.Parameters.Add(return_val);

                if (myBDFAConn.State != ConnectionState.Open) { myBDFAConn.Open(); }
                myBDFACmd.ExecuteScalar();

                cvalue = myBDFACmd.Parameters["ret_val"].Value.ToString();

                return cvalue;
            }
            catch (Exception exp)
            {
                //Return the error message
                cvalue = "ERROR:" + exp.Message;
                return cvalue;
            }
            finally
            {
                // Close the connection when done with it.
                if (myBDFAConn.State == ConnectionState.Open)
                    myBDFAConn.Close();
            }
        }

       
        
        
        /// <summary>
        /// Jun Lee 11/14/2012 
        /// Get Customer Service Email From BDFAPPS.SYS_APP_CONFIG.
        /// </summary>
        /// <param></param>        
        /// <returns>string</returns>
        public static string GetCustomerServiceEmail()
        {
            // Prepare Connection and Command object for Oracel Database
            OracleConnection connBDFApps = new OracleConnection();
            OracleCommand cmdBDFApps = new OracleCommand();

            string customerServiceEmail;

            try
            {
                // Write SQL to retrieve the email
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Config_Value ");
                sql.Append("FROM BDFAPPS.Sys_App_Config ");
                sql.Append("WHERE Config_App = 'VITAP' ");
                sql.Append("AND Config_Name = 'FW_CUSTOMER_SERVICE_EMAIL'");

                // Get Connection String and assign to Connection object: connBDFApps
                connBDFApps.ConnectionString = ConnStrBDFApps().Trim();

                // Setup Command object: cmdBDFApps
                cmdBDFApps.CommandText = sql.ToString();
                cmdBDFApps.Connection = connBDFApps;

                // Open connection
                connBDFApps.Open();
                // Execute query
                customerServiceEmail = cmdBDFApps.ExecuteScalar().ToString();
            }
            catch (Exception exp)
            {
                customerServiceEmail = "ERROR:" + exp.Message;
            }
            // Close the connection
            finally
            {
                connBDFApps.Close();
            }
            return customerServiceEmail;
        }

        /// <summary>
        /// OCP 50815 by Jun Lee 12/05/2012
        /// Get Client Service Email for Finance Homepage From BDFAPPS.SYS_APP_CONFIG.
        /// </summary>
        /// <param></param>        
        /// <returns>string</returns>
        public static string GetClientServiceEmail()
        {
            // Prepare Connection and Command object for Oracel Database
            OracleConnection connBDFApps = new OracleConnection();
            OracleCommand cmdBDFApps = new OracleCommand();

            string customerServiceEmail;

            try
            {
                // Write SQL to retrieve the email
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Config_Value ");
                sql.Append("FROM BDFAPPS.Sys_App_Config ");
                sql.Append("WHERE Config_App = 'VITAP' ");
                sql.Append("AND Config_Name = 'FW_CLIENT_SERVICE_EMAIL'");

                // Get Connection String and assign to Connection object: connBDFApps
                connBDFApps.ConnectionString = ConnStrBDFApps().Trim();

                // Setup Command object: cmdBDFApps
                cmdBDFApps.CommandText = sql.ToString();
                cmdBDFApps.Connection = connBDFApps;

                // Open connection
                connBDFApps.Open();
                // Execute query
                customerServiceEmail = cmdBDFApps.ExecuteScalar().ToString();
            }
            catch (Exception exp)
            {
                customerServiceEmail = "ERROR:" + exp.Message;
            }
            // Close the connection
            finally
            {
                connBDFApps.Close();
            }
            return customerServiceEmail;
        }


        public static string GetCaamIssoEmail()
        {
            // Prepare Connection and Command object for Oracel Database
            OracleConnection connBDFApps = new OracleConnection();
            OracleCommand cmdBDFApps = new OracleCommand();

            string caamIssoEmail;

            try
            {
                // Write SQL to retrieve the email
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Config_Value ");
                sql.Append("FROM BDFAPPS.Sys_App_Config ");
                sql.Append("WHERE Config_App = 'CAAM' ");
                sql.Append("AND Config_Name = 'CAAM_ISSO_EMAIL'");

                // Get Connection String and assign to Connection object: connBDFApps
                connBDFApps.ConnectionString = ConnStrBDFApps().Trim();

                // Setup Command object: cmdBDFApps
                cmdBDFApps.CommandText = sql.ToString();
                cmdBDFApps.Connection = connBDFApps;

                // Open connection
                connBDFApps.Open();
                // Execute query
                caamIssoEmail = cmdBDFApps.ExecuteScalar().ToString();
            }
            catch (Exception exp)
            {
                caamIssoEmail = "ERROR:" + exp.Message;
            }
            // Close the connection
            finally
            {
                connBDFApps.Close();
            }
            return caamIssoEmail;
        }

        //public static string GetCustomerServiceEmail()
        //{
        //    return getConfigValue("VITAP", "NOT_EMAIL_PBS");

        //}

        // Added from decompiled dll
        static DataAccess()
        {
            filePath = "";
            OpenAppDate = "";
            CloseAppDate = "";
            CloseAppStartDate = "";
            CloseAppEndDate = "";
        }

        // Added from decompiled dll
        public static string IsLeaseEaOpen()
        {
            Exception exception;
            string s = "";
            string str2 = "";
            string errDesc = "OPEN";
            OracleConnection connection = new OracleConnection();
            OracleCommand command = new OracleCommand();
            try
            {
                DateTime time;
                DateTime time2;
                connection = new OracleConnection();
                command = new OracleCommand();
                connection.ConnectionString = ConnStrBDFApps();
                command.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='LEASEEA' and CONFIG_NAME='AVAILDATE'";
                command.Connection = connection;
                command.Connection.Open();
                OracleDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    s = reader["CONFIG_VALUE"].ToString().Trim();
                }
                command.CommandText = "select CONFIG_VALUE from BDFAPPS.sys_app_config where CONFIG_APP='LEASEEA'and CONFIG_NAME='CUTOFFDATE'";
                OracleDataReader reader2 = command.ExecuteReader();
                while (reader2.Read())
                {
                    str2 = reader2["CONFIG_VALUE"].ToString().Trim();
                }
                try
                {
                    time = DateTime.Parse(s, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None);
                }
                catch (Exception exception1)
                {
                    exception = exception1;
                    EventLog.AddWebErrors("Leaseea", "GSA.R7BC.Utility", "IsLeaseEaOpen", "The AvailDate has an invalid format");
                    return "The AvailDate has an invalid format";
                }
                try
                {
                    time2 = DateTime.Parse(str2, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None);
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    EventLog.AddWebErrors("Leaseea", "GSA.R7BC.Utility", "IsLeaseEaOpen", "The CutoffDate has an invalid format");
                    return "The Cutoff Date has an invalid format";
                }
                if (time2 < time)
                {
                    errDesc = "The Cutoff Date must be later than the availability date";
                    EventLog.AddWebErrors("Leaseea", "GSA.R7BC.Utility", "IsLeaseEaOpen", errDesc);
                }
                if ((time <= DateTime.Now) && (DateTime.Now <= time2))
                {
                    errDesc = "OPEN";
                }
                else
                {
                    errDesc = "The application is only avilable from ";
                    errDesc = errDesc + time.ToShortDateString();
                    errDesc = errDesc + " to ";
                    errDesc = errDesc + time2.ToShortDateString();
                }
            }
            catch (Exception exception3)
            {
                exception = exception3;
                errDesc = "ERROR:" + exception.Message;
                EventLog.AddWebErrors("Leaseea", "GSA.R7BC.Utility", "IsLeaseEaOpen", errDesc);
            }
            finally
            {
                connection.Close();
            }
            return errDesc;
        }

    }
}
