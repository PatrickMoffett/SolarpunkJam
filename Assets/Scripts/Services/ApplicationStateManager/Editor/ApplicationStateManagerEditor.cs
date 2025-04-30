using System.Collections.Generic;
using StackStateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Services
{
    public class ApplicationStateManagerEditor : EditorWindow
    {

        private DropdownField _stateDropDownField;
        private Button _pushButton;
        private Button _popButton;
        private VisualElement _visualStateStackList;
        
        private ApplicationStateManager _applicationStateManager;
        private List<string> _applicationStateNames;
        private TypeCache.TypeCollection _types;
    
        [MenuItem("Services/ApplicationStateManager Debug Window")]
        public static void OpenWindow()
        {
            ApplicationStateManagerEditor wnd = GetWindow<ApplicationStateManagerEditor>();
            wnd.titleContent = new GUIContent("ApplicationStateManagerEditor");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Services/ApplicationStateManager/Editor/ApplicationStateManagerEditor.uxml");
            VisualElement labelFromUxml = visualTree.Instantiate();
            root.Add(labelFromUxml);
        
            //Get visual elements
            _visualStateStackList = root.Q<VisualElement>("StateStackList");
            _pushButton = root.Q<Button>("PushStateButton");
            _popButton = root.Q<Button>("PopStateButton");
            _stateDropDownField = root.Q<DropdownField>("StateToPushDropDown");

            //get all types inheriting from BaseApplicationState and add them to Drop Down List as choices
            _types = TypeCache.GetTypesDerivedFrom<BaseApplicationState>();
            _applicationStateNames = new List<string>();
            foreach (var type in _types)
            {
                _applicationStateNames.Add(type.Name);
            }
            _stateDropDownField.choices = _applicationStateNames;
            
            //binds buttons to functions
            _popButton.clicked += PopButtonClicked;
            _pushButton.clicked += PushButtonClicked;

            
            if (ServiceLocator.Instance != null)
            {
                _applicationStateManager = ServiceLocator.Instance.Get<ApplicationStateManager>();
                RefreshStateListVisual(null);
                _applicationStateManager.StatePushed += RefreshStateListVisual;
                _applicationStateManager.StatePopped += RefreshStateListVisual;
            }
        }

        private void OnDestroy()
        {
            //Stop listening for buttons
            _popButton.clicked -= PopButtonClicked;
            _pushButton.clicked -= PushButtonClicked;
        }

        private void RefreshStateListVisual(StackStateMachineBaseState baseState)
        {
            for (int i = _visualStateStackList.childCount - 1; i >= 0; i--)
            {
                _visualStateStackList.RemoveAt(i);
            }

            var states = _applicationStateManager.GetStates();
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var field = new TextField
                {
                    value = states[i].GetType().FullName
                };
                _visualStateStackList.Insert(0,field); 
            }
        }

        private void PushButtonClicked()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("Cannot change state while game isn't running");
                return;
            }else if (_applicationStateManager == null)
            {
                Debug.LogError("Application State Manager not set");
                return;
            }else if (_stateDropDownField.index < 0 || _stateDropDownField.index > _types.Count)
            {
                Debug.LogError("Valid Application State not selected");
                return;
            }
            //Call Push State on Application State Manager
            //TODO: figure out how to allow dictionary options to be added
            typeof(ApplicationStateManager).GetMethod("PushState") //get the method
                ?.MakeGenericMethod(_types[_stateDropDownField.index]) //create a method for our type
                .Invoke(_applicationStateManager, new object[] {false,null}); //invoke the new method with params

        }
        private void PopButtonClicked()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("Cannot change state while game isn't running");
                return;
            }else if (_applicationStateManager == null)
            {
                Debug.LogError("Application State Manager not set");
                return;
            }
        
            _applicationStateManager.PopState();
        }
    }
}