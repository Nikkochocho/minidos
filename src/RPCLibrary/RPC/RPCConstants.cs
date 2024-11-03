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
    public class RPCConstants
    {
        // ANSI Terminal codes
        public const string ANSI_CLEAR_SCREEN_CODE               = "\033[2J";
        public const string ANSI_SET_CURSOR_HOME_POSITION        = "\033[H";

        public static char[] ANSI_CLEAR_SCREEN_CODE_ARRAY        = ANSI_CLEAR_SCREEN_CODE.ToArray();
        public static char[] ANSI_SET_CURSOR_HOME_POSITION_ARRAY = ANSI_SET_CURSOR_HOME_POSITION.ToArray();

        // Resources that executes on server
        public const string SERVER_SHARED_FILE_PROTOCOL          = "share://";

        // Data transfer and buffer size
        public const int DEFAULT_BLOCK_SIZE                      = 512;
        public const int RECV_BUFFER_SIZE                        = 512;
        public const int SCREEN_FRAME_BUFFER_SIZE                = 5120;
    }
}
