using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using System.ComponentModel;
using GSA.R7BD.Utility;

// Change history:
// 9/17/2010 - OCP 26869 R7 Web App Migration - Ken Sickler 

namespace GSA.R7BD.Utility
{
    public class Audit : Component
    {
        public enum UserEvent { LogonSuccessful=1, LogonFailed=2, Logoff=3, SessionTimeout=4 };

        // This routine writes a User Event audit entry 
        // into the SYS_USE_ADUIT table
        public void WriteUserEvent(string AppName, string UserID, UserEvent uEvent)
        {
            System.Data.OracleClient.OracleConnection ConnOracle;
            System.Data.OracleClient.OracleCommand CmdOracle;

            //Parameters for the Oracle Package
            OracleParameter return_val = new OracleParameter("v_success", OracleType.VarChar, 1);
            return_val.Direction = ParameterDirection.ReturnValue;
            OracleParameter p_external_user_id = new OracleParameter("p_external_user_id", OracleType.VarChar, 250);
            OracleParameter p_system_use = new OracleParameter("p_system_use", OracleType.VarChar, 250);
            OracleParameter p_system_activity = new OracleParameter("p_system_activity", OracleType.VarChar, 250);

            ConnOracle = new System.Data.OracleClient.OracleConnection();
            CmdOracle = new System.Data.OracleClient.OracleCommand();

            try
            {
                p_external_user_id.Value = UserID;
                switch (uEvent)
                {
                    case UserEvent.LogonSuccessful:
                        p_system_activity.Value = "Logon successful";
                        break;

                    case UserEvent.LogonFailed:
                        p_system_activity.Value = "Logon failed";
                        break;

                    case UserEvent.Logoff:
                        p_system_activity.Value = "Logoff";
                        break;

                    case UserEvent.SessionTimeout:
                        p_system_activity.Value = "Session timeout";
                        break;
                }
                p_system_use.Value = AppName;
             
                //Use Oracle Package
                ConnOracle.ConnectionString = GSA.R7BD.Utility.DataAccess.ConnStrBDFApps();
                CmdOracle.Connection = ConnOracle;
                CmdOracle.CommandText = "bdfapps.SYS_USE_AUDIT_PKG.CREATE_SYS_USE_AUDIT_REC";
                CmdOracle.CommandType = CommandType.StoredProcedure;
                CmdOracle.Parameters.Clear();
                CmdOracle.Parameters.Add(p_external_user_id);
                CmdOracle.Parameters.Add(p_system_activity);
                CmdOracle.Parameters.Add(p_system_use);
                CmdOracle.Parameters.Add(return_val);

                if (ConnOracle.State != ConnectionState.Open) { ConnOracle.Open(); }
                CmdOracle.ExecuteScalar();

                //Check the return value
                if (CmdOracle.Parameters["v_success"].Value.ToString().Trim() == "F")
                {
                    GSA.R7BD.Utility.EventLog.AddWebErrors(AppName, "GSA.R7BD.Utility", "Audit", "Call to bdfapps.SYS_USE_AUDIT_PKG.CREATE_SYS_USE_AUDIT_REC failed.");
                }

            }
            catch (Exception ex)
            {
                // Write the exception to the error log
                GSA.R7BD.Utility.EventLog.AddWebErrors(AppName, "GSA.R7BD.Utility", "Audit", ex.Message);
            }
            finally
            {
                if (ConnOracle.State == ConnectionState.Open) 
                {
                    ConnOracle.Close();   
                }
            }
        }

        public static DataSet AuditColumns(string strAppName, string strTable, string strAudInd)
        {
            DataSet ds = new DataSet();
            OracleDataAdapter daColumns = new OracleDataAdapter();

            try
            {
                using (OracleConnection conBdrpt = new OracleConnection(DataAccess.ConnStrBDFApps()))
                {
                    string sqlSelect = "select * from bdfapps.sys_audit_column where system_of_record = '" + strAppName + "' and LOWER(changing_table) = lower('" + strTable + "') and audit_ind = '" + strAudInd + "'";
                    daColumns.SelectCommand = new OracleCommand(sqlSelect, conBdrpt);
                    daColumns.Fill(ds);
                }
            }
            catch (Exception exp)
            {
                EventLog.AddWebErrors("Utility", "Audit.cs", "AuditColumns", exp.Message);
            }

            return ds;
        }

        public static void InsertAuditData(string external_user_id, string system_of_record, string changing_table, string changing_column, string record_pk, string created_by, string old_value, string new_value)
        {
            string strSQL = "";

            try
            {
                using (OracleConnection conBdrpt = new OracleConnection(DataAccess.ConnStrBDFApps()))
                {
                    if (conBdrpt.State != ConnectionState.Open) { conBdrpt.Open(); }

                    strSQL = ("insert into bdfapps.sys_data_audit columns(sys_data_audit_id, external_user_id, system_of_record, changing_table, ");
                    strSQL = strSQL + ("changing_column, record_pk, created_by, created_date, old_value, new_value) values (bdfapps.sys_data_audit_seq.NEXTVAL, ");
                    strSQL = strSQL + ("'" + external_user_id + "', '" + system_of_record + "', '" + changing_table + "', '" + changing_column + "', ");
                    strSQL = strSQL + ("'" + record_pk + "', '" + created_by + "', sysdate, '" + old_value + "', '" + new_value + "') ");

                    OracleCommand cmd = new OracleCommand(strSQL, conBdrpt);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception exp)
            {
                EventLog.AddWebErrors("Utility", "Audit.cs", "InsertAuditData", exp.Message);
            }
        }
    }
}
