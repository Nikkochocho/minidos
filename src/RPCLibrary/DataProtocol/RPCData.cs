﻿

namespace RPCLibrary.DataProtocol
{
    public record RPCData
    {
        public const int TYPE_LUA_FILENAME = 0;
        public const int TYPE_LUA_EXECUTABLE = 1;

        public int Type { get; set; }
        public bool EndOfData { get; set; }
        public byte[]? Data { get; set; }
        public int DataSize { 
            get
            {
                return (Data != null ? Data.Length : 0);
            }
            set
            {
                Data = new byte[value];
            }
        }
    }
}
