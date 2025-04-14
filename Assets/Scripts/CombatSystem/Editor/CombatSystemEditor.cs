using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
public class CombatSystemEditor : EditorWindow
{
    private DropdownField _effectDropDownField;
    private Button _addEffectButton;
    private Button _removeEffectButton;
    private ListView _statusEffectList;
    private Label _gameObjectNameLabel;
    private VisualElement _attributesBlock;
    private VisualElement _attributesContainer;

    private List<StatusEffect> _statusEffectsScriptableObjects;
    private List<string> _statusEffectScriptableObjectNames;

    private CombatSystem _lastSelectedCombatSystem;
    private AttributeSet _attributeSet;
    private List<StatusEffect> _activeStatusEffects;

    private CombatSystem _debugSource;

    [MenuItem("CombatSystem/CombatSystem Debug Window")]
    public static void OpenWindow()
    {
        CombatSystemEditor wnd = GetWindow<CombatSystemEditor>();
        wnd.titleContent = new GUIContent("CombatSystemEditorEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/CombatSystem/Editor/CombatSystemEditor.uxml");
        VisualElement labelFromUxml = visualTree.Instantiate();
        root.Add(labelFromUxml);

        //Get visual elements
        _statusEffectList = root.Q<ListView>("EffectList");
        _addEffectButton = root.Q<Button>("AddEffectButton");
        _removeEffectButton = root.Q<Button>("RemoveEffectButton");
        _effectDropDownField = root.Q<DropdownField>("EffectToAddDropDown");
        _gameObjectNameLabel = root.Q<Label>("GameobjectName");
        _attributesBlock = root.Q<VisualElement>("AttributesBlock");
        _attributesContainer = _attributesBlock.Q<VisualElement>("AttributesContainer");

        _gameObjectNameLabel.name = "No Object Selected!";

        //get all status effect Scriptable Objects
        string[] guids = AssetDatabase.FindAssets("t:"+ nameof(StatusEffect));
        _statusEffectsScriptableObjects = new List<StatusEffect>();
        for(int i =0;i<guids.Length;i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            _statusEffectsScriptableObjects.Add(AssetDatabase.LoadAssetAtPath<StatusEffect>(path));
        }
        
        //put all status effect names into dropdown list
        _statusEffectScriptableObjectNames = new List<string>();
        foreach (var effect in _statusEffectsScriptableObjects)
        {
            _statusEffectScriptableObjectNames.Add(effect.name);
        }
        _effectDropDownField.choices = _statusEffectScriptableObjectNames;

        
        //bind create list item function
        _statusEffectList.makeItem += MakeStatusEffectListItem;
        _statusEffectList.bindItem += BindItem;
        _statusEffectList.itemsSource = _activeStatusEffects;
        _statusEffectList.fixedItemHeight = 16f;


        //binds buttons to functions
        _removeEffectButton.clicked += RemoveEffectButtonClicked;
        _addEffectButton.clicked += AddEffectButtonClicked;
        
        EditorApplication.playModeStateChanged += change =>
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                _lastSelectedCombatSystem = null;
                _gameObjectNameLabel.text = "No Object Selected";
                // Clear previous content.
                _attributesContainer.Clear();
            }
        };
    }
    private void OnDestroy()
    {
        //Stop listening for buttons
        _removeEffectButton.clicked -= RemoveEffectButtonClicked;
        _addEffectButton.clicked -= AddEffectButtonClicked;
    }
    private void OnInspectorUpdate()
    {
        if (!EditorApplication.isPlaying) { return; }
        if (!_debugSource)
        {
            GameObject debug = new GameObject("Debug CombatSystem");
            _debugSource = debug.AddComponent<CombatSystem>();
            AttributeSet attributeSet = debug.GetComponent<AttributeSet>();
            attributeSet.Reset();
            attributeSet.InitializeAttributeDictionary();
            DontDestroyOnLoad(debug);
        }

        if (_lastSelectedCombatSystem == null)
        {
            GameObject[] gameObjects = new GameObject[1]; 
            gameObjects[0]= GameObject.Find("Player");
            if (gameObjects[0] != null)
            {
                newCombatSystemSelected(gameObjects);
            }
        }
        else if(Selection.gameObjects.Length > 0 &&
                !Selection.gameObjects.Contains(_lastSelectedCombatSystem.gameObject))
        {
            newCombatSystemSelected(Selection.gameObjects);
        }
        else
        {
            Refresh();
        }
    }

    private void newCombatSystemSelected(GameObject[] gameObjects)
    {
        foreach (var gameObject in gameObjects)
        {
            CombatSystem combatSystem= gameObject.GetComponent<CombatSystem>();
            if (combatSystem == null)
            {
                continue;
            }
            if (combatSystem == _lastSelectedCombatSystem)
            {
                // If the selected combat system is the same as the last one, do nothing.
                //continue;
            }
            
            _gameObjectNameLabel.text = gameObject.name;
            _lastSelectedCombatSystem = combatSystem;
            _attributeSet = gameObject.GetComponent<AttributeSet>();
            _statusEffectList.itemsSource = _lastSelectedCombatSystem.GetStatusEffects();
            _lastSelectedCombatSystem.StatusEffectAdded += Refresh;
            _lastSelectedCombatSystem.StatusEffectRemoved += Refresh;
            Refresh();
            return;           
        }
    }

    private void Refresh()
    {
        // Refresh your status effect list if needed.
        _statusEffectList.RefreshItems();

        if (_attributesContainer == null)
        {
            Debug.LogError("AttributesContainer not found in UXML.");
            return;
        }

        // Clear previous content.
        _attributesContainer.Clear();

        // Check if the AttributeSet exists.
        if (_attributeSet == null)
        {
            return;
        }

        // Iterate over each attribute entry in your AttributeSet.
        foreach (var entry in _attributeSet.AttributesDictionary)
        {
            // Create a new VisualElement for this attribute.
            VisualElement attributeRow = new VisualElement();
            attributeRow.style.flexDirection = FlexDirection.Row;
            attributeRow.style.marginBottom = 4;

            // Create a label for the attribute name (assumes your ScriptableObject's name is descriptive).
            Label nameLabel = new Label(entry.Key.name)
            {
                style =
            {
                flexGrow = 1f,
                unityTextAlign = TextAnchor.MiddleLeft,
                minWidth = 100
            }
            };

            // Create labels for the current value and base value.
            Label currentValueLabel = new Label("Current: " + entry.Value.CurrentValue.ToString())
            {
                style =
            {
                flexGrow = 1f,
                unityTextAlign = TextAnchor.MiddleCenter,
                minWidth = 80
            }
            };

            Label baseValueLabel = new Label("Base: " + entry.Value.BaseValue.ToString())
            {
                style =
            {
                flexGrow = 1f,
                unityTextAlign = TextAnchor.MiddleRight,
                minWidth = 80
            }
            };

            // Add the labels to the row.
            attributeRow.Add(nameLabel);
            attributeRow.Add(currentValueLabel);
            attributeRow.Add(baseValueLabel);

            // Finally, add the row to the container.
            _attributesContainer.Add(attributeRow);
        }
    }


    VisualElement MakeStatusEffectListItem()
    {
        Label label = new Label
        {
            style =
            {
                flexGrow = 1f,
                alignContent = new StyleEnum<Align>(Align.Stretch)
            }
        };
        return label;
    }

    void BindItem(VisualElement e, int i)
    {
        if (_lastSelectedCombatSystem == null) { return; }
        ((Label)e).text = _lastSelectedCombatSystem.GetStatusEffects()[i].EffectName;
    }

    private void AddEffectButtonClicked()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("Cannot change state while game isn't running");
            return;
        }

        if (!_lastSelectedCombatSystem)
        {
            Debug.LogError("No Combat System found");
            return;
        }

        OutgoingStatusEffectInstance outgoingStatusEffect =
            new OutgoingStatusEffectInstance(_statusEffectsScriptableObjects[_effectDropDownField.index], _debugSource);
        _lastSelectedCombatSystem.ApplyStatusEffect(outgoingStatusEffect);
    }
    private void RemoveEffectButtonClicked()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("Cannot change state while game isn't running");
            return;
        }

        if (_lastSelectedCombatSystem == null)
        {
            Debug.LogError("Can't remove effects with no CombatSystem Selected");
            return;
        }
        _lastSelectedCombatSystem.RemoveStatusEffect(_lastSelectedCombatSystem.GetStatusEffects()[_statusEffectList.selectedIndex]);
    }
}
