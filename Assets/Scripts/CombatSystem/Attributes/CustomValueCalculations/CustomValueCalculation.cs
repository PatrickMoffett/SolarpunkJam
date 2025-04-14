using UnityEngine;

public abstract class CustomValueCalculation : ScriptableObject
{
    public abstract float Calculate(CombatSystem source, CombatSystem target);
}
