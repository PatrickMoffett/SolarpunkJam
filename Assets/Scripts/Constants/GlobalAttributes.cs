
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

// Helper to make referencing attributes easier in code (Expand with new attributes as needed)
public static class GlobalAttributes
{
    public static AttributeType HealthAttribute = 
        Addressables.LoadAssetAsync<AttributeType>("Health").WaitForCompletion();

    public static AttributeType MaxHealthAttribute =
       Addressables.LoadAssetAsync<AttributeType>("MaxHealth").WaitForCompletion();

    public static AttributeType ManaAttribute =
       Addressables.LoadAssetAsync<AttributeType>("Mana").WaitForCompletion();

    public static AttributeType MaxManaAttribute =
       Addressables.LoadAssetAsync<AttributeType>("MaxMana").WaitForCompletion();
    
    public static AttributeType JumpHeightAttribute =
       Addressables.LoadAssetAsync<AttributeType>("JumpHeight").WaitForCompletion();
    
    public static AttributeType MoveSpeedAttribute =
       Addressables.LoadAssetAsync<AttributeType>("MoveSpeed").WaitForCompletion();
}