// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.WebService
{
    using System;
    using System.Net;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebSocketApp : IDisposable
    {
        private readonly CancellationTokenSource cancellationSource;
        private readonly IVisualObjectsBox visualObjectBox;
        private HttpListener httpListener;

        public WebSocketApp(IVisualObjectsBox visualObjectBox)
        {
            this.visualObjectBox = visualObjectBox;
            this.cancellationSource = new CancellationTokenSource();
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

                if (this.cancellationSource != null && !this.cancellationSource.IsCancellationRequested)
                {
                    this.cancellationSource.Cancel();
                    this.cancellationSource.Dispose();
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

        public void Start(string url)
        {
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            ServiceEventSource.Current.Message("Starting web socket listener on " + url);

            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(url);
            this.httpListener.Start();

            Task.Run(
                async () =>
                {
                    CancellationToken cancellationToken = this.cancellationSource.Token;

                    // This loop continuously listens for incoming client connections
                    // as you might normally do with a web socket server.
                    while (true)
                    {
                        ServiceEventSource.Current.Message("Waiting for connection..");

                        cancellationToken.ThrowIfCancellationRequested();

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
                                        cancellationToken.ThrowIfCancellationRequested();

                                        string response = await this.visualObjectBox.GetObjectsAsync(cancellationToken);
                                        byte[] buffer = Encoding.UTF8.GetBytes(response);

                                        try
                                        {
                                            await
                                                browserSocket.SendAsync(
                                                    new ArraySegment<byte>(buffer, 0, buffer.Length),
                                                    WebSocketMessageType.Text,
                                                    true,
                                                    cancellationToken);

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
                                        await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
                                    }
                                }

                                ServiceEventSource.Current.Message("Client disconnected.");
                            },
                            TaskContinuationOptions.OnlyOnRanToCompletion);
                    }
                },
                this.cancellationSource.Token);
        }
    }
}