using System.Collections;
using UnityEngine;

public class PanelBase : MonoBehaviour
{
    public string skinPath;
    public GameObject skin;
    public PanelMgr.PanelLayer layer;
    public object[] args;

    #region 生命周期

    public virtual void Init(params object[] args)
    {
        this.args = args;
    }

    public virtual void OnShowing() { }
    public virtual void OnShowed() { }
    public virtual void Update() { }
    public virtual void OnClosing() { }
    public virtual void OnClosed() { }
    #endregion

    #region 操作

    protected virtual void Close()
    {
        string name = this.GetType().ToString();
        PanelMgr.instance.ClosePanel(name);
    }
    #endregion
}
