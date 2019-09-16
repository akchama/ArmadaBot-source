using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using ArmadaBot.Util;

namespace ArmadaBot
{
    public class Server
    {
        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static Socket _targetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static IPEndPoint Remote { get; set; }
        public static IPEndPoint Local { get; private set; }
        public static ushort FiddlerPort { get; private set; } = 7777;
        public static int LocalPort { get; private set; } = 1060;
        public static bool Connected { get; set; }

        public void Start()
        {
            try
            {
                Server.Local = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Server.LocalPort);
                _mainSocket.Bind(Local);
                _mainSocket.Listen(10);
                while (true)
                {
                    var source = _mainSocket.Accept();
                    var destination = new Server();
                    _targetSocket = destination._mainSocket;
                    var state = new State(source, destination._mainSocket);
                    destination.Connect(Remote, source);
                    source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch (Exception ex)
            {
                if (ex is SocketException && (ex as SocketException).SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    Server.LocalPort++;
                    Server.FiddlerPort++;
                    this.Start();
                }
                Bot.Log(ex.ToString());
                Bot.Log(string.Join("", new string[]
                {
                        HelpTools.FromByteToString(80),
                        HelpTools.FromByteToString(114),
                        HelpTools.FromByteToString(111),
                        HelpTools.FromByteToString(120),
                        HelpTools.FromByteToString(121),
                        HelpTools.FromByteToString(32),
                        HelpTools.FromByteToString(99),
                        HelpTools.FromByteToString(108),
                        HelpTools.FromByteToString(111),
                        HelpTools.FromByteToString(115),
                        HelpTools.FromByteToString(101),
                        HelpTools.FromByteToString(100),
                        HelpTools.FromByteToString(46)
                }));
                Form1.form1.StartLocalProxy();
                BotSession.lostConnection = true;
            }
        }

        public void Stop()
        {
            try
            {
                _mainSocket.Close();
                _targetSocket.Close();
            }
            catch (Exception)
            {
            }
        }

        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var state = new State(_mainSocket, destination);
            _mainSocket.Connect(remoteEndpoint);
            _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            var state = (State)result.AsyncState;
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    var buffer = new byte[bytesRead];
                    Array.Copy(state.Buffer, buffer, bytesRead);
                    if (state.SourceSocket.RemoteEndPoint.ToString().Equals(Remote.ToString()))
                    {
                        //Client.Read(buffer);
                        //if (!Server.Connected && Account.gClass.entityInfo != null)
                        //{
                        //    Server.Connected = true;
                        //    BotSession.logginIn = false;
                        //    BotMethods.WriteLine("Map Loaded.");
                        //    BotCalculator.StartThreads();
                        //}

                    }
                    if (state.DestinationSocket.RemoteEndPoint.ToString().Equals(Remote.ToString()))
                    {
                        //Client.Out(buffer);
                    }
                    state.DestinationSocket.Send(buffer, buffer.Length, SocketFlags.None);
                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch
            {
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }

        private class State
        {
            public Socket SourceSocket { get; private set; }
            public Socket DestinationSocket { get; private set; }
            public byte[] Buffer { get; private set; }

            public State(Socket source, Socket destination)
            {
                SourceSocket = source;
                DestinationSocket = destination;
                Buffer = new byte[32768];
            }
        }
    }
}
