﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Alpaca.Markets
{
    /// <summary>
    /// Provides unified type-safe access for Alpaca streaming API.
    /// </summary>
    [Obsolete("This class is deprecated and will be removed in the upcoming releases. Use the AlpacaStreamingClient class instead.", true)]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    public sealed class SockClient : IDisposable
    {
        private readonly AlpacaStreamingClient _client;

        /// <summary>
        /// Creates new instance of <see cref="SockClient"/> object.
        /// </summary>
        /// <param name="keyId">Application key identifier.</param>
        /// <param name="secretKey">Application secret key.</param>
        /// <param name="alpacaRestApi">Alpaca REST API endpoint URL.</param>
        /// <param name="webSocketFactory">Factory class for web socket wrapper creation.</param>
        public SockClient(
            String keyId,
            String secretKey,
            String? alpacaRestApi = null,
            IWebSocketFactory? webSocketFactory = null)
            : this(createConfiguration(
                keyId, secretKey, alpacaRestApi.GetUrlSafe(Environments.Live.AlpacaTradingApi), webSocketFactory))
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="SockClient"/> object.
        /// </summary>
        /// <param name="keyId">Application key identifier.</param>
        /// <param name="secretKey">Application secret key.</param>
        /// <param name="alpacaRestApi">Alpaca REST API endpoint URL.</param>
        /// <param name="webSocketFactory">Factory class for web socket wrapper creation.</param>
        public SockClient(
            String keyId,
            String secretKey,
            Uri alpacaRestApi,
            IWebSocketFactory? webSocketFactory)
            : this(createConfiguration(keyId, secretKey, alpacaRestApi, webSocketFactory))
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="SockClient"/> object.
        /// </summary>
        /// <param name="configuration">Configuration parameters object.</param>
        private SockClient(
            AlpacaStreamingClientConfiguration configuration) =>
            _client = new AlpacaStreamingClient(configuration);


        /// <summary>
        /// Occured when new account update received from stream.
        /// </summary>
        [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "Compiler issue")]
        public event Action<IAccountUpdate>? OnAccountUpdate
        {
            add => _client.OnAccountUpdate += value;
            remove => _client.OnAccountUpdate -= value;
        }

        /// <summary>
        /// Occured when new trade update received from stream.
        /// </summary>
        [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "Compiler issue")]
        public event Action<ITradeUpdate>? OnTradeUpdate
        {
            add => _client.OnTradeUpdate += value;
            remove => _client.OnTradeUpdate -= value;
        }
        
        /// <inheritdoc/>
        public void Dispose() => _client.Dispose();

        private static AlpacaStreamingClientConfiguration createConfiguration(
            String keyId,
            String secretKey,
            Uri alpacaRestApi,
            IWebSocketFactory? webSocketFactory) =>
            new AlpacaStreamingClientConfiguration
            {
                SecurityId = new SecretKey(
                    keyId ?? throw new ArgumentException("Application key id should not be null.", nameof(keyId)),
                    secretKey ?? throw new ArgumentException("Application secret key should not be null.", nameof(secretKey))),
                ApiEndpoint = alpacaRestApi ?? Environments.Live.AlpacaTradingApi,
                WebSocketFactory = webSocketFactory ?? WebSocket4NetFactory.Instance,
            };
    }
}
