using System;
using System.Collections.Generic;
using System.Text;


// Change history:
// 8/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility {
    public class ConnectionString {
        protected internal static string CSforPasswordsTable() {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("Provider=VFPOLEDB.1;Data Source=");
            sSQL.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("PASSWORDSTABLE"));
            sSQL.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            //sSQL.Append("Provider=VFPOLEDB.1;Data Source=m:\\finance\\;Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            return sSQL.ToString();
        }

        //protected internal static string CSforLotusNotesTable() {
        //    StringBuilder sSQL = new StringBuilder();
        //    //sSQL.Append("Provider=VFPOLEDB.1;Data Source=m:\\ccmail;Database='';Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
        //    sSQL.Append("Provider=VFPOLEDB.1;Data Source=");
        //    sSQL.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("LOTUSNOTESTABLE"));
        //    sSQL.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
        //    return sSQL.ToString();
        //}

        protected internal static string CSUsersTables() {
            StringBuilder sSQL = new StringBuilder();
            //sSQL.Append("Provider=VFPOLEDB.1;Data Source=c:\\data;Database='';Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            sSQL.Append("Provider=VFPOLEDB.1;Data Source=");
            sSQL.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("USERSTABLE"));
            sSQL.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            return sSQL.ToString();
        }

        //protected internal static string CSIPInternal() {
        //    StringBuilder sSQL = new StringBuilder();
        //    //sSQL.Append("Provider=VFPOLEDB.1;Data Source=c:\\data;Database='';Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
        //    sSQL.Append("Provider=VFPOLEDB.1;Data Source=");
        //    sSQL.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("IPINTERNALTABLE"));
        //    sSQL.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
        //    return sSQL.ToString();
        //}

        public static string CSMailDir() {
            StringBuilder sSQL = new StringBuilder();
            //sSQL.Append("Provider=VFPOLEDB.1;Data Source=M:\\Vautorun\\data;Database='';Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            sSQL.Append("Provider=VFPOLEDB.1;Data Source=");
            sSQL.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("CCMAILDIR:"));
            sSQL.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            return sSQL.ToString();
        }

        public static string CSMailList()
        {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("Provider=VFPOLEDB.1;Data Source=");
            sSQL.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("MAILLIST:"));      
            sSQL.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            return sSQL.ToString(); 
        }

        public static string CSAddrBook()
        {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("Provider=VFPOLEDB.1;Data Source=");
            sSQL.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("ADDRBOOK:"));
            sSQL.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            return sSQL.ToString(); 
        }

        public static string CSOItems()
        {
            StringBuilder sSQL = new StringBuilder();
            sSQL.Append("Provider=VFPOLEDB.1;Data Source=");
            sSQL.Append(GSA.R7BD.Utility.Utilities.getDrivePathByLetter("OITEMS:"));
            sSQL.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            return sSQL.ToString();
        }


        // Added from decompiled dll
        public static string CSVitap()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Provider=VFPOLEDB.1;Data Source=");
            builder.Append(Utilities.getDrivePathByLetter("VITAPDATA:"));
            builder.Append(";Mode=Share Deny None;Extended Properties='';User ID='';Password='';Mask Password=False;Cache Authentication=False;Encrypt Password=False;Collating Sequence=MACHINE");
            return builder.ToString();
        }


    }
}
