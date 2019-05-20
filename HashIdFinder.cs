using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LM2L
{
    public partial class HashIdFinder : Form
    {
        public Dictionary<uint, string> hashIdBank; //This is where we're gonna put the hashID with their paired string

        public HashIdFinder()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Open and parse hashid.bin
            BinaryReader br = new BinaryReader(File.OpenRead(textBox1.Text));
            hashIdBank = new Dictionary<uint, string>(); //Initialize hashIdBank
            uint entryCount = UInt32_ReverseEndianness(br.ReadUInt32()) - 1; //First uint is entry counter, the entry counter plus some unk value is also considered to be an entry so -1
            br.BaseStream.Position += 0x4; //Skip unk value
            List<entry> entries = new List<entry>();
            for (uint i = 0; i < entryCount; i++)
            {
                var currEntry = new entry();
                currEntry.strOffset = UInt32_ReverseEndianness(br.ReadUInt32());
                currEntry.hashID = UInt32_ReverseEndianness(br.ReadUInt32());
                entries.Add(currEntry);
            }
            uint strStartOffset = ((entryCount + 1) * 8) + 4;
            foreach (var currEntry in entries)
            {
                br.BaseStream.Position = strStartOffset + currEntry.strOffset;
                string temp = ReadNullTerminatedAsciiStringFromBR(br);
                hashIdBank.Add(currEntry.hashID, temp);
            }
            br.Close();
            toolStripStatusLabel1.Text = "Loaded hashlist!";
        }
        public class entry
        {
            public uint strOffset;
            public uint hashID;
        }

        public static string ReadNullTerminatedAsciiStringFromBR(BinaryReader br)
        {
            List<byte> list = new List<byte>();
            byte current = br.ReadByte();
            list.Add(current);
            current = br.ReadByte();
            while (current != 0x0)
            {
                list.Add(current);
                current = br.ReadByte();
            }
            return Encoding.ASCII.GetString(list.ToArray());
        }

        public static uint UInt32_ReverseEndianness(uint input)
        {
            var thing = BitConverter.GetBytes(input);
            Array.Reverse(thing);
            var thing2 = BitConverter.ToUInt32(thing, 0);
            return thing2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            hashIdBank = new Dictionary<uint, string>();
            toolStripStatusLabel1.Text = "Cleared hashlist!";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                textBox3.Text = hashIdBank[Convert.ToUInt32(textBox2.Text, 16)];
                toolStripStatusLabel1.Text = "Found hashID!";
            }
            catch
            {
                toolStripStatusLabel1.Text = "Woops! Couldn't find hashID!";
            }
        }
    }
}
