using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMgr
{
    public static Connection srvConn = new Connection();
    public static void Update()
    {
        srvConn.Update();
    }

    public static ProtocolBase GetHeartBeatProtocol()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeartBeat");
        return protocol;
    }


}
