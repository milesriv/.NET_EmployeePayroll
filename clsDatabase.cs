using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace NET2_Project2
{
    class clsDatabase
    {
        //Acquires Connection to the Payroll Database
        public static SqlConnection AcquireConnection()
        {
            SqlConnection cnSQL = null;

            if (ConfigurationManager.ConnectionStrings["Payroll"] != null)
            {
                cnSQL = new SqlConnection();
                cnSQL.ConnectionString = ConfigurationManager.ConnectionStrings["Payroll"].ToString();

                try
                {
                    cnSQL.Open();
                }
                catch (Exception ex)
                {
                    cnSQL.Dispose();
                    cnSQL = null;
                }
            }
            return cnSQL;
        }

        //Fills dsEmpData DataSet with All Employee Information using GetAllEmployees Stored Procedure
        public static DataSet GetEmployeeData()
        {
            SqlConnection cnSQL;
            SqlCommand cmdSQL;
            SqlDataAdapter daSQL;
            DataSet dsSQL = null;
            Boolean blnErrorOccured = false;

            cnSQL = AcquireConnection();
            if (cnSQL == null)
            {
                blnErrorOccured = true;
            }

            else
            {
                cmdSQL = new SqlCommand();
                cmdSQL.Connection = cnSQL;
                cmdSQL.CommandType = CommandType.StoredProcedure;
                cmdSQL.CommandText = "GetAllEmployees";

                dsSQL = new DataSet();
                try
                {
                    daSQL = new SqlDataAdapter(cmdSQL);
                    daSQL.Fill(dsSQL);
                    daSQL.Dispose();
                }
                catch (Exception ex)
                {
                    dsSQL.Dispose();
                    dsSQL = null;
                }
            }

            if (blnErrorOccured)
            {
                return null;
            }

            else
            {
                return dsSQL;
            }
        }

        //Fills dsPayrollData DataSet with Employee Information using GetPayrollbyEmployee Stored Procedure based on passed in Employee ID
        public static DataSet GetEmployeePayroll(Int32 intEmpID)
        {
            SqlConnection cnSQL;
            SqlCommand cmdSQL;
            SqlDataAdapter daSQL;
            DataSet dsSQL = null;
            Boolean blnErrorOccured = false;

                if (intEmpID < 1)
                {
                    blnErrorOccured = true;
                }

                else
                {
                    cnSQL = AcquireConnection();
                    if (cnSQL == null)
                    {
                        blnErrorOccured = true;
                    }

                    else
                    {
                        cmdSQL = new SqlCommand();
                        cmdSQL.Connection = cnSQL;
                        cmdSQL.CommandType = CommandType.StoredProcedure;
                        cmdSQL.CommandText = "GetPayrollByEmployee";

                        cmdSQL.Parameters.Add(new SqlParameter("@EmpID", SqlDbType.Int));
                        cmdSQL.Parameters["@EmpID"].Direction = ParameterDirection.Input;
                        cmdSQL.Parameters["@EmpID"].Value = intEmpID;

                        cmdSQL.Parameters.Add(new SqlParameter("@ErrCode", SqlDbType.Int));
                        cmdSQL.Parameters["@ErrCode"].Direction = ParameterDirection.ReturnValue;

                        dsSQL = new DataSet();

                        try
                        {
                            daSQL = new SqlDataAdapter(cmdSQL);
                            daSQL.Fill(dsSQL);
                            daSQL.Dispose();
                        }
                        catch (Exception ex)
                        {
                            blnErrorOccured = true;
                        dsSQL.Dispose();
                        }
                        finally
                        {
                            cmdSQL.Parameters.Clear();
                            cmdSQL.Dispose();
                            cnSQL.Close();
                            cnSQL.Dispose();
                        }
                    }
                }

                if (blnErrorOccured)
                {
                    return null;
                }
                else
                {
                     return dsSQL;
                }
            
        }

        //Store all Employee Names into a DataSet using GetEmployeeNames Stored Procedure
        public static DataSet GetEmployeeNames()
        {
            SqlConnection cnSQL;
            SqlCommand cmdSQL;
            SqlDataAdapter daSQL;
            DataSet dsSQL = null;
            Boolean blnErrorOccured = false;

            cnSQL = AcquireConnection();
            if (cnSQL == null)
            {
                blnErrorOccured = true;
            }

            else
            {
                cmdSQL = new SqlCommand();
                cmdSQL.Connection = cnSQL;
                cmdSQL.CommandType = CommandType.StoredProcedure;
                cmdSQL.CommandText = "GetEmployeeNames";

                dsSQL = new DataSet();
                try
                {
                    daSQL = new SqlDataAdapter(cmdSQL);
                    daSQL.Fill(dsSQL);
                    daSQL.Dispose();
                }
                catch (Exception ex)
                {
                    dsSQL.Dispose();
                    dsSQL = null;
                }
            }

            if (blnErrorOccured)
            {
                return null;
            }

            else
            {
                return dsSQL;
            }

        }

        //Retrieve an Employee's Payrate based on passed in Employee ID using the GetPayrateByID Stored Procedure
        public static Decimal GetPayrateByID(Int32 intEmpID)
        {
            SqlConnection cnSQL;
            SqlCommand cmdSQL;
            Decimal decPay = 0m;
            Boolean blnErrorOccured = false;
            Int32 intRetCode;

            if (intEmpID < 1)
            {
                blnErrorOccured = true;
            }

            else
            {
                cnSQL = AcquireConnection();
                if (cnSQL == null)
                {
                    blnErrorOccured = true;
                }

                else
                {
                    cmdSQL = new SqlCommand();
                    cmdSQL.Connection = cnSQL;
                    cmdSQL.CommandType = CommandType.StoredProcedure;
                    cmdSQL.CommandText = "GetPayrateByID";

                    cmdSQL.Parameters.Add(new SqlParameter("@EmpID", SqlDbType.Int));
                    cmdSQL.Parameters["@EmpID"].Direction = ParameterDirection.Input;
                    cmdSQL.Parameters["@EmpID"].Value = intEmpID;

                    cmdSQL.Parameters.Add(new SqlParameter("@PayRate", SqlDbType.SmallMoney));
                    cmdSQL.Parameters["@PayRate"].Direction = ParameterDirection.Output;

                    cmdSQL.Parameters.Add(new SqlParameter("@ErrCode", SqlDbType.Int));
                    cmdSQL.Parameters["@ErrCode"].Direction = ParameterDirection.ReturnValue;

                    try
                    {
                        intRetCode = cmdSQL.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        blnErrorOccured = true;
                    }
                    finally
                    {
                        cnSQL.Close();
                        cnSQL.Dispose();
                    }

                    if(!blnErrorOccured)
                    {
                        if (cmdSQL.Parameters["@PayRate"].Value == DBNull.Value)
                        {
                            blnErrorOccured = true;
                        }
                        else
                        {
                            decPay = Convert.ToDecimal(cmdSQL.Parameters["@PayRate"].Value);
                        }
                    }
                    cmdSQL.Parameters.Clear();
                    cmdSQL.Dispose();
                }
            }

            if (blnErrorOccured)
            {
                return -1.0m;
            }
            else
            {
                return decPay;
            }

        }

        //Add Data to Payroll Table using InsertPayroll Stored Procedure
        public static Int32 InsertPayroll(Int32 intEmpID, String strWeek, Decimal decHoursWorked)
        {
            SqlConnection cnSQL;
            SqlCommand cmdSQL;
            Int32 intReturnCode = 0;
            Boolean blnErrorOccured = false;

            if (intEmpID < 1)
            {
                blnErrorOccured = true;
            }

            else
            {
                cnSQL = AcquireConnection();
                if (cnSQL == null)
                {
                    blnErrorOccured = true;
                }

                else
                {
                    cmdSQL = new SqlCommand();
                    cmdSQL.Connection = cnSQL;
                    cmdSQL.CommandType = CommandType.StoredProcedure;
                    cmdSQL.CommandText = "InsertPayroll";

                    cmdSQL.Parameters.Add(new SqlParameter("@EmpID", SqlDbType.Int));
                    cmdSQL.Parameters["@EmpID"].Direction = ParameterDirection.Input;
                    cmdSQL.Parameters["@EmpID"].Value = intEmpID;

                    cmdSQL.Parameters.Add(new SqlParameter("@WeekEnding", SqlDbType.NChar));
                    cmdSQL.Parameters["@WeekEnding"].Direction = ParameterDirection.Input;
                    cmdSQL.Parameters["@WeekEnding"].Value = strWeek;

                    cmdSQL.Parameters.Add(new SqlParameter("@HoursWorked", SqlDbType.Decimal));
                    cmdSQL.Parameters["@HoursWorked"].Direction = ParameterDirection.Input;
                    cmdSQL.Parameters["@HoursWorked"].Value = intEmpID;

                    cmdSQL.Parameters.Add(new SqlParameter("@ErrCode", SqlDbType.Int));
                    cmdSQL.Parameters["@ErrCode"].Direction = ParameterDirection.ReturnValue;

                    try
                    {
                        intReturnCode = cmdSQL.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        blnErrorOccured = true;
                    }
                    finally
                    {
                        cnSQL.Close();
                        cnSQL.Dispose();
                        cmdSQL.Parameters.Clear();
                        cmdSQL.Dispose();
                    }
                }
            }

            if (blnErrorOccured)
            {
                return -1;
            }
            else
            {
                return 0;
            }

        }
    }
}
