// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Relay;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static async Task RunAsync(string[] args)
        {
            var cts = new CancellationTokenSource();
            if (args.Length < 4)
            {
                Console.WriteLine("server [ns] [hc] [keyname] [key]");
                return;
            }

            
            var ns = args[0];
            var hc = args[1];
            var keyname = args[2];
            var key = args[3];

            
            // Create a new hybrid connection listener to listen for 
            // incoming connections.
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyname, key);
            var listener = new HybridConnectionListener(new Uri(String.Format("sb://{0}/{1}", ns, hc)), tokenProvider);
            
            // Subscribe to the status events and provide monitoring
            // transparency into the client
            listener.Connecting += (o, e) => { Console.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { Console.WriteLine("Offline"); };
            listener.Online += (o, e) => { Console.WriteLine("Online"); };

            // Opening the listener will establish the control channel to
            // the Azure Relay service. The control channel will be continuously 
            // maintained and reestablished when connectivity is disrupted.
            await listener.OpenAsync(cts.Token);
            Console.WriteLine("Server listening");

            // trigger cancellation when the user presses enter. Not awaited.
#pragma warning disable CS4014
            cts.Token.Register(() => listener.CloseAsync(CancellationToken.None));
            Task.Run(() => Console.In.ReadLineAsync().ContinueWith((s) => { cts.Cancel(); }));
#pragma warning restore CS4014

            // Enters the accept loop where new incoming connections are being
            // awaited and then handed off to a parallel task for handling: 
            do
            {
                // Accept the next available, pending connection request. 
                // Shutting down the listener will allow a clean exit with 
                // this method returning null
                var relayConnection = await listener.AcceptConnectionAsync();
                if (relayConnection == null)
                    break;

                // We turn off the warning here since we intentionally don't
                // 'await' this call, but rather let the task handling the
                // connection run out on its own without holding for it
                //
                // The task processes a new session. The connection is a fully
                // bi=drectional stream that the code puts a stream reader and a
                // stream writer over. That allows the code to read UTF-8 text
                // data that comes from the sender and to write text replies
                // back.
                //
#pragma warning disable CS4014 
                Task.Run(async () =>
                {
                    Console.WriteLine("New session");
                    // The connection is a fully bi-directional stream. 
                    // We put a stream reader and a stream writer over it 
                    // that allows us to read UTF-8 text data that comes from 
                    // the sender and to write text replies back.
                    var reader = new StreamReader(relayConnection);
                    var writer = new StreamWriter(relayConnection) { AutoFlush = true };
                    do
                    {
                        // Read a line of input until a newline is encountered
                        string line = await reader.ReadLineAsync();
                        if (String.IsNullOrEmpty(line))
                        {
                            // If there's no input data, we will signal that 
                            // we will no longer send data on this connection
                            // and then break out of the processing loop.
                            await relayConnection.ShutdownAsync(cts.Token);
                            break;
                        }
                        // Output received line to the console and write it back
                        // to the client, prefixed with "Echo:"
                        Console.WriteLine(line);
                        // Write the line back to the client, prepending "Echo:"
                        await writer.WriteLineAsync("Echo: " + line);
                    }
                    while (!cts.IsCancellationRequested);
                    Console.WriteLine("End session");
                    // closing the connection from this end
                    await relayConnection.CloseAsync(cts.Token);
                });
#pragma warning restore CS4014

            }
            while (true);

            // close the listener after we exit the processing loop
            await listener.CloseAsync(cts.Token);
        }

        static void Main(string[] args)
        {
            RunAsync(args).GetAwaiter().GetResult();
        }
    }
}
