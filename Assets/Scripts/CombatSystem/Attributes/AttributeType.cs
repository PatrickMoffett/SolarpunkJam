using UnityEngine;

[CreateAssetMenu(menuName = "CombatSystem/AttributeType")]
public class AttributeType : ScriptableObject
{
    // Default value for this attribute
    public float DefaultValue;

    // Another attribute to use as a maximum value (optional). Example: MaxHealth
    public AttributeType MaxAttribute;
}