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

using KeraLua;
using NetCoreAudio;
using NLua;
using RPCLibrary.Client;
using RPCLibrary.Config;
using RPCLibrary.DataProtocol;
using System.Net.Sockets;
using System.Text;

namespace RPCLibrary
{
    public class LuaEngineConstants
    {
        public const string ZIP_EXTENSION = ".zip";
        public const string LUA_EXTENSION = ".lua";
    }

    public class LuaEngine
    {
        private NLua.Lua               __state = new NLua.Lua();
        private readonly TcpClient     __tcpClient;
        private readonly RPCClient     __rpcClient;
        private readonly ServerParms   __parms;
        private readonly List<Player>  __playerQueue;
        private bool                   __isScriptRunning = false;
        private bool                   __enableAutoCarriageReturn = true;
        private string                 __currentPath;

        public string Args { get; set; } = "";


        public LuaEngine(TcpClient tcpClient, ServerParms parms)
        {
            __playerQueue = new List<Player>();
            __rpcClient   = new RPCClient(tcpClient);
            __tcpClient   = tcpClient;
            __parms       = parms;

            RegisterLuaFunctions();
        }

        public bool RunScript(string fileName)
        {

            __isScriptRunning = true;
            __currentPath     = $"{Path.GetDirectoryName(fileName)}{Path.DirectorySeparatorChar}";

            __state.DoFile(fileName);
            __isScriptRunning = false;

            return true;
        }

        public void StopScript()
        {
            __isScriptRunning = false;
        }

        private void SendScreenResponse(string text)
        {
            if(!__tcpClient.Connected)
            {
                StopScript();
                return;
            }

            try
            {
                byte[] buffer = Encoding.Default.GetBytes(text);
                RPCData data = new RPCData()
                {
                    Type = RPCData.TYPE_LUA_SCREEN_RESPONSE,
                    EndOfData = !__isScriptRunning,
                    Data = buffer,
                };

                __rpcClient.Send(data);
            }
            catch (Exception ex)
            {
                StopScript();
            }
        }

        private void StateDebugHook(object sender, NLua.Event.DebugHookEventArgs e)
        {
            if (!__isScriptRunning)
            {
                NLua.Lua l = (NLua.Lua)sender;

                // Stop audio queue
                _stopPlayerQueue();

                l.State.Error("Lua Execution stopped");
            }
        }

        private void RegisterLuaFunctions()
        {
            __state.State.Encoding = Encoding.UTF8;
            __state.RegisterFunction(nameof(_print),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._print),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"print = function(...) _print({...}); end");
            __state.RegisterFunction(nameof(_wait),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._wait),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"wait = function(timeout) _wait(timeout); end");
            __state.RegisterFunction(nameof(_enableAutoCarriageReturn),
                        this,
                        typeof(LuaEngine).GetMethod(nameof(LuaEngine._enableAutoCarriageReturn),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"enable_auto_carriage_return = function(enable) _enableAutoCarriageReturn(enable); end");
            __state.RegisterFunction(nameof(_clear),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._clear),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"clear = function() _clear(); end");
            __state.RegisterFunction(nameof(_home),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._home),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"home = function() _home(); end");
            __state.RegisterFunction(nameof(_getArgs),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._getArgs),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"get_args = function() return _getArgs(); end");
            __state.RegisterFunction(nameof(_askGPT),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._askGPT),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"ask_gpt = function(question) return _askGPT(question); end");
            __state.RegisterFunction(nameof(_play),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._play),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"play = function(filename) return _play(filename); end");
            __state.RegisterFunction(nameof(_stopPlayerQueue),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._stopPlayerQueue),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"stop_player_queue = function() return _stopPlayerQueue(); end");
            __state.RegisterFunction(nameof(_getCurrentPath),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._getCurrentPath),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"get_current_path = function() return _getCurrentPath(); end");

            // This handler MUST TO BE ALWAYS THE LAST ONE registered 
            __state.SetDebugHook(LuaHookMask.Line, 0);
            __state.DebugHook += StateDebugHook;
        }

        private void _print(LuaTable luaTable)
        {
            var strBuilder = new StringBuilder();

            foreach (var values in luaTable.Values)
            {
                if (strBuilder.Length > 0)
                {
                    strBuilder.Append(' ');
                }
                strBuilder.Append(values.ToString());
            }

            var cr   = (__enableAutoCarriageReturn ? "\n" : "");
            var text = $"{strBuilder.ToString()}{cr}";

            if (__parms.ShowScreenContentOnServer)
            {
                Console.Write(text);
            }

            SendScreenResponse(text);
        }

        private void _wait(int timeout)
        {
            Thread.Sleep((int) timeout);
        }

        private void _enableAutoCarriageReturn(bool enable)
        {
            __enableAutoCarriageReturn = enable;
        }

        private void _clear()
        {
            if (__parms.ShowScreenContentOnServer)
            {
                Console.Clear();
            }

            SendScreenResponse(RPCData.ANSI_CLEAR_SCREEN_CODE);
        }

        private void _home()
        {
            if (__parms.ShowScreenContentOnServer)
            {
                Console.SetCursorPosition(0, 0);
            }

            SendScreenResponse(RPCData.ANSI_SET_CURSOR_HOME_POSITION);
        }

        private string _getArgs()
        {
            return Args;
        }

        private string _askGPT(string question)
        {
            OpenAIClient openAI = new OpenAIClient(__parms.ApiKey, __parms.MaxTokens);

            return openAI.Ask(question);
        }

        private bool _play(string filename)
        {
            Player player = new Player();

            try
            {
                player.Play(filename).Wait();
                __playerQueue.Add(player);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private void _stopPlayerQueue()
        { 
            foreach(Player player in __playerQueue)
            {
                player?.Stop();
            }
        }

        private string _getCurrentPath()
        {
            return __currentPath;
        }
    }
}
