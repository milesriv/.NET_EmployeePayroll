using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Data.SqlClient;

namespace NET2_Project2
{
    public partial class frmPayrollData : Form
    {
        DataSet dsEmpNames = null;  //***DataSet for Employee Names*** 
        DataSet dsDuplicate = null; //***DataSet to check if Employee already has entry for Ending Week*** 
        DataTable dtNewData = null; //***DataTable used for dgvPayroll***
        Int32 intEarlierIndex = -1; //***Integer used to determine ComboBox index before btnRemove is clicked***

        public frmPayrollData()
        {
            InitializeComponent();
        }

        private void frmPayrollData_Load(object sender, EventArgs e)
        {
            lblError.Text = "";
            LoadEmployeeNames();
            CreateDuplicateCheck();
            CreateDataTable();
            txtWeek.Select();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmPayrollData_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you wish to exit?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else if (result == DialogResult.Yes)
            {
                if(dsEmpNames != null)
                {
                    dsEmpNames.Dispose();
                }
                
                if(dtNewData != null)
                {
                    dtNewData.Dispose();
                }

                if(dsDuplicate != null)
                {
                    dsDuplicate.Dispose();
                }

                e.Cancel = false;
            }
        }

        //***Procedure to obtain a DataSet that populates the ComboBox*** 
        private void LoadEmployeeNames()
        {
            dsEmpNames = clsDatabase.GetEmployeeNames();

            if (dsEmpNames == null)
            {
                lblError.Text = "Unable to Retrieve Employee Names";
            }
            else if (dsEmpNames.Tables.Count < 1)
            {
                lblError.Text = "Unable to Retrieve Employee Names";
                dsEmpNames.Dispose();
            }
            else if (dsEmpNames.Tables[0].Rows.Count < 1)
            {
                lblError.Text = "Unable to Retrieve Employee Names";
            }
            else
            {
                cboEmployee.DataSource = dsEmpNames.Tables[0];
                cboEmployee.DisplayMember = "FullName";
                cboEmployee.ValueMember = "EmpID";
            }
        }

        private void CreateDuplicateCheck()
        {
            SqlConnection cnSQL;
            SqlDataAdapter daSQL;

            if (dsDuplicate != null)
            {
                dsDuplicate.Dispose();
            }

            cnSQL = clsDatabase.AcquireConnection();
            if (cnSQL == null)
            {
                lblError.Text = "Error Connecting to Database";
            }

            dsDuplicate = new DataSet();
            try
            {

                daSQL = new SqlDataAdapter("SELECT EmpID, WeekEnding FROM dbo.Payroll;", cnSQL);
                daSQL.Fill(dsDuplicate);
                daSQL.Dispose();
            }

            catch (Exception ex)
            {
                lblError.Text = "Error Retrieving Payroll data";
                dsDuplicate.Dispose();
            }

            finally
            {
                cnSQL.Close();
                cnSQL.Dispose();
            }
        }

        //***Formats an empty DataTable and sets dgvPayroll's DataSource to said DataTable for data entry*** 
        private void CreateDataTable()
        {
            if (dtNewData != null)
            {
                dtNewData.Dispose();
            }

            dtNewData = new DataTable();
            dtNewData.TableName = "Payroll Data";
            dtNewData.Columns.Add("EmpID", typeof(Int32));
            dtNewData.Columns.Add("Name", typeof(String));
            dtNewData.Columns.Add("WeekEnding", typeof(String));
            dtNewData.Columns.Add("HoursWorked", typeof(Decimal));
            dtNewData.Columns.Add("TotalPay", typeof(String));

            dgvPayroll.DataSource = dtNewData;
            dgvPayroll.Columns[1].Width = 200;
            dgvPayroll.Columns[2].Width = 125;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddValidate();
        }

