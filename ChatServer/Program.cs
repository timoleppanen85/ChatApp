﻿using ChatServer;
using ChatServer.Net.IO;
using System.Net;
using System.Net.Sockets;
// See https://aka.ms/new-console-template for more information

_users = new List<Client>();
_listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 14555);
_listener.Start();

while (true)
{
    var client = new Client(_listener.AcceptTcpClient());
    _users.Add(client);

    /* Broadcast connection to everyone */
    BroadcastConnection();
}

static void BroadcastConnection()
{
    foreach (var user in _users)
    {
        foreach (var usr in _users)
        {
            var broadcastPacket = new PacketBuilder();
            broadcastPacket.WriteOpCode(1);
            broadcastPacket.WriteMessage(usr.Username);
            broadcastPacket.WriteMessage(usr.UID.ToString());
            user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
        }
    }
}



public static partial class Program
{
    static List<Client> _users;
    static TcpListener _listener;

    public static void BroadcastMessage(string message)
    {
        foreach(var user in _users)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(message);
            user.ClientSocket.Client.Send(messagePacket.GetPacketBytes());
        }
    }

    public static void BroadcastDisconnect(string uid)
    {
        var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
        _users.Remove(disconnectedUser);

        foreach(var user in _users)
        {
            var broadcastPacket = new PacketBuilder();
            broadcastPacket.WriteOpCode(10);
            broadcastPacket.WriteMessage(uid);
            user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
        }

        BroadcastMessage($"[{disconnectedUser.Username}] has disconnected");
    }
}