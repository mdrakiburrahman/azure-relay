# Azure Hybrid Connections samples for .NET Standard

Websocket: ➡️
HTTP: 🕸️

1. [x] [➡️ `1.simple-websocket`](./1.simple-websocket/README.md) - The "Simple Websocket" sample illustrates the basic functions of the API and shows how to bi-directionally exchange blocks of text over a connection
2. [ ] [➡️ `2.thrift`](./2.thrift/README.md) - The "Thrift" sample is a variation of the C# sample that is part of the Apache Thrift project and shows how to use the Thrift RPC model
over Hybrid Connections.
3. [ ] [➡️ `3.bond`](./3.bond/README.md) - The "Bond" sample illustrates how to use Microsoft Bond Comm RPC with the Relay. The sample includes an standardalone implementation of 
an alternate Epoxy transport that uses Hybrid Connections instead of TCP.
4. [ ] [➡️ `4.portbridge`](./4.portbridge/README.md) - The "PortBridge" sample is a port of one of the classic flagship samples of the WCF Relay capability over to the new Hybrid Connection Relay. PortBridge creates multiplexed socket tunnels that can bridge TCP and Named Pipe socket connections across the Relay, including **⚠️ SQL Server ⚠️** and Remote Deskop Connections.
5. [ ] [🕸️ `5.simple-http`](./5.simple-http/README.md) - The Simple HTTP sample illustrates the basic functions of the HTTP API and shows how to handle a simple HTTP request
6. [ ] [🕸️ `6.hcreverseproxy`](./6.hcreverseproxy/README.md) - The Reverse Proxy sample shows how to build a simple reverse proxy that allows exposing existing web sites and services through the Relay.
7. [ ] [🕸️ `7.aspnet`](./7.aspnet/README.md) - The ASP.NET sample is an hosting extension for ASP.NET Core, allowing ASP.NET Core based sites and services to be hosted behind the Relay.
8. [ ] [🕸️ `8.rolebasedaccesscontrol`](./8.rolebasedaccesscontrol/README.md) - A sample illustrating how to create a Relay Sender and a Relay Listener instances with **⚠️ Managed Identities ⚠️** and AAD RBAC Authentication methods.
