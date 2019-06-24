using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LM2L
{
    public partial class ModelThingy : Form
    {
        public ModelThingy()
        {
            InitializeComponent();

            glViewport = new GLControl(new GraphicsMode(32, 24, 8), 3, 3, GraphicsContextFlags.ForwardCompatible);
        }

        #region OGL Stuff

        // r/therewasanattempt

        private Vector2 InitialMov;
        private Vector3 MdlCenter;
        private Vector3 Translation;
        private Matrix4 Transform;
        private float Dimension;

        private void GlViewport_Load(object sender, EventArgs e)
        {
            Console.WriteLine("OGL Load");
            glViewport.MakeCurrent();
        }

        private void GlViewport_Paint(object sender, PaintEventArgs e)
        {
            Console.WriteLine("OGL Paint");
        }

        private void GlViewport_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("OGL MouseDown");
            if (e.Button != 0)
            {
                InitialMov = new Vector2(e.X, e.Y);
            }
        }

        private void GlViewport_MouseMove(object sender, MouseEventArgs e)
        {
            Console.WriteLine("OGL MouseMove");
            if (e.Button != 0)
            {
                if ((e.Button & MouseButtons.Left) != 0)
                {
                    float X = (float)(((e.X - InitialMov.X) / Width) * Math.PI);
                    float Y = (float)(((e.Y - InitialMov.Y) / Height) * Math.PI);

                    Transform.Row3.Xyz -= Translation;

                    Transform *=
                        Matrix4.CreateRotationX(Y) *
                        Matrix4.CreateRotationY(X);

                    Transform.Row3.Xyz += Translation;
                }

                if ((e.Button & MouseButtons.Right) != 0)
                {
                    float X = (InitialMov.X - e.X) * Dimension * 0.005f;
                    float Y = (InitialMov.Y - e.Y) * Dimension * 0.005f;

                    Vector3 Offset = new Vector3(-X, Y, 0);

                    Translation += Offset;

                    Transform *= Matrix4.CreateTranslation(Offset);
                }

                InitialMov = new Vector2(e.X, e.Y);

                //UpdateTransforms();
            }
        }

        private void GlViewport_MouseWheel(object sender, MouseEventArgs e)
        {
            Console.WriteLine("OGL MouseWheel");
            if (e.Button != MouseButtons.Right)
            {
                float Step = e.Delta > 0
                    ? Dimension * 0.025f
                    : -Dimension * 0.025f;

                Translation.Z += Step;

                Transform *= Matrix4.CreateTranslation(0, 0, Step);

                //UpdateTransforms();
            }
        }

        private void GlViewport_Resize(object sender, EventArgs e)
        {
            Console.WriteLine("OGL Resize");
            //Renderer?.Resize(Viewport.Width, Viewport.Height);

            glViewport.Invalidate();
        }

        #endregion

        #region File Handling
        enum DataTypes : uint
        {
            unk001 = 0x0201B501,
            unk002 = 0x12017103,
            unk003 = 0x12017104,
            unk004 = 0x12017105, //some sort of table
            unk005 = 0x1201B002,
            submeshInfo = 0x1201B003,
            vtxStartOffset = 0x1201B004, //vertex start offset pointers for submeshes
            unk006 = 0x1201B006, //material(s)?
            unk007 = 0x1201B007, //lotsa 0xFFFFFFFF with some data inbetween
            unk008 = 0x1201B008,
            unk009 = 0x1201B009,
            unk010 = 0x1201B101,
            unk011 = 0x1201B102,
            unk012 = 0x1201B103,
            unk013 = 0x1301B001, //mostly 0x3F800000 and null, always of size 0x40
            meshData = 0x1301B005, //vertex and index buffer
            texture = 0x1701B502, //texture
            unk014 = 0x9201B100
        }

        class FileEntry
        {
            public DataTypes type;
            public uint length;
            public uint offset;

            public FileEntry(DataTypes type, uint length, uint offset)
            {
                this.type = type;
                this.length = length;
                this.offset = offset;
            }
        }

        class submeshInfo
        {
            public uint indexStartOffset; //relative to buffer start
            public ushort indexCount; //divide by 3 to get face count
            public ushort idkEven1;
            public ushort idkEven2; //increments by 0x4 with each subsequent entry
            public ushort idkEven3;
            public ulong dataFormat;
            public uint idkEven4; //always null?
            public uint idkEven5; //always null?
            public uint idkEven6; //increments with each subsequent entry
            public ushort vertexCount;
            public ushort idkEven7; //always 0x100?
            public uint hashID;

            public submeshInfo(uint indexStartOffset, ushort indexCount, ushort idkEven1, ushort idkEven2, ushort idkEven3, ulong dataFormat, uint idkEven4,
                uint idkEven5, uint idkEven6, ushort vertexCount, ushort idkEven7, uint hashID)
            {
                this.indexStartOffset = indexStartOffset;
                this.indexCount = indexCount;
                this.idkEven1 = idkEven1;
                this.idkEven2 = idkEven2;
                this.idkEven3 = idkEven3;
                this.dataFormat = dataFormat;
                this.idkEven4 = idkEven4;
                this.idkEven5 = idkEven5;
                this.idkEven6 = idkEven6;
                this.vertexCount = vertexCount;
                this.idkEven7 = idkEven7;
                this.hashID = hashID;
            }
        }

        /// <summary>
        /// Represents a texture
        /// </summary>
        class texture
        {
            public uint length;
            public uint offset;
            public ushort width;
            public ushort height;
            public byte miplevel;
            public bool alpha; //true - ETC1A4, false - ETC1
            public uint hashID;
        }

        /// <summary>
        /// Represents model data that includes vertex and other stuff and index data
        /// </summary>
        class modelData
        {
            public uint length;
            public uint offset;

            public modelData()
            {

            }

            public modelData(uint length, uint offset)
            {
                this.length = length;
                this.offset = offset;
            }
        }

        /// <summary>
        /// Represents a group of models that share the same data
        /// </summary>
        class modelGroup
        {
            //public List<material> materials = new List<material>();
            public modelData mdlData = new modelData();
            public List<uint> vtxPointers = new List<uint>();
            //that other weird section here
            public List<submeshInfo> submeshMeta = new List<submeshInfo>();
            //Everything else goes here
            //As you can probably tell, I have no idea what other
            //sections do ATM, so that comment will have to represent them
        }

        /// <summary>
        /// Parses model data from LM2 files
        /// </summary>
        /// <param name="file000path">Path to file000</param>
        /// <param name="file002path">Path to file002</param>
        /// <param name="file003path">Path to file003</param>
        public static void parseFiles(string file000path, string file002path, string file003path)
        {
            //Create lists and variables
            List<FileEntry> files = listFileContents(file000path);
            List<texture> textures = new List<texture>();
            List<modelGroup> groups = new List<modelGroup>();
            modelGroup group = null; //Current model group that we append stuff to
            uint count = 0; //Temporary counter

            //Open readers
            BinaryReader br2 = new BinaryReader(File.OpenRead(file002path));
            BinaryReader br3 = new BinaryReader(File.OpenRead(file003path));

            //Actual parsing
            foreach (var file in files)
            {
                switch (file.type)
                {
                    case DataTypes.texture:
                        var temp = ReadTextureMeta(br2, file.length, file.offset);
                        if (temp != null) textures.Add(temp);
                        break;
                    case DataTypes.unk006:
                        //This section always begins a new model group
                        if (group != null) groups.Add(group);
                        group = new modelGroup();
                        break;
                    case DataTypes.meshData:
                        group.mdlData = new modelData(file.length, file.offset);
                        break;
                    case DataTypes.vtxStartOffset:
                        br3.BaseStream.Position = file.offset; //Jump to location
                        count = file.length / 0x4;
                        for (uint i = 0; i < count; i++) group.vtxPointers.Add(br3.ReadUInt32());
                        break;
                    case DataTypes.submeshInfo:
                        br3.BaseStream.Position = file.offset; //Jump to location
                        if (count != (file.length / 0x28)) Console.WriteLine("A mismatch happened with model groups!");
                        count = file.length / 0x28;
                        for (uint i = 0; i < count; i++)
                            group.submeshMeta.Add(new submeshInfo(br3.ReadUInt32(), br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt64(), br3.ReadUInt32(), br3.ReadUInt32(), br3.ReadUInt32(), br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt32()));
                        break;
                }
            }
            //Additional check for leftover model group
            if (group != new modelGroup()) groups.Add(group);
        }

        static texture ReadTextureMeta(BinaryReader br, uint length, uint offset)
        {
            if (br.ReadUInt32() != 0xE977D350) return null; //Just in case we read a different section
            texture output = new texture();
            output.length = length;
            output.offset = offset;
            output.hashID = br.ReadUInt32();
            if (br.ReadUInt32() != length)
            {
                Console.WriteLine("Oops! A mismatch happened in texture meta...");
                br.BaseStream.Position += 0x2C; //Skip the rest of this section, just in case
                return null;
            }
            br.BaseStream.Position += 0x4 + 0x8; //Skip hashID copy and unknown stuff
            output.width = br.ReadUInt16();
            output.height = br.ReadUInt16();
            br.BaseStream.Position += 0x3; //Skip more
            output.miplevel = (byte)(br.ReadByte() / 17);
            br.BaseStream.Position += 0x14; //More skipping
            byte fmt = br.ReadByte();
            if (fmt == 12) output.alpha = false;
            else if (fmt == 13) output.alpha = true;
            br.BaseStream.Position += 0x3;

            return output;
        }

        /// <summary>
        /// Parses file entries in file000
        /// </summary>
        /// <param name="file000path">Path to file000</param>
        static List<FileEntry> listFileContents(string file000path)
        {
            //Preps
            List<FileEntry> entries = new List<FileEntry>(); //Create list
            BinaryReader br = new BinaryReader(File.OpenRead(file000path)); //Open file

            //Skip useless stuff
            while (br.ReadUInt32() == 0x2001301)
            {
                br.BaseStream.Position += 0x14; //Skip all 0x2001301 sections, since the purpose of those is unknown as of this point
            }
            br.BaseStream.Position -= 0x4; //Go four bytes back, because the previous loop read these to check for next section

            //Read the entries
            while (br.BaseStream.Position < br.BaseStream.Length) //Loop through as many entries as we can before the file ends
            {
                try //Put this whole contraption into a try-catch clause, just in case we reach EOF or something else goes wrong
                {
                    entries.Add(new FileEntry((DataTypes)br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32()));

                    //vertex/index data format identifiers:
                    //0x63503799 72D28D0D - short vertex, byte index
                    //0xDC0291B3 11E26127 - short vertex, ushort index
                    //0x93359708 679BEB7C
                    //0x1A833CEE C88C1762
                    //0xD81AC10B 8980687F
                }
                catch
                {
                    //Don't do anything if something goes wrong
                }
            }

            return entries;
        }

        #endregion
    }
}
