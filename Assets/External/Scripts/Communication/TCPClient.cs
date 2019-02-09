using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ExtensionMethods;

namespace Hanafuda
{
    public partial class Communication
    {
        public class TCPClient
        {
            public TcpClient Client;

            private SortedList<short, Action<byte[]>> Handlers = new SortedList<short, Action<byte[]>>();

            public void RegisterHandler(short id, Action<byte[]> callback)
            {
                Handlers.Add(id, callback);
            }
            public void UnregisterHandler(short id)
            {
                Handlers.Remove(id);
            }

            public async Task Send<T>(short msgID, T message)
            {
                byte[] result = BitConverter.GetBytes(msgID).Concat(message.Serialize()).ToArray();
                await Client.GetStream().WriteAsync(result,0,result.Length);
            }

            public TCPClient(string ip, int port)
            {
                Client = new TcpClient(ip,port);
            }
        }
    }
}
