StreamTcpServer
=======================
    
Sample project to show how powerful Akka Streams can be in helping building a high level Tcp Server.

## Requirements
As stated in the brief introduction the sample requires the .Net Framework 4.5.1. or later

## Description

The web sever shall be able to accept incoming connections and receive byte sequences from them that represent printable ASCII signs. 
These byte sequences or strings should be split at all newline-characters into smaller parts. 
After that, the server shall respond to the client with each of the split lines. 
Alternatively, it could do something else with the lines and give a special answer token, but we want to keep it simple in this example and therefore don't introduce any fancy features. 
Remember, the server needs to be able to handle multiple requests at the same time, which basically means that no request is allowed to block any other request from further execution. 
Solving all of these requirements can be hard in an imperative way - with Akka Streams however, we shouldn't need more than a few lines to solve any of these. 
First, let's have an overview over the server itself:

![Server Overview](https://raw.githubusercontent.com/Tochemey/StreamTcpServer/3eea5fde16ad03aee037376307e54deb3b5c357b/serverRequestHandler.png)

Basically, there are only three main building blocks. The first one needs to accept incoming connections. 
The second one needs to handle incoming requests and the third one needs to send a response. 

The Server logic is represented by:

![Server Logic](https://raw.githubusercontent.com/Tochemey/StreamTcpServer/3eea5fde16ad03aee037376307e54deb3b5c357b/dataflow.png)

The overall architecture of the server:

![Server Overall Architecture](https://raw.githubusercontent.com/Tochemey/StreamTcpServer/3eea5fde16ad03aee037376307e54deb3b5c357b/serverFlow.png)

## Usage
Pull the code and build it with Visual Studio. Edit the App.config file to set the server host and port.
After that you can run the exe file since we use TopShelf.
From another CLI you can run the following command if you have netcat:

```
echo "Hello World\nHow are you?" | netcat <server_address> <port>

```

## Advanced Tcp Server
Work in progress...