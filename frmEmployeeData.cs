using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NET2_Project2
{
    public partial class frmEmployeeData : Form
    {
        DataSet dsEmpData = null;       //DataSet for Employee Information
        DataSet dsPayrollData = null;   //DataSet for Payroll Information
        Int32 intEmpCounter = 0;        //Integer used to navigate through Employee and Payroll Information

        public frmEmployeeData()
        {
            InitializeComponent();
        }

        //Employee with ID Number 1 is displayed upon Form Load
        private void frmEmployeeData_Load(object sender, EventArgs e)
        {
            lblError.Text = "";
            LoadEmployeeData();
            //ShowEmployeeData();     //Not Needed
            LoadPayrollData(intEmpCounter + 1);     //intEmpCounter + 1 is passed in to point to correct Employee ID for LoadPayrollData Procedure
        }

        //Closing Event to ensure user wishes to exit. Upon exiting, DataSets are Disposed if they exist.
        private void frmEmployeeData_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you wish to return to the Main Menu?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else if (result == DialogResult.Yes)
            {
                e.Cancel = false;

                if (dsEmpData != null)
                {
                    dsEmpData.Dispose();
                }

                if (dsPayrollData != null)
                {
                    dsPayrollData.Dispose();
                }
            }
        }

        //Closes Form
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Populates DataSet with All Employee Information and then calls ShowEmployeeData()
        private void LoadEmployeeData()
        {
            dsEmpData = clsDatabase.GetEmployeeData();

            if (dsEmpData == null)
            {
                lblError.Text = "Unable to Retrieve Employee Information";
            }
            else if (dsEmpData.Tables.Count < 1)
            {
                lblError.Text = "Unable to Retrieve Employee Information";
                dsEmpData.Dispose();
            }
            else if (dsEmpData.Tables[0].Rows.Count < 1)
            {
                lblError.Text = "Unable to Retrieve Employee Information";
            }
            else
            {
                ShowEmployeeData();
            }
        }

        //Procedure to populate Dataset with Payroll information based on currently selected Employee's ID and then display returned Dataset to Data Grid View
        private void LoadPayrollData(Int32 intEmpID)
        {
            dsPayrollData = clsDatabase.GetEmployeePayroll(intEmpID);

            if (dsPayrollData == null)
            {
                lblError.Text = "Unable to Retrieve Employee Payroll Information";
            }
            else if (dsPayrollData.Tables.Count < 1)
            {
                lblError.Text = "Unable to Retrieve Employee Payroll Information";
                dsPayrollData.Dispose();
            }
            else if (dsPayrollData.Tables[0].Rows.Count < 1)
            {
                lblError.Text = "No Payroll Information Exists for this Employee";
                dgvPayroll.DataSource = null;
                dgvPayroll.Rows.Clear();
            }
            else
            {
                dgvPayroll.DataSource = dsPayrollData.Tables[0];

                dgvPayroll.Columns["EmpID"].DefaultCellStyle.Format = "D6";
                //dgvPayroll.Columns["WeekEnding"].DefaultCellStyle.Format = "yyyy-MM-dd";

                //string test = String.Format("{0: yyyy/MM/dd}" , dgvPayroll.Columns["WeekEnding"].ToString());
                //dgvPayroll.Columns["WeekEnding"] = test;

                dgvPayroll.Columns["Payrate"].DefaultCellStyle.Format = "C";
                dgvPayroll.Columns["TotalPay"].DefaultCellStyle.Format = "C";
            }
        }

        //Procedure to format and populate text boxes with data from DataSet retrieved from LoadEmployeeData()
        private void ShowEmployeeData()
        {
            //Gives leading zeros for Employee ID Number
            txtID.Text = String.Format("{0:D6}", dsEmpData.Tables[0].Rows[intEmpCounter]["EmpID"]);

            txtLName.Text = dsEmpData.Tables[0].Rows[intEmpCounter]["LName"].ToString();
            txtFName.Text = dsEmpData.Tables[0].Rows[intEmpCounter]["FName"].ToString();
            if (dsEmpData.Tables[0].Rows[intEmpCounter]["MInit"] == DBNull.Value)
            {
                txtMInitial.Text = "";
            }
            else
            {
                txtMInitial.Text = dsEmpData.Tables[0].Rows[intEmpCounter]["MInit"].ToString();
            }

            //Database object to string, then parsing Int32 to allow String.Format to work properly
            //Puts Dashes in specified locations
            txtSSAN.Text = String.Format("{0:###-##-####}", Int32.Parse(dsEmpData.Tables[0].Rows[intEmpCounter]["SSAN"].ToString()));

            //Currency Format
            txtPay.Text = String.Format("{0:C}", dsEmpData.Tables[0].Rows[intEmpCounter]["PayRate"]);
        }

        //Shows Employee with ID Number 1
        private void btnFirst_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            intEmpCounter = 0;
            ShowEmployeeData();
            LoadPayrollData(intEmpCounter + 1);
        }

        //Shows Employee with Highest ID Number
        private void btnLast_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            intEmpCounter = dsEmpData.Tables[0].Rows.Count - 1;
            ShowEmployeeData();
            LoadPayrollData(intEmpCounter + 1);
        }

        //Shows Previous Employee, loops to last if on first already
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            intEmpCounter -= 1;
            
            if (intEmpCounter < 0)
            {
                intEmpCounter = dsEmpData.Tables[0].Rows.Count - 1;
            }

            ShowEmployeeData();
            LoadPayrollData(intEmpCounter + 1);
        }

        //Shows Next Employee, loops to first if on last already
        private void btnNext_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            intEmpCounter += 1;

            if(intEmpCounter >= dsEmpData.Tables[0].Rows.Count)
            {
                intEmpCounter = 0;
            }

            ShowEmployeeData();
            LoadPayrollData(intEmpCounter + 1);
        }
    }
}
