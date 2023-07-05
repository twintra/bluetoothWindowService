using BluetoothService.Extensions;
using BluetoothService.Libs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BluetoothService.Controllers
{
    internal class SocketClient
    {
        TcpClient client = null;
        NetworkStream stream = null;
        public SocketClient()
        {
        }

        public bool ClientAvailable { get => (client != null) && client.Connected; }

        public void AddClient(TcpClient client)
        {
            Close();
            this.client = client;
            stream = client.GetStream();
        }

        public string Read()
        {
            if (!ClientAvailable) return null;

            try
            {
                byte[] buffer = new byte[client.Available];

                stream.Read(buffer, 0, buffer.Length);
                string s = Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                {
                    OnHandShakeRequest(s);
                    return null;
                }
                else
                {
                    if (buffer.Length == 0) return null;
                    bool fin = (buffer[0] & 0b10000000) != 0;
                    bool mask = (buffer[1] & 0b10000000) != 0;
                    int opcode = buffer[0] & 0b00001111;
                    int offset = 2;
                    ulong msglen = (ulong)buffer[1] & 0b01111111;

                    if (msglen == 126)
                    {
                        msglen = BitConverter.ToUInt16(new byte[] { buffer[3], buffer[2] }, 0);
                        offset = 4;
                    }
                    else if (msglen == 127)
                    {
                        msglen = BitConverter.ToUInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                        offset = 10;
                    }
                    //
                    if (msglen == 0)
                    {
                        Console.WriteLine("msglen == 0");
                    }
                    else if (mask)
                    {
                        byte[] decoded = new byte[msglen];
                        byte[] masks = new byte[4] { buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3] };
                        offset += 4;

                        for (ulong i = 0; i < msglen; ++i)
                        {
                            decoded[i] = (byte)(buffer[((ulong)offset) + i] ^ masks[i % 4]);
                        }

                        // check if client disconnected
                        if (decoded.Length == 2 && decoded[0] == 3 && decoded[1] == 233)
                        {
                            Console.WriteLine("==> Client disconnected");
                            Close();
                            return null;
                        }

                        string text = Encoding.UTF8.GetString(decoded);

                        return text;

                    }
                    else
                    {
                        Console.WriteLine("mask bit not set");
                        return null;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Close();
                return null;
            }


        }

        public void Close()
        {
            if (ClientAvailable)
            {
                stream.Close();
                stream = null;
                client.Close();
                client = null;
            }
        }

        private void OnHandShakeRequest(String s)
        {
            Console.WriteLine("==> Req GET");
            Guid clientid = Guid.NewGuid();
            Console.WriteLine("==> Handshaking from clientid: {0}", clientid);

            string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
            string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

            // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
            byte[] response = Encoding.UTF8.GetBytes(
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Connection: Upgrade\r\n" +
                "Upgrade: websocket\r\n" +
                "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

            stream.Write(response, 0, response.Length);

            Console.WriteLine("==> swkaSha1Base64: {0}", swkaSha1Base64);
            Console.WriteLine("Handshake successful, connection established");

            SendMessage("Handshake successful, connection established");
        }



        public void SendMessage(String message)
        {
            Console.WriteLine("==> SendMessage : {0}", message);
            SendMessageToClient(MessageUtil.Message(message));
        }
        public void SendData(Dictionary<string, dynamic> data)
        {
            Console.WriteLine("==> SendData : {0}", data);
            SendMessageToClient(MessageUtil.Data(data));
        }

        public void SendMessageToClient(string msg)
        {
            if (stream == null) return;
            Console.WriteLine("==> sending message to client : {0}", msg.Length > 100 ? msg.Substring(0, 100) + "..." : msg);
            Queue<string> que = new Queue<string>(msg.SplitInGroups(125));
            int len = que.Count;

            while (que.Count > 0)
            {
                var header = GetHeader(
                    que.Count <= 1,
                    que.Count != len
                );

                byte[] list = Encoding.UTF8.GetBytes(que.Dequeue());
                header = (header << 7) + list.Length;
                try
                {
                    if (!stream.CanWrite) return;
                    stream.Write(IntToByteArray((ushort)header), 0, 2);
                    stream.Write(list, 0, list.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine("==> SendMessageToClient error: {0} \n {1}", e.Message, e.StackTrace);
                }

            }
        }
        private static int GetHeader(bool finalFrame, bool contFrame)
        {
            int header = finalFrame ? 1 : 0; //fin: 0 = more frames, 1 = final frame
            header = (header << 1) + 0; //rsv1
            header = (header << 1) + 0; //rsv2
            header = (header << 1) + 0; //rsv3
            header = (header << 4) + (contFrame ? 0 : 1); //opcode : 0 = continuation frame, 1 = text
            header = (header << 1) + 0; //mask: server -> client = no mask

            return header;
        }

        private static byte[] IntToByteArray(ushort value)
        {
            var ary = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(ary);
            }

            return ary;
        }
    }
}
