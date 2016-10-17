using System;
using System.Collections; 
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.DirectoryServices;

// Change history:
// 8/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility
{
    public partial class ADAuthentication : Component
    {
        protected DirectoryEntry de = null;
        protected DirectorySearcher ds = null;
        protected string strDomainPath = null;
        protected DomainNames domain;
        public enum DomainNames{ ENT=0, R7BC_WEB=1, WebApps=2 };
        protected string strLoginID = "";
        protected string strName = "";
        protected string strEmail = "";

        public string LoginID
        {
            get
            {
                return strLoginID;
            }
        }

        public string Name
        {
            get
            {
                return strName;
            }
        }

        public string Email
        {
            get
            {
                return strEmail;
            }
        }
 
        public ADAuthentication(DomainNames domain)
        {
            switch (domain)
            {
                case DomainNames.ENT:
                    domain = DomainNames.ENT;
                    strDomainPath = "LDAP://ent.ds.gsa.gov/DC=ent,DC=ds,DC=gsa,DC=gov";
                    break;

                case DomainNames.R7BC_WEB:
                    domain = DomainNames.R7BC_WEB;
                    strDomainPath = "LDAP://R7BC_WEB.INT";
                    break;

                case DomainNames.WebApps:
                    domain = DomainNames.WebApps;
                    strDomainPath = "LDAP://webapps.int/OU=OCFOExternalWeb,DC=webapps,DC=int";
                    break;
            }

        }  // End of method

        public bool Authenticate(string UserName, string Password)
        {
            string userEmail = "";
            string strUserName = "";
            bool bResult = false;

            try
            {
                switch (domain)
                {
                    case DomainNames.ENT:
                        strUserName = "ENT\\" + UserName.Trim(); 
                        break;

                    case DomainNames.R7BC_WEB:
                        strUserName = "R7BC_WEB.INT\\" + UserName.Trim();
                        break;

                    case DomainNames.WebApps:
                        strUserName = UserName.Trim(); 
                        break;
                }

                DirectoryEntry de = new DirectoryEntry(strDomainPath, strUserName, Password.Trim(), AuthenticationTypes.Secure);
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.ReferralChasing = ReferralChasingOption.All;
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName.Trim() + "))";
                ds.PropertiesToLoad.Add("sAMAccountName"); //1. ENT Account
                ds.PropertiesToLoad.Add("mail"); //2. EMail
                ds.PropertiesToLoad.Add("name"); //3. FullName

                Hashtable associateDetailsTable = new Hashtable();
                ResultPropertyValueCollection resultCollection;
                SearchResult myUsers = ds.FindOne();
                //User is found in the Active Directory
                if (myUsers != null && myUsers.Properties.Values.Count > 0)
                {
                    //1. LoginID
                    resultCollection = myUsers.Properties["sAMAccountName"];
                    foreach (object result in resultCollection) { associateDetailsTable.Add("LoginID", result.ToString()); }
                    //2. Email
                    resultCollection = myUsers.Properties["mail"];
                    foreach (object result in resultCollection) { associateDetailsTable.Add("Email", result.ToString()); }
                    //3. Name
                    resultCollection = myUsers.Properties["name"];
                    foreach (object result in resultCollection) { associateDetailsTable.Add("Name", result.ToString()); }
                    userEmail = associateDetailsTable["Email"].ToString().Trim().ToLower() + "/" + associateDetailsTable["LoginID"].ToString().Trim().ToUpper() + "/" + associateDetailsTable["Name"].ToString().Trim().ToUpper();
     
                    // Now get the properties of the userID
                    strEmail = associateDetailsTable["Email"].ToString().Trim().ToLower();
                    strName = associateDetailsTable["Name"].ToString().Trim().ToUpper();
                    strLoginID = associateDetailsTable["LoginID"].ToString().Trim().ToUpper();

                    bResult = true;
                }
                return bResult;
            }
            catch (Exception exp)
            {
                EventLog.AddWebErrors("GSA.R7BD.Utility", "ADAuthentication", "Authenticate", "User - " + UserName.Trim() + " , Error - " + exp.Message);
                return false;
            }
        }  //end of method

        public bool isMemberOfADGroup(string UserName, string Password, string GroupName)
        {
            bool userFound = false;
            string strUserName = "";

            try
            {
                switch (domain)
                {
                    case DomainNames.ENT:
                        strUserName = "ENT\\" + UserName.Trim();
                        break;

                    case DomainNames.R7BC_WEB:
                        strUserName = "R7BC_WEB.INT\\" + UserName.Trim();
                        break;

                    case DomainNames.WebApps:
                        strUserName = UserName.Trim();
                        break;
                }

                DirectoryEntry de = new DirectoryEntry(strDomainPath, strUserName, Password.Trim(), AuthenticationTypes.Secure);
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.ReferralChasing = ReferralChasingOption.All;
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName.Trim() + "))";
                ds.PropertiesToLoad.Add("sAMAccountName"); //1. ENT 
                ds.PropertiesToLoad.Add("MemberOf"); //2. Membership                
                SearchResult myUsers = ds.FindOne();

                //User is found in the Active Directory
                if (myUsers != null && myUsers.Properties.Values.Count > 0)
                {
                    int NumberOfGroups = myUsers.Properties["memberOf"].Count - 1;
                    string tempString = "";
                    while (NumberOfGroups >= 0)
                    {
                        tempString = myUsers.Properties["MemberOf"][NumberOfGroups].ToString();
                        tempString = tempString.Substring(0, tempString.IndexOf(",", 0)).Replace("CN=", "");
                        //Above we set tempString to the first index of "," starting from the zeroth element of itself.
                        //tempString = tempString.Replace("CN=", "") ;
                        //Above, we remove the "CN=" from the beginning of the string 
                        // tempString = tempString.ToLower(); //'Lets make all letters lowercase
                        // tempString = tempString.Trim();  //Finnally, we trim any blank characters from the edges                 
                        if (GroupName.ToLower().Trim() == tempString.Trim().ToLower()) { userFound = true; break; }
                        //If we have a match, the return is true username is a member of grouptoCheck  
                        NumberOfGroups = NumberOfGroups - 1;
                    }
                }
            }
            catch (Exception exp)
            {
                EventLog.AddWebErrors("GSA.R7BD.Utility", "ADAuthentication", "isMemberOfADGroup", "User - " + UserName.Trim() + " , Error - " + exp.Message);
            }
            return userFound;

        }  //end of method

        public string[] getGroupMemberships(string UserName, string Password)
        {
            string[] userMemberships = null;
            string strUserName = "";

            try
            {
                switch (domain)
                {
                    case DomainNames.ENT:
                        strUserName = "ENT\\" + UserName.Trim();
                        break;

                    case DomainNames.R7BC_WEB:
                        strUserName = "R7BC_WEB.INT\\" + UserName.Trim();
                        break;

                    case DomainNames.WebApps:
                        strUserName = UserName.Trim();
                        break;
                }

                DirectoryEntry de = new DirectoryEntry(strDomainPath, strUserName, Password.Trim(), AuthenticationTypes.Secure);
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.ReferralChasing = ReferralChasingOption.All;
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName.Trim() + "))";
                ds.PropertiesToLoad.Add("MemberOf"); //1. Membership                
                SearchResult myUsers = ds.FindOne();

                //User is found in the Active Directory
                if (myUsers != null && myUsers.Properties.Values.Count > 0)
                {
                    int NumberOfGroups = myUsers.Properties["memberOf"].Count;
                    userMemberships = new string[NumberOfGroups];
                    string tempString = "";
                    for (int i = 0; i < NumberOfGroups; i++)
                    {
                        tempString = myUsers.Properties["MemberOf"][i].ToString();
                        tempString = tempString.Substring(0, tempString.IndexOf(",", 0)).Replace("CN=", "");
                        userMemberships[i] = tempString.Trim().ToUpper();
                    }
                }
            }
            catch (Exception exp)
            {
                EventLog.AddWebErrors("GSA.R7BD.Utility", "ADAuthentication", "getGroupMemberships", "User - " + UserName.Trim() + " , Error - " + exp.Message);
            }
            return userMemberships;

        }  //end of method


        // Added from decompiled dll
        public string getDistinguishedName(string UserName, string Password)
        {
            string username = "";
            string str2 = "";
            try
            {
                switch (this.domain)
                {
                    case DomainNames.ENT:
                        username = @"ENT\" + UserName.Trim();
                        break;

                    case DomainNames.R7BC_WEB:
                        username = @"R7BC_WEB.INT\" + UserName.Trim();
                        break;

                    case DomainNames.WebApps:
                        username = UserName.Trim();
                        break;

                    //case DomainNames.ENT2008:
                    //    username = UserName.Trim();
                    //    break;
                }
                DirectoryEntry searchRoot = new DirectoryEntry(this.strDomainPath, username, Password, AuthenticationTypes.Secure);
                DirectorySearcher searcher = new DirectorySearcher(searchRoot)
                {
                    ReferralChasing = ReferralChasingOption.All,
                    SearchScope = SearchScope.Subtree,
                    Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName + "))"
                };
                searcher.PropertiesToLoad.Add("sAMAccountName");
                searcher.PropertiesToLoad.Add("mail");
                searcher.PropertiesToLoad.Add("name");
                searcher.PropertiesToLoad.Add("distinguishedName");
                Hashtable hashtable = new Hashtable();
                SearchResult result = searcher.FindOne();
                if ((result != null) && (result.Properties.Values.Count > 0))
                {
                    ResultPropertyValueCollection values = result.Properties["distinguishedName"];
                    foreach (object obj2 in values)
                    {
                        hashtable.Add("distinguishedName", obj2.ToString());
                    }
                    if (hashtable.ContainsKey("distinguishedName"))
                    {
                        str2 = hashtable["distinguishedName"].ToString().Trim().ToUpper();
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.AddWebErrors("GSA.R7BD.Utility", "ADAuthentication", "getDistinguishedName", "User - " + UserName.Trim() + " , Error - " + exception.Message);
            }
            return str2.Trim();
        }

        // Added from decompiled dll
        public string getSupervisorLoginID(string emailAddr, string BartUser, string Password)
        {
            string username = "";
            string str2 = "";
            try
            {
                switch (this.domain)
                {
                    case DomainNames.ENT:
                        username = @"ENT\" + BartUser.Trim();
                        break;

                    case DomainNames.R7BC_WEB:
                        username = @"R7BC_WEB.INT\" + BartUser.Trim();
                        break;

                    case DomainNames.WebApps:
                        username = BartUser.Trim();
                        break;

                    //case DomainNames.ENT2008:
                    //    username = BartUser.Trim();
                    //    break;
                }
                DirectoryEntry searchRoot = new DirectoryEntry(this.strDomainPath, username, Password, AuthenticationTypes.Secure);
                DirectorySearcher searcher = new DirectorySearcher(searchRoot)
                {
                    ReferralChasing = ReferralChasingOption.All,
                    SearchScope = SearchScope.Subtree,
                    Filter = "(&(objectClass=user)(objectClass=person)(mail=" + emailAddr.Trim().ToLower().Trim() + "))"
                };
                searcher.PropertiesToLoad.Add("sAMAccountName");
                searcher.PropertiesToLoad.Add("mail");
                searcher.PropertiesToLoad.Add("name");
                Hashtable hashtable = new Hashtable();
                SearchResult result = searcher.FindOne();
                if ((result != null) && (result.Properties.Values.Count > 0))
                {
                    ResultPropertyValueCollection values = result.Properties["sAMAccountName"];
                    foreach (object obj2 in values)
                    {
                        hashtable.Add("LoginID", obj2.ToString());
                    }
                    values = result.Properties["mail"];
                    foreach (object obj2 in values)
                    {
                        hashtable.Add("Email", obj2.ToString());
                    }
                    values = result.Properties["name"];
                    foreach (object obj2 in values)
                    {
                        hashtable.Add("Name", obj2.ToString());
                    }
                    str2 = hashtable["LoginID"].ToString().Trim().ToUpper();
                }
            }
            catch (Exception exception)
            {
                str2 = "ERROR";
                EventLog.AddWebErrors("GSA.R7BD.Utility", "ADAuthentication", "getSupervisorLoginID", "User - " + BartUser.Trim() + " , Error - " + exception.Message);
            }
            return str2.Trim();
        }


    } // end of class ADAuthentication

} // End of Namespace

