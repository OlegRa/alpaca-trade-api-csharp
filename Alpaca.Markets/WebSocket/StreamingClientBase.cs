﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alpaca.Markets
{
    internal abstract class StreamingClientBase<TConfiguration> : IStreamingClient 
        where TConfiguration : StreamingClientConfiguration
    {
        private readonly SynchronizationQueue _queue = new ();

        internal readonly TConfiguration Configuration;

        private readonly WebSocketsTransport _webSocket;

        private protected StreamingClientBase(
            TConfiguration configuration)
        {
            Configuration = configuration.EnsureNotNull(nameof(configuration));
            Configuration.EnsureIsValid();

            _webSocket = new WebSocketsTransport(Configuration.ApiEndpoint);

            _webSocket.Opened += OnOpened;
            _webSocket.Closed += OnClosed;

            _webSocket.MessageReceived += OnMessageReceived;
            _webSocket.DataReceived += onDataReceived;

            _webSocket.Error += HandleError;
            _queue.OnError += HandleError;
        }

        public event Action<AuthStatus>? Connected;

        public event Action? SocketOpened;

        public event Action? SocketClosed;

        public event Action<Exception>? OnError;

        public Task ConnectAsync(
            CancellationToken cancellationToken = default)
            => _webSocket.StartAsync(cancellationToken);

        public async Task<AuthStatus> ConnectAndAuthenticateAsync(
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<AuthStatus>(TaskCreationOptions.RunContinuationsAsynchronously);
            Connected += HandleConnected;
            OnError += HandleOnError;

            await ConnectAsync(cancellationToken).ConfigureAwait(false);
            return await tcs.Task.ConfigureAwait(false);

            void HandleConnected(AuthStatus authStatus)
            {
                Connected -= HandleConnected;
                OnError -= HandleOnError;

                tcs.SetResult(authStatus);
            }

            void HandleOnError(Exception exception) =>
                HandleConnected(
                    exception is SocketException { SocketErrorCode: SocketError.IsConnected }
                        ? AuthStatus.Authorized
                        : AuthStatus.Unauthorized);
        }

        /// <inheritdoc />
        public Task DisconnectAsync(
            CancellationToken cancellationToken = default)
            => _webSocket.StopAsync(cancellationToken);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnOpened() => SocketOpened?.Invoke();

        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual void OnClosed() => SocketClosed?.Invoke();

        protected virtual void OnMessageReceived(
            String message)
        {
        }

        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual void Dispose(
            Boolean disposing)
        {
            if (!disposing)
            {
                return;
            }

            _webSocket.Opened -= OnOpened;
            _webSocket.Closed -= OnClosed;

            _webSocket.MessageReceived -= OnMessageReceived;
            _webSocket.DataReceived -= onDataReceived;

            _webSocket.Error -= HandleError;
            _queue.OnError -= OnError;

            _webSocket.Dispose();
            _queue.Dispose();
        }

        [SuppressMessage(
            "Design", "CA1031:Do not catch general exception types",
            Justification = "Expected behavior - we report exceptions via OnError event.")]
        protected void HandleMessage<TKey>(
            IDictionary<TKey, Action<JToken>> handlers,
            TKey messageType,
            JToken message)
            where TKey : class
        {
            try
            {
                if (handlers.EnsureNotNull(nameof(handlers))
                    .TryGetValue(messageType, out var handler))
                {
                    _queue.Enqueue(() => handler(message));
                }
                else
                {
                    HandleError(new InvalidOperationException(
                        $"Unexpected message type '{messageType}' received."));
                }
            }
            catch (Exception exception)
            {
                HandleError(exception);
            }
        }

        protected void OnConnected(
            AuthStatus authStatus) =>
            Connected?.Invoke(authStatus);

        protected void HandleError(
            Exception exception)
        {
            if (exception is SocketException { SocketErrorCode: SocketError.IsConnected }) 
            {
                return; // We skip that error because it doesn't matter for us
            }
            OnError?.Invoke(exception);
        }

        protected ValueTask SendAsJsonStringAsync(
            Object value,
            CancellationToken cancellationToken = default)
        {
            using var textWriter = new StringWriter();

            var serializer = new JsonSerializer();
            serializer.Serialize(textWriter, value);
            return _webSocket.SendAsync(textWriter.ToString(), cancellationToken);
        }

        private void onDataReceived(
            Byte[] binaryData) =>
            OnMessageReceived(Encoding.UTF8.GetString(binaryData));
    }
}
