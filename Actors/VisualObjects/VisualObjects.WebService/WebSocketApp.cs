// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.WebService
{
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Globalization;
    using System.Net;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebSocketApp : ICommunicationListener, IDisposable
    {
        private readonly IVisualObjectsBox visualObjectBox;
        private string listeningAddress;
        private string publishAddress;
        private HttpListener httpListener;
        private readonly string appRoot;
        private readonly string webSocketRoot;
        private readonly ServiceInitializationParameters serviceInitializationParameters;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public WebSocketApp(IVisualObjectsBox visualObjectBox, string appRoot, string webSocketRoot, ServiceInitializationParameters initParams)
        {
            this.visualObjectBox = visualObjectBox;
            this.appRoot = string.IsNullOrWhiteSpace(appRoot) ? string.Empty : appRoot.TrimEnd('/') + '/';
            this.webSocketRoot = string.IsNullOrWhiteSpace(webSocketRoot) ? string.Empty : webSocketRoot.TrimEnd('/') + '/';
            this.serviceInitializationParameters = initParams;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.Message("Initialize");

            EndpointResourceDescription serviceEndpoint = this.serviceInitializationParameters.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            int port = serviceEndpoint.Port;

            this.listeningAddress = string.Format(CultureInfo.InvariantCulture, "http://+:{0}/{1}{2}", port, this.appRoot, this.webSocketRoot);

            this.publishAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            this.Start();

            ServiceEventSource.Current.Message("Starting web socket server on {0}", this.listeningAddress);

            return Task.FromResult(this.publishAddress);
        }

        public void Start()
        {
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(this.listeningAddress);
            this.httpListener.Start();

            Task.Run(
                async () =>
                {
                    // This loop continuously listens for incoming client connections
                    // as you might normally do with a web socket server.
                    while (true)
                    {
                        ServiceEventSource.Current.Message("Waiting for connection..");

                        this.cts.Token.ThrowIfCancellationRequested();

                        HttpListenerContext context = await this.httpListener.GetContextAsync();

                        Task<Task> acceptTask = context.AcceptWebSocketAsync(null).ContinueWith(
                            async task =>
                            {
                                HttpListenerWebSocketContext websocketContext = task.Result;

                                ServiceEventSource.Current.Message("Connection from " + websocketContext.Origin);

                                using (WebSocket browserSocket = websocketContext.WebSocket)
                                {
                                    while (true)
                                    {
                                        this.cts.Token.ThrowIfCancellationRequested();
                                        byte[] buffer = Encoding.UTF8.GetBytes(this.visualObjectBox.GetJson());

                                        try
                                        {
                                            await
                                                browserSocket.SendAsync(
                                                    new ArraySegment<byte>(buffer, 0, buffer.Length),
                                                    WebSocketMessageType.Text,
                                                    true,
                                                    this.cts.Token);

                                            if (browserSocket.State != WebSocketState.Open)
                                            {
                                                break;
                                            }
                                        }
                                        catch (WebSocketException ex)
                                        {
                                            // If the browser quit or the socket was closed, exit this loop so we can get a new browser socket.
                                            ServiceEventSource.Current.Message(ex.InnerException != null ? ex.InnerException.Message : ex.Message);

                                            break;
                                        }

                                        // wait a bit and continue. This determines the client refresh rate.
                                        await Task.Delay(TimeSpan.FromMilliseconds(1), this.cts.Token);
                                    }
                                }

                                ServiceEventSource.Current.Message("Client disconnected.");
                            },
                            TaskContinuationOptions.OnlyOnRanToCompletion);
                    }
                },
                this.cts.Token);
        }

        Task ICommunicationListener.CloseAsync(CancellationToken cancellationToken)
        {
            this.StopAll();
            return Task.FromResult(true);
        }

        void ICommunicationListener.Abort()
        {
            this.StopAll();
            this.Dispose();
        }
        private void StopAll()
        {
            this.cts.Cancel();

            try
            {
                if (this.httpListener != null)
                {
                    ServiceEventSource.Current.Message("Stopping web socket server.");
                    this.httpListener.Abort();
                }
                if (this.cts != null)
                {
                    this.cts.Dispose();
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Dispose()
        {
            try
            {
                if (this.httpListener != null && this.httpListener.IsListening)
                {
                    ServiceEventSource.Current.Message("Stopping web socket server.");
                    this.httpListener.Close();
                }

            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ae)
            {
                ae.Handle(
                    ex =>
                    {
                        ServiceEventSource.Current.Message(ex.Message);
                        return true;
                    });
            }
        }
    }
}