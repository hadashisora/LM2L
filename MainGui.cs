using System;
using System.Windows.Forms;
using static LM2L.Program;

namespace LM2L
{
    public partial class MainGui : Form
    {
        public MainGui()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Check if user left these paths empty
            if (textBox1.Text == null) { Console.WriteLine("No Powe file path"); return; }
            if (textBox2.Text == null) { Console.WriteLine("No textures file path!"); return; }
            DealWithTextures(textBox1.Text, textBox2.Text, checkBox1.Checked, checkBox2.Checked, checkBox3.Checked, checkBox7.Checked, checkBox9.Checked);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Check if user left these paths empty
            if (textBox3.Text == null) { Console.WriteLine("No .dict file path"); return; }
            if (textBox4.Text == null) { Console.WriteLine("No .dict file path!"); return; }
            DealWithDataDict(textBox3.Text, textBox4.Text, checkBox4.Checked, checkBox8.Checked, checkBox6.Checked, checkBox5.Checked);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new LM2L.Help().Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ParseFileZero(textBox5.Text);
        }
    }
}
