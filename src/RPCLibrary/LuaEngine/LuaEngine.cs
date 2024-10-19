using KeraLua;
using NLua;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace RPCLibrary
{
    public class LuaEngineConstants
    {
        public const string ZIP_EXTENSION = ".zip";
        public const string LUA_EXTENSION = ".lua";
    }

    public record OpenAIParms
    {
        public string ApiKey { get; set; }
        public int MaxTokens { get; set; }

    }

    public class LuaEngine
    {
        private NLua.Lua             __state = new NLua.Lua();
        private readonly TcpClient   __tcpClient;
        private readonly OpenAIParms __openAiParms;
        private bool                 __isScriptRunning = false;
        private bool                 __enableAutoCarriageReturn = true;

        public string Args { get; set; } = "";


        public LuaEngine(TcpClient tcpClient, OpenAIParms openAiParms)
        {
            __tcpClient   = tcpClient;
            __openAiParms = openAiParms;

            RegisterLuaFunctions();
        }

        public bool RunScript(string fileName)
        {

            __isScriptRunning = true;
            __state.DoFile(fileName);
            __isScriptRunning = false;

            return true;
        }

        private void SendScreenResponse(string text)
        {
            RPCClient rpcClient = new RPCClient(__tcpClient);
            byte[]    buffer    = Encoding.Default.GetBytes(text);
            RPCData   data      = new RPCData()
            {
                Type = RPCData.TYPE_LUA_SCREEN_RESPONSE,
                EndOfData = !__isScriptRunning,
                Data = buffer,
            };

            rpcClient.Send(data);
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
            OpenAIClient openAI = new OpenAIClient(__openAiParms.ApiKey, __openAiParms.MaxTokens);

            return openAI.Ask(question);
        }
    }
}
