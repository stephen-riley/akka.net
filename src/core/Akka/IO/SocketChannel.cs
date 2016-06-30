//-----------------------------------------------------------------------
// <copyright file="SocketChannel.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Akka.Actor;
using Akka.Util;

namespace Akka.IO
{
    /* 
     * SocketChannel does not exists in the .NET BCL - This class is an adapter to hide the differences in CLR & JVM IO.
     * This implementation uses blocking IO calls, and then catch SocketExceptions if the socket is set to non blocking. 
     * This might introduce performance issues, with lots of thrown exceptions
     */
    public class SocketChannel 
    {
        private readonly Socket _socket;
        private IActorRef _connection;
        private bool _connected;
        private IAsyncResult _connectResult;

        public SocketChannel(Socket socket) 
        {
            _socket = socket;
        }

        public static SocketChannel Open()
        {
            // TODO: Mono does not support IPV6 Uris correctly https://bugzilla.xamarin.com/show_bug.cgi?id=43649 (Aaronontheweb 9/13/2016)
            if (RuntimeDetector.IsMono)
                return new SocketChannel(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
            return new SocketChannel(new Socket(SocketType.Stream, ProtocolType.Tcp));
        }

        public SocketChannel ConfigureBlocking(bool block)
        {
            _socket.Blocking = block;
            return this;
        }

        public Socket Socket
        {
            get { return _socket; }
        }

        public void Register(IActorRef connection, SocketAsyncOperation? initialOps)
        {
            _connection = connection;
        }


        public virtual bool IsOpen()
        {
            return _connected;
        }

        public bool Connect(EndPoint address)
        {
#if CORECLR
            AsyncCallback endAction = ar => { };
            var beginConnectMethod = _socket
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(method => method.Name.Equals("BeginConnect"))
                .FirstOrDefault(method =>
                {
                    var parameters = method.GetParameters();
                    return parameters.Length == 3 && parameters[0].ParameterType == typeof(EndPoint) &&
                           parameters[1].ParameterType == typeof(AsyncCallback) && parameters[2].ParameterType == typeof(object);
                });

            _connectResult = (IAsyncResult)beginConnectMethod.Invoke(_socket, new object[] { address, endAction, null });
            if (_connectResult.CompletedSynchronously)
            {
                _socket
                    .GetType()
                    .GetMethod("EndConnect", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(_socket, new object[] { _connectResult });
                _connected = true;
                return true;
            }
            return false;
#else
            _connectResult = _socket.BeginConnect(address, ar => { }, null);
            if (_connectResult.CompletedSynchronously)
            {
                _socket.EndConnect(_connectResult);
                _connected = true;
                return true;
            }
            return false;
#endif
        }

        public bool FinishConnect()
        {
#if CORECLR
            if (_connectResult.CompletedSynchronously)
                return true;
            if (_connectResult.IsCompleted)
            {
                _socket
                    .GetType()
                    .GetMethod("EndConnect", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(_socket, new object[] { _connectResult });
                _connected = true;
            }
            return _connected;
#else
            if (_connectResult.CompletedSynchronously)
                return true;
            if (_connectResult.IsCompleted)
            {
                _socket.EndConnect(_connectResult);
                _connected = true;
            }

            return _connected;
#endif
        }

        public SocketChannel Accept()
        {
            //TODO: Investigate. If we don't wait 1ms we get intermittent test failure in TcpListenerSpec.  
            return _socket.Poll(1, SelectMode.SelectRead)
                ? new SocketChannel(_socket.Accept()) {_connected = true}
                : null;
        }

        public int Read(ByteBuffer buffer)
        {
            if (!_socket.Poll(0, SelectMode.SelectRead))
                return 0;
            var data = new byte[buffer.Remaining];
            var length = _socket.Receive(data);
            if (length == 0)
                return -1;
            buffer.Put(data, 0, length);
            return length;
        }

        public int Write(ByteBuffer buffer)
        {
            if (!_socket.Poll(0, SelectMode.SelectWrite))
                return 0;
            var data = new byte[buffer.Remaining];
            buffer.Get(data);
            return _socket.Send(data);
        }

        public void Close()
        {
            _connected = false;
            _socket.Dispose();
        }
        internal IActorRef Connection { get { return _connection; } }
    }
}