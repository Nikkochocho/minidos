/*
 * MiniDOS
 * Copyright (C) 2024  Lara H. Ferreira and others.
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace RPCLibrary.Compression
{
    public class BitCompression
    {
        private const char __TURN_ON_PIXEL  = ' ';
        private const char __TURN_OFF_PIXEL = 'x';

        public const int DEFAULT_UNCOMPRESSED_DATA = 5120;

        public static byte[] Compress(char[] aData)
        {
            int    bitCount = 0;
            int    count    = 0;
            byte   bit      = 0;
            int    size     = (aData.Length / 8);
            byte[] result   = new byte[(size > 0 ? size : 1)];

            foreach (char c in aData)
            {
                byte b = (byte)(c != ' ' ? 0 : 0x80);

                if (count == 0)
                {
                    bit = b;
                }
                else
                {
                    bit = (byte)(bit >> 1);
                    bit = (byte)(bit | b);
                }

                count++;

                if (count == 8)
                {
                    result[bitCount] = bit;
                    count = 0;
                    bit   = 0;
                    bitCount++;
                }
            }

            return result;
        }

        public static string UnCompress(byte[] aCompressed)
        {
            string line = "";

            foreach (byte b in aCompressed)
            {
                byte packedFrame = b;

                for (int i = 0; i < 8; i++)
                {
                    byte bit = (byte)(packedFrame & 1);

                    packedFrame = (byte)(packedFrame >> 1);

                    if (bit == 1)
                    {
                        line += __TURN_ON_PIXEL;
                    }
                    else
                    {
                        line += __TURN_OFF_PIXEL;
                    }
                }
            }

            return line;
        }

        public static void UnCompress(byte[] aCompressed, char[] aUncompressed, int aCompressedSize = 0)
        {
            int ucount   = 0;
            int boundary = (aCompressedSize == 0 ? aCompressed.Length : aCompressedSize);

            for(int count = 0; count < boundary; count++)
            {
                byte packedFrame = aCompressed[count];

                for (int i = 0; i < 8; i++)
                {
                    byte bit = (byte)(packedFrame & 1);

                    packedFrame = (byte)(packedFrame >> 1);

                    if (bit == 1)
                    {
                        aUncompressed[ucount++] = __TURN_ON_PIXEL;
                    }
                    else
                    {
                        aUncompressed[ucount++] = __TURN_OFF_PIXEL;
                    }
                }
            }
        }

        public static string ToString(List<byte> aData)
        {
            string result = string.Empty;
            string sep    = "";

            foreach (byte b in aData)
            {
                result += sep + b;

                if (sep == "")
                {
                    sep = ",";
                }
            }

            return result;
        }
    }
}
