using UnityEngine;

// Helper to make referencing attributes easier in code (Expand with new attributes as needed)
public static class GlobalAttributes
{
    public static AttributeType HealthAttribute =
        Resources.Load<AttributeType >("Health");

    public static AttributeType MaxHealthAttribute =
       Resources.Load<AttributeType>("MaxHealth");

    public static AttributeType ManaAttribute =
       Resources.Load<AttributeType>("Mana");

    public static AttributeType MaxManaAttribute =
       Resources.Load<AttributeType>("MaxMana");
    
    public static AttributeType JumpHeightAttribute =
       Resources.Load<AttributeType>("JumpHeight");
    
    public static AttributeType MoveSpeedAttribute =
       Resources.Load<AttributeType>("MoveSpeed");

    public static AttributeType MoveAccelerationAttribute =
   Resources.Load<AttributeType>("MoveAcceleration");

    public static AttributeType AirControlAccelerationAttribute =
   Resources.Load<AttributeType>("AirControlAcceleration");
}