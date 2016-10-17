using System;
using System.Data;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Text;

// Change history:
// 8/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility {
    class SharedPassword {

        // 03/07/2014 OCP 57515 RSNAP II: Jun Lee
        // Using Password Web Service, this routine returns the password for the database and user id passed 
        protected internal static string GetPassword(string databaseName, string userID) {

            string dbPwd = "";
            try
            {
                //srDBPassword.ServiceClient myService = new srDBPassword.ServiceClient();

                srDBPassword.ServiceClient myService = new srDBPassword.ServiceClient();
                dbPwd = myService.GetData(userID.Trim().ToLower(), databaseName.Trim().ToLower(), "a0c88e69-4489-4bac-b563-b2ab63526223");
                myService.Close();
            }
            catch (Exception exp)
            {
                dbPwd = "ERROR: " + exp.Message;
                EventLog.AddWebErrors("R7BD.Utility", "SharedPassword", "GetPassword", exp.Message + " " + userID.ToString() + "' in Database '" + databaseName.ToString() + "'");
            }
            return dbPwd.Trim();

            //string passwordResult = "";
            //StringBuilder connStr = new StringBuilder();
            ////string Filepath = "d:\\components\\MSCTL32.tlb";
            //StringBuilder FilePath = new StringBuilder();
            //FilePath.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MSCTL32"));
            //FilePath.Append("\\MSCTL32.tlb"); 
            //try {
            //    DataSet dsXML = new DataSet();
            //    if (System.IO.File.Exists(FilePath.ToString())) {	//file exists)				
            //        try {
            //            FileStream fsXML = new FileStream(FilePath.ToString(), FileMode.Open, FileAccess.Read);
            //            dsXML.ReadXml(fsXML);
            //            fsXML.Close();
            //            }
            //        catch (Exception e) {
            //            Console.Write(e.Message);
            //        }
            //    }
            //    //Now lets read the value from dataset
            //    if (dsXML.Tables["Stramboli"].Rows.Count > 0) {
            //        System.Data.DataRow[] DR = dsXML.Tables["Stramboli"].Select("Request='" + XMLEncode(userID.Trim()) + "' and Visit='" + XMLEncode(databaseName.Trim()) + "'");
            //        if (DR.Length == 1) {
            //            passwordResult = DR[0]["Hits"].ToString();
            //            passwordResult = XMLDecode(passwordResult);
            //            passwordResult = Decode(passwordResult);
            //        }
            //    }
            //    if (passwordResult == "")
            //    {
            //        EventLog.AddWebErrors("R7BD.Utility", "SharedPassword", "GetPassword", "Did not find password for userID '" + userID.ToString() + "' in Database '" + databaseName.ToString() + "'");
            //    }

            //}
            //catch (System.Exception e) {
            //    Console.Write(e.Message);
            //}
            ////if password is empty in XML file then look at SANBox
            //if (passwordResult.Length == 0) {
            //    System.Data.OleDb.OleDbDataReader passwordReader;
            //    System.Data.OleDb.OleDbConnection passwordConnection = new System.Data.OleDb.OleDbConnection();
            //    System.Data.OleDb.OleDbCommand passwordCommand = new System.Data.OleDb.OleDbCommand();

            //    //Set connection / command information  			
            //    //passwordConnection.ConnectionString = "Provider=VFPOLEDB.1;Data Source=M:\\finance;Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE";
            //    connStr.Append("Provider=VFPOLEDB.1;Data Source=");
            //    connStr.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("PASSWORDSTABLE:"));
            //    connStr.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            //    passwordConnection.ConnectionString = connStr.ToString(); 
            //    passwordCommand.CommandText = "select password from passwords.dbf where upper(database) == '" + databaseName.Trim().ToUpper() + "' and upper(userid) == '" + userID.Trim().ToUpper() + "'";
            //    //Open and execute the command
            //    try {
            //        passwordCommand.Connection = passwordConnection;
            //        passwordCommand.Connection.Open();
            //        passwordReader = passwordCommand.ExecuteReader();

            //        if (passwordReader.Read()) {
            //            passwordResult = Decode(passwordReader.GetString(0));
            //        }

            //        passwordCommand.Connection.Close();
            //    }

            //    catch (System.Exception e) {
            //        Console.Write(e.Message);
            //    }
            //}

            //return passwordResult;

        } //end of this method

        //This routine is copied from VITAP_E1 (VB6)
        protected internal static string Decode(string strPassword) {
            return PasswordEncryption(strPassword.Trim(), "decode");
        }

        //This routine will copied from VITAP_E1 (VB6)
        protected internal static string Encode(string strPassword) {
            return PasswordEncryption(strPassword.Trim(), "encode");
        }

        //This routine will copied from VITAP_E1 (VB6)
        private static string PasswordEncryption(string strTheWord, string strAction) {
            string strOrigWord;
            int i;
            int intTheFactor;
            strOrigWord = strTheWord.Trim();
            strTheWord = "";

            for (i = 1; i <= strOrigWord.Length; i++) {
                switch (i) {
                    case 1:
                        intTheFactor = -3;
                        break;
                    case 2:
                        intTheFactor = -4;
                        break;
                    case 3:
                        intTheFactor = 4;
                        break;
                    case 4:
                        intTheFactor = -5;
                        break;
                    case 5:
                        intTheFactor = 6;
                        break;
                    case 6:
                        intTheFactor = 1;
                        break;
                    case 7:
                        intTheFactor = -1;
                        break;
                    case 8:
                        intTheFactor = -2;
                        break;
                    case 9:
                        intTheFactor = 3;
                        break;
                    case 10:
                        intTheFactor = 5;
                        break;
                    default:
                        intTheFactor = -2;
                        break;
                }   //end of switch

                if (strAction == "encode") {
                    //strTheWord = strTheWord & Chr(Asc(Mid(strOrigWord, i, 1)) + intTheFactor);   --- original
                    char[] mychar = (strOrigWord.Substring(i - 1, 1)).ToCharArray();
                    char x = mychar[0];
                    int test = (int)x + intTheFactor;
                    char mytestword = (char)test;
                    strTheWord = strTheWord + mytestword.ToString();
                }
                else {
                    //strTheWord = strTheWord & Chr(Asc(Mid(strOrigWord, i, 1)) + intTheFactor);  -------original	
                    char[] mychar = (strOrigWord.Substring(i - 1, 1)).ToCharArray();
                    char x = mychar[0];
                    int test = (int)x - intTheFactor;
                    char mytestword = (char)test;
                    strTheWord = strTheWord + mytestword.ToString();
                }

            }  //end of for Loop

            return strTheWord;

        } //end of this method

        //This method will provide database name currently used by App - from Message Table.
        //protected internal static string getDataBaseName() {
        //    System.Data.OleDb.OleDbDataReader DBNReader;
        //    System.Data.OleDb.OleDbConnection DBNConnection = new System.Data.OleDb.OleDbConnection();
        //    System.Data.OleDb.OleDbCommand DBNCommand = new System.Data.OleDb.OleDbCommand();
        //    string DBNResult = "";
        //    //Set connection / command information
        //    DBNConnection.ConnectionString = "Provider=VFPOLEDB.1;Data Source=\\\\e07bds-san\\vol1\\finance;Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE";
        //    DBNCommand.CommandText = "SELECT appname from message.dbf where app == 'PORA'";
        //    //Open the connection and execute it
        //    try {
        //        DBNCommand.Connection = DBNConnection;
        //        DBNCommand.Connection.Open();
        //        DBNReader = DBNCommand.ExecuteReader();
        //        if (DBNReader.Read()) {
        //            DBNResult = DBNReader.GetString(0);
        //        }
        //    }
        //    catch (System.Exception excp) {
        //        Debug.Write(excp.Message + "," + excp.Source);
        //    }
        //    finally {
        //        DBNCommand.Connection.Close();
        //    }
        //    //retrun the database name
        //    return DBNResult;
        //}

        //This routine returns the password for the database and user id passed
        public static string GetPassword(string databaseName, string userID, string Filepath) {
            string passwordResult = "";
            try {
                DataSet dsXML = new DataSet();
                if (System.IO.File.Exists(Filepath.Trim())) {	//file exists)				
                    try {
                        FileStream fsXML = new FileStream(Filepath.Trim(), FileMode.Open, FileAccess.Read);
                        dsXML.ReadXml(fsXML);
                        fsXML.Close();
                    }
                    catch (Exception e) {
                        Console.Write(e.Message);
                    }
                }
                //Now lets read the value from dataset
                if (dsXML.Tables["Stramboli"].Rows.Count > 0) {
                    System.Data.DataRow[] DR = dsXML.Tables["Stramboli"].Select("Request='" + XMLEncode(userID.Trim()) + "' and Visit='" + XMLEncode(databaseName.Trim()) + "'");
                    if (DR.Length == 1) {
                        passwordResult = DR[0]["Hits"].ToString();
                        passwordResult = XMLDecode(passwordResult);
                        passwordResult = Decode(passwordResult);
                    }
                }
            }
            catch (System.Exception e) {
                Console.Write(e.Message);
            }
            return passwordResult;
        } //end of this method

        //These two method will be used for decoding XMLEncoding into digits in XML files on the server.
        public static string XMLDecode(string strWord) {
            return XMLDecryption(strWord.Trim());
        }

        private static string XMLDecryption(string strTheWord) {
            string strOrigWord;
            int i;
            strOrigWord = strTheWord.Trim();
            strTheWord = "";

            for (i = 1; i <= strOrigWord.Length; i = i + 3) {
                //strTheWord = strTheWord & Chr(Asc(Mid(strOrigWord, i, 1)) + intTheFactor);  -------original	
                string mychar = (strOrigWord.Substring(i - 1, 3));
                int x = System.Convert.ToInt32(mychar);
                x = x - 3;
                char y = (char)x;
                string test = y.ToString();
                //char mytestword = (char) test;
                strTheWord = strTheWord + test;//.ToString();				
            }  //end of for Loop
            return strTheWord;
        } //end of this method

        public static string XMLEncode(string strWord) {
            return XMLEncryption(strWord.Trim());
        }

        private static string XMLEncryption(string strTheWord) {
            string strOrigWord;
            int i;
            strOrigWord = strTheWord.Trim();
            strTheWord = "";

            for (i = 1; i <= strOrigWord.Length; i++) {
                //strTheWord = strTheWord & Chr(Asc(Mid(strOrigWord, i, 1)) + intTheFactor);   --- original
                char[] mychar = (strOrigWord.Substring(i - 1, 1)).ToCharArray();
                char x = mychar[0];
                int test = (int)x + 3;
                string ToBeAdded = "";
                if (test < 100) {
                    ToBeAdded = "0" + test.ToString();
                }
                else {
                    ToBeAdded = test.ToString();
                }
                strTheWord = strTheWord + ToBeAdded;//.ToString();								
            }  //end of for Loop
            return strTheWord;
        } //end of this method


    }
}
