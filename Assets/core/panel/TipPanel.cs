using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : PanelBase
{
    private Text text;
    private Button btn;
    string str = "";

    #region 生命周期

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "TipPanel";
        layer = PanelMgr.PanelLayer.Tips;

        if(args.Length == 1)
        {
            str = (string)args[0];
        }
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;

        text = skinTrans.Find("Text").GetComponent<Text>();
        text.text = str;
        btn = skinTrans.Find("Btn").GetComponent<Button>();
        btn.onClick.AddListener(OnBtnClick);
    }

    #endregion

    public void OnBtnClick()
    {
        Close();
    }
}
