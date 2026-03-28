using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NModbus;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VictoraUpdated
{
    public partial class popup : Form
    {
        public popup()
        {
            InitializeComponent();
            
            richTextBox1.ReadOnly = true;
            richTextBox2.ReadOnly = true;

            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBox4.ReadOnly = true;
            textBox5.ReadOnly = true;
            textBox6.ReadOnly = true;
            textBox7.ReadOnly = true;
            textBox8.ReadOnly = true;
            textBox9.ReadOnly = true;

            fetchPartname();
            NextMarkingId();
            fetchRange();
        }

        TcpClient tcp;

        string conn = "Server=DESKTOP-HP0MNU0;Database=SO-563;User Id=sa;Password=12345;TrustServerCertificate=True;";
        //string conn = "Server=SHIVINFUSED;Database=SO-563;User Id=sa;Password=12345;TrustServerCertificate=True;";

        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader reader;
        SqlDataAdapter da;
        string filepath;



        string leakIp = "192.168.3.250";

        int leakPort = 9004;
        int modbusport = 9003;


        private void fetchPartname()
        {
            using (con = new SqlConnection(conn))
            {
                con.Open();
                using (cmd = new SqlCommand("Select Partname from tblItemMaster Where PartCode=@Partcode", con))
                {
                    cmd.Parameters.AddWithValue("@Partcode", textBox1.Text);
                    using (da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            textBox5.Text = dt.Rows[0][0].ToString();
                        }
                    }
                }
            }
        }

        private void popup_Load(object sender, EventArgs e)
        {

        }

        private void popup_FormClosed(object sender, FormClosedEventArgs e)
        {

           
            Application.Exit();
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

        private async void writeToDataRegister(ushort register, ushort Value)
        {
            try
            {
                using (TcpClient plc = new TcpClient())
                {
                    await plc.ConnectAsync(leakIp, modbusport);
                    if (plc.Connected)
                    {
                        byte slaveAddress = 1;
                        ushort registerAddress = register;
                        ushort value = Value;
                        var master = factory.CreateMaster(plc);
                        await master.WriteSingleRegisterAsync(slaveAddress, registerAddress, value);
                        AddtoRichTextBox("Write Successful.");
                    }
                }
            }
            catch (Exception ex)
            {
                AddtoRichTextBox($"Error :{ex}");
                panel2.BackColor = Color.Red;
            }
        }

        private void partialInsert()
        {
            using (con = new SqlConnection(conn))
            {
                con.Open();
                using (cmd = new SqlCommand("Insert into LaserSerialNo (LeakValue,Status,Partcode,Partname,Format,LaserSerialId,Range) Values" +
                    " (@leak,@status,@partcode,@partname,@format,@Id,@range)", con))
                {
                    DateTime dt = DateTime.Now;
                    cmd.Parameters.AddWithValue("@leak", textBox7.Text);
                    cmd.Parameters.AddWithValue("@status", textBox9.Text);

                    cmd.Parameters.AddWithValue("@partcode", textBox1.Text);
                    cmd.Parameters.AddWithValue("@partname", textBox5.Text);
                    cmd.Parameters.AddWithValue("@format", textBox2.Text);
                    cmd.Parameters.AddWithValue("@Id", Convert.ToInt32(textBox6.Text));
                    cmd.Parameters.AddWithValue("@range", int.Parse(label14.Text));

                    int rows = cmd.ExecuteNonQuery();
                    AddtoRichTextBox("Partial Insertion Successful.");
                   


                }
            }
        }

        private void NextMarkingId()
        {
            try
            {
                using (con = new SqlConnection(conn))
                {
                    con.Open();
                    using (cmd = new SqlCommand("sp_GetMaxSERIALId", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Format", textBox2.Text);
                        cmd.ExecuteScalar();
                        using (da = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dataTable = new DataTable())
                            {
                                da.Fill(dataTable);
                                if (dataTable.Rows.Count > 0)
                                {

                                    textBox6.Text = dataTable.Rows[0][0].ToString();
                                }
                                
                            }
                        }
                    }
                }
            }
            catch { }

        }
        private void NextMarkingData()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conn))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_GetMaxSERIALNo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Partcode", SqlDbType.VarChar).Value = textBox1.Text;
                        cmd.Parameters.Add("@Format", SqlDbType.VarChar).Value = textBox2.Text;

                        object result = cmd.ExecuteScalar(); // Fetch single value

                        if (result != null && result != DBNull.Value)
                        {
                            textBox8.Text = result.ToString();

                        }
                        else
                        {
                            AddtoRichTextBox("Error: No Markingdata found for particular Partcode And Format.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddtoRichTextBox($"Error in Fetching NextMarking Data: {ex.Message}");
            }
        }

        private void SaveMarkingDataToFile()
        {
            try
            {
                richTextBox2.AppendText(textBox8.Text);
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NextMarkingData.txt");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.WriteAllText(filePath, richTextBox1.Text);

                AddtoRichTextBox($"The data: {richTextBox2.Text}\nsuccessfully saved to location: {filePath}");

            }
            catch (Exception ex)
            {

                richTextBox1.Clear();
                richTextBox1.Clear();
                richTextBox1.SelectionColor = System.Drawing.Color.Red;
                richTextBox1.AppendText($"Error saving data to file: {ex.Message}" + Environment.NewLine);
                richTextBox1.SelectionColor = System.Drawing.Color.Black;

            }
        }

        private void insertFullDetails()
        {
            try
            {
                using (con = new SqlConnection(conn))
                {
                    con.Open();
                    using (cmd = new SqlCommand("Update LaserSerialNo Set MarkingData=@marking, Date=Cast(GetDate() As Date),Time=Cast(GetDate() AS Time(7)) Where Partcode=@partcode And LaserSerialId=@id", con))
                    {
                        cmd.Parameters.AddWithValue("@marking", textBox8.Text);
                        cmd.Parameters.AddWithValue("@partcode", textBox1.Text);
                        cmd.Parameters.AddWithValue("@id", textBox6.Text);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            AddtoRichTextBox("Final Insertion successful for Status OK");
                        }
                       
                    }
                }

            }
            catch (Exception ex) { AddtoRichTextBox($"Error: {ex}"); }

        }


        private void fetchRange()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conn))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Top 1  [Range]  FROM FormatTbl    WHERE [Format] = @format AND [Date] = Cast(Getdate() as date)", con))
                    {


                        cmd.Parameters.Add("@format", SqlDbType.VarChar).Value = textBox2.Text;

                        object result = cmd.ExecuteScalar(); // Fetch single value

                        if (result != null && result != DBNull.Value)
                        {
                            label14.Text = result.ToString();

                        }
                        else
                        {
                            AddtoRichTextBox("Error: No Markingdata found for particular Partcode And Format.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddtoRichTextBox($"Error in Fetching NextMarking Data: {ex.Message}");
            }
        }

        private void fillDatatable()
        {
            try
            {
                using (con = new SqlConnection(conn))

                {
                    con.Open();
                    using (cmd = new SqlCommand("SELECT * FROM LaserSerialNo WHERE CAST([Date] AS DATE) = CAST(GETDATE() AS DATE)", con))
                    {


                        using (da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                dataGridView1.DataSource = dt;
                                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:{ex}");
            }
        }

        private async void fetchLeak()
        {
            try
            {


                tcp = new TcpClient();//for leakValue
                await tcp.ConnectAsync(leakIp, leakPort);

                if (!tcp.Connected)
                {
                    panel1.BackColor = Color.OrangeRed;
                    return;
                }


                panel1.BackColor = Color.GreenYellow;

                NetworkStream ns = tcp.GetStream();

                byte[] buffer = new byte[1024];

                while (true)
                {


                    int count = await ns.ReadAsync(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        richTextBox2.Clear();
                        richTextBox1.Clear();
                        textBox8.Clear();
                        string Data = Encoding.ASCII.GetString(buffer, 0, count);
                        string[] received = Data.Split(':');
                        textBox7.Text = received[0];
                        textBox9.Text = received[1];
                        fetchRange();
                        if (textBox9.Text == "OK")
                        {

                            NextMarkingId();
                            NextMarkingData();
                            partialInsert();
                            insertFullDetails();
                            SaveMarkingDataToFile();
                            writeToDataRegister(5, 1);
                            panel3.BackColor = System.Drawing.Color.GreenYellow;
                        }
                        else
                        {
                            writeToDataRegister(5, 2);
                            partialInsert();
                            panel3.BackColor = System.Drawing.Color.OrangeRed;

                        }
                        fillDatatable();



                    }
                }



            }
            catch (Exception ex)
            {
                AddtoRichTextBox($"{ex}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            fetchLeak();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            textBox3.Text = dt.ToString("dd-MM-yyyy");
            textBox4.Text = dt.ToString("HH:mm:ss");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Report report=new Report();
            report.Show();
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }
    }
}
