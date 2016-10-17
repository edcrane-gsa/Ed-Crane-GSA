using System;
using System.Data.OracleClient;
using System.Data;
//using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using System.Security.Permissions;
using System.Security;
using GSA.R7BD.Utility;

// Change history:
// 8/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility {
    public class EventLog {
        public enum LogType { EventViewer = 1, XMLFile = 2 };
        //This routine will write an error to the event log
        public static void WriteEntry(string AppName, string ComponentName, string MethodName, string ErrDesc, System.Diagnostics.EventLogEntryType EventType) {
            System.Diagnostics.EventLog AppEventLog = new System.Diagnostics.EventLog("Application");
            string Message = "Message from web App - " + AppName.Trim().ToUpper() + " : Component - " + ComponentName.Trim().ToUpper() + " : Method - " + MethodName.Trim() + " : Error Desc - " + ErrDesc.Trim();
            try {	//Write the error to the event log
                AppEventLog.Source = "7BD Web Applications";
                AppEventLog.WriteEntry(Message, EventType);
            }
            catch (System.Exception ex) {
                AddWebErrors(AppName, ComponentName, MethodName, ex.Message);
            }
            finally {
                AppEventLog.Dispose();
            }
        }	 //end of this method

        //This routine will write an error to the event log
        public static void WriteEntry(string AppName, string ComponentName, string MethodName, string ErrDesc) {
            System.Diagnostics.EventLog AppEventLog = new System.Diagnostics.EventLog("Application");
            string Message = "Message from web App - " + AppName.Trim().ToUpper() + " : Component - " + ComponentName.Trim().ToUpper() + " : Method - " + MethodName.Trim() + " : Error Desc - " + ErrDesc.Trim();
            try {
                //Write the error to the event log
                AppEventLog.Source = "7BD Web Applications";
                AppEventLog.WriteEntry(Message, System.Diagnostics.EventLogEntryType.Error);
            }
            catch (System.Exception ex) {
                AddWebErrors(AppName, ComponentName, MethodName, ex.Message);
            }
            finally {
                AppEventLog.Dispose();
            }
        }	 //end of this method

        public static void WriteEntry(string AppName, string ComponentName, string MethodName, string ErrDesc, GSA.R7BD.Utility.EventLog.LogType TypeOfLog) {
            System.Diagnostics.EventLog AppEventLog = new System.Diagnostics.EventLog("Application");
            string Message = "Message from web App - " + AppName.Trim().ToUpper() + " : Component - " + ComponentName.Trim().ToUpper() + " : Method - " + MethodName.Trim() + " : Error Desc - " + ErrDesc.Trim();
            try {
                if (TypeOfLog == LogType.EventViewer) {
                    //Write the error to the event log
                    AppEventLog.Source = "7BD Web Applications";
                    AppEventLog.WriteEntry(Message, System.Diagnostics.EventLogEntryType.Error);
                }
                else {//write to XML FILE
                    AddWebErrors(AppName, ComponentName, MethodName, ErrDesc);
                }

            }
            catch (System.Exception ex) {
                AddWebErrors(AppName, ComponentName, MethodName, ex.Message);
            }
            finally {
                AppEventLog.Dispose();
            }
        }	 //end of this method

        //Madan Saini - 03/18/2004 This method will write to the Oracle Table - Web_error_log
        //public static void WriteEntry(GSA.R7BD.Utility.DataAccess.DatabaseType ConnType, string AppName, string LoggedUser, string ErrDesc) {
        //    OracleConnection myConn = new OracleConnection();
        //    OracleCommand myCmd = new OracleCommand();
        //    string ServerName = System.Environment.MachineName.ToUpper().Trim();
        //    try {
        //        myConn.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrVitapOracle("iuser_auditlog", ConnType, GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB);
        //        myCmd.CommandText = "Insert into web_error_log (error_date,app_name,server_name,login_id,err_description) values (sysdate,'" + AppName.ToUpper().Trim() + "','" + ServerName + "','" + LoggedUser.ToUpper().Trim() + "','" + ErrDesc.Trim() + "')";
        //        myCmd.Connection = myConn;
        //        if (myConn.State != System.Data.ConnectionState.Open) { myConn.Open(); }
        //        myCmd.ExecuteNonQuery();
        //    }
        //    catch (System.Exception ex) {
        //        AddWebErrors(AppName, "Utility", "WriteEntry", ex.Message);
        //    }
        //    finally {
        //        myConn.Close();
        //        myConn.Dispose();
        //    }
        //}	 //end of this method
        public static void WriteEntry(string AppName, string LoggedUser, string ErrDesc)
        {
            OracleConnection myConn = new OracleConnection();
            OracleCommand myCmd = new OracleCommand();
            string ServerName = System.Environment.MachineName.ToUpper().Trim();
            try
            {
                myConn.ConnectionString = DataAccess.ConnStrVitapOracle(DataAccess.User.iuser_audit);  
                myCmd.CommandText = "Insert into web_error_log (error_date,app_name,server_name,login_id,err_description) values (sysdate,'" + AppName.ToUpper().Trim() + "','" + ServerName + "','" + LoggedUser.ToUpper().Trim() + "','" + ErrDesc.Trim() + "')";
                myCmd.Connection = myConn;
                if (myConn.State != System.Data.ConnectionState.Open) { myConn.Open(); }
                myCmd.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                AddWebErrors(AppName, "Utility", "WriteEntry", ex.Message);
            }
            finally
            {
                myConn.Close();
                myConn.Dispose();
            }
        }

        //This rountine writes log entry to web_log table in vitap database
        public static void WriteWebLog(string appName, string allProcess, string clientIP, string userID, System.Data.OracleClient.OracleConnection conVITAP) {
            System.Data.OracleClient.OracleCommand cmdVITAP = new System.Data.OracleClient.OracleCommand();
            if (conVITAP.State != ConnectionState.Open) {conVITAP.Open();}
            cmdVITAP.Connection = conVITAP;
            cmdVITAP.CommandText = Queries.newWebLog(appName, allProcess, clientIP, userID);
            cmdVITAP.ExecuteNonQuery();
        }


        //Madan 09/29/03 - This method will be used to write the Error Logs to a XML file on disk
       public static void AddWebErrors(string AppName, string ComponentName, string MethodName, string ErrDesc) {
            DataSet dsXML = new ERRORS();
            string dstPath = "";
                
            dstPath = Utilities.getDrivePathByLetter("ERRORLOG:");
            dstPath+= "\\WebErrorLog.xml";

            try {
                // Create a file permission set indicating all of this method's intentions.
                FileIOPermission fp = new FileIOPermission(FileIOPermissionAccess.AllAccess, Path.GetFullPath(dstPath));
                fp.AddPathList(FileIOPermissionAccess.Write | FileIOPermissionAccess.Append, Path.GetFullPath(dstPath));

                // Verify that we can be granted all the permissions we'll need.
                fp.Demand();

                // Assert the desired permissions here.
                fp.Assert();

                if (System.IO.File.Exists(dstPath)) {
                    dsXML.ReadXml(dstPath);
                }
                DataRow Row = dsXML.Tables["ErrorHistory"].NewRow();
                object[] myRowArray = new object[6];
                myRowArray[0] = System.Environment.MachineName;
                myRowArray[1] = AppName.Trim();
                myRowArray[2] = MethodName.Trim();
                myRowArray[3] = ComponentName.Trim();
                myRowArray[4] = ErrDesc.Trim();
                myRowArray[5] = System.DateTime.Now;
                Row.ItemArray = myRowArray;
                dsXML.Tables["ErrorHistory"].Rows.Add(Row);
                dsXML.AcceptChanges();
                dsXML.WriteXml(dstPath);
            }
            catch (System.Exception Excp) {
                Console.Write(Excp.Message);
            }
            finally {
                dsXML.Dispose();
            }
        }

        //Madan Saini 09/26/2006 - This Method will be used for Logging the successful Logins from Notes Login Screen.
        //Starting with - PegOiReview.
        public static void RecordLogin(string Username, string Email, string IPAddress, string WebsiteName) {
            DataSet dsXML = new ERRORS();
            string dstPath = "";

            dstPath = Utilities.getDrivePathByLetter("ERRORLOG:");
            dstPath += "\\UserHistory.xml";

            try {
                // Create a file permission set indicating all of this method's intentions.
                FileIOPermission fp = new FileIOPermission(FileIOPermissionAccess.AllAccess, Path.GetFullPath(dstPath));
                fp.AddPathList(FileIOPermissionAccess.Write | FileIOPermissionAccess.Append, Path.GetFullPath(dstPath));
                // Verify that we can be granted all the permissions we'll need.
                fp.Demand();
                // Assert the desired permissions here.
                fp.Assert();
                //Now add the record to xml table for History
                if (System.IO.File.Exists(dstPath)) {
                    dsXML.ReadXml(dstPath);
                    DataRow Row = dsXML.Tables["UserLogins"].NewRow();
                    object[] myRowArray = new object[5];
                    myRowArray[0] = Username.Trim().ToUpper();
                    myRowArray[1] = Email.Trim().ToLower();
                    myRowArray[2] = IPAddress.Trim();
                    myRowArray[3] = System.DateTime.Now;
                    myRowArray[4] = WebsiteName.Trim();
                    Row.ItemArray = myRowArray;
                    dsXML.Tables["UserLogins"].Rows.Add(Row);
                    dsXML.AcceptChanges();
                    dsXML.WriteXml(dstPath);
                }
            }
            catch (System.Exception ex) {
                GSA.R7BD.Utility.EventLog.WriteEntry("EventLog.cs", "GSA.R7BD.POIRBiz", "RecordLogin", "Error Occurred in RecordLogin procedure for User :" + Username + " email: " + Email + ". The exception is: " + ex.Source + ex.Message, GSA.R7BD.Utility.EventLog.LogType.XMLFile);
            }
        }//end of this method
    }
}
