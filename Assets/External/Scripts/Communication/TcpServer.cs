using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class Communication
    {
        public class TCPServer
        {
            public TcpListener Server;
            public List<TcpClient> Clients = new List<TcpClient>();
            public static bool Active = false;

            private SortedList<short, Action<byte[]>> Handlers = new SortedList<short, Action<byte[]>>();

            public void RegisterHandler(short id, Action<byte[]> callback)
            {
                Handlers.Add(id, callback);
            }
            public void UnregisterHandler(short id)
            {
                Handlers.Remove(id);
            }

            public async Task SendToAll<T>(short msgID, T message)
            {
                byte[] result = msgID.Serialize().Concat(message.Serialize()).ToArray();
                for (int client = 0; client < Clients.Count; client++)
                    await Clients[client].GetStream().WriteAsync(result, 0, result.Length);
            }

            private IEnumerator ReadMessages(TcpClient player)
            {
                while (true)
                {
                    if (Handlers.Count == 0) continue;
                    Action Read = async () =>
                    {
                        byte[] message = new byte[1024];
                        await player.GetStream().ReadAsync(message, 0, 1024);
                        short ID = BitConverter.ToInt16(message.Take(sizeof(short)).ToArray(),0);
                        if (Handlers.Keys.Contains(ID))
                            Handlers[ID](message.Skip(sizeof(short)).ToArray());
                        else
                            Debug.LogError($"Unregistrierter Handler: {ID}");
                    };
                    Read();
                    yield return null;
                }
            }
            private async void WaitForClients(int maxClients)
            {
                while (Clients.Count < maxClients)
                {
                    TcpClient client = await Server.AcceptTcpClientAsync();
                    Clients.Add(client);
                    Debug.Log("Client Connected");
                    Global.instance.StartCoroutine(ReadMessages(client));
                }
            }
            public TCPServer(int maxConnections)
            {
                Server = new TcpListener(IPAddress.Any, 9000);
                Server.Start();
                Active = true;
                WaitForClients(maxConnections);
            }
        }
    }
}