using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MuOnlineOfflineEditor
{
    class ServerConnection
    {
        public ServerConnection(string IPAddress, string EncryptionKey)
        {
            IP = IPAddress;
            encryptionKey = EncryptionKey;
        }

        string IP = "";
        string encryptionKey = "";

        public string SendToServer(string message)
        {
            string answer = SocketSendMessage(message);

            if (answer == "<error>")
            {
                return "";
            }

            return answer;
        }

        private string SocketSendMessage(string message)
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[4400];

            // Connect to a remote device.
            try
            {
                IPAddress ipAddress = IPAddress.Parse(IP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
                Socket sndr = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    Crypto cr = new Crypto();
                    sndr.Connect(remoteEP);
                    byte[] msg = Encoding.ASCII.GetBytes(cr.Crypt(message, encryptionKey));
                    int bytesSent = sndr.Send(msg);


                    int bytesRec = sndr.Receive(bytes);
                    string recString = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    sndr.Shutdown(SocketShutdown.Both);
                    sndr.Close();

                    return cr.DeCrypt(recString, encryptionKey);

                }

                catch (Exception ex)
                {
                    return "<error>" + ex.ToString() + "<\\error>";
                }

            }
            catch (Exception ex)
            {
                return "<error>" + ex.ToString() + "<\\error>";
            }
        }
    }
}
