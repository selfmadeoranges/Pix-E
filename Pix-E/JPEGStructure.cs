using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace JPEGStructure
{
    public class JPEGFile
    {
        public SOI StartOfImage { get; set; }
        public List<APPn> ApplicationMarkers { get; set; }
        public List<DQT> QuantizationTables { get; set; }
        public SOF StartOfFrame { get; set; }
        public List<DHT> HuffmanTables { get; set; }
        public SOS StartOfScan { get; set; }
        public byte[] ImageData { get; set; }
        public EOI EndOfImage { get; set; }

        public JPEGFile()
        {
            ApplicationMarkers = new List<APPn>();
            QuantizationTables = new List<DQT>();
            HuffmanTables = new List<DHT>();
        }

        public JPEGFile Load(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            var jpegFile = new JPEGFile();
            int index = 0;

            while (index < fileBytes.Length - 1) // Ensure there's always a next byte to read
            {
                if (fileBytes[index] == 0xFF)
                {
                    int markerIndex = index + 1;

                    // Skip padding 0xFF bytes
                    while (markerIndex < fileBytes.Length && fileBytes[markerIndex] == 0xFF)
                    {
                        markerIndex++;
                    }

                    if (markerIndex >= fileBytes.Length)
                    {
                        throw new Exception("Invalid JPEG file: No EOI marker found.");
                    }

                    byte marker1 = fileBytes[index];
                    byte marker2 = fileBytes[markerIndex];
                    index = markerIndex + 1;

                    int length;
                    byte[] data;

                    switch (marker2)
                    {
                        case 0xD8: // SOI
                            jpegFile.StartOfImage = new SOI(marker1, marker2);
                            break;

                        case 0xE0: // APP0
                        case 0xE1: // APP1
                        case 0xE2: // APP2
                        case 0xE3: // APP3
                        case 0xE4: // APP4
                        case 0xE5: // APP5
                        case 0xE6: // APP6
                        case 0xE7: // APP7
                        case 0xE8: // APP8
                        case 0xE9: // APP9
                        case 0xEA: // APP10
                        case 0xEB: // APP11
                        case 0xEC: // APP12
                        case 0xED: // APP13
                        case 0xEE: // APP14
                        case 0xEF: // APP15
                            length = (fileBytes[index] << 8) + fileBytes[index + 1];
                            data = new byte[length - 2];
                            Array.Copy(fileBytes, index + 2, data, 0, length - 2);
                            jpegFile.ApplicationMarkers.Add(new APPn(marker1, marker2, data));
                            index += length;
                            break;

                        case 0xDB: // DQT
                            length = (fileBytes[index] << 8) + fileBytes[index + 1];
                            data = new byte[length - 2];
                            Array.Copy(fileBytes, index + 2, data, 0, length - 2);
                            jpegFile.QuantizationTables.Add(new DQT(marker1, marker2, data));
                            index += length;
                            break;

                        case 0xC0: // SOF0
                            length = (fileBytes[index] << 8) + fileBytes[index + 1];
                            byte precision = fileBytes[index + 2];
                            ushort height = (ushort)((fileBytes[index + 3] << 8) + fileBytes[index + 4]);
                            ushort width = (ushort)((fileBytes[index + 5] << 8) + fileBytes[index + 6]);
                            byte numberOfComponents = fileBytes[index + 7];
                            jpegFile.StartOfFrame = new SOF(marker1, marker2, precision, height, width, numberOfComponents);
                            index += length;
                            break;

                        case 0xC4: // DHT
                            length = (fileBytes[index] << 8) + fileBytes[index + 1];
                            data = new byte[length - 2];
                            Array.Copy(fileBytes, index + 2, data, 0, length - 2);
                            jpegFile.HuffmanTables.Add(new DHT(marker1, marker2, data));
                            index += length;
                            break;

                        case 0xDA: // SOS
                            length = (fileBytes[index] << 8) + fileBytes[index + 1];
                            data = new byte[length - 2];
                            Array.Copy(fileBytes, index + 2, data, 0, length - 2);
                            jpegFile.StartOfScan = new SOS(marker1, marker2, data);
                            index += length;

                            // Read until EOI (0xFFD9)
                            int imageDataStart = index;
                            while (index + 1 < fileBytes.Length && !(fileBytes[index] == 0xFF && fileBytes[index + 1] == 0xD9))
                            {
                                index++;
                            }

                            jpegFile.ImageData = new byte[index - imageDataStart];
                            Array.Copy(fileBytes, imageDataStart, jpegFile.ImageData, 0, index - imageDataStart);
                            index += 2; // Skip the EOI marker (0xFFD9)
                            break;

                        case 0xD9: // EOI
                            jpegFile.EndOfImage = new EOI(marker1, marker2);
                            return jpegFile;

                        default:
                            // Unknown marker, skipping length
                            if (index + 1 < fileBytes.Length)
                            {
                                length = (fileBytes[index] << 8) + fileBytes[index + 1];
                                index += length;
                            }
                            break;
                    }
                }
                else
                {
                    index++;
                }
            }

            throw new Exception("Invalid JPEG file: No EOI marker found.");
        }
    }

    public struct SOI
    {
        public byte Marker1;
        public byte Marker2;

        public SOI(byte marker1, byte marker2)
        {
            Marker1 = marker1;
            Marker2 = marker2;
        }
    }

    public struct APPn
    {
        public byte Marker1;
        public byte Marker2;
        public byte[] Data;

        public APPn(byte marker1, byte marker2, byte[] data)
        {
            Marker1 = marker1;
            Marker2 = marker2;
            Data = data;
        }
    }

    public struct DQT
    {
        public byte Marker1;
        public byte Marker2;
        public byte[] Data;

        public DQT(byte marker1, byte marker2, byte[] data)
        {
            Marker1 = marker1;
            Marker2 = marker2;
            Data = data;
        }
    }

    public struct SOF
    {
        public byte Marker1;
        public byte Marker2;
        public byte Precision;
        public ushort Height;
        public ushort Width;
        public byte NumberOfComponents;

        public SOF(byte marker1, byte marker2, byte precision, ushort height, ushort width, byte numberOfComponents)
        {
            Marker1 = marker1;
            Marker2 = marker2;
            Precision = precision;
            Height = height;
            Width = width;
            NumberOfComponents = numberOfComponents;
        }
    }

    public struct DHT
    {
        public byte Marker1;
        public byte Marker2;
        public byte[] Data;

        public DHT(byte marker1, byte marker2, byte[] data)
        {
            Marker1 = marker1;
            Marker2 = marker2;
            Data = data;
        }
    }

    public struct SOS
    {
        public byte Marker1;
        public byte Marker2;
        public byte[] Data;

        public SOS(byte marker1, byte marker2, byte[] data)
        {
            Marker1 = marker1;
            Marker2 = marker2;
            Data = data;
        }
    }

    public struct EOI
    {
        public byte Marker1;
        public byte Marker2;

        public EOI(byte marker1, byte marker2)
        {
            Marker1 = marker1;
            Marker2 = marker2;
        }
    }
}
