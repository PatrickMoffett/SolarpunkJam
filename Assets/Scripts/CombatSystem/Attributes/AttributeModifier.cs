using System;

[Serializable]
public class AttributeModifier
{
    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Set
    }
    /// <summary>
    /// Which attribute to affect
    /// </summary>
    public AttributeType attribute;
    /// <summary>
    /// Operator to apply to Attribute
    /// </summary>
    public Operator operation;
    
    public AttributeModifierValue attributeModifierValue;
    
}
