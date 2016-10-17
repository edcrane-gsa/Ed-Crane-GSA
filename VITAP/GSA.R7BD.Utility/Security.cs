using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.Data.OleDb;
using GSA.R7BD.Utility;
using System.Data.OracleClient;
//using System.Runtime.InteropServices;
using System.IO;
using System.DirectoryServices;
using System.Linq;
using System.Configuration;

// Change history:
// 8/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility {
    public partial class Security : Component {
        public enum ServerEnvironment { Development = 1, Test = 2, Production = 3 };

        public Security() {
            InitializeComponent();
        }
        public Security(IContainer container) {
            container.Add(this);

            InitializeComponent();
        }

        // 03/07/2014 OCP 57515 RSNAP II: Jun Lee
        // Using Password Web Service, this routine returns the password for the database and user id passed 
        public static string GetPassword(string databaseName, string userID) {

            return SharedPassword.GetPassword(databaseName, userID);

            //string passwordResult = "";
            //string strDefaultPath = "";

            //strDefaultPath = GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32:");
            //strDefaultPath += "\\MSCTL32.TLB";
            //try {	//get it from xml file
            //    //passwordResult = SharedPassword.GetPassword(databaseName.Trim(), userID.Trim(), "d:\\components\\MSCTL32.TLB");
            //    passwordResult = SharedPassword.GetPassword(databaseName.Trim(), userID.Trim(), strDefaultPath);
            //    //if not found in XML file then get ftom SAN box
            //    if (passwordResult == "") {
            //        passwordResult = SharedPassword.GetPassword(databaseName.Trim(), userID.Trim());
            //    }
            //}
            //catch (System.Exception e) {
            //    Console.Write(e.Message);
            //}
            //return passwordResult;
        } //end of this method 

        //to find out from which table if doc type is give
        public static string FindTable(string docType) {
            if (docType.Length > 12) {
                string DCType = (docType.Trim().Substring(11, 2));
                //Select Case DCType
                switch (DCType) {
                    case ("FP"):
                    case ("TP"):
                    case ("GP"):
                    case ("PP"):
                    case ("XP"):
                    case ("OP"):
                    case ("1B"):
                    case ("2A"):
                    case ("4B"):
                    case ("FX"):
                    case ("GX"):
                    case ("IX"):
                    case ("OX"):
                    case ("PT"):
                        return "MF_PO";
                    case ("FO"):
                    case ("TO"):
                    case ("GO"):
                    case ("PO"):
                    case ("XO"):
                    case ("OO"):
                    case ("1E"):
                    case ("2D"):
                    case ("FZ"):
                    case ("GZ"):
                    case ("IT"):
                    case ("OZ"):
                        return "MF_IO";
                    case ("FW"):
                    case ("TW"):
                    case ("GW"):
                    case ("PW"):
                    case ("XW"):
                    case ("OW"):
                    case ("FY"):
                    case ("GY"):
                    case ("OY"):
                    case ("PY"):
                        return "MF_TG";
                    default:
                        return "";
                } //end of switch 				    
            }     //end of if statement	
            else
                return "";
        } //end of method

        public static string findBookMonth(string Acpd_id) {
            switch (Acpd_id) {
                case "00":
                    return "PY";
                case "01":
                    return "10";
                case "02":
                    return "11";
                case "03":
                    return "12";
                case "04":
                    return "01";
                case "05":
                    return "02";
                case "06":
                    return "03";
                case "07":
                    return "04";
                case "08":
                    return "05";
                case "09":
                    return "06";
                case "10":
                    return "07";
                case "11":
                    return "08";
                case "12":
                    return "09";
                default:
                    return "";
            } //end of switch
        }  //end of method		

        //This method is used to verify that user is a 
        //part of Lotus Notes- table (m:\ccmail\maildir.dbf)
        //7/17/2003 - Madan - Pointing the table to Oracle - VITAP.world
        //Input parameter UserName = First Name Initial, LastName
        //public static bool IsLotusNotesUser(string firstName, string lastName, System.Data.OracleClient.OracleConnection App_Connection) {
        //    string Message = "";
        //    string UserName1 = firstName.Trim().ToUpper() + " " + lastName.Trim().ToUpper();
        //    string UserName2 = lastName.Trim().ToUpper() + ", " + firstName.Trim().ToUpper();
        //    try {
        //        System.Data.OracleClient.OracleCommand CmdLotusMail = new System.Data.OracleClient.OracleCommand();
        //        CmdLotusMail.CommandText = "SELECT Notesname from maildir WHERE Notesname = '" + Utilities.SqlEncode(UserName1.ToUpper().Trim()) + "' or upper(Name) ='" + Utilities.SqlEncode(UserName2) + "'";
        //        CmdLotusMail.Connection = App_Connection;  //ConnLotusMail;
        //        if (App_Connection.State != System.Data.ConnectionState.Open) {
        //            CmdLotusMail.Connection.Open();
        //        }
        //        System.Data.OracleClient.OracleDataReader myReader = CmdLotusMail.ExecuteReader();
        //        if (myReader.Read()) {
        //            Message = myReader["Notesname"].ToString().Trim();
        //        }
        //        myReader.Close();
        //        CmdLotusMail.Connection.Close();
        //    }
        //    catch (System.Exception ex) { // if exception occurs, make an entry in the server event log
        //        EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "IsLutusNotesUser", ex.Message);

        //    }
        //    if (Message != "") {
        //        return true;
        //    }
        //    else {
        //        return false;
        //    }
        //}  //end of this method


        //This method finds out if the user is a valid one in -- Users table at m:\oitems
        public static bool IsValidUser(string firstName, string lastName) {
            string Result = "";
            try {
                System.Data.OleDb.OleDbConnection ConnUser = new System.Data.OleDb.OleDbConnection();
                System.Data.OleDb.OleDbCommand CmdUser = new System.Data.OleDb.OleDbCommand();
                ConnUser.ConnectionString = ConnectionString.CSUsersTables();
                CmdUser.CommandText = Queries.getValidUser(firstName, lastName);
                CmdUser.Connection = ConnUser;
                CmdUser.Connection.Open();
                OleDbDataReader myReader = CmdUser.ExecuteReader();
                if (myReader.Read()) {
                    Result = myReader["fname"].ToString().Trim();
                }
                myReader.Close();
                CmdUser.Connection.Close();
            }
            catch (System.Exception ex) {
                Console.Write(ex.Message + " " + ex.Source);
            }
            if (Result != "") {
                return true;
            }
            else {
                return false;
            }

        }  //end of this method

        //This method will be used for validating the DFAS passwords for new User Accounts
        public static string ValidatePassword(string Password) {
            string IsValid = "";
            int CSPLCHARS = 1;
            int CNUMCHARS = 1;
            int SPLFOUND = 0;
            int NUMFOUND = 0;
            char[] arrChars = new char[] { '#', '$', '%', '^', '@' };
            string SpecialChars = "#$%^@";
            char[] arrNum = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            string NumerChars = "0123456789";
            try {
                //check password length
                if (Password.Length < 8 || Password.Length > 12) { IsValid = "Password must be at least 8-12 characters long.<br />"; }
                //check special characters

                if (Password.IndexOfAny(arrChars) == -1) { IsValid = IsValid + "Password must contain at least one of these ('#','$','%','^','@') special Characters.<br />"; }
                else { //find out how many special characters
                    for (int i = 0; i < Password.Length; i++) {
                        string StrChar = Password.Substring(i, 1);
                        if (SpecialChars.IndexOf(StrChar) != -1) {
                            SPLFOUND = SPLFOUND + 1;
                        }
                    }
                    if (SPLFOUND != CSPLCHARS) { IsValid = IsValid + "Password must contain " + CSPLCHARS.ToString() + " special characters.<br />"; }
                }

                //Check Numeric
                if (Password.IndexOfAny(arrNum) == -1) { IsValid = IsValid + "Password must contain at least one numeric (0,1,2,3,4,5,6,7,8,9) Character.<br />"; }
                else {//find out how many special chars
                    for (int i = 0; i < Password.Length; i++) {
                        string StrChar = Password.Substring(i, 1);
                        if (NumerChars.IndexOf(StrChar) != -1) {
                            NUMFOUND = NUMFOUND + 1;
                        }
                    }
                    if (NUMFOUND != CNUMCHARS) { IsValid = IsValid + "Password must contain " + CNUMCHARS.ToString() + " numeric characters."; }
                }
            }
            catch (System.Exception exp) {
                Console.Write(exp.Message);
            }
            return IsValid.Trim();
        }//end of this method
        #region Web_users membership

        // Added by Leo
        // This method validate an Internet user's membership of a web application
        public static bool IsValidUser(string loginID, string password, string appName, System.Data.OracleClient.OracleConnection conVITAP) {
            bool valid = false;
            string encodedPassword = SharedPassword.Encode(password);
            loginID = loginID.ToUpper();

            System.Data.OracleClient.OracleCommand cmdVITAP = new System.Data.OracleClient.OracleCommand();
            if (conVITAP.State != ConnectionState.Open) {
                conVITAP.Open();
            }
            cmdVITAP.Connection = conVITAP;
            cmdVITAP.CommandText = Queries.getValidUser(loginID, encodedPassword, appName);
            System.Data.OracleClient.OracleDataReader rdrReader = cmdVITAP.ExecuteReader();
            if (rdrReader.Read()) {
                valid = true;
            }
            else {
                valid = false;
            }

            cmdVITAP.Dispose();

            return valid;
        } //end of this method

        // Madan Saini 03/29/2004 -- Clear Text Passwords for PMT
        // This method validate an Internet user's membership of a web application
        public static bool IsDBUserValid(string loginID, string password, string appName, System.Data.OracleClient.OracleConnection conVITAP) {
            bool valid = false;
            //string encodedPassword = SharedPassword.Encode(password);
            loginID = loginID.ToUpper();

            System.Data.OracleClient.OracleCommand cmdVITAP = new System.Data.OracleClient.OracleCommand();
            if (conVITAP.State != ConnectionState.Open) {
                conVITAP.Open();
            }
            cmdVITAP.Connection = conVITAP;
            cmdVITAP.CommandText = Queries.getValidUser(loginID, password, appName);
            System.Data.OracleClient.OracleDataReader rdrReader = cmdVITAP.ExecuteReader();
            if (rdrReader.Read()) {
                valid = true;
            }
            else {
                valid = false;
            }

            cmdVITAP.Dispose();

            return valid;
        } //end of this method


        public static bool IsPassowrdExpired(string loginID, System.Data.OracleClient.OracleConnection conVITAP) {
            bool valid = false;
            System.Data.OracleClient.OracleCommand cmdVITAP = new System.Data.OracleClient.OracleCommand();
            if (conVITAP.State != ConnectionState.Open) {
                conVITAP.Open();
            }
            cmdVITAP.Connection = conVITAP;
            cmdVITAP.CommandText = "";
            System.Data.OracleClient.OracleDataReader rdrReader = cmdVITAP.ExecuteReader();

            return valid;
        }

        #endregion
        //To find out if user is Internal IP or External - using FoxPro Table
        //public static bool IsUserIPInternal(string IPAddress) {
        //    string Result = "";
        //    try {
        //        System.Data.OleDb.OleDbConnection ConnIP = new System.Data.OleDb.OleDbConnection();
        //        System.Data.OleDb.OleDbCommand CmdIP = new System.Data.OleDb.OleDbCommand();
        //        ConnIP.ConnectionString = ConnectionString.CSIPInternal();  //"Provider=VFPOLEDB.1;Data Source=c:\\data;Database='';Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE";
        //        CmdIP.CommandText = Queries.getIPInternal();  //"Select * from Int_IP";
        //        CmdIP.Connection = ConnIP;
        //        CmdIP.Connection.Open();
        //        OleDbDataReader myReader = CmdIP.ExecuteReader();
        //        while (myReader.Read()) {
        //            string strExt_IP = myReader["IP3"].ToString().Trim();
        //            int FoundVal = IPAddress.IndexOf(strExt_IP);
        //            if (FoundVal != -1) {  //found 
        //                Result = "Found";
        //                break;
        //            }
        //        }
        //    }
        //    catch (System.Exception ex) {
        //        EventLog.WriteEntry("GSA.R7BD.Utility", "Security", "IsUserIPInternal", ex.Message, EventLog.LogType.XMLFile);
        //    }
        //    if (Result == "Found") {
        //        return true;
        //    }
        //    else {
        //        return false;
        //    }
        //}//end of this method

        //To find out if user is Internal IP or External - using Oracle Table - Orabill
        public static bool IsUserIPInternalOracle(string IPAddress) {
            string Result = "";
            DataTable myResults = new DataTable();
            OracleConnection myConn = new OracleConnection();
            OracleCommand myCmd = new OracleCommand();
            //myConn.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrOrabill("iuser_bart",GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB, GSA.R7BD.Utility.DataAccess.FileSource.XML);
            myConn.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrBDRPT(DataAccess.User.iuser_bart);        
            try {
                if (myConn.State != ConnectionState.Open) { myConn.Open(); }
                OracleDataAdapter myAdapter = new OracleDataAdapter("SELECT IPADDR FROM webappobj.ipaddresslist WHERE IPSTATUS='A'", myConn);
                myCmd.Connection = myConn;
                myCmd.CommandType = CommandType.Text;
                myAdapter.Fill(myResults);
                myAdapter.Dispose();
                if (myResults.Rows.Count > 0) {
                    foreach (DataRow row in myResults.Rows) {
                        string strExt_IP = row["IPADDR"].ToString().Trim();
                        int FoundVal = IPAddress.IndexOf(strExt_IP);
                        if (FoundVal != -1) {  //found 
                            Result = "Found";
                            break;
                        }
                    }
                }
            }
            catch (System.Exception ex) {
                EventLog.WriteEntry("GSA.R7BD.Utility", "Security", "IsUserIPInternalOracle", ex.Message, EventLog.LogType.XMLFile);
            }
            finally { if (myConn.State != ConnectionState.Closed) { myConn.Close(); } myConn.Dispose(); myCmd.Dispose(); }

            if (Result == "Found") {return true;}
            else {return false;}
        }//end of this method

        //To find out if user is Internal IP or External - using Oracle Table - TSTORA/Orabill
        public static bool IsUserIPInternalOracle(string IPAddress,ServerEnvironment ServerType) {
            string Result = "";
            DataTable myResults = new DataTable();
            OracleConnection myConn = new OracleConnection();
            OracleCommand myCmd = new OracleCommand();
            
            //if (ServerType == ServerEnvironment.Production) { myConn.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrOrabill("iuser_bart", GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB, GSA.R7BD.Utility.DataAccess.FileSource.XML); }
            //else { myConn.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrOrabillTest("iuser_bart", GSA.R7BD.Utility.DataAccess.OracleDriver.OraOleDB, GSA.R7BD.Utility.DataAccess.FileSource.XML); }
            myConn.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrBDRPT(DataAccess.User.iuser_bart); 

            try {
                if (myConn.State != ConnectionState.Open) { myConn.Open(); }
                OracleDataAdapter myAdapter = new OracleDataAdapter("SELECT IPADDR FROM webappobj.ipaddresslist WHERE IPSTATUS='A'", myConn);
                myCmd.Connection = myConn;
                myCmd.CommandType = CommandType.Text;
                myAdapter.Fill(myResults);
                myAdapter.Dispose();
                if (myResults.Rows.Count > 0) {
                    foreach (DataRow row in myResults.Rows) {
                        string strExt_IP = row["IPADDR"].ToString().Trim();
                        int FoundVal = IPAddress.IndexOf(strExt_IP);
                        if (FoundVal != -1) {  //found 
                            Result = "Found";
                            break;
                        }
                    }
                }
            }
            catch (System.Exception ex) {
                EventLog.WriteEntry("GSA.R7BD.Utility", "Security", "IsUserIPInternalOracle", ex.Message, EventLog.LogType.XMLFile);
            }
            finally { if (myConn.State != ConnectionState.Closed) { myConn.Close(); } myConn.Dispose(); myCmd.Dispose(); }

            if (Result == "Found") { return true; }
            else { return false; }
        }//end of this method

        //OCP - 46096, RatnaM added 04/23/2012
        //To check if the user's ip is in exclude list in source file
        public static bool IsIpInExcludeList(string userIp)
        {
            DataSet ds = new DataSet();
            bool isIpInExclude = false;
            try
            {
                //source file - get directory from Environment variable
                //string sourceFile = System.Environment.GetEnvironmentVariable("FINANCECONFIG");

                string sourceFile = ConfigurationManager.AppSettings["FINANCECONFIG"];
                if (sourceFile == null)
                { 
                    sourceFile = "D:\\financeconfig\\"; 
                }

                if (!sourceFile.EndsWith("\\"))
                {
                    sourceFile += "\\";
                }
                sourceFile += "exclude_ip.xml";
                if (System.IO.File.Exists(sourceFile))
                {
                    //read xml and populate in datasource
                    ds.ReadXml(sourceFile);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string iprange = dr["key"].ToString();
                        string[] range = iprange.Split('-');
                        string fromIP = range[0].ToString().Trim();
                        string toIP = range[1].ToString().Trim();
                        //split FromIP, ToIP and user's IP into 4 octets
                        int[] splitFromIp = fromIP.Split('.').Select(p => Convert.ToInt32(p)).ToArray();
                        int[] splitToIp = toIP.Split('.').Select(p => Convert.ToInt32(p)).ToArray();
                        int[] splitUserIp = userIp.Split('.').Select(p => Convert.ToInt32(p)).ToArray();
                        //Compare the Octets to see if user's IP falls within the range
                        if (splitUserIp[0] >= splitFromIp[0] && splitUserIp[0] <= splitToIp[0])
                        {
                            if (splitUserIp[1] >= splitFromIp[1] && splitUserIp[1] <= splitToIp[1])
                            {
                                if (splitUserIp[2] >= splitFromIp[2] && splitUserIp[2] <= splitToIp[2])
                                {
                                    if (splitUserIp[3] >= splitFromIp[3] && splitUserIp[3] <= splitToIp[3])
                                    {
                                        isIpInExclude = true;
                                    }
                                }
                            }
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("GSA.R7BD.Utility", "Security", "IsIpInExcludeList", ex.Message, EventLog.LogType.XMLFile);
            }
            return isIpInExclude;
        }

        public static void RegisterUser(string firstName, string LastName, string Phone, string Initials, string OrgCode, string Corr) {
            //System.DateTime DateAdded = System.DateTime.Today; 
            if (Initials.Length == 0) Initials = "";
            if (OrgCode.Length == 0) OrgCode = "";
            if (Corr.Length == 0) Corr = "";
            if (Phone.Length == 0) Phone = "";

            try {
                System.Data.OleDb.OleDbConnection ConnReg = new System.Data.OleDb.OleDbConnection();
                System.Data.OleDb.OleDbCommand CmdReg = new System.Data.OleDb.OleDbCommand();
                ConnReg.ConnectionString = ConnectionString.CSUsersTables(); //"Provider=VFPOLEDB.1;Data Source=c:\\data;Database='';Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE";
                CmdReg.CommandText = Queries.RegisterNewUser(firstName, LastName, Phone, Initials, OrgCode, Corr);  //"INSERT into Users (Fname,Lname,Phone,Initials,Orgcode,Corr,Dateadded) values ('" + firstName + "','" +  LastName + "','" + Phone + "','" + Initials + "','" + OrgCode + "','" + Corr + "', Date())";
                CmdReg.Connection = ConnReg;
                CmdReg.Connection.Open();
                CmdReg.ExecuteNonQuery();
            }
            catch (System.Exception ex) {
                Console.Write(ex.Message);

            }

        }//end of method

        // Addded by Madan on 02/21/2003 For Letting the website pull another page is Pegasys is down.
        public static string CheckApplicationStatus(String WebsiteName) {
            string strMemo = "Pegasys is Down";
            try {
                System.Data.OleDb.OleDbConnection ConnWeb = new System.Data.OleDb.OleDbConnection();
                System.Data.OleDb.OleDbCommand CmdWeb = new System.Data.OleDb.OleDbCommand();
                ConnWeb.ConnectionString = ConnectionString.CSforPasswordsTable();
                CmdWeb.CommandText = Queries.getWebsiteSite(WebsiteName);
                CmdWeb.Connection = ConnWeb;
                CmdWeb.Connection.Open();
                OleDbDataReader myReader = CmdWeb.ExecuteReader();
                if (myReader.Read()) {
                    string strTerm = myReader["Term"].ToString().Trim();
                    if (strTerm == "True") {
                        strMemo = myReader["Message"].ToString().Trim();
                    }
                    else { // if termimnate is false, means everything is ok, send empty string.
                        strMemo = "";
                    }
                }
                else {
                    strMemo = "";
                }
            }
            catch (System.Exception ex) {
                Console.Write(ex.Message);
            }
            return strMemo;
        }	//end of method


        public static void SendEmail(string From, string To, string CC, string Bcc, string Mailname, string Subject, string NoteText, string Attach) {
            try {
                System.Data.OleDb.OleDbConnection ConnReg = new System.Data.OleDb.OleDbConnection();
                System.Data.OleDb.OleDbCommand CmdReg = new System.Data.OleDb.OleDbCommand();
                ConnReg.ConnectionString = ConnectionString.CSMailList(); 
                CmdReg.CommandText = Queries.email(From, To, CC, Bcc, Mailname, Subject, NoteText, Attach);
                CmdReg.Connection = ConnReg;
                CmdReg.Connection.Open();
                CmdReg.ExecuteNonQuery();
            }
            catch (System.Exception ex) {
                Console.Write(ex.Message);
            }
        }//end of method	

        public static string AuthenticateLotusNotesUser(string firstName, string lastName, System.Data.OracleClient.OracleConnection App_Connection) {
            string Message = "";
            string UserName1 = firstName.Trim().ToUpper() + " " + lastName.Trim().ToUpper();
            string UserName2 = lastName.Trim().ToUpper() + ", " + firstName.Trim().ToUpper();
            try {
                System.Data.OracleClient.OracleCommand CmdLotusMail = new System.Data.OracleClient.OracleCommand();
                CmdLotusMail.CommandText = "SELECT Notesname from maildir WHERE Notesname = '" + Utilities.SqlEncode(UserName1.ToUpper().Trim()) + "' or upper(Name) ='" + Utilities.SqlEncode(UserName2) + "'";
                CmdLotusMail.Connection = App_Connection;// ConnLotusMail; 
                if (App_Connection.State != System.Data.ConnectionState.Open) {
                    CmdLotusMail.Connection.Open();
                }
                System.Data.OracleClient.OracleDataReader NotesReader = CmdLotusMail.ExecuteReader();
                if (NotesReader.Read()) {
                    Message = NotesReader["Notesname"].ToString().Trim();
                }
                NotesReader.Close();
                CmdLotusMail.Connection.Close();
            }
            catch (System.Exception ex) { // if exception occurs, make an entry in the server event log
                EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "IsLutusNotesUser", ex.Message);
            }
            return Message;
        }//end of method 

        public static string getDecodedPwd(string EncodedPwd) {
            return SharedPassword.Decode(EncodedPwd);
        }

        //MS 07/14/2004 - This method will be used to read a Log File and return a file containing a particular string
        //Returns path of the New file
        public static string ParseIISLogFile(string FileLocation, string FileName, string ParamSearch) {
            string FullName = FileLocation.Trim() + "\\" + FileName.Trim();
            string NewFile = "C:\\inetpub\\wwwroot\\testapp\\" + "New_" + FileName.Substring(0, 6) + ".txt";
            //System.IO.File.CreateText(NewFile);   
            System.IO.StreamWriter myWriter = new StreamWriter(NewFile);
            //open the file for Read
            System.IO.StreamReader myReader = new StreamReader(FullName);

            try {
                if (System.IO.File.Exists(FullName)) {
                    string Line = myReader.ReadLine();
                    //if the line contains paramSearch add it to new file
                    while (Line != null) {
                        if (Line.ToUpper().IndexOf(ParamSearch.ToUpper()) != -1) {
                            myWriter.WriteLine(Line);
                        }
                    }

                    //flush the contents
                    myWriter.Flush();
                }
            }

            catch (System.Exception exp) { }

            finally {
                myReader.Close();
                myWriter.Close();
            }

            return NewFile;

        }

        //2/09/2005 - Madan - web_users table in VITAP.world
        //Input parameter UserName = First Name Initial, LastName
        public static string IsValidWebUser(string UserName, string Password, System.Data.OracleClient.OracleConnection App_Connection) {
            string Message = "";
            try {
                System.Data.OracleClient.OracleCommand CmdLotusMail = new System.Data.OracleClient.OracleCommand();
                CmdLotusMail.CommandText = "SELECT password from web_users WHERE (externalservice ='T' or app_dfas_fl ='T') and upper(login_id) = '" + Utilities.SqlEncode(UserName.ToUpper().Trim()) + "'";
                CmdLotusMail.Connection = App_Connection;  //ConnLotusMail;
                if (App_Connection.State != System.Data.ConnectionState.Open) {
                    CmdLotusMail.Connection.Open();
                }
                System.Data.OracleClient.OracleDataReader myReader = CmdLotusMail.ExecuteReader();
                if (myReader.Read()) {
                    Message = myReader["password"].ToString().Trim();
                }
                myReader.Close();
                CmdLotusMail.Connection.Close();
            }
            catch (System.Exception ex) { // if exception occurs, make an entry in the server event log
                EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "IsLutusNotesUser", ex.Message);

            }
            return Message.Trim();
        }  //end of this method

        //public static string ADAuthentication(string UserName, string Password) {
        //    string userEmail="";
        //    try {                
        //        DirectoryEntry de = new DirectoryEntry("LDAP://ent.ds.gsa.gov/DC=ent,DC=ds,DC=gsa,DC=gov", "ENT\\" + UserName.Trim(), Password.Trim(), AuthenticationTypes.Secure);
        //        DirectorySearcher ds = new DirectorySearcher(de);
        //        ds.ReferralChasing = ReferralChasingOption.All;
        //        ds.SearchScope = SearchScope.Subtree;
        //        ds.Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName.Trim() + "))";
        //        ds.PropertiesToLoad.Add("sAMAccountName"); //1. ENT Account
        //        ds.PropertiesToLoad.Add("mail"); //2. EMail
        //        ds.PropertiesToLoad.Add("name"); //3. FullName

        //        Hashtable associateDetailsTable = new Hashtable();
        //        ResultPropertyValueCollection resultCollection;
        //        SearchResult myUsers = ds.FindOne();
        //        //User is found in the Active Directory
        //        if (myUsers != null && myUsers.Properties.Values.Count > 0) {
        //            //1. LoginID
        //            resultCollection = myUsers.Properties["sAMAccountName"];
        //            foreach (object result in resultCollection) { associateDetailsTable.Add("LoginID", result.ToString()); }
        //            //2. Email
        //            resultCollection = myUsers.Properties["mail"];
        //            foreach (object result in resultCollection) { associateDetailsTable.Add("Email", result.ToString()); }
        //            //3. Name
        //            resultCollection = myUsers.Properties["name"];
        //            foreach (object result in resultCollection) { associateDetailsTable.Add("Name", result.ToString()); }
        //            userEmail = associateDetailsTable["Email"].ToString().Trim().ToLower() + "/" + associateDetailsTable["LoginID"].ToString().Trim().ToUpper() + "/" + associateDetailsTable["Name"].ToString().Trim().ToUpper();
        //        }               
        //        return userEmail;            
        //    }
        //    catch (Exception exp) {
        //        userEmail = "ERROR";
        //        EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "ADAuthentication", "User - " + UserName.Trim() + " , Error - " + exp.Message);
        //        return userEmail;
        //    }
        //}//end of method

        //public static bool isMemberOfADGroup(string UserName, string Password, string GroupName) {
        //    bool userFound = false;
        //    try {
        //        DirectoryEntry de = new DirectoryEntry("LDAP://ent.ds.gsa.gov/DC=ent,DC=ds,DC=gsa,DC=gov", "ENT\\" + UserName.Trim(), Password.Trim(), AuthenticationTypes.Secure);
        //        DirectorySearcher ds = new DirectorySearcher(de);
        //        ds.ReferralChasing = ReferralChasingOption.All;
        //        ds.SearchScope = SearchScope.Subtree;
        //        ds.Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName.Trim() + "))";
        //        ds.PropertiesToLoad.Add("sAMAccountName"); //1. ENT 
        //        ds.PropertiesToLoad.Add("MemberOf"); //2. Membership                
        //        SearchResult myUsers = ds.FindOne();

        //        //User is found in the Active Directory
        //        if (myUsers != null && myUsers.Properties.Values.Count > 0) {
        //            int NumberOfGroups = myUsers.Properties["memberOf"].Count - 1;
        //            string tempString = "";
        //            while (NumberOfGroups >= 0) {
        //                tempString = myUsers.Properties["MemberOf"][NumberOfGroups].ToString();
        //                tempString = tempString.Substring(0, tempString.IndexOf(",", 0)).Replace("CN=", "") ;
        //                //Above we set tempString to the first index of "," starting from the zeroth element of itself.
        //                //tempString = tempString.Replace("CN=", "") ;
        //                //Above, we remove the "CN=" from the beginning of the string 
        //               // tempString = tempString.ToLower(); //'Lets make all letters lowercase
        //               // tempString = tempString.Trim();  //Finnally, we trim any blank characters from the edges                 
        //                if (GroupName.ToLower().Trim() == tempString.Trim().ToLower()) { userFound = true; break; }
        //                //If we have a match, the return is true username is a member of grouptoCheck  
        //                NumberOfGroups = NumberOfGroups - 1; 
        //            } 
        //        }               
        //    }
        //    catch (Exception exp) {
        //        EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "isMemberOfADGroup", "User - " + UserName.Trim() + " , Error - " + exp.Message);
        //    }
        //    return userFound;

        //}//end of method

        //public static string[] getGroupMemberships(string UserName, string Password) {
        //    string[] userMemberships = null;
        //    try {
        //        DirectoryEntry de = new DirectoryEntry("LDAP://ent.ds.gsa.gov/DC=ent,DC=ds,DC=gsa,DC=gov", "ENT\\" + UserName.Trim(), Password.Trim(), AuthenticationTypes.Secure);
        //        DirectorySearcher ds = new DirectorySearcher(de);
        //        ds.ReferralChasing = ReferralChasingOption.All;
        //        ds.SearchScope = SearchScope.Subtree;
        //        ds.Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName.Trim() + "))";               
        //        ds.PropertiesToLoad.Add("MemberOf"); //1. Membership                
        //        SearchResult myUsers = ds.FindOne();

        //        //User is found in the Active Directory
        //        if (myUsers != null && myUsers.Properties.Values.Count > 0) {
        //            int NumberOfGroups = myUsers.Properties["memberOf"].Count;
        //            userMemberships = new string[NumberOfGroups];
        //            string tempString = "";
        //            for (int i = 0; i < NumberOfGroups; i++) {
        //                tempString = myUsers.Properties["MemberOf"][i].ToString();
        //                tempString = tempString.Substring(0, tempString.IndexOf(",", 0)).Replace("CN=", "");
        //                userMemberships[i] = tempString.Trim().ToUpper();
        //            }
        //        }
        //    }
        //    catch (Exception exp) {
        //        EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "isMemberOfADGroup", "User - " + UserName.Trim() + " , Error - " + exp.Message);
        //    }
        //    return userMemberships;

        //}//end of method





        //public static string N2NAuthentication(string NotesName, string InternetPassword) {
        //    string AuthName = "";
        //    Proposion.N2N.NsfConnection myConnection = new Proposion.N2N.NsfConnection();
        //    try {
        //        string connstr2 = "database='names.nsf'; server='GEMS-07-02/R07/GSA/GOV'; IniFile=; password='12345678'; Authname='" + NotesName.Trim().Replace("'", "\\'") + "' ; AuthPassword='" + InternetPassword.Trim() + "'";
        //        myConnection.ConnectionString = connstr2;
        //        myConnection.Open();
        //        //if the connection opens here means successful and can return				
        //        AuthName = myConnection.AuthenticatedUser.CanonicalName.Trim();
        //        AuthName = AuthName.Substring(3, AuthName.IndexOf("/") - 3);
        //    }
        //    catch (System.Exception e) {
        //        AuthName = "ERROR";
        //        EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "N2NAuthentication", "User - " + NotesName.Trim() + " , Error - " + e.Message);
        //    }
        //    finally { myConnection.Close(); }

        //    return AuthName.Trim();
        //}//end of method   

        //public static string N2NAuthEmailBack(string NotesName, string InternetPassword) {
        //    string Email = ""; string AuthName = "";
        //    Proposion.N2N.NsfConnection myConnection = new Proposion.N2N.NsfConnection();
        //    try {
        //        string connstr2 = "database='names.nsf'; server='GEMS-07-02/R07/GSA/GOV'; IniFile=; password='12345678'; Authname='" + NotesName.Trim().Replace("'", "\\'") + "' ; AuthPassword='" + InternetPassword.Trim() + "'";
        //        myConnection.ConnectionString = connstr2;
        //        myConnection.Open();
        //        AuthName = myConnection.AuthenticatedUser.CanonicalName.Trim();
        //        AuthName = AuthName.Substring(3, AuthName.IndexOf("/") - 3);
        //        string commandString = "SELECT InternetAddress FROM _People WHERE FORMULA([fullname=\'" + myConnection.AuthenticatedUser.CanonicalName.Trim().Replace("'", "\\\\'") + "\'])";
        //        Proposion.N2N.NsfCommand myCommand = new Proposion.N2N.NsfCommand(commandString, myConnection);
        //        Proposion.N2N.NsfDataReader myReader = myCommand.ExecuteReader();
        //        if (myReader.Read()) {
        //            Email = myReader["InternetAddress"].ToString().Trim();
        //        }
        //        myReader.Close();
        //        myReader.Dispose();
        //        Email = Email + "/" + AuthName;
        //    }
        //    catch (System.Exception e) {
        //        Email = "ERROR";
        //        EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "N2NAuthEmailBack", "User - " + NotesName.Trim() + " , Error - " + e.Message);
        //    }
        //    finally { myConnection.Close(); }
        //    return Email;
        //}//end of method   

        ////This method will give firstname + Last name as saved in Notes database. Would take 60-90 seconds.
        ////Madan Saini 06\13\2006 - On special Request from Suresh.
        //public static string N2NAuthFullName(string NotesName, string InternetPassword) {
        //    string FullName = "";
        //    Proposion.N2N.NsfConnection myConnection = new Proposion.N2N.NsfConnection();
        //    try {
        //        string connstr2 = "database='names.nsf'; server='GEMS-07-02/R07/GSA/GOV'; IniFile=; password='12345678'; Authname='" + NotesName.Trim().Replace("'", "\\'") + "' ; AuthPassword='" + InternetPassword.Trim() + "'";
        //        myConnection.ConnectionString = connstr2;
        //        myConnection.Open();
        //        FullName = myConnection.AuthenticatedUser.CanonicalName.Trim();
        //        //				string commandString = "SELECT FirstName,LastName,middlename, InternetAddress FROM _People WHERE FORMULA([fullname=\'" + myConnection.AuthenticatedUser.CanonicalName.Trim() + "\'])" ;
        //        //				Proposion.N2N.NsfCommand myCommand = new Proposion.N2N.NsfCommand(commandString, myConnection);
        //        //				Proposion.N2N.NsfDataReader myReader = myCommand.ExecuteReader();
        //        //				if(myReader.Read()){
        //        //					if(myReader["FirstName"]==null){FullName = myReader["FirstName"].ToString().Trim() + " " + myReader["LastName"].ToString().Trim();}
        //        //					else{FullName = myReader["FirstName"].ToString().Trim() + " " + myReader["MiddleName"].ToString().Trim()+ ". " + myReader["LastName"].ToString().Trim();}
        //        //				}
        //        //				myReader.Close(); 
        //        //				myReader.Dispose(); 
        //    }
        //    catch (System.Exception e) {
        //        FullName = "ERROR";
        //        EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "N2NAuthEmailBack", "User - " + NotesName.Trim() + " , Error - " + e.Message);
        //    }
        //    finally { myConnection.Close(); }
        //    return FullName.Trim();
        //}//end of method   

        ////Madan Saini 09\12\2007 - This method will be used for saving the Request Access Details directly into Notes Database .nsf
        //public static string N2NSaveRequestData(string NotesName, string InternetPassword) {
        //    string FullName = "";
        //    Proposion.N2N.NsfConnection myConnection = new Proposion.N2N.NsfConnection();
        //    try {
        //        string connstr2 = "database='names.nsf'; server='GEMS-07-02/R07/GSA/GOV'; IniFile=; password='12345678'; Authname='" + NotesName.Trim().Replace("'", "\\'") + "' ; AuthPassword='" + InternetPassword.Trim() + "'";
        //        myConnection.ConnectionString = connstr2;
        //        myConnection.Open();
        //        string commandString = "INSERT INTO ";
        //        Proposion.N2N.NsfCommand myCommand = new Proposion.N2N.NsfCommand(commandString, myConnection);
        //        myCommand.ExecuteNonQuery();
        //    }
        //    catch (System.Exception e) {
        //        FullName = "ERROR";
        //        EventLog.AddWebErrors("GSA.R7BD.Utility", "Security", "N2NAuthEmailBack", "User - " + NotesName.Trim() + " , Error - " + e.Message);
        //    }
        //    finally { myConnection.Close(); }
        //    return "SUCCESS";
        //}//end of method   


    }
}
