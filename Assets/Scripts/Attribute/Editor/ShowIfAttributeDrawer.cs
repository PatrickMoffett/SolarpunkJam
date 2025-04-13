using System.Reflection;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using PlasticPipe.PlasticProtocol.Messages;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

[CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
public class ShowIfAttributeDrawer : PropertyDrawer
{
    private bool MeetsConditions(SerializedProperty property)
    {
        var showIfAttribute = this.attribute as ShowIfAttribute;
        object target = EditorHelper.GetTargetObjectParentOfProperty(property);
        
        List<bool> conditionValues = new List<bool>();

        foreach (var condition in showIfAttribute.Conditions)
        {
            FieldInfo conditionField = EditorHelper.GetField(target, condition);
            if (conditionField != null &&
                conditionField.FieldType == typeof(bool))
            {
                conditionValues.Add((bool)conditionField.GetValue(target));
            }

            MethodInfo conditionMethod = EditorHelper.GetMethod(target, condition);
            if (conditionMethod != null &&
                conditionMethod.ReturnType == typeof(bool) &&
                conditionMethod.GetParameters().Length == 0)
            {
                conditionValues.Add((bool)conditionMethod.Invoke(target, null));
            }
        }

        if (conditionValues.Count > 0)
        {
            bool met;
            if (showIfAttribute.Operator == ConditionOperator.And)
            {
                met = true;
                foreach (var value in conditionValues)
                {
                    met = met && value;
                }
            }
            else
            {
                met = false;
                foreach (var value in conditionValues)
                {
                    met = met || value;
                }
            }
            return met;
        }
        else
        {
            Debug.LogError("Invalid boolean condition fields or methods used!");
            return true;
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent 
                 label)
    {
        // Calcluate the property height, if we don't meet the condition and the
        // draw mode is DontDraw, then height will be 0.
        bool meetsCondition = MeetsConditions(property);
        var showIfAttribute = this.attribute as ShowIfAttribute;

        if (!meetsCondition && showIfAttribute.Action == 
            ActionOnConditionFail.DontDraw)
            return 0;

        if (property.hasVisibleChildren && property.isExpanded)
        {
            float sum = 0f;
            var it = property.Copy();
            int depth = it.depth;
            while (it.NextVisible(true)
                   && it.depth> depth)
            {
                sum += EditorGUI.GetPropertyHeight(it);
            }

            return base.GetPropertyHeight(property, label) + sum + 30f;
        }

        return base.GetPropertyHeight(property, label);
        
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent 
           label)
    {
        bool meetsCondition = MeetsConditions(property);
        // Early out, if conditions met, draw and go.
        if (meetsCondition)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return; 
        }

        var showIfAttribute = this.attribute as ShowIfAttribute;
        if(showIfAttribute.Action == ActionOnConditionFail.DontDraw)
        {
            return;
        }
        else if (showIfAttribute.Action == ActionOnConditionFail.JustDisable)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }

    }
}