using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Services;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 
/// </summary>
public class UIManager : IService
{
    /// <summary>
    /// enum for describing UI Layers.
    /// Higher values are rendered on top
    /// </summary>
    public enum UILayer
    {
        /// Normal Layer, Bottom Layer
        Default,
        /// Loading Screen Layer, Renders on top of Default
        Loading, 
        /// Error Layer, Renders on top of everything
        Error,
    }
    
    private static readonly string ROOT_PREFAB = UIPrefabs.UIRoot;
    private Transform[] _layerTransforms;
    private GameObject _rootObject;
    private readonly Dictionary<string, UIWidget> _uiWidgets = new Dictionary<string, UIWidget>();
    
    public UIManager()
    {
        // Instantiate Root UI Canvas
        _rootObject = Object.Instantiate<GameObject>(Resources.Load<GameObject>(ROOT_PREFAB));
        Object.DontDestroyOnLoad(_rootObject);
        _layerTransforms = new Transform[((int)Enum.GetValues(typeof(UILayer)).Cast<UILayer>().Last())+1];

        for (int i = 0; i < _layerTransforms.Length; i++)
        { 
            _layerTransforms[i] = _rootObject.transform.GetChild(i);
        }
    }
    
    /// <summary>
    /// Load a new UIWidget and add it to the screen
    /// </summary>
    /// <param name="uiPrefabResourceName">name of a UI GameObject Prefab in a Resource folder</param>
    /// <param name="uICategory">Layer to add the new UIWidget to</param>
    /// <returns></returns>
    public UIWidget LoadUI(string uiPrefabResourceName, UILayer uICategory = UILayer.Default)
    {
        GameObject go = Object.Instantiate(Resources.Load<GameObject>(uiPrefabResourceName), _layerTransforms[(int)uICategory]);
        UIWidget uiWidget = new UIWidget(go);
        _uiWidgets[uiWidget.GUID] = uiWidget;
        return uiWidget;
    }

    /// <summary>
    /// Load a new UIWidget and add it to the screen
    /// </summary>
    /// <param name="uiPrefab">UI GameObject Prefab to create as a UIWidget</param>
    /// <param name="uICategory">Layer to add the new UIWidget to</param>
    /// <returns></returns>
    public UIWidget LoadUI(GameObject uiPrefab, UILayer uICategory = UILayer.Default)
    {
        GameObject go = Object.Instantiate(uiPrefab,_layerTransforms[(int)uICategory]);
        UIWidget uiWidget = new UIWidget(go);
        _uiWidgets[uiWidget.GUID] = uiWidget;
        return uiWidget;
    }
    
    /// <summary>
    /// Remove UIWidget from the screen
    /// </summary>
    /// <param name="widgetToRemove">UIWidget to remove from the screen</param>
    public void RemoveUIWidget(UIWidget widgetToRemove)
    {
        if (!_uiWidgets.ContainsKey(widgetToRemove.GUID)) return;
        Object.Destroy(_uiWidgets[widgetToRemove.GUID].UIObject);
        _uiWidgets.Remove(widgetToRemove.GUID);
    }
    /// <summary>
    /// Remove UIWidget from the screen by GUID
    /// </summary>
    /// <param name="guid">GUID of UIWidget to remove</param>
    public void RemoveUIByGuid(string guid)
    {
        if (!_uiWidgets.ContainsKey(guid)) return;
        Object.Destroy(_uiWidgets[guid].UIObject);
        _uiWidgets.Remove(guid);
    }
}
