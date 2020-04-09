using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class LoginPanel : PanelBase
{
    private InputField idInput;
    private Button loginBtn;

    private InputField serverAddress;

    #region 生命周期
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoginPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.Find("IDInput").GetComponent<InputField>();
        loginBtn = skinTrans.Find("LoginBtn").GetComponent<Button>();
        serverAddress = skinTrans.Find("ServerAddress").GetComponent<InputField>();

        loginBtn.onClick.AddListener(OnLoginClick);
    }

    public void OnLoginClick()
    {
        if(idInput.text == "")
        {
            Debug.Log("用户名不能为空！");
            PanelMgr.instance.OpenPanel<TipPanel>("", "用户名不能为空！");       
            return;
        }

        if(NetMgr.srvConn.status != Connection.Status.Connected)
        {
            //string host = serverAddress.text.Split(':')[0];
            //int port = int.Parse(serverAddress.text.Split(':')[1]);
            NetMgr.srvConn.proto = new ProtocolBytes();

            string host = "127.0.0.1";
            int port = 1234; 

            if(!NetMgr.srvConn.Connect(host, port))
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "连接服务器失败！");
            }
        }
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        Debug.Log("发送" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnLoginBack);
    }

    public void OnLoginBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if(ret == 0)
        {
            Debug.Log("登陆成功");
            PanelMgr.instance.OpenPanel<TipPanel>("", "登陆成功！");
            PanelMgr.instance.OpenPanel<RoomListPanel>("");
            GameMgr.instance.id = idInput.text;
            Close();
        }
        else
        {
            Debug.Log("登录失败！");
            PanelMgr.instance.OpenPanel<TipPanel>("", "登录失败！");
        }
    }
    #endregion
}