        //***Allows Enter Key to be used to add Data***
        private void txtHoursWorked_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                AddValidate();
                e.Handled = true;   //***Stops Windows "Ding" noise***
            }
        }

        //***Validate the two Text Fields and block individual employees having multiple entires to ensure Data Integrity***
        private void AddValidate()
        {
            Boolean blnOK = true;
            Boolean blnDupOk = true;    //***Boolean to ensure Week entered is valid and can be compared with formatted entry***
            Int32 intNumericCheck;      //***Week is numeric***
            Decimal decNumericCheck;    //***Hours is numeric***
            Decimal decTestPlaces;      //***Hours only has 2 decimal places***
            Int32 intTestPlaces;        //***Hours only has 2 decimal places***
            String strErrorMsg = "";

            //***********Week Checks**************
            if (txtWeek.Text.Trim().Length != 8)
            {
                blnOK = false;
                blnDupOk = false;
                strErrorMsg += "Please enter a valid Week \n";
            }

            else if (Int32.TryParse(txtWeek.Text.Trim(), out intNumericCheck) == false)
            {
                blnOK = false;
                blnDupOk = false;
                strErrorMsg += "Please enter a valid Week \n";
            }

            else if (txtWeek.Text.Trim().StartsWith("-"))
            {
                blnOK = false;
                blnDupOk = false;
                strErrorMsg += "Please enter a valid Week \n";
            }
            //***********Week Checks**************


            //***********Duplicate Employee Check**************
            if (blnDupOk)
            {
                //***Substring formatting used when adding, used here for duplicate check***
                String strWeek = txtWeek.Text;
                strWeek = strWeek.Substring(0, 4) + "/" + strWeek.Substring(4, 2) + "/" + strWeek.Substring(6, 2);

                if (dgvPayroll.Rows.Count > 0)
                {
                    //***Foreach Loop to ensure each employee will not have multiple entries in DataTable***
                    foreach (DataGridViewRow row in dgvPayroll.Rows)
                    {
                        if (cboEmployee.Text == row.Cells["Name"].Value.ToString() && strWeek == row.Cells["WeekEnding"].Value.ToString())
                        {
                            blnOK = false;
                            strErrorMsg += "Employees cannot have multiple entries for the same Week \n";
                        }
                    }
                }
            }
            //***********Duplicate Employee Check**************


            //***********Hours Worked Checks**************
            if (txtHoursWorked.Text.Trim().Length == 0)
            {
                blnOK = false;
                strErrorMsg += "Please enter Hours Worked \n";
            }

            else if (Decimal.TryParse(txtHoursWorked.Text.Trim(), out decNumericCheck) == false)
            {
                 blnOK = false;
                 strErrorMsg += "Hours Worked must be Numeric \n";
            }

            else if (Convert.ToDecimal(txtHoursWorked.Text.Trim()) < 0)
            {
                blnOK = false;
                strErrorMsg += "Hours Worked cannot be less than Zero \n";
            }

            else
            {
                decTestPlaces = Convert.ToDecimal(txtHoursWorked.Text.Trim()) * 100;
                intTestPlaces = Convert.ToInt32(decTestPlaces);

                if (decTestPlaces != intTestPlaces)
                {
                    blnOK = false;
                    strErrorMsg += "Hours Worked cannot have more than two decimals ";
                }
            }
            //***********Hours Worked Checks**************


            if (blnOK)
            {
                AddPayrollData();
            }
            else
            {
                lblError.Text = strErrorMsg;
            }
        }

        //***Adds new row with data to dtNewData which is then displayed on dgvPayroll***
        private void AddPayrollData()
        {
            lblError.Text = "";

            Int32 intEmpID = Convert.ToInt32(cboEmployee.SelectedValue);

            //***Formatting "WeekEnding" Column data with substrings for readability, changed back when adding to Database***
            String strWeek = txtWeek.Text;
            strWeek = strWeek.Substring(0, 4) + "/" + strWeek.Substring(4, 2) + "/" + strWeek.Substring(6, 2);

            //***Obtaining Employee's Payrate based on their ID from Database and then calculating Total Pay***
            Decimal decPayRate = clsDatabase.GetPayrateByID(intEmpID);
            Decimal decTotalPay = decPayRate * Convert.ToDecimal(txtHoursWorked.Text);

            dtNewData.Rows.Add(cboEmployee.SelectedValue, cboEmployee.Text, strWeek, txtHoursWorked.Text, decTotalPay.ToString("C"));


            //***If the User had to remove a Row in dgvPayroll***
            //***then they will be returned to the employee they were at before they caught and fixed their mistake***
            if (intEarlierIndex != -1)
            {
                cboEmployee.SelectedIndex = intEarlierIndex;
                intEarlierIndex = -1;
            }

            else
            {
                //***Cycle through Combobox for quicker data entry, never going passed the end of the list***
                if (cboEmployee.Items.Count != cboEmployee.SelectedIndex + 1)
                {
                    cboEmployee.SelectedIndex += 1; 
                }

                else
                {
                    cboEmployee.SelectedIndex = 0;
                }
            }
            
            txtHoursWorked.Text = "";
            txtHoursWorked.Focus();
            btnCommit.Enabled = true;
            btnExport.Enabled = true;
            btnPrint.Enabled = true;
            btnRemove.Enabled = true;
        }

        //***Button Procedure used to Remove a Row in dgvPayroll in case a mistake was made***
        private void btnRemove_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            //***Store the position in the ComboBox the user was at, jump to the ComboBox index where the mistake was made for efficiency, then delete the row***
            if (dgvPayroll.SelectedRows.Count > 0)
            {
                intEarlierIndex = cboEmployee.SelectedIndex;
                cboEmployee.SelectedIndex = Convert.ToInt32(dgvPayroll.SelectedRows[0].Cells[0].Value) - 1;
                dgvPayroll.Rows.RemoveAt(dgvPayroll.SelectedRows[0].Index);

                txtHoursWorked.Focus();

                //***If dgvPayroll is empty, disable buttons***
                if (dgvPayroll.Rows.Count == 0)
                {
                    btnCommit.Enabled = false;
                    btnExport.Enabled = false;
                    btnPrint.Enabled = false;
                    btnRemove.Enabled = false;
                }
            }
            else
            {
                lblError.Text = "Please Select a Row to be Removed";
            }
        }

        //***Button Procedure to Set up an XML Save Dialog Box and then write the data in dtNewData to an XML file***
        private void btnExport_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            DialogResult dlgAnswer;

            sfdXML.DefaultExt = "xml";
            sfdXML.Filter = "XML files (*.xml)|*.xml|All files(*.*)|*.*";
            sfdXML.InitialDirectory = "C:\\";
            sfdXML.OverwritePrompt = true;
            sfdXML.Title = "Save XML File";

            if (dtNewData.Rows.Count > 0)
            {
                dlgAnswer = sfdXML.ShowDialog();
                if (dlgAnswer == DialogResult.OK)
                {
                    dtNewData.WriteXml(sfdXML.FileName);
                    lblError.Text = "DataTable Exported to XML!";
                }
            }          
        }

        //***Button Procedure to Show the Dialog Box for printing and then call on Print() with user's print settings***
        private void btnPrint_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            DialogResult dlgAnswer;

            dlgAnswer = pdlgData.ShowDialog();
            if(dlgAnswer == DialogResult.OK)
            {
                pdPrint.PrinterSettings = pdlgData.PrinterSettings;

                pdPrint.Print();
            }
        }

        //***Print Page Event***
        private void pdPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Font fDoc;          //***General Use***
            Font fDocBold;      //***Header Row***
            Font fDocTitle;     //***Title and Date***
            Font fDocSummary;   //***Footer Text***

            Single sglXPos;
            Single sglYPos;
            Int32 intRow;
            Decimal decTotalValue;

            fDoc = new Font("Arial", 12);
            fDocBold = new Font("Arial", 12, FontStyle.Bold);
            fDocTitle = new Font("Helvetica", 20);
            fDocSummary = new Font("Cambria", 16, FontStyle.Bold);

            //***Title***
            e.Graphics.DrawString("Employee Payroll Data", fDocTitle, System.Drawing.Brushes.Black, Convert.ToSingle(50.0), Convert.ToSingle(75.0));

            //***Today's Date using specified format***
            e.Graphics.DrawString(DateTime.Now.ToString("yyyy/MM/dd"), fDocTitle, System.Drawing.Brushes.Black, Convert.ToSingle(530.0), Convert.ToSingle(75.0));

            //***Header Row***
            sglYPos = Convert.ToSingle(150);
            e.Graphics.DrawString("EmpID", fDocBold, System.Drawing.Brushes.Black, Convert.ToSingle(50.0), sglYPos);
            e.Graphics.DrawString("Name", fDocBold, System.Drawing.Brushes.Black, Convert.ToSingle(120.0), sglYPos);
            e.Graphics.DrawString("WeekEnding", fDocBold, System.Drawing.Brushes.Black, Convert.ToSingle(325.0), sglYPos);
            e.Graphics.DrawString("HoursWorked", fDocBold, System.Drawing.Brushes.Black, Convert.ToSingle(450.0), sglYPos);
            e.Graphics.DrawString("TotalPay", fDocBold, System.Drawing.Brushes.Black, Convert.ToSingle(600.0), sglYPos);

            decTotalValue = Convert.ToDecimal(0.0);

            //***Iterate through dtNewData, print out the data and add each employee's Total Pay***
            for (intRow = 0; intRow < dtNewData.Rows.Count; intRow++)
            {
                sglYPos += Convert.ToSingle(fDoc.Height);

                sglXPos = Convert.ToSingle(50.0);
                e.Graphics.DrawString(dtNewData.Rows[intRow]["EmpID"].ToString(), fDoc, System.Drawing.Brushes.Black, sglXPos, sglYPos);

                sglXPos = Convert.ToSingle(120.0);
                e.Graphics.DrawString(dtNewData.Rows[intRow]["Name"].ToString(), fDoc, System.Drawing.Brushes.Black, sglXPos, sglYPos);

                sglXPos = Convert.ToSingle(325.0);
                e.Graphics.DrawString(dtNewData.Rows[intRow]["WeekEnding"].ToString(), fDoc, System.Drawing.Brushes.Black, sglXPos, sglYPos);

                sglXPos = Convert.ToSingle(450.0);
                e.Graphics.DrawString(dtNewData.Rows[intRow]["HoursWorked"].ToString(), fDoc, System.Drawing.Brushes.Black, sglXPos, sglYPos);

                sglXPos = Convert.ToSingle(600.0);
                e.Graphics.DrawString(dtNewData.Rows[intRow]["TotalPay"].ToString(), fDoc, System.Drawing.Brushes.Black, sglXPos, sglYPos);

                //***NumberStyles.Currency used to allow decimal parse of string with $
                decTotalValue += Decimal.Parse(dtNewData.Rows[intRow]["TotalPay"].ToString(), NumberStyles.Currency);
            }

            //***Footer Row giving the number of Employee Records printed as well as the Grand Total of Salaries Paid***
            sglYPos += (Convert.ToSingle(fDoc.Height * 5));
            e.Graphics.DrawString(dtNewData.Rows.Count + " Records Printed, Total Value: " + decTotalValue.ToString("C"), fDocSummary, System.Drawing.Brushes.Black, Convert.ToSingle(50.0), sglYPos);

            lblError.Text = "Records Printed!";
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("DataGrid will be Cleared\nEnsure all Data is Valid before Commiting\n\nDo you wish to Commit to Database?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.No)
            {
                
            }
            else if (result == DialogResult.Yes)
            {
                Boolean blnOK = true;
                String strErrorRows = "";   //***String used to tell user which Employee ID's already have a record for the Week***
                Int32 intRow;
                CreateDuplicateCheck();



                //***Check to ensure an entry for this employee hasn't been entered on this Ending Week***
                //***Cycle through Data trying to be inserted into Database***
                foreach (DataGridViewRow row in dgvPayroll.Rows)
                {
                    //***Formatting WeekEnding entry for Duplicate Check***
                    String strWeekEnding = row.Cells["WeekEnding"].Value.ToString();
                    strWeekEnding = strWeekEnding.Substring(0, 4) + strWeekEnding.Substring(5, 2) + strWeekEnding.Substring(8, 2);

                    //***Cycle through what the DataBase currently contains***
                    for (intRow = 0; intRow < dsDuplicate.Tables[0].Rows.Count; intRow++)
                    {
                        if (row.Cells["EmpID"].Value.ToString() == dsDuplicate.Tables[0].Rows[intRow]["EmpID"].ToString() &&
                            strWeekEnding == dsDuplicate.Tables[0].Rows[intRow]["WeekEnding"].ToString())
                        {
                            blnOK = false;
                            strErrorRows += row.Cells["EmpID"].Value.ToString() + ",";
                        }
                    }
                }

                //***Commit and actually update Database with New Data***
                if (blnOK)
                {
                    foreach (DataGridViewRow row in dgvPayroll.Rows)
                    {
                        Int32 intEmployeeID = Convert.ToInt32(row.Cells["EmpID"].Value);
                        Decimal decHours = Convert.ToDecimal(row.Cells["HoursWorked"].Value);

                        //***Formatting WeekEnding entry for Database Insert***
                        String strWeekEnding = row.Cells["WeekEnding"].Value.ToString();
                        strWeekEnding = strWeekEnding.Substring(0, 4) + strWeekEnding.Substring(5, 2) + strWeekEnding.Substring(8, 2);

                        Int32 intDBPush = clsDatabase.InsertPayroll(intEmployeeID, strWeekEnding, decHours);

                        if (intDBPush == -1)
                        {
                            lblError.Text = "Error adding Records to Database. ";
                        }

                        else
                        {
                            lblError.Text = "Records added to Database!";
                            btnCommit.Enabled = false;
                            btnExport.Enabled = false;
                            btnPrint.Enabled = false;
                            btnRemove.Enabled = false;
                            txtHoursWorked.Text = "";
                            txtWeek.Text = "";
                            cboEmployee.SelectedIndex = 0;
                            dtNewData.Clear();
                            txtWeek.Focus();
                        }
                    }
                }
                else
                {
                    lblError.Text = "The following Employee ID's already have an entry for this Week: " + strErrorRows + "\nCommit Aborted";
                }
            }

            
        }
    }
}
