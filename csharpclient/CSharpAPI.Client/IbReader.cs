using System;
using System.Text;
using System.IO;

namespace IBApi
{
    /// <summary>
    /// Wrapper around BinaryReader to parse IB Server response stream. This class is not thread-safe.
    /// </summary>
    public class IbReader
    {
        private readonly BinaryReader reader;
        
        public IbReader(BinaryReader reader)
        {
            this.reader = reader;
        }

        public double ReadDouble()
        {
            string doubleAsstring = ReadString();
            if (string.IsNullOrEmpty(doubleAsstring) || doubleAsstring == "0")
            {
                return 0;
            }
            else return Double.Parse(doubleAsstring, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public double ReadDoubleMax() 
        {
            string str = ReadString();
            return (str == null || str.Length == 0) ? Double.MaxValue : Double.Parse(str, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public long ReadLong()
        {
            string longAsstring = ReadString();
            if (string.IsNullOrEmpty(longAsstring) || longAsstring == "0")
            {
                return 0;
            }
            else return Int64.Parse(longAsstring);
        }

        public int ReadInt()
        {
            string intAsstring = ReadString();
            if (string.IsNullOrEmpty(intAsstring) || intAsstring == "0")
            {
                return 0;
            }
            else return Int32.Parse(intAsstring);
        }

        public int ReadIntMax()
        {
            string str = ReadString();
            return (str == null || str.Length == 0) ? Int32.MaxValue : Int32.Parse(str);
        }

        public bool ReadBoolFromInt()
        {
            string str = ReadString();
            return str == null ? false : (Int32.Parse(str) != 0);
        }

        public string ReadString()
        {
            byte b = reader.ReadByte();
            if (b == 0) return null;

            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append((char)b);
            while (true)
            {
                b = reader.ReadByte();
                if (b == 0)
                {
                    break;
                }
                else
                {
                    strBuilder.Append((char)b);
                }
            }
            return strBuilder.ToString();
        }
    }
}
