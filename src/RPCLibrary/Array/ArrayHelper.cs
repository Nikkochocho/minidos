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

namespace RPCLibrary.Array
{
    public class ArrayHelper
    {
        public static bool Contains(char[] array1, char[] array2)
        {
            int len = Math.Min(array1.Length, array2.Length);

            for (int i = 0; i < len; i++)
            {
                if (array2[i] < array1[i])
                    return false;

                if (array2[i] > array1[i])
                    return false;
            }

            return true;
        }
        public static void Convert(byte[] aSource, ref char[] dest)
        {
            int count = 0;

            foreach (byte b in aSource)
            {
                dest[count++] = System.Convert.ToChar(b);
            }
        }
    }
}
