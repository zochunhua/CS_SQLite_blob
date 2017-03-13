using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Finisar.SQLite;

namespace CS_SQLite_blob
{
    public partial class Form1 : Form
    {
        byte[] m_bytes;
        String m_StrFilePath;
        String m_Strdgvuid;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = @"C:\\";
            dlg.Title = "Select Image File";
            dlg.Filter = "Image Files  (*.jpg ; *.jpeg ; *.png ; *.gif ; *.tiff ; *.nef)|*.jpg;*.jpeg;*.png;*.gif;*.tiff;*.nef";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                m_StrFilePath = "";
                MessageBox.Show(dlg.FileName.ToString());
                if (m_bytes != null)
                {
                    m_bytes = null;
                }
                m_bytes = File.ReadAllBytes(dlg.FileName.ToString());
                m_StrFilePath = dlg.FileName.ToString();
                MemoryStream ms = new MemoryStream(m_bytes);
                pictureBox1.Image = Image.FromStream(ms);

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SQLite.initSQLiteDatabase();
            String SQLStr = String.Format("SELECT uid,name FROM Blob_Data;");
            DataTable dt = SQLite.GetDataTable(SQLite.DBpath, SQLStr);
            dataGridView1.DataSource = dt;//讀取資料程式
            m_Strdgvuid = "1";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_bytes != null && m_StrFilePath != null)
            {
                String SQLStr = String.Format("INSERT INTO 'Blob_Data' (name, data) VALUES ('{0}', @data);", m_StrFilePath);
                SQLite.SQLiteInsertImge(SQLite.DBpath, SQLStr, m_bytes);
                MessageBox.Show(SQLStr + "\n OK");

                SQLStr = String.Format("SELECT uid,name FROM Blob_Data;");
                DataTable dt = SQLite.GetDataTable(SQLite.DBpath, SQLStr);
                dataGridView1.DataSource = dt;//讀取資料程式
            }

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            GridViewFunction();
        }
        public void GridViewFunction()
        {
            try
            {
                int index = dataGridView1.SelectedRows[0].Index;//取得被選取的第一列位置
                string Struid = dataGridView1.Rows[index].Cells[0].Value.ToString();
                m_Strdgvuid = Struid;
            }
            catch
            {
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            String SQLStr = String.Format("SELECT data FROM Blob_Data WHERE uid={0};", m_Strdgvuid);
            SQLiteDataReader dr = SQLite.GetDataReader(SQLite.DBpath, SQLStr);
            /*
            dr.Read();
            
            Byte[] blob = null;
            blob = new Byte[(dr.GetBytes(0, 0, null, 0, int.MaxValue))];
            dr.GetBytes(0, 0, blob, 0, blob.Length);
            MemoryStream ms = new MemoryStream(blob);
            pictureBox1.Image = Image.FromStream(ms);
            //*/
            if (dr.Read())
            {
                m_bytes = null;
                m_bytes = (byte[])dr[0];
                MemoryStream ms = new MemoryStream(m_bytes);
                pictureBox1.Image = Image.FromStream(ms);
            }

        }
    }
}
