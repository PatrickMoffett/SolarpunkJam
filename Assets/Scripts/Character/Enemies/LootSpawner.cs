using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootDrop
{
	[Tooltip("The prefab to spawn")]
	public GameObject prefab;

	[Tooltip("Relative drop chance;")]
	public float dropWeight = 0.0f;
}

public class LootSpawner : MonoBehaviour
{
	public List<LootDrop> drops = new List<LootDrop>();
	public bool singleLootSpawn = true;
	private bool lootAlreadySpawned = false;
    public void SpawnLoot()
	{
        if (drops == null || drops.Count == 0)
        {
            return; // No loot to spawn
        }

        // prevent multiple loot spawns, sometimes this was launching twice. 
        // TODO: HACK: find a better solution to prevent this
        if (singleLootSpawn && lootAlreadySpawned)
        {
            return;
        }

        // Compute total weight
        float total = 0f;
		foreach (var drop in drops)
		{
			total += Mathf.Max(0f, drop.dropWeight);
		}

		if (total <= 0f)
		{
			return; 
		}

		// Roll a random value and adjust it to the total weight
		float roll = Random.value * total;

		// Walk the list until cumulative weight exceeds roll
		float cumulative = 0f;
		foreach (var drop in drops)
		{
			float w = Mathf.Max(0f, drop.dropWeight);
			cumulative += w;
			if (roll <= cumulative)
            {
                lootAlreadySpawned = true;
                SpawnDrop(drop);
                break;
            }
        }
	}

    private void SpawnDrop(LootDrop drop)
    {
        if (drop.prefab == null)
        {
            return; // No prefab to spawn
        }

        GameObject spawn = Instantiate(
            drop.prefab,
            transform.position,
            Quaternion.identity
        );
    }
}