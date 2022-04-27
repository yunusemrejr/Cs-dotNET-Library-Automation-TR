using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KutuphaneOtomasyonuUygulamasi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MySqlConnection con = new MySqlConnection("host=localhost;user=root;password=;port=3306;database=kutuphane");
            DataSet ds;
            MySqlDataAdapter da;
            ds = new DataSet();
            con.Open();
            da = new MySqlDataAdapter("select * from kitaplar", con);
            da.Fill(ds, "kutuphane");
            

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = "kutuphane";



            con.Close();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        

        private void button4_Click(object sender, EventArgs e)
        {

            int rowIndex = dataGridView1.CurrentCell.RowIndex;
            dataGridView1.Rows.RemoveAt(rowIndex);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string MyConnection2 = "datasource=localhost;port=3306;username=root;password=;database=kutuphane";
                string Query = "INSERT INTO kitaplar VALUES (" + this.textBox6.Text + ",'" + this.textBox3.Text + "','" + this.textBox4.Text + "'," + this.textBox5.Text  + ");";
                MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
                MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
                MySqlDataReader MyReader2;
                MyConn2.Open();
                MyReader2 = MyCommand2.ExecuteReader();
                MessageBox.Show("kitap eklendi!");
                while (MyReader2.Read())
                {
                }
                MyConn2.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("problem var: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string MyConnection2 = "datasource=localhost;port=3306;username=root;password=;database=kutuphane";
                string Query = "delete from kutuphane.kitaplar where ID='" + this.textBox7.Text + "';";
                MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
                MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
                MySqlDataReader MyReader2;
                MyConn2.Open();
                MyReader2 = MyCommand2.ExecuteReader();
                MessageBox.Show("silme gerçekleşti");
                while (MyReader2.Read())
                {
                }
                MyConn2.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("problem var: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
             
                string MyConnection2 = "datasource=localhost;port=3306;username=root;password=;database=kutuphane";
                string Query = "select *from kutuphane.login where username='" + this.textBox1.Text + "' AND password='"+ this.textBox2.Text + "';";
                MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
                MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
                MySqlDataReader MyReader2;
                MyConn2.Open();
 
                MyReader2 = MyCommand2.ExecuteReader();

                if (MyReader2.HasRows)
                {
                    MessageBox.Show("GİRİŞ BAŞARILI");

                }
                else
                {
                    MessageBox.Show("YANLIŞ BİLGİ");

                }
                
                MyConn2.Close();
             
             
        }
    }
}
