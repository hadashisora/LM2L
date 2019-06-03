using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace LM2L
{
    class Program
    {
        static string output = "";

        static void Main()
        {
            //Print some useless stuff to CMD before starting the GUI
            Console.WriteLine("Luigi's Mansion 2 tool by CHEMI6DER/TheFearsomeDzeraora");
            Console.WriteLine("Usage: use the GUI, I didn't bother with the CLI this time");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Important note: Both Powe and texture files must begin with");
            Console.WriteLine("the data itself, no preceding data is allowed.");
            Console.WriteLine("-----------");
            Console.WriteLine("Another important note: very little is actually known about");
            Console.WriteLine("file formats in this game, so if the program throws an");
            Console.WriteLine("exception related to file handling(not to incompetence of the");
            Console.WriteLine("user to correctly input the proper file paths) please report");
            Console.WriteLine("to the GitHub issues page with files that caused the crash in");
            Console.WriteLine("attachements to it.");
            var gui = new MainGui();
            Application.Run(gui);
        }

        //DebugPrint(text, toCMD, toTXT);

        public static void DealWithTextures(string PowePath, string TexPath, bool doMipmaps, bool toCMD, bool toTXT, bool numberFiles, bool flipY)
        {
            //Open both files in the beginning for reading so that if any of them don't exist
            //(e.g. if user made a typo in the file path) the app with crash before it does anything :)
            BinaryReader PoweReader = new BinaryReader(File.OpenRead(PowePath));
            BinaryReader TexReader = new BinaryReader(File.OpenRead(TexPath));
            output = ""; //Reset output log
            int counter = 0; //Counter for Powe entries
            List<Powe> entries = new List<Powe>();
            try
            {
                while (PoweReader.ReadUInt32() == 0xE977D350)
                {
                    var item = new Powe();
                    //Read data from file
                    item.number = counter;
                    item.id = PoweReader.ReadUInt32();
                    item.length = PoweReader.ReadUInt32();
                    item.idCopy = PoweReader.ReadUInt32();
                    PoweReader.BaseStream.Position += 0x8; //Skip some data that isn't useful for us
                    item.width = PoweReader.ReadUInt16();
                    item.height = PoweReader.ReadUInt16();
                    PoweReader.BaseStream.Position += 0x3; //Skip again
                    item.mipLevel = PoweReader.ReadByte();
                    PoweReader.BaseStream.Position += 0x14; //More skipping
                    item.texFmt = PoweReader.ReadByte();
                    PoweReader.BaseStream.Position += 0x3; //Skip over the end of current Powe section
                    //Print some debug info
                    DebugPrint("Powe Section:" + counter, toCMD, toTXT);
                    DebugPrint("\rID          : 0x" + Convert.ToString(item.id, 16), toCMD, toTXT);
                    DebugPrint("\rLength      : 0x" + Convert.ToString(item.length, 16), toCMD, toTXT);
                    DebugPrint("\rID copy     : 0x" + Convert.ToString(item.idCopy, 16), toCMD, toTXT);
                    DebugPrint("\rWidth       : " + item.width, toCMD, toTXT);
                    DebugPrint("\rHeight      : " + item.height, toCMD, toTXT);
                    DebugPrint("\rMipmap level: 0x" + Convert.ToString(item.mipLevel, 16), toCMD, toTXT);
                    DebugPrint("\rTexFmt      :" + ((CtrTexFormat)item.texFmt).ToString() + "\n\r", toCMD, toTXT); //Use CtrTexFormat enum to get the texture format name instead of just raw byte
                    counter++;
                    entries.Add(item);
                }
            }
            catch
            {
                //Exception catcher here, because this code is dumb and will go past the end of file without 
                //any concerns, which in turn causes an exception
                Console.WriteLine("Caught an exception. Probably reached EOF.");
            }
            if (toTXT) File.WriteAllText(Path.ChangeExtension(PowePath, "txt"), output); //Save the Powe log now, as we don't need this later
            string folderPath = Path.GetDirectoryName(PowePath) + "\\extract\\";
            Directory.CreateDirectory(folderPath);
            //And now onto actually extracting the textures!
            foreach (var entry in entries) //Loop through each entry in the Powe file
            {
                for (byte i = 0; i < entry.mipLevel / 17; i++)
                {
                    Console.WriteLine("Reading texture " + entry.number + " mipmap" + i);
                    ushort width = (ushort)(entry.width / Math.Pow(2, i)); //Calculate width of the current mipmap level(unchanged if level is 0)
                    Console.WriteLine(width);
                    ushort height = (ushort)(entry.height / Math.Pow(2, i)); //Calculate height of the current mipmap level(unchanged if level is 0)
                    Console.WriteLine(height);
                    if ((!doMipmaps && i == 0) || (doMipmaps)) //Logic for choosing if we should extract mipmaps or not
                    {
                        //Presonally, I don't find any purpose of extracting those since they're
                        //just downscaled versions of textures. I'm leaving it in just because
                        //the format supports this, not because there's a purpose for it
                        var testStream = new MemoryStream(); //Create a memory stream where we place all texture data from current texture offset to EOF
                        var temp = TexReader.BaseStream.Position; //Save the position into a variable, because CopyTo() is gonna overwrite it
                        TexReader.BaseStream.CopyTo(testStream); //Copy data from current offset to EOF
                        var texture = DecodeETC1(testStream.ToArray(), width, height, entry.texFmt == (byte)CtrTexFormat.ETC1_A4 ? true : false, flipY); //Make Ohana3DS-Rebirth code decompress the ETC1(A4) texture from this data
                        TexReader.BaseStream.Position = temp; //Restore the old position
                        if (doMipmaps) texture.Save(folderPath + (numberFiles ? (string.Format("{0:D2}", entry.number) + "_") : "") + Convert.ToString(entry.id, 16) + "_mip" + i + ".png", ImageFormat.Png); //Write texture file when extracting mipmaps. The only difference is "_mipmap№" being added at the end of filename
                        else texture.Save(folderPath + (numberFiles ? (string.Format("{0:D2}", entry.number) + "_") : "") + Convert.ToString(entry.id, 16) + ".png", ImageFormat.Png); //Same as above, just for when we don't extract mipmaps
                    }
                    //Calculate the adress of the next texture/mipmap with some simple math, not using the actual length parameter, because it counts the whole texture with mipmaps
                    if (entry.texFmt == (byte)CtrTexFormat.ETC1) TexReader.BaseStream.Position += (width * height) / 2; //Apparently the size of an etc1 texture in bytes is just (width*height)/2
                    if (entry.texFmt == (byte)CtrTexFormat.ETC1_A4) TexReader.BaseStream.Position += width * height; //And etc1a4 is just simply width*height
                }
            }
            //Write the first adress after textures in the file. This is useful because there's
            //usually other data(like models) after the textures
            Console.WriteLine("End of textures address: 0x" + Convert.ToString(TexReader.BaseStream.Position, 16) + "\r");
            //Close streams, because if we don't the program will keep the file open until you close it, and other
            //software doesn't really like files opened in C#(like for some reason even when reading the file the write
            //access is locked from all other programs which is strange)
            PoweReader.Close();
            TexReader.Close();
            Console.WriteLine("Done!");
        }

        public static void DealWithDataDict(string DataPath, string DictPath, bool outCompressed, bool outDecompressed, bool toCMD, bool toTXT)
        {
            //Open both files in the beginning for reading so that if any of them don't exist
            //(e.g. if user made a typo in the file path) the app with crash before it does anything :)
            BinaryReader DataReader = new BinaryReader(File.OpenRead(DataPath));
            BinaryReader DictReader = new BinaryReader(File.OpenRead(DictPath));
            output = ""; //Reset output log
            string basePath = Path.GetDirectoryName(DataPath) + "\\" + Path.GetFileNameWithoutExtension(DataPath) + "_unpack\\"; //Compute the "base" filepath which we'll use for outputting files and log using path of .data file
            if (Path.GetDirectoryName(DataPath) != Path.GetDirectoryName(DictPath)) Console.WriteLine(".dict and .data should be in the same directory!"); //Print a message into CMD if the paths don't match up
            if (DictReader.ReadUInt32() != 0xA9F32458) { Console.WriteLine("This is not a .dict file! I'm outa here!"); return; } //Check if it's really a .dict file or not
            DictReader.BaseStream.Position += 0x2; //Skip to offset 0x6
            bool isCompressed = DictReader.ReadByte() == 0x0 ? false : true; //If 0x0 - no compression, 0x1 - ZLib
            DebugPrint((isCompressed ? "compressed" : "uncompressed") + "\r\n", toCMD, toTXT);
            DictReader.BaseStream.Position += 0x1; //Skip to offset 0x8
            uint fileCount = DictReader.ReadUInt32();
            uint largestCompressedFile = DictReader.ReadUInt32();
            DictReader.BaseStream.Position = 0x2C; //Go to offset 0x2C
            byte[] unkArray = new byte[fileCount]; //IDK the purpose of this 'array', but might come useful later as it is the same length as there are files in this archive
            for (uint i = 0; i < fileCount; i++) unkArray[i] = DictReader.ReadByte();
            FileTableEntry[] fileInfo = new FileTableEntry[fileCount];
            for (uint i = 0; i < fileCount; i++) //Parse and load all those entries into the list
            {
                var entry = new FileTableEntry();
                entry.startOffset = DictReader.ReadUInt32();
                entry.decompressedLength = DictReader.ReadUInt32();
                entry.compressedLength = DictReader.ReadUInt32();
                entry.unk = DictReader.ReadUInt32();
                //for (uint a = 0; a < 4; a++) entry.unk[a] = DictReader.ReadByte();
                fileInfo[i] = entry;
                //Print some debug info TBD
                DebugPrint("File info   :" + i, toCMD, toTXT);
                DebugPrint("\rStart offset: 0x" + Convert.ToString(entry.startOffset, 16), toCMD, toTXT);
                DebugPrint("\rDecomp len  : 0x" + Convert.ToString(entry.decompressedLength, 16), toCMD, toTXT);
                DebugPrint("\rComp length : 0x" + Convert.ToString(entry.compressedLength, 16), toCMD, toTXT);
                DebugPrint("\runk         : 0x" + Convert.ToString(entry.unk, 16) + "\n\r", toCMD, toTXT);
            }
            //There are also two strings ".data\0" and ".debug\0" right after the file info
            if (toTXT) File.WriteAllText(Path.ChangeExtension(DictPath, "txt"), output); //Save the dict log now, as we don't need this later
            DictReader.Close(); //Close DictReader because we don't need it anymore
            //Now comes the fun part where we extract the files!
            //Yes, I know that I can simplify this and put everyting in the previous
            //for loop, but this code is mean more for uderstanding the file format
            //and experimenting rather than making it pretty and efficient.
            Directory.CreateDirectory(basePath);
            for (uint i = 0; i < fileCount; i++)
            {
                Console.WriteLine("Extracting file " + i);
                if (!isCompressed) //Extract a file if no compression is used
                {
                    //This code isn't affected by the flags that are set in the GUI, so a decompressed file will be output anyway :)
                    DataReader.BaseStream.Position = fileInfo[i].startOffset;
                    BinaryWriter br = new BinaryWriter(File.Create(basePath + string.Format("file{0:D3}", i)));
                    br.Write(DataReader.ReadBytes((int)fileInfo[i].decompressedLength));
                    br.Close();
                }
                else if (isCompressed) //Extract a file if compression is used
                {
                    DataReader.BaseStream.Position = fileInfo[i].startOffset;
                    byte[] compressedData = DataReader.ReadBytes((int)fileInfo[i].compressedLength);
                    //Split the thing in two for the functionality of the flags in the GUI
                    if (outCompressed)
                    {
                        BinaryWriter br = new BinaryWriter(File.Create(basePath + string.Format("file{0:D3}_zlib", i)));
                        br.Write(compressedData);
                        br.Close();
                    }
                    if (outDecompressed)
                    {
                        BinaryWriter br = new BinaryWriter(File.Create(basePath + string.Format("file{0:D3}_dec", i)));
                        br.Write(DecompressZLib(compressedData));
                        br.Close();
                    }
                }
                else Console.WriteLine("Erm...HOW!?!?!?"); //Just for a stupid gag. This case should be never triggered, unless something goes horribly wrong(and I mean system-wise)
            }
            DataReader.Close();
            Console.WriteLine("Done!");
        }

        public static void ParseFileZero(string ZeroPath)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(ZeroPath)); //Open file
            output = ""; //Clear out the string where we'll print our output text
            while (br.ReadUInt32() == 0x2001301)
            {
                br.BaseStream.Position += 0x14; //Skip all 0x2001301 sections, since purpose of those is unknown as of this point
            }
            br.BaseStream.Position -= 0x4; //Go four bytes back, because the previous loop read these to check for next section
            while (br.BaseStream.Position < br.BaseStream.Length) //Loop through as many entries as we can before the file ends
            {
                try //Put this whole contraption into a try-catch clause, just in case we reach EOF or something else goes wrong
                {
                    output += "FmtID:         0x" + string.Format("{0:X4}", br.ReadUInt16()); //Format identifier
                    //FMTS(entry types):
                    //0xB004 - vertex buffer pointers for submeshes
                    //0xB005 - vertex buffer
                    //0xB006 - IDK
                    //0xB501 - useless crap, points into the middle of data
                    //0xB502 - texture
                    
                    output += "\nThing2:       0x" + string.Format("{0:X4}", br.ReadUInt16()); //IDK what this is
                    output += "\nLength:       0x" + string.Format("{0:X8}", br.ReadUInt32()); //Length of subfile (in file002) in bytes
                    output += "\nStart offset: 0x" + string.Format("{0:X8}", br.ReadUInt32()) + "\n\r"; //Start offset of file (again in file002, relative to it's beginning)
                }
                catch
                {
                    //Don't do anything if something goes wrong
                }
            }
            File.WriteAllText(Path.ChangeExtension(ZeroPath, "txt"), output); //Flush text into a file
        }

        public static void DebugPrint(string text, bool CMD, bool TXT)
        {
            if (CMD) Console.WriteLine(text);
            if (TXT) output += text;
        }

        public class Powe
        {
            public int number; //Not actually part of the stored data, just for keeping track of these

            //Powe magic
            public uint id;       //0x4, texture id, unique for each texture in most cases(though some identical textures in different archives have the same id)
            public uint length;      //0x8, length of the texture in bytes(including mipmaps)
            public uint idCopy;   //0xC, seems to be a copy of id
            //Whole buncha other (mostly empty or constant) data here, purpose unknown
            public ushort width;    //0x18, texture width
            public ushort height;   //0x1A, texture height
            public byte mipLevel;   //0x1F, number of mipmap levels, this also counts the regular full-res texture
            public byte texFmt;     //0x38, texture format, see enumerable from SDK below, seems like LM2 uses only ETC1 and ETC1A4
            //Also some other (probably useless) data
        }

        public class FileTableEntry
        {
            public uint startOffset; //0x0, offset at which the file starts in the data
            public uint decompressedLength; //0x4, file length after decompression(present whether the data is compressed or not, so for data without compression this is the file size)
            public uint compressedLength; //0x8, file length in compressed form(only present if compression is used, other wise equals null)
            public uint unk; //0xC, probably file attributes
            //public byte[] unk = new byte[4]; //0xC, probably file attributes
        }

        public static byte[] DecompressZLib(byte[] data)
        {
            try
            {
                var br = new BinaryReader(new MemoryStream(data));
                var ms = new MemoryStream();
                br.BaseStream.Position = 2;
                using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length - 6)), CompressionMode.Decompress)) ds.CopyTo(ms);
                Console.WriteLine("     Successfully decompressed file");
                return ms.ToArray();
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("     Failed to decompress file");
                return new byte[0];
            }
        }

        enum CtrTexFormat
        {
            RGBA8888 = 0,
            RGB888 = 1,
            RGBA5551 = 2,
            RGB565 = 3,
            RGBA4444 = 4,
            LA88 = 5,
            HL8 = 6,
            L8 = 7,
            A8 = 8,
            LA44 = 9,
            L4 = 10,
            A4 = 11,
            ETC1 = 12,
            ETC1_A4 = 13,

            SHADOW = 0x10,
            GAS = 0x20,
            REF = 0x30,
        }

        #region ETC1
        //ETC1 decoder
        //From Ohana3DS-Rebirth by gdkchan
        private static int[,] etc1LUT = { { 2, 8, -2, -8 }, { 5, 17, -5, -17 }, { 9, 29, -9, -29 }, { 13, 42, -13, -42 }, { 18, 60, -18, -60 }, { 24, 80, -24, -80 }, { 33, 106, -33, -106 }, { 47, 183, -47, -183 } };

        public static Bitmap DecodeETC1(byte[] data, ushort width, ushort height, bool alpha, bool flipY)
        {
            long dataOffset = 0;
            byte[] output = new byte[width * height * 4];
            byte[] decodedData = etc1Decode(data, width, height, alpha);
            int[] etc1Order = etc1Scramble(width, height);

            int i = 0;
            for (int tY = 0; tY < height / 4; tY++)
            {
                for (int tX = 0; tX < width / 4; tX++)
                {
                    int TX = etc1Order[i] % (width / 4);
                    int TY = (etc1Order[i] - TX) / (width / 4);
                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            dataOffset = ((TX * 4) + x + (((TY * 4) + y) * width)) * 4;
                            long outputOffset = ((tX * 4) + x + (((tY * 4 + y)) * width)) * 4;

                            Buffer.BlockCopy(decodedData, (int)dataOffset, output, (int)outputOffset, 4);
                        }
                    }
                    i += 1;
                }
            }
            Bitmap img = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData imgData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(output, 0, imgData.Scan0, output.Length);
            img.UnlockBits(imgData);
            if (flipY) img.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return img;
        }

        private static byte[] etc1Decode(byte[] input, int width, int height, bool alpha)
        {
            byte[] output = new byte[(width * height * 4)];
            long offset = 0;

            for (int y = 0; y < height / 4; y++)
            {
                for (int x = 0; x < width / 4; x++)
                {
                    byte[] colorBlock = new byte[8];
                    byte[] alphaBlock = new byte[8];
                    if (alpha)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            colorBlock[7 - i] = input[offset + 8 + i];
                            alphaBlock[i] = input[offset + i];
                        }
                        offset += 16;
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            colorBlock[7 - i] = input[offset + i];
                            alphaBlock[i] = 0xff;
                        }
                        offset += 8;
                    }

                    colorBlock = etc1DecodeBlock(colorBlock);

                    bool toggle = false;
                    long alphaOffset = 0;
                    for (int tX = 0; tX < 4; tX++)
                    {
                        for (int tY = 0; tY < 4; tY++)
                        {
                            int outputOffset = (x * 4 + tX + ((y * 4 + tY) * width)) * 4;
                            int blockOffset = (tX + (tY * 4)) * 4;
                            Buffer.BlockCopy(colorBlock, blockOffset, output, outputOffset, 3);

                            byte a = toggle ? (byte)((alphaBlock[alphaOffset++] & 0xf0) >> 4) : (byte)(alphaBlock[alphaOffset] & 0xf);
                            output[outputOffset + 3] = (byte)((a << 4) | a);
                            toggle = !toggle;
                        }
                    }
                }
            }

            return output;
        }

        private static byte[] etc1DecodeBlock(byte[] data)
        {
            uint blockTop = BitConverter.ToUInt32(data, 0);
            uint blockBottom = BitConverter.ToUInt32(data, 4);

            bool flip = (blockTop & 0x1000000) > 0;
            bool difference = (blockTop & 0x2000000) > 0;

            uint r1, g1, b1;
            uint r2, g2, b2;

            if (difference)
            {
                r1 = blockTop & 0xf8;
                g1 = (blockTop & 0xf800) >> 8;
                b1 = (blockTop & 0xf80000) >> 16;

                r2 = (uint)((sbyte)(r1 >> 3) + ((sbyte)((blockTop & 7) << 5) >> 5));
                g2 = (uint)((sbyte)(g1 >> 3) + ((sbyte)((blockTop & 0x700) >> 3) >> 5));
                b2 = (uint)((sbyte)(b1 >> 3) + ((sbyte)((blockTop & 0x70000) >> 11) >> 5));

                r1 |= r1 >> 5;
                g1 |= g1 >> 5;
                b1 |= b1 >> 5;

                r2 = (r2 << 3) | (r2 >> 2);
                g2 = (g2 << 3) | (g2 >> 2);
                b2 = (b2 << 3) | (b2 >> 2);
            }
            else
            {
                r1 = blockTop & 0xf0;
                g1 = (blockTop & 0xf000) >> 8;
                b1 = (blockTop & 0xf00000) >> 16;

                r2 = (blockTop & 0xf) << 4;
                g2 = (blockTop & 0xf00) >> 4;
                b2 = (blockTop & 0xf0000) >> 12;

                r1 |= r1 >> 4;
                g1 |= g1 >> 4;
                b1 |= b1 >> 4;

                r2 |= r2 >> 4;
                g2 |= g2 >> 4;
                b2 |= b2 >> 4;
            }

            uint table1 = (blockTop >> 29) & 7;
            uint table2 = (blockTop >> 26) & 7;

            byte[] output = new byte[(4 * 4 * 4)];
            if (!flip)
            {
                for (int y = 0; y <= 3; y++)
                {
                    for (int x = 0; x <= 1; x++)
                    {
                        Color color1 = etc1Pixel(r1, g1, b1, x, y, blockBottom, table1);
                        Color color2 = etc1Pixel(r2, g2, b2, x + 2, y, blockBottom, table2);

                        int offset1 = (y * 4 + x) * 4;
                        output[offset1] = color1.B;
                        output[offset1 + 1] = color1.G;
                        output[offset1 + 2] = color1.R;

                        int offset2 = (y * 4 + x + 2) * 4;
                        output[offset2] = color2.B;
                        output[offset2 + 1] = color2.G;
                        output[offset2 + 2] = color2.R;
                    }
                }
            }
            else
            {
                for (int y = 0; y <= 1; y++)
                {
                    for (int x = 0; x <= 3; x++)
                    {
                        Color color1 = etc1Pixel(r1, g1, b1, x, y, blockBottom, table1);
                        Color color2 = etc1Pixel(r2, g2, b2, x, y + 2, blockBottom, table2);

                        int offset1 = (y * 4 + x) * 4;
                        output[offset1] = color1.B;
                        output[offset1 + 1] = color1.G;
                        output[offset1 + 2] = color1.R;

                        int offset2 = ((y + 2) * 4 + x) * 4;
                        output[offset2] = color2.B;
                        output[offset2 + 1] = color2.G;
                        output[offset2 + 2] = color2.R;
                    }
                }
            }

            return output;
        }

        private static Color etc1Pixel(uint r, uint g, uint b, int x, int y, uint block, uint table)
        {
            int index = x * 4 + y;
            uint MSB = block << 1;

            int pixel = index < 8
                ? etc1LUT[table, ((block >> (index + 24)) & 1) + ((MSB >> (index + 8)) & 2)]
                : etc1LUT[table, ((block >> (index + 8)) & 1) + ((MSB >> (index - 8)) & 2)];

            r = saturate((int)(r + pixel));
            g = saturate((int)(g + pixel));
            b = saturate((int)(b + pixel));

            return Color.FromArgb((int)r, (int)g, (int)b);
        }

        private static byte saturate(int value)
        {
            if (value > 0xff) return 0xff;
            if (value < 0) return 0;
            return (byte)(value & 0xff);
        }

        private static int[] etc1Scramble(int width, int height)
        {
            //Maybe theres a better way to do this?
            int[] tileScramble = new int[((width / 4) * (height / 4))];
            int baseAccumulator = 0;
            int rowAccumulator = 0;
            int baseNumber = 0;
            int rowNumber = 0;

            for (int tile = 0; tile < tileScramble.Length; tile++)
            {
                if ((tile % (width / 4) == 0) && tile > 0)
                {
                    if (rowAccumulator < 1)
                    {
                        rowAccumulator += 1;
                        rowNumber += 2;
                        baseNumber = rowNumber;
                    }
                    else
                    {
                        rowAccumulator = 0;
                        baseNumber -= 2;
                        rowNumber = baseNumber;
                    }
                }

                tileScramble[tile] = baseNumber;

                if (baseAccumulator < 1)
                {
                    baseAccumulator++;
                    baseNumber++;
                }
                else
                {
                    baseAccumulator = 0;
                    baseNumber += 3;
                }
            }

            return tileScramble;
        }
        #endregion
    }
}
