using System;
using System.Collections.Generic;
using System.Text;

// Change history:
// 8/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility {
    class Queries {
        //add here different queries here
        //protected internal static string getPassword(string databaseName, string userID) {
        //    StringBuilder sSQL = new StringBuilder();
        //    sSQL.Append("select password from passwords.dbf ");
        //    sSQL.Append(" where upper(database) == '" + databaseName.Trim().ToUpper() + "' and upper(userid) == '" + userID.Trim().ToUpper() + "'");
        //    return sSQL.ToString();
        //}

        protected internal static string getWebsiteSite(string websiteName) {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("select Message, Term from Message.dbf ");
            sSQL.Append(" where upper(App) == '" + websiteName.Trim().ToUpper() + "'");
            return sSQL.ToString();
        }

        //protected internal static string getLotusNotesName(string UserName) {
        //    StringBuilder sSQL = new StringBuilder();
        //    sSQL.Append("SELECT Notesname from maildir ");
        //    sSQL.Append(" where upper(Notesname) = '" + UserName.ToUpper().Trim() + "'");
        //    return sSQL.ToString();
        //}

        protected internal static string getValidUser(string firstName, string lastName) {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("Select fname from Users.dbf ");
            sSQL.Append(" WHERE upper(Fname) == '" + firstName.Trim().ToUpper() + "' AND upper(Lname) == '" + lastName.Trim().ToUpper() + "'");
            return sSQL.ToString();
        }

        protected internal static string getValidUser(string loginID, string encodedPassword, string appName) {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("select password ");
            sSQL.Append("from web_users ");
            sSQL.Append("where login_id = '" + loginID + "' ");
            sSQL.Append("and password = '" + encodedPassword + "' ");
            sSQL.Append("and " + appName + " = 'T'");
            return sSQL.ToString();
        }

        protected internal static string getPasswordDate(string loginID) {
            StringBuilder sSQL = new StringBuilder();
            return sSQL.ToString();
        }

        //protected internal static string getIPInternal() {
        //    StringBuilder sSQL = new StringBuilder();
        //    sSQL.Append("Select * from Int_IP");
        //    return sSQL.ToString();
        //}

        protected internal static string RegisterNewUser(string firstName, string LastName, string Phone, string Initials, string OrgCode, string Corr) {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("INSERT into Users (Fname,Lname,Phone,Initials,Orgcode,Corr,Dateadded) ");
            sSQL.Append("values ('" + firstName + "','" + LastName + "','" + Phone + "','" + Initials + "','" + OrgCode + "','" + Corr + "', Date())");
            return sSQL.ToString();
        }

        protected internal static string email(string From, string To, string CC, string Bcc, string Mailname, string Subject, string NoteText, string Attach) {
            string sSQL = "";
            sSQL = "INSERT into Maillist (From,To,CC,Bcc,Mailname,Subject,HTML,Notetext,Attach,Sent,Key_date, Key_time, send_date, send_time, out) ";
            sSQL = sSQL + " values ('" + From.Trim() + "','" + To.Trim() + "','" + CC.Trim() + "','" + Bcc.Trim() + "','" + Mailname.Trim() + "','" + Subject.Trim() + "', .F. ,'" + NoteText + "','" + Attach.Trim() + "',.F. , date(), time(), {//},'',.f.)";
            //sSQL.Append("INSERT into Maillist (From,To,CC,Bcc,Mailname,Subject,HTML,Notetext,Attach,Sent,Key_date, Key_time) ");			
            //sSQL.Append(" values ('" + From.Trim() + "','" +  To.Trim() + "','" + CC.Trim() + "','" + Bcc.Trim() + "','" + Mailname.Trim() + "','" + Subject.Trim() + "', .F. ,'" + NoteText.Trim() + "','" + Attach.Trim() + "',.F. , date(), time())"); 

            return sSQL;
        }

        protected internal static string newWebLog(string appName, string allProcess, string clientIP, string userID) {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("insert into Web_log(allprocess, transdate, user_ip, user_id, app_name)");
            sSQL.Append(" values('" + allProcess + "', sysdate, '" + clientIP + "', '" + userID + "', '" + appName + "')");

            return sSQL.ToString();
        }
    }
}
