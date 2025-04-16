using UnityEngine;

[RequireComponent(typeof(CombatSystem))]
[RequireComponent(typeof(AttributeSet))]
public class Character : MonoBehaviour
{
    protected CombatSystem _combatSystem;
    protected AttributeSet _attributeSet;
    protected virtual void Awake()
    {
        // Initialize the CombatSystem and AttributeSet components
        _combatSystem = GetComponent<CombatSystem>();
        _attributeSet = GetComponent<AttributeSet>();
    }
}