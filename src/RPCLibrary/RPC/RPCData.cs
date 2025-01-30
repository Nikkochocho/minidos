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

namespace RPCLibrary.RPC
{
    public class RPCData
    {
        public const int TYPE_LUA_FILENAME                    = 0;
        public const int TYPE_LUA_PARMS                       = 1;
        public const int TYPE_LUA_EXECUTABLE                  = 2;
        public const int TYPE_LUA_SCREEN_RESPONSE             = 3;
        public const int TYPE_LUA_ANSI_COMMAND_RESPONSE       = 4;
        public const int TYPE_LUA_COMPRESSED_SCREEN_RESPONSE  = 5;

        private bool __noAlloc = false;
        private int __dataSize = 0;

        public int Type { get; set; }
        public bool EndOfData { get; set; }
        public bool IsZipped { get; set; }
        public byte[]? Data { get; set; }
        public int DataSize
        {
            get
            {
                if (!__noAlloc)
                {
                    return Data != null ? Data.Length : 0;
                }
                else
                {
                    return __dataSize;
                }
            }
            set
            {
                if (!__noAlloc)
                {
                    Data = new byte[value];
                }
                else
                {
                    __dataSize = value;
                }
            }
        }

        public RPCData(bool noAlloc = false)
        {
            __noAlloc = noAlloc;
        }
    }
}
