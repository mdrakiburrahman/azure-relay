﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing.xunit;
using Microsoft.Azure.Relay.AspNetCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Azure.Relay.AspNetCore.Listener
{
    public class RequestTests : IClassFixture<LaunchSettingsFixture>
    {
        private readonly LaunchSettingsFixture launchSettingsFixture;

        public RequestTests(LaunchSettingsFixture launchSettingsFixture)
        {
            this.launchSettingsFixture = launchSettingsFixture;
        }

        [ConditionalFact]
        public async Task Request_SimpleGet_Success()
        {
            string root;
            using (var server = Utilities.CreateHttpServerReturnRoot(String.Empty, out root))
            {
                Task<string> responseTask = SendRequestAsync(root + "/SomePath?SomeQuery=foo");

                var context = await server.AcceptAsync(Utilities.DefaultTimeout);

                // General fields
                var request = context.Request;

                // Request Keys
                Assert.Equal("GET", request.Method);
                Assert.Equal(Stream.Null, request.Body);
                Assert.NotNull(request.Headers);
                Assert.Equal("https", request.Scheme);
                //Assert.Equal("/basepath", request.PathBase);
                Assert.Equal("/SomePath", request.Path);
                Assert.Equal("?SomeQuery=foo", request.QueryString);
                Assert.Equal(new Version(1, 1), request.ProtocolVersion);

                //Assert.Equal("::1", request.RemoteIpAddress.ToString());
                //Assert.NotEqual(0, request.RemotePort);
                //Assert.Equal("::1", request.LocalIpAddress.ToString());
                //Assert.NotEqual(0, request.LocalPort);

                // Note: Response keys are validated in the ResponseTests

                context.Dispose();

                string response = await responseTask;
                Assert.Equal(string.Empty, response);
            }
        }

#if FEATURE_PENDING
        [ConditionalTheory]
        [InlineData("/", "/", "", "/")]
        [InlineData("/basepath/", "/basepath", "/basepath", "")]
        [InlineData("/basepath/", "/basepath/", "/basepath", "/")]
        [InlineData("/basepath/", "/basepath/subpath", "/basepath", "/subpath")]
        [InlineData("/base path/", "/base%20path/sub%20path", "/base path", "/sub path")]
        [InlineData("/base葉path/", "/base%E8%91%89path/sub%E8%91%89path", "/base葉path", "/sub葉path")]
        [InlineData("/basepath/", "/basepath/sub%2Fpath", "/basepath", "/sub%2Fpath")]
        public async Task Request_PathSplitting(string pathBase, string requestPath, string expectedPathBase, string expectedPath)
        {
            string root;
            using (var server = Utilities.CreateHttpServerReturnRoot(pathBase, out root))
            {
                Task<string> responseTask = SendRequestAsync(root + requestPath);

                var context = await server.AcceptAsync(Utilities.DefaultTimeout);

                // General fields
                var request = context.Request;

                // Request Keys
                Assert.Equal("http", request.Scheme);
                Assert.Equal(expectedPath, request.Path);
                Assert.Equal(expectedPathBase, request.PathBase);
                Assert.Equal(string.Empty, request.QueryString);
                context.Dispose();

                string response = await responseTask;
                Assert.Equal(string.Empty, response);
            }
        }

        [ConditionalTheory]
        [InlineData("/path%")]
        [InlineData("/path%XY")]
        [InlineData("/path%F")]
        [InlineData("/path with spaces")]
        public async Task Request_MalformedPathReturns400StatusCode(string requestPath)
        {
            string root;
            using (var server = Utilities.CreateHttpServerReturnRoot("/", out root))
            {
                var responseTask = SendSocketRequestAsync(root, requestPath);
                var contextTask = server.AcceptAsync(Utilities.DefaultTimeout);
                var response = await responseTask;
                var responseStatusCode = response.Substring(9); // Skip "HTTP/1.1 "
                Assert.Equal("400", responseStatusCode);
            }
        }


        [ConditionalFact]
        public async Task Request_DoubleEscapingAllowed()
        {
            string root;
            using (var server = Utilities.CreateHttpServerReturnRoot("/", out root))
            {
                var responseTask = SendSocketRequestAsync(root, "/%252F");
                var context = await server.AcceptAsync(Utilities.DefaultTimeout);
                Assert.Equal(CombinePath(new Uri(root), "/%2F"), context.Request.Path);
            }
        }
#endif

#if FEATURE_PENDING
        [ConditionalFact]
        public async Task Request_FullUriInRequestLine_ParsesPath()
        {
            string root;
            using (var server = Utilities.CreateHttpServerReturnRoot("/", out root))
            {
                // Send a HTTP request with the request line:
                // GET http://localhost:5001 HTTP/1.1
                var responseTask = SendSocketRequestAsync(root, root);
                var context = await server.AcceptAsync(Utilities.DefaultTimeout);
                Assert.Equal(CombinePath(new Uri(root), "/"), context.Request.Path);
                Assert.Equal("", context.Request.PathBase);
                Assert.Equal(root, context.Request.RawUrl);
                Assert.False(root.EndsWith("/")); // make sure root doesn't have a trailing slash
            }
        }
#endif

#if FEATURE_PENDING
        [ConditionalFact]
        public async Task Request_OptionsStar_EmptyPath()
        {
            string root;
            using (var server = Utilities.CreateHttpServerReturnRoot("/", out root))
            {
                var responseTask = SendSocketRequestAsync(root, "*", "OPTIONS");
                var context = await server.AcceptAsync(Utilities.DefaultTimeout);
                Assert.Equal("", context.Request.PathBase);
                Assert.Equal("", context.Request.Path);
                Assert.Equal("*", context.Request.RawUrl);
                context.Dispose();
            }
        }
#endif

#if FEATURE_PENDING
        [ConditionalTheory]
        [InlineData("%D0%A4", "Ф")]
        [InlineData("%d0%a4", "Ф")]
        [InlineData("%E0%A4%AD", "भ")]
        [InlineData("%e0%A4%Ad", "भ")]
        [InlineData("%F0%A4%AD%A2", "𤭢")]
        [InlineData("%F0%a4%Ad%a2", "𤭢")]
        [InlineData("%48%65%6C%6C%6F%20%57%6F%72%6C%64", "Hello World")]
        [InlineData("%48%65%6C%6C%6F%2D%C2%B5%40%C3%9F%C3%B6%C3%A4%C3%BC%C3%A0%C3%A1", "Hello-µ@ßöäüàá")]
        // Test the borderline cases of overlong UTF8.
        [InlineData("%C2%80", "\u0080")]
        [InlineData("%E0%A0%80", "\u0800")]
        [InlineData("%F0%90%80%80", "\U00010000")]
        [InlineData("%63", "c")]
        [InlineData("%32", "2")]
        [InlineData("%20", " ")]
        // Internationalized
        [InlineData("%C3%84ra%20Benetton", "Ära Benetton")]
        [InlineData("%E6%88%91%E8%87%AA%E6%A8%AA%E5%88%80%E5%90%91%E5%A4%A9%E7%AC%91%E5%8E%BB%E7%95%99%E8%82%9D%E8%83%86%E4%B8%A4%E6%98%86%E4%BB%91", "我自横刀向天笑去留肝胆两昆仑")]
        // Skip forward slash
        [InlineData("%2F", "%2F")]
        [InlineData("foo%2Fbar", "foo%2Fbar")]
        [InlineData("foo%2F%20bar", "foo%2F bar")]
        public async Task Request_PathDecodingValidUTF8(string requestPath, string expect)
        {
            string root;
            string actualPath;
            using (var server = Utilities.CreateHttpServerReturnRoot("/", out root))
            {
                var responseTask = SendSocketRequestAsync(root, "/" + requestPath);
                var context = await server.AcceptAsync(Utilities.DefaultTimeout);
                actualPath = context.Request.Path;
                context.Dispose();

                var response = await responseTask;
                Assert.Equal("200", response.Substring(9));
            }

            Assert.Equal(expect, actualPath.TrimStart('/'));
        }

        [ConditionalTheory]
        [InlineData("/%%32")]
        [InlineData("/%%20")]
        [InlineData("/%F0%8F%8F%BF")]
        [InlineData("/%")]
        [InlineData("/%%")]
        [InlineData("/%A")]
        [InlineData("/%Y")]
        public async Task Request_PathDecodingInvalidUTF8(string requestPath)
        {
            string root;
            using (var server = Utilities.CreateHttpServerReturnRoot(String.Empty, out root))
            {
                var responseTask = SendSocketRequestAsync(root, requestPath);
                var contextTask = server.AcceptAsync(Utilities.DefaultTimeout);

                var response = await responseTask;
                Assert.Equal("400", response.Substring(9));
            }
        }

        [ConditionalTheory]
        // Overlong ASCII
        [InlineData("/%C0%A4", "/%C0%A4")]
        [InlineData("/%C1%BF", "/%C1%BF")]
        [InlineData("/%E0%80%AF", "/%E0%80%AF")]
        [InlineData("/%E0%9F%BF", "/%E0%9F%BF")]
        [InlineData("/%F0%80%80%AF", "/%F0%80%80%AF")]
        [InlineData("/%F0%80%BF%BF", "/%F0%80%BF%BF")]
        // Mixed
        [InlineData("/%C0%A4%32", "/%C0%A42")]
        [InlineData("/%32%C0%A4%32", "/2%C0%A42")]
        [InlineData("/%C0%32%A4", "/%C02%A4")]
        public async Task Request_OverlongUTF8Path(string requestPath, string expectedPath)
        {
            string root;
            using (var server = Utilities.CreateHttpServerReturnRoot("/", out root))
            {
                var responseTask = SendSocketRequestAsync(root, requestPath);
                var context = await server.AcceptAsync(Utilities.DefaultTimeout);
                Assert.Equal(CombinePath(new Uri(root), expectedPath), context.Request.Path);
                context.Dispose();

                var response = await responseTask;
                Assert.Equal("200", response.Substring(9));
            }
        }
#endif

        private async Task<string> SendRequestAsync(string uri)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(uri);
            }
        }

        private async Task<string> SendSocketRequestAsync(string address, string path, string method = "GET")
        {
            var uri = new Uri(address);
            if ( !Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                path = CombinePath(uri, path);
            }            
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"{method} {path} HTTP/1.1");
            builder.AppendLine("Connection: close");
            builder.Append("HOST: ");
            builder.AppendLine(uri.Authority);
            builder.AppendLine();

            byte[] request = Encoding.ASCII.GetBytes(builder.ToString());

            using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(uri.Host, uri.Port);
                using (var ssl = new SslStream(new NetworkStream(socket)))
                {
                    ssl.AuthenticateAsClient(uri.Authority);
                    ssl.Write(request);
                    var response = new byte[12];
                    await Task.Run(() => ssl.Read(response, 0, response.Length));
                    return Encoding.ASCII.GetString(response);
                }
            }
        }

        private static string CombinePath(Uri uri, string path)
        {
            return uri.AbsolutePath + (path.StartsWith("/")?"":"/") + path;
        }
    }
}
