using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace GenerateFile
{
    public partial class Generate : Form
    {
        public static int AppId = 0;
        public Generate()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtAppId.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter the AppId");
                }
                else if (txtFileName.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter the file name");
                }
                else
                    GenerateFile();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        public void GenerateFile()
        {
            string strQuery = "Select * from VW_IPaymentApp where AppId = @AppId";
            DataSet dsiPaymentAppInfo = GetiPaymentInfo(Convert.ToInt32(txtAppId.Text.Trim()), strQuery);


            string strFileName = txtFileName.Text.Trim();
            FileStream fsiPayment = new FileStream(strFileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter swiPayment = new StreamWriter(fsiPayment);
            string strData = "";
            
            if (dsiPaymentAppInfo.Tables["VW_IPaymentApp"].Rows.Count > 0)
            {
                DataRow driPaymentInfo = dsiPaymentAppInfo.Tables["VW_IPaymentApp"].Rows[0];
                for (int i = 0; i < dsiPaymentAppInfo.Tables["VW_IPaymentApp"].Columns.Count; i++)
                    strData += driPaymentInfo.Table.Columns[i].ColumnName.ToString().Trim() + Convert.ToChar(9);
                swiPayment.WriteLine(strData);
            }//end count not 0

            strData = "";
            dsiPaymentAppInfo = GetiPaymentInfo(Convert.ToInt32(txtAppId.Text.Trim()), strQuery);
            if (dsiPaymentAppInfo.Tables["VW_IPaymentApp"].Rows.Count > 0)
            {
                DataRow driPaymentInfo = dsiPaymentAppInfo.Tables["VW_IPaymentApp"].Rows[0];
                for (int i = 0; i < dsiPaymentAppInfo.Tables["VW_IPaymentApp"].Columns.Count; i++)
                    strData += driPaymentInfo[i].ToString().Trim() + Convert.ToChar(9);
                swiPayment.WriteLine(strData);
            }//end count not 0

            swiPayment.Close();
            fsiPayment.Close();

        }

        public DataSet GetiPaymentInfo(int AppId, string strQuery)
        {
            string ConnString = "Data Source=SERVER;Persist Security Info=True;Password=succeed;User ID=SA;Initial Catalog=AgentPortal;";
            SqlConnection Conn = new SqlConnection(ConnString);
            try
            {
                SqlCommand cmd = new SqlCommand(strQuery, Conn);
                cmd.Connection.Open();
                cmd.Parameters.Add(new SqlParameter("@AppId",AppId));
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet ds = new DataSet();
                adapter.Fill(ds, "VW_IPaymentApp");
                return ds;
            }
            catch (Exception err)
            {
                throw err;
            }
            finally
            {
                Conn.Close();
                Conn.Dispose();
            }
        }

    }
}