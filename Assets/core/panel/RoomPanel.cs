using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : PanelBase
{
    private List<Transform> prefabs = new List<Transform>();
    private Button closeBtn;
    private Button startBtn;
    private Button[] kickBtns = new Button[6];

    #region 生命周期
    ///<summary> 初始化 </summary>
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;

        for(int i = 0; i < 6; i++)
        {
            string name = "PlayerPrefab" + i.ToString();
            Transform prefab = skinTrans.Find(name);
            prefabs.Add(prefab);
        }

        closeBtn = skinTrans.Find("CloseBtn").GetComponent<Button>();
        startBtn = skinTrans.Find("StartBtn").GetComponent<Button>();

        closeBtn.onClick.AddListener(OnCloseClick);
        startBtn.onClick.AddListener(OnStartClick);

        NetMgr.srvConn.msgDist.AddListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.AddListener("Fight", RecvFight);
        NetMgr.srvConn.msgDist.AddListener("BeKicked", RecvBeKicked);

        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        NetMgr.srvConn.Send(protocol);
    }
    #endregion

    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.DelListener("Fight", RecvFight);
        NetMgr.srvConn.msgDist.DelListener("BeKicked", RecvBeKicked);
    }

    public void RecvBeKicked(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int type = proto.GetInt(start, ref start);
        if(type == 0)
            PanelMgr.instance.OpenPanel<TipPanel>("", "被房主踢出房间！");
        else
            PanelMgr.instance.OpenPanel<TipPanel>("", "空闲超过5分钟自动销毁房间！");
        PanelMgr.instance.OpenPanel<RoomListPanel>("");
        Close();
    }

    public void RecvGetRoomInfo(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);

        int i = 0;
        string ownerID = null;

        for(i = 0; i < count; i++)
        {
            string id = proto.GetString(start, ref start);
            int team = proto.GetInt(start, ref start);
            int isOwner = proto.GetInt(start, ref start);

            Transform trans = prefabs[i];
            Text text = trans.Find("Text").GetComponent<Text>();
            Button kickBtn = trans.Find("KickBtn").GetComponent<Button>();
            kickBtn.gameObject.SetActive(true);

            string str = "名字：" + id + "\r\n";
            str += "阵营：" + (team == 1 ? "红" : "绿") + "\r\n";

            if (isOwner == 1)
            {
                str += "【房主】" + "\r\n";
                ownerID = id;
                Debug.Log(ownerID);
            }
            if(id == GameMgr.instance.id)
            {
                str += "【我自己】";
                kickBtn.gameObject.SetActive(false);
            }        

            text.text = str;

            kickBtns[i] = kickBtn;

            kickBtn.onClick.AddListener(() => OnKickClick(id));

            if(team == 1)
            {
                trans.GetComponent<Image>().color = Color.red;
            }
            else
            {
                trans.GetComponent<Image>().color = Color.green;
            }
        }

        if (ownerID != GameMgr.instance.id)
        {
            for(int j = 0; j < i; j++)
            {
                kickBtns[j].gameObject.SetActive(false);
            }
        }

        for (; i < 6; i++)
        {
            Transform trans = prefabs[i];
            Text text = trans.Find("Text").GetComponent<Text>();
            Button kickBtn = trans.Find("KickBtn").GetComponent<Button>();
            kickBtns[i] = kickBtn;
            text.text = "【等待玩家】";
            trans.GetComponent<Image>().color = Color.gray;
            kickBtn.gameObject.SetActive(false);
        }
    }

    public void OnCloseClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveRoom");
        NetMgr.srvConn.Send(protocol, OnCloseBack);
    }

    public void OnCloseBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if(ret == 0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "退出成功！");
            PanelMgr.instance.OpenPanel<RoomListPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "退出失败！");
        }
    }

    public void OnStartClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartFight");
        NetMgr.srvConn.Send(protocol, OnStartBack);
    }

    public void OnStartBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if(ret != 0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "开始游戏失败！两队至少都需要一名玩家，只有队长可以开始战斗！");
        }
    }

    public void RecvFight(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //MultiBattle.instance.StartBattle(proto);
        Close();
    }

    public void OnKickClick(string id)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("KickPlayer");
        protocol.AddString(id);
        NetMgr.srvConn.Send(protocol, OnKickBack);
    }

    public void OnKickBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if(ret == 0)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "踢出失败！");
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "踢出成功！");
        }
    }
}
