using KeraLua;
using NLua;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace RPCLibrary
{
    public class LuaEngine
    {
        private NLua.Lua           _state = new NLua.Lua();
        private readonly TcpClient _tcpClient;
        private readonly string    _apiKey;
        private bool               isScriptRunning = false;

        public string Args { get; set; } = "";


        public LuaEngine(TcpClient tcpClient, string apiKey)
        {
            _apiKey    = apiKey;
            _tcpClient = tcpClient;

            _state.State.Encoding = Encoding.UTF8;
            _state.RegisterFunction(nameof(_print), 
                                    this, 
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._print),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            _state.DoString(@"print = function(...) _print({...}); end");
            _state.RegisterFunction(nameof(_wait),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._wait),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            _state.DoString(@"wait = function(timeout) _wait(timeout); end");
            _state.RegisterFunction(nameof(_clear),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._clear),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            _state.DoString(@"clear = function() _clear(); end");
            _state.RegisterFunction(nameof(_getArgs),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._getArgs),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            _state.DoString(@"get_args = function() return _getArgs(); end");
            _state.RegisterFunction(nameof(_askGPT),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._askGPT),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            _state.DoString(@"ask_gpt = function(question) return _askGPT(question); end");
            _state.RegisterFunction(nameof(_askGPTPrint),
                                    this,
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._askGPTPrint),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            _state.DoString(@"ask_gpt_print = function(question) _askGPTPrint(question); end");
        }

        public bool RunScript(string fileName)
        {

            isScriptRunning = true;
            _state.DoFile(fileName);
            isScriptRunning = false;

            return true;
        }

        private void SendScreenResponse(string text)
        {
            RPCClient rpcClient = new RPCClient(_tcpClient);
            byte[] buffer = Encoding.Default.GetBytes(text);
            RPCData data = new RPCData()
            {
                Type = RPCData.TYPE_LUA_SCREEN_RESPONSE,
                EndOfData = !isScriptRunning,
                Data = buffer,
            };

            rpcClient.Send(data);
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

            var text = strBuilder.ToString();
            Console.WriteLine(text);

            SendScreenResponse(text);
        }

        private void _wait(int timeout)
        {
            Thread.Sleep((int) timeout);
        }

        private void _clear()
        {
            Console.Clear();
            SendScreenResponse(RPCData.ANSI_CLEAR_SCREEN_CODE);
        }

        private string _getArgs()
        {
            return Args;
        }

        private string _askGPT(string question)
        {
            OpenAIClient openAI = new OpenAIClient(_apiKey);

            return openAI.Ask(question);
        }

        private void _askGPTPrint(string question)
        {
            OpenAIClient openAI   = new OpenAIClient(_apiKey);
            string       response = openAI.Ask(question);

            Console.WriteLine(response);

            SendScreenResponse(response);
        }
    }
}
