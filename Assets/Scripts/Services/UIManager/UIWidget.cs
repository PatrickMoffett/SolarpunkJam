using UnityEngine;

public class UIWidget
{
    public string GUID { get; private set; }
    public GameObject UIObject { get; private set; }
    public UIWidget(GameObject go)
    {
        GUID = System.Guid.NewGuid().ToString();
        UIObject = go;
    }
}
