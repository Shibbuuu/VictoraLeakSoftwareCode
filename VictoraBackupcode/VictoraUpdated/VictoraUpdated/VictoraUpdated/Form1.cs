
using System.Data;
namespace VictoraUpdated
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            

            fetchPartcode();
        }

        string conn = "Server=DESKTOP-HP0MNU0;Database=SO-563;User Id=sa;Password=12345;Encrypt=False;TrustServerCertificate=True;";
        //string conn = "Server=SHIVINFUSED;Database=SO-563;User Id=sa;Password=12345;Encrypt=False;TrustServerCertificate=True;";



        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader reader;
        SqlDataAdapter da;
        string filepath;

        private async void fetchPartcode()
        {
            try
            {
                comboBox2.DataSource = null;

                using (SqlConnection con = new SqlConnection(conn))
                {
                    await con.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SELECT Id, PartCode FROM tblItemMaster", con))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            comboBox2.DataSource = dt;
                            comboBox2.DisplayMember = "PartCode";
                            comboBox2.ValueMember = "Id";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox2.Text=="2026")
            {
                popup popup = new popup();
                popup.Show();
            }
            else
            {
                MessageBox.Show("Please Enter right Password.");
            }
            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
