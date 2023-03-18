# `Simple` WebSocket Hybrid Connection Sample 

<!-- TOC depthfrom:2 -->

- [`Simple` WebSocket Hybrid Connection Sample](#simple-websocket-hybrid-connection-sample)
  - [Build](#build)
  - [Run](#run)
  - [Scale](#scale)

<!-- /TOC -->


This sample shows how to use the Hybrid Connections API for WebSockets from C#.

It consists of a simple command line client and a simple command line server app. Both applications take the same set of input arguments to launch.
## Build
Updated to Dotnet Framework v4.8. Build like this:

![Build](_images/1-build.png)

## Run

Once you have built the solution, first start the server with 

```server.exe  [ns] [hc] [keyname] [key]```

and then start the client with 

```client.exe  [ns] [hc] [keyname] [key]```

whereby the arguments are as follows:

* [ns] - fully qualified domain name for the Azure Relay namespace, eg. contoso.servicebus.windows.net
* [hc] - name of a previously configured Hybrid Connection (see [main README](../../README.md))
* [keyname] - name of a SAS rule that confers the required right(s). "Listen" is required for the 
server, "Send" is required for the client. The default management rule confers both.
* [key] - the key for the SAS rules

Example:

```powershell


```

Once started, the client will connect to the server. You can then enter lines of text which will be sent to the server and returned. A blank line closes the connection.

## Scale

You can start multiple server instances bound to the same Hybrid Connection to see the effects of the Relay's load balancing capability, as client connections will be distributed across existing listeners, up to 25 concurrent.
 * `TODO` In less confusing words....

You can obviously also start multiple server instances bound to the same Hybrid Connection concurrently on different machines. 
 * `TODO` In less confusing words....

You can create several hundred concurrent client instances if you like.
 * `TODO` In less confusing words....


