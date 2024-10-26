using KeraLua;
using NetCoreAudio;
using NLua;
using RPCLibrary.Client;
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

    public record ServerParms
    {
        public string DownloadFolder { get; set; }
        public string SharedFolder { get; set; }
        public string ApiKey { get; set; }
        public int MaxTokens { get; set; }
    }

    public class LuaEngine
    {
        private NLua.Lua             __state = new NLua.Lua();
        private readonly TcpClient   __tcpClient;
        private readonly ServerParms __parms;
        private bool                 __isScriptRunning = false;
        private bool                 __enableAutoCarriageReturn = true;
        private string               __currentPath;

        public string Args { get; set; } = "";


        public LuaEngine(TcpClient tcpClient, ServerParms parms)
        {
            __tcpClient = tcpClient;
            __parms     = parms;

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
            if(__isScriptRunning)
            {
                // TODO: FINISH HIM !!!
            }
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
                RPCClient rpcClient = new RPCClient(__tcpClient);
                byte[] buffer = Encoding.Default.GetBytes(text);
                RPCData data = new RPCData()
                {
                    Type = RPCData.TYPE_LUA_SCREEN_RESPONSE,
                    EndOfData = !__isScriptRunning,
                    Data = buffer,
                };

                rpcClient.Send(data);
            }
            catch (Exception ex)
            {
                StopScript();
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
            __state.RegisterFunction(nameof(_getCurrentPath),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._getCurrentPath),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            __state.DoString(@"get_current_path = function() return _getCurrentPath(); end");
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

            Console.Write(text);

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
            Console.Clear();
            SendScreenResponse(RPCData.ANSI_CLEAR_SCREEN_CODE);
        }

        private void _home()
        {
            Console.SetCursorPosition(0, 0);
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

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private string _getCurrentPath()
        {
            return __currentPath;
        }
    }
}
