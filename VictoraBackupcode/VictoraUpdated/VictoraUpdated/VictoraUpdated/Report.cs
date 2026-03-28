
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VictoraUpdated
{
    public partial class Report : Form
    {
        public Report()
        {
            InitializeComponent();
        }

        string conn = "Server=DESKTOP-HP0MNU0;Database=SO-563;User Id=sa;Password=12345;TrustServerCertificate=True;";
        //string conn = "Server=SHIVINFUSED;Database=SO-563;User Id=sa;Password=12345;TrustServerCertificate=True;";

        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader reader;
        SqlDataAdapter da;
        string filepath;

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            try
            {
                using (con = new SqlConnection(conn))

                {
                    con.Open();
                    using (cmd = new SqlCommand("SELECT * FROM LaserSerialNo WHERE Date Between @from And @to", con))
                    {
                        cmd.Parameters.AddWithValue("@From",dateTimePicker1.Value.AddDays(-1));
                        cmd.Parameters.AddWithValue("@To",dateTimePicker2.Value.AddDays(0));

                        using (da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                dataGridView1.DataSource = dt;
                                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                                AddtoRichTextBox("Datatable Fetched Successfully.");

                            }
                            else
                            {
                                richTextBox1.SelectionColor = System.Drawing.Color.OrangeRed;
                                AddtoRichTextBox("No Marking Data exists within given Time Range.");
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                AddtoRichTextBox($"Error:{ex}");
            }
        }

        private void AddtoRichTextBox(string Text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new Action(() => { richTextBox1.AppendText(Text + Environment.NewLine); }));
            }
            else
            {
                richTextBox1.AppendText(Text + Environment.NewLine);
            }
        }

        private void Report_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string folderPath = @"C:\Users\india\Download";
            string filePath = Path.Combine(folderPath, "markingReport.xlsx");
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (con = new SqlConnection(conn))

                {
                    con.Open();
                    using (cmd = new SqlCommand("SELECT * FROM LaserSerialNo WHERE Date Between @from And @to", con))
                    {
                        cmd.Parameters.AddWithValue("@from", dateTimePicker1.Value.AddDays(-1));
                        cmd.Parameters.AddWithValue("@to", dateTimePicker2.Value.AddDays(0));

                        using (da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                using (var workbook = new XLWorkbook())
                                {
                                    var worksheet = workbook.Worksheets.Add("Sheet1");
                                    worksheet.Cell(1, 1).InsertTable(dt);
                                    workbook.SaveAs(filePath);

                                    richTextBox1.Clear();
                                    richTextBox1.SelectionColor = System.Drawing.Color.Green;
                                    richTextBox1.AppendText($"[{DateTime.Now}] {dt.Rows.Count} records exported successfully.\n");
                                }

                            }
                        }
                    }
                }
                
                

            }
            catch (Exception ex)
            {
                richTextBox1.SelectionColor = Color.Red;
                AddtoRichTextBox($"Error: {ex}");
            }
        }
    }
}
