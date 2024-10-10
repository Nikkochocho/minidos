using NLua;
using RPCLibrary.Client;
using RPCLibrary.DataProtocol;
using System.Net.Sockets;
using System.Text;

namespace RPCLibrary
{
    public class LuaEngine
    {
        private Lua _state = new Lua();
        private readonly TcpClient _tcpClient;
        private bool isScriptRunning = false;


        public LuaEngine(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _state.RegisterFunction(nameof(_print), 
                                    this, 
                                    typeof(LuaEngine).GetMethod(nameof(LuaEngine._print),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            _state.DoString(@"print = function(...) _print({...}); end");
        }

        public bool RunScript(string fileName)
        {

            isScriptRunning = true;
            _state.DoFile(fileName);
            isScriptRunning = false;

            return true;
        }

        private void Send(string text)
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

            Send(text);
        }
    }
}
