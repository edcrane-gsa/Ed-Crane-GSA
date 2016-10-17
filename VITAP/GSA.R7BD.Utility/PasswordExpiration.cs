using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;

// Added reference: ActiveDS.tlb in the system32 directory. 
// References > Add Reference > COM tap > Active DS Type Library

using ActiveDs;

namespace GSA.R7BD.Utility
{
    /// <summary>
    /// ESC0100044488 - Rupa Duthulur
    /// Decomission of the R7BC servers and migration of External Users/groups to WEBAPPS domain
    /// WEBAPPS domain requires the external users to change their password every 90 days
    /// IsUserExpired checks if the User's password has expired on the WEBAPPS domain
    /// UpdatePassword updates the new password on the WEBAPPS domain
    /// </summary>


    public class PasswordExpiration
    {
        //Commented out by Rupa D - because of use of Single Sign On
        /// <summary>
        /// ESC49330, ESC49331, ESC49321 - Jun Lee
        /// Check if the User's password is expired on the Webapps Active Directory
        /// To expire external user’s password properly, 
        /// two critical data are necessary: PasswordExpirationDate and PasswordNeedToBeChanged.
        /// If Today is greater than PasswordExpirationDate or PasswordNeedToBeChanged is true,
        /// current user's password is expired.
        /// </summary>
        /// <param name="parentDE">DirectoryEntry</param>
        /// <param name="UserName">string</param>
        /// <returns>bool</returns>
        public static bool IsUserExpired(DirectoryEntry parentDE, string UserName)
        {

            bool isExpired = false;

            try
            {
                // Search and Get User object from Active Directory
                DirectorySearcher ds = new DirectorySearcher(parentDE);
                ds.ReferralChasing = ReferralChasingOption.All;
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName.Trim() + "))";

                ds.PropertiesToLoad.Add("sAMAccountName");      //1. WEBAPPS Account  
                ds.PropertiesToLoad.Add("MemberOf");            //Membership  
                ds.PropertiesToLoad.Add("pwdLastSet");          //Password Last Set

                SearchResult myUsers = ds.FindOne();


                if (myUsers.Properties.Contains("pwdLastSet"))
                {
                    // Do work with data returned for address entry
                    DirectoryEntry deUser = myUsers.GetDirectoryEntry();
                    LargeInteger acctPwdChange = deUser.Properties["pwdLastSet"].Value as LargeInteger;


                    long dateAcctPwdChange = (((long)(acctPwdChange.HighPart) << 32) + (long)acctPwdChange.LowPart);

                    if (dateAcctPwdChange == 0)
                    {
                        isExpired = true;
                    }
                    else if (dateAcctPwdChange > 0)
                    {
                        // Get max password age in current domain: WebApps
                        int maxPasswordAge = GetMaxPasswordAge().Days;

                        // Convert FileTime to DateTime and get what today's date is.
                        // Add maxPwdAgeDays to dtAcctPwdChange
                        DateTime passwordExpirationDate = DateTime.FromFileTime(dateAcctPwdChange).AddDays(maxPasswordAge);

                        // Test the expiration condtion
                        if (DateTime.Now > passwordExpirationDate)
                        {
                            isExpired = true;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "PasswordExpiration.cs", "IsUserExpired", exp.Message);
            }
            return isExpired;
        }

        /// <summary>
        /// Change/Update User's password if password is expired on the Webapps AD 
        /// </summary>
        /// <param name="adminDE">DirectoryEntry</param>
        /// <param name="UserName">string</param>
        /// <param name="OldPassword">string</param>
        /// <param name="NewPassword">string</param>
        /// <returns>bool</returns>

        public static bool UpdatePassword(DirectoryEntry adminDE, string UserName, string OldPassword, string NewPassword)
        {
            try
            {
                DirectorySearcher ds = new DirectorySearcher(adminDE);
                ds.ReferralChasing = ReferralChasingOption.All;
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = "(&(objectClass=user)(objectClass=person)(sAMAccountName=" + UserName.Trim() + "))";
                ds.PropertiesToLoad.Add("sAMAccountName"); //1. WEBAPPS Account  
                ds.PropertiesToLoad.Add("MemberOf"); //Membership  
                SearchResult myUsers = ds.FindOne();
                if (myUsers != null && myUsers.Properties.Values.Count > 0)
                {
                    DirectoryEntry objUser = myUsers.GetDirectoryEntry();
                    objUser.Invoke("ChangePassword", new Object[] { OldPassword, NewPassword });
                    objUser.CommitChanges();
                    return true;
                }
            }
            catch (Exception exp)
            {
                GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "PasswordExpiration.cs", "UpdatePassword", exp.Message.ToString());
            }
            return false;
        }


        /// <summary>
        /// ESC49330, ESC49331, ESC49321 - Jun Lee
        /// Get MaxPasswordAge from Domain Level on the Webapps Active Directory
        /// </summary>
        /// <returns>TimeSpan</returns>
        private static TimeSpan GetMaxPasswordAge()
        {
            TimeSpan maxPwdAge = TimeSpan.MinValue;
            try
            {
                // Get the current Domain object
                using (Domain currentDomain = Domain.GetCurrentDomain())
                {
                    using (DirectoryEntry domain = currentDomain.GetDirectoryEntry())
                    {
                        DirectorySearcher ds = new DirectorySearcher(domain, "(objectClass=*)", null, SearchScope.Base);
                        SearchResult sr = ds.FindOne();

                        // Get max password age from current domain: WebApps
                        if (sr.Properties.Contains("maxPwdAge"))
                        {
                            maxPwdAge = TimeSpan.FromTicks((long)sr.Properties["maxPwdAge"][0]);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                GSA.R7BD.Utility.EventLog.AddWebErrors("Utility", "PasswordExpiration.cs", "GetMaxPasswordAge", exp.Message);                
            }
            return maxPwdAge.Duration();
        }
    }
}









//
// Previous code in IsUserExpired()
// 
//long ticks;
//if (myUsers != null && myUsers.Properties.Values.Count > 0)
//{
//    if (myUsers.Properties.Contains("pwdLastSet"))
//    {
//        ticks = Convert.ToInt64(myUsers.Properties["pwdLastSet"][0]);
//    }
//    else
//    {
//        ticks = -1;
//    }

//    if (ticks == 0)
//    {
//        return true; //User must change password at next login
//    }
//    else if (ticks == -1)
//    {
//        //throw new InvalidOperationException("User does not have a password");
//        return false;
//    }
//}