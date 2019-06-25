﻿using OpenTK;
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

        class SubmeshInfo
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

            public SubmeshInfo(uint indexStartOffset, ushort indexCount, ushort idkEven1, ushort idkEven2, ushort idkEven3, ulong dataFormat, uint idkEven4,
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
        class Texture
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
        class ModelData
        {
            public uint length;
            public uint offset;

            public ModelData()
            {

            }

            public ModelData(uint length, uint offset)
            {
                this.length = length;
                this.offset = offset;
            }
        }

        /// <summary>
        /// Represents a group of models that share the same data
        /// </summary>
        class ModelGroup
        {
            //public List<material> materials = new List<material>();
            public ModelData mdlData = new ModelData();
            public List<uint> vtxPointers = new List<uint>();
            //that other weird section here
            public List<SubmeshInfo> submeshMeta = new List<SubmeshInfo>();
            //Everything else goes here
            //As you can probably tell, I have no idea what other
            //sections do ATM, so that comment will have to represent them
        }

        class Triangle
        {
            public ushort vertex1;
            public ushort vertex2;
            public ushort vertex3;

            public Triangle(ushort vertex1, ushort vertex2, ushort vertex3)
            {
                this.vertex1 = vertex1;
                this.vertex2 = vertex2;
                this.vertex3 = vertex3;
            }
        }

        /// <summary>
        /// Decodes floating point numbers encoded as signed 16-bit integers.
        /// <para>Note: this code assumes the input is in LE byte order.</para>
        /// </summary>
        /// <param name="input">Float encoded as a 16-bit signed integer.</param>
        /// <returns>Single precision floating point number decoded from input.</returns>
        public static float UShortToFloatDecode(short input)
        {
            //I'll have to write a short doc here, because the entirety of the internet
            //doesn't have even a single explaination on how this actually works, and I
            //really struggled because of that.

            //This assumes the ushort is in LE, for BE the values are swapped around.
            //Basically the first byte stores the fraction of the number, we can calculate
            //it by normalizing the 0-255 range to be 0.0-1.0(not to 1.0 but close).
            //The second byte is the signed integer part, it doesn't even have to be 
            //normalized.
            //Then we just add the two together to get the final result!
            //The same can be achieved by just dividing the whole short by 256 as float.
            //I split it just so one can understand the working principle better.

            float fraction = (float)BitConverter.GetBytes(input)[0] / (float)256; //VS says the cast is redundant, but in fact VS is retarded, because it doesn't divide as pure float if you don't cast the numbers
            sbyte integer = (sbyte)BitConverter.GetBytes(input)[1];
            return integer + fraction;
        }

        public static Vector2 NormalizeSignedUvCoordsToFloat(short inU, short inV)
        {
            float U = 0;
            float V = 0;
            //Normalize U coordinate
            if (inU >= 0)
            {
                U = ((float)inU / (float)0xFFFE) + 0.5f; //Normalize positive range
            }
            else
            {
                U = ((float)inU / (float)0xFFFE) + 0.5f; //Normalize negative range
            }
            //Normalize V coordinate
            if (inV >= 0)
            {
                V = ((float)inV / (float)0xFFFE) + 0.5f; //Normalize positive range
            }
            else
            {
                V = ((float)inV / (float)0xFFFE) + 0.5f; //Normalize negative range
            }
            return new Vector2(U, V);
        }

        public static Vector2 NormalizeUvCoordsToFloat(ushort inU, ushort inV)
        {
            float U = 0;
            float V = 0;
            //Normalize U coordinate
            U = ((float)inU / (float)0xFFFF);
            //Normalize V coordinate
            V = ((float)inV / (float)0xFFFF);
            return new Vector2(U, V);
        }


        /// <summary>
        /// Parses model data from LM2 files
        /// </summary>
        /// <param name="file000path">Path to file000</param>
        /// <param name="file002path">Path to file002</param>
        /// <param name="file003path">Path to file003</param>
        public static void ParseFiles(string file000path, string file002path, string file003path)
        {
            //Create lists and variables
            List<FileEntry> files = listFileContents(file000path);
            List<Texture> textures = new List<Texture>();
            List<ModelGroup> groups = new List<ModelGroup>();
            ModelGroup group = null; //Current model group that we append stuff to
            uint count = 0; //Temporary counter

            //Open readers
            BinaryReader br2 = new BinaryReader(File.OpenRead(file002path));
            BinaryReader br3 = new BinaryReader(File.OpenRead(file003path));

            //Metadata parsing, sets everything up for later stages
            //This is obviously far from complete, as IDK most of
            //format identifiers
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
                        group = new ModelGroup();
                        break;
                    case DataTypes.meshData:
                        group.mdlData = new ModelData(file.length, file.offset);
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
                            group.submeshMeta.Add(new SubmeshInfo(br3.ReadUInt32(), br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt64(), br3.ReadUInt32(), br3.ReadUInt32(), br3.ReadUInt32(), br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt32()));
                        break;
                }
            }
            //Save the last model group, as there's no section that signals the end of it
            if (group != new ModelGroup()) groups.Add(group);

            //Parsing of the model data
            foreach (var group2 in groups)
            {
                //Read each mesh
                for (int i = 0; i < group2.vtxPointers.Count; i++)
                {
                    SubmeshInfo currentSubmesh = group2.submeshMeta[i];

                    List<Vector3> vertices = new List<Vector3>();
                    List<Vector2> texCoords = new List<Vector2>();
                    List<Triangle> faces = new List<Triangle>();

                    //from /art/feghosts/ghostchaser
                    //0x63503799 72D28D0D
                    //0xDC0291B3 11E26127
                    //0x93359708 679BEB7C
                    //0x1A833CEE C88C1762
                    //0xD81AC10B 8980687F

                    //from /art/levels/global
                    //0x2AA2C56A 0FFA5BDE
                    //0x5D6C62BA B3F4492E
                    //0x3CC7AB6B 4821B2DF
                    //0x408E2B1F 5576A693
                    //0x0B663399 DF24890D
                    //0x7EB9853D F4F13EB1
                    //0x314A20AE FADABB22
                    //0x0F3F68A2 87C2B716
                    //0x27F99377 1090E6EB
                    //0x4E315C83 A856FBF7

                    //from /art/levels/bunker/bundle
                    //0xF0892648 0A2ABABC
                    //0xBD15F722 F07FC596
                    //0xDC81A4F8 C34EE96C
                    //0xFBACD243 DDCC31B7

                    switch (currentSubmesh.dataFormat)
                    {
                        case 0x6350379972D28D0D:
                            //This format uses bytes to store indices and short float for vertices
                            //Read faces
                            br3.BaseStream.Position = group2.mdlData.offset + currentSubmesh.indexStartOffset; //Go to index start offset, adding it with mdlData.offset because the offset in SubmeshInfo is relative
                            for (int c = 0; c < currentSubmesh.indexCount / 3; c++) faces.Add(new Triangle(br3.ReadByte(), br3.ReadByte(), br3.ReadByte())); //Read a triangles/faces
                            //Read vertex/data block...how do I even call this!?
                            //Length of "data block" is 0x46, includes XYZ vertex coords in short float
                            //and UVs in ushort and some dummy data
                            br3.BaseStream.Position = group2.mdlData.offset + group2.vtxPointers[i]; //Go to data start offset, again adding two values because vtxPointers is relative
                            for (int c = 0; c < currentSubmesh.vertexCount; c++)
                            {
                                vertices.Add(new Vector3(UShortToFloatDecode(br3.ReadInt16()), UShortToFloatDecode(br3.ReadInt16()), UShortToFloatDecode(br3.ReadInt16()))); //Read vertex coordinates
                                br3.BaseStream.Position += 0x4; //Skip two unknown values
                                //float U = (float)br3.ReadUInt16() / (float)0xFFFF; //Read and normalize U coordinate
                                //float V = (float)br3.ReadUInt16() / (float)0xFFFF; //Read and normalize V coordinate
                                //texCoords.Add(new Vector2(UShortToFloatDecode(br3.ReadInt16()), UShortToFloatDecode(br3.ReadInt16())));
                                texCoords.Add(NormalizeUvCoordsToFloat(br3.ReadUInt16(), br3.ReadUInt16()));
                                //texCoords.Add(new Vector2(br3.ReadInt16(), br3.ReadInt16()));
                                br3.BaseStream.Position += 0x38;
                            }
                            WriteWavefrontObj(currentSubmesh.hashID, vertices, texCoords, faces);
                            break;
                        case 0xDC0291B311E26127:
                            break;
                        case 0x93359708679BEB7C:
                            //Luigi's model in /art/levels/global
                            //This format uses ushorts to store indices and short float for vertices
                            //Read faces
                            br3.BaseStream.Position = group2.mdlData.offset + currentSubmesh.indexStartOffset; //Go to index start offset, adding it with mdlData.offset because the offset in SubmeshInfo is relative
                            for (int c = 0; c < currentSubmesh.indexCount / 3; c++) faces.Add(new Triangle(br3.ReadUInt16(), br3.ReadUInt16(), br3.ReadUInt16())); //Read a triangles/faces
                            //Read vertex/data block...how do I even call this!?
                            //Length of "data block" is 0x46, includes XYZ vertex coords in short float
                            //and UVs in ushort and some dummy data
                            br3.BaseStream.Position = group2.mdlData.offset + group2.vtxPointers[i]; //Go to data start offset, again adding two values because vtxPointers is relative
                            for (int c = 0; c < currentSubmesh.vertexCount; c++)
                            {
                                vertices.Add(new Vector3(UShortToFloatDecode(br3.ReadInt16()), UShortToFloatDecode(br3.ReadInt16()), UShortToFloatDecode(br3.ReadInt16()))); //Read vertex coordinates
                                br3.BaseStream.Position += 0x4; //Skip two unknown values
                                //float U = (float)br3.ReadUInt16() / (float)0xFFFF; //Read and normalize U coordinate
                                //float V = (float)br3.ReadUInt16() / (float)0xFFFF; //Read and normalize V coordinate
                                //texCoords.Add(new Vector2(UShortToFloatDecode(br3.ReadInt16()), UShortToFloatDecode(br3.ReadInt16())));
                                texCoords.Add(NormalizeUvCoordsToFloat(br3.ReadUInt16(), br3.ReadUInt16()));
                                //texCoords.Add(new Vector2(br3.ReadInt16(), br3.ReadInt16()));
                                br3.BaseStream.Position += 0x8;
                            }
                            WriteWavefrontObj(currentSubmesh.hashID, vertices, texCoords, faces);
                            break;
                        case 0x1A833CEEC88C1762:
                            break;
                        case 0xD81AC10B8980687F:
                            break;
                        case 0x2AA2C56A0FFA5BDE:
                            break;
                        case 0x5D6C62BAB3F4492E:
                            break;
                        case 0x3CC7AB6B4821B2DF:
                            break;
                        case 0x408E2B1F5576A693:
                            break;
                        case 0x0B663399DF24890D:
                            break;
                        case 0x7EB9853DF4F13EB1:
                            break;
                        case 0x314A20AEFADABB22:
                            break;
                        case 0x0F3F68A287C2B716:
                            break;
                        case 0x27F993771090E6EB:
                            break;
                        case 0x4E315C83A856FBF7:
                            break;
                        case 0xF08926480A2ABABC:
                            break;
                        case 0xBD15F722F07FC596:
                            break;
                        case 0xDC81A4F8C34EE96C:
                            break;
                        case 0xFBACD243DDCC31B7:
                            break;
                        default:
                            //Default case for when unimplemented model format is used
                            Console.WriteLine("Unimplemented model data format! 0x" + string.Format("{0:X16}", currentSubmesh.dataFormat));
                            break;
                    }
                    
                }

                //Mix meshes that have the same hashID
            }
        }

        /// <summary>
        /// Saves the decoded model data into a Wavefront OBJ file.
        /// </summary>
        /// <param name="hashID">hashID of the model</param>
        /// <param name="vertices">List of vertex coordinates</param>
        /// <param name="texCoords">List of texture coordinates</param>
        /// <param name="faces">List of faces</param>
        static void WriteWavefrontObj(uint hashID, List<Vector3> vertices, List<Vector2> texCoords, List<Triangle> faces)
        {
            string obj = ""; //This is where we'll be writing our output
            for (int i = 0; i < vertices.Count; i++)
            {
                obj += "v " + vertices[i].X.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + " " + vertices[i].Y.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + " " + vertices[i].Z.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + "\r\n";
                obj += "vt " + texCoords[i].X.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + " " + texCoords[i].Y.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + "\r\n";
            }
            obj += "\r\n";
            for (int i = 0; i < faces.Count; i++)
            {
                obj += string.Format("f {0}/{0} {1}/{1} {2}/{2} \r\n", faces[i].vertex1 + 1, faces[i].vertex2 + 1, faces[i].vertex3 + 1);
            }
            File.WriteAllText("C:\\Users\\Sora\\Desktop\\mdltest\\" + hashID.ToString("X8") + ".obj", obj);
        }

        static Texture ReadTextureMeta(BinaryReader br, uint length, uint offset)
        {
            if (br.ReadUInt32() != 0xE977D350) return null; //Just in case we read a different section
            Texture output = new Texture();
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
