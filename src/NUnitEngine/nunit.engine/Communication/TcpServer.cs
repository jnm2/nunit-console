// ***********************************************************************
// Copyright (c) 2019 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
#if !NET20
using System.Threading.Tasks;
#endif

namespace NUnit.Engine.Communication
{
    internal sealed class TcpServer : IDisposable
    {
        private readonly Socket _listeningSocket;
        private readonly Action<NetworkStream> _handleConnection;
        private volatile bool _isDisposed;

        public IPEndPoint ListeningOn => (IPEndPoint)_listeningSocket.LocalEndPoint;

        public static TcpServer Start(IPEndPoint bindTo, Action<NetworkStream> handleConnection)
        {
            if (bindTo is null) throw new ArgumentNullException(nameof(bindTo));

            var listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(bindTo);

            const int maxWaitingConnections = 10;
            listeningSocket.Listen(backlog: maxWaitingConnections);

            return new TcpServer(listeningSocket, handleConnection);
        }

        private TcpServer(Socket listeningSocket, Action<NetworkStream> handleConnection)
        {
            _listeningSocket = listeningSocket;
            _handleConnection = handleConnection;
            SubscribeToNextConnection();
        }

        /// <summary>
        /// Stops the server from listening but does not shut down any active connections. Signaling or waiting for
        /// active connections would be done one level higher by the connection handler that was passed to <see
        /// cref="Start"/>.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;

            ((IDisposable)_listeningSocket).Dispose();
        }

        private void SubscribeToNextConnection()
        {
            var args = new SocketAsyncEventArgs();
            args.Completed += OnConnectionAccepted;
            _listeningSocket.AcceptAsync(args);
        }

        private void OnConnectionAccepted(object sender, SocketAsyncEventArgs e)
        {
            if (_isDisposed) return;

            SubscribeToNextConnection();

#if NET20
            ThreadPool.QueueUserWorkItem(HandleConnectionSynchronously, state: e.AcceptSocket);
#else
            Task.Factory.StartNew(
                HandleConnectionSynchronously,
                e.AcceptSocket,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Current);
#endif
        }

        // Ideally this would be async so as not to block a thread, but making the connection handler asynchronous is a
        // lot of pain as long as we support .NET Framework versions earlier than 4.5.
        private void HandleConnectionSynchronously(object state)
        {
            using (var stream = new NetworkStream((Socket)state, ownsSocket: true))
            {
                _handleConnection.Invoke(stream);
            }
        }
    }
}
