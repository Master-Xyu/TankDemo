using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.IO;

public class Connection
{
    const int Buffer_Size = 1024;
    private Socket socket;
    private byte[] readBuff = new byte[Buffer_Size];
    private int buffCount = 0;

    private Int32 msgLength = 0;
    private byte[] lenBytes = new byte[sizeof(Int32)];

    public ProtocolBase proto;

    public float lastTickTime = 0;
    public float heartBeatTime = 30;

    public MsgDistribution msgDist = new MsgDistribution();

    public enum Status
    {
        None,
        Connected,
    };

    public Status status = Status.None;

    public bool Connect(string host, int port)
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(host, port);

            socket.BeginReceive(readBuff, buffCount, Buffer_Size - buffCount, SocketFlags.None, ReceiveCb, readBuff);
            Debug.Log("连接成功");
            status = Status.Connected;
            return true;
        }catch(Exception e)
        {
            Debug.Log("连接失败：" + e.Message);
            return false;
        }
    }

    public bool Close()
    {
        try
        {
            socket.Close();
            return true;
        }catch(Exception e)
        {
            Debug.Log("关闭失败：" + e.Message);
            return false;
        }
    }

    public void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            int count = socket.EndReceive(ar);
            buffCount = buffCount + count;
            ProcessData();
            socket.BeginReceive(readBuff, buffCount, Buffer_Size - buffCount, SocketFlags.None, ReceiveCb, readBuff);
        }catch(Exception e)
        {
            Debug.Log("ReceiveCb失败：" + e.Message);
            status = Status.None;
        }
    }

    private void ProcessData()
    {
        if (buffCount < sizeof(Int32))
            return;

        Array.Copy(readBuff, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if (buffCount < msgLength + sizeof(Int32))
            return;

        ProtocolBase protocol = proto.Decode(readBuff, sizeof(Int32), msgLength);
        Debug.Log("收到消息 " + protocol.GetDesc());
        lock (msgDist.msgList)
        {
            msgDist.msgList.Add(protocol);
        }
        int count = buffCount - msgLength - sizeof(Int32);
        Array.Copy(readBuff, sizeof(Int32) + msgLength, readBuff, 0, count);
        buffCount = count;
        if(buffCount > 0)
        {
            ProcessData();
        }
    }

    public bool Send(ProtocolBase protocol)
    {
        if (status != Status.Connected)
        {
            Debug.LogError("【Connection】还没连接就发送数据是不好的");
            return true;
        }

        byte[] b = protocol.Encode();
        byte[] length = BitConverter.GetBytes(b.Length);

        byte[] sendbuff = length.Concat(b).ToArray();
        socket.Send(sendbuff);
        Debug.Log("发送消息 " + protocol.GetDesc());
        return true;
    }

    public bool Send(ProtocolBase protocol, string cbName, MsgDistribution.Delegate cb)
    {
        if (status != Status.Connected)
            return false;
        msgDist.AddOnceListener(cbName, cb);
        return Send(protocol);
    }

    public bool Send(ProtocolBase protocol, MsgDistribution.Delegate cb)
    {
        string cbName = protocol.GetName();
        return Send(protocol, cbName, cb);
    }

    public void Update()
    {
        msgDist.Update();
        if(status == Status.Connected)
        {
            if(Time.time - lastTickTime > heartBeatTime)
            {
                ProtocolBase protocol = NetMgr.GetHeartBeatProtocol();
                Send(protocol);
                lastTickTime = Time.time;
            }
        }
    }
}
