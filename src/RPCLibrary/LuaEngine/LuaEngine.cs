using KeraLua;
using NLua;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System.Net.Sockets;
using System.Text;

namespace RPCLibrary
{
    public class LuaEngine
    {
        private NLua.Lua _state = new NLua.Lua();
        private readonly TcpClient _tcpClient;
        private bool isScriptRunning = false;

        public string Args { get; set; } = "";


        public LuaEngine(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
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
            byte[] buffer = Encoding.ASCII.GetBytes(text);
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
    }
}
