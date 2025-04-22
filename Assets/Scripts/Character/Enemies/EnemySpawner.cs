using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnDelay = 2f;
    private void Start()
    {
        SpawnEnemy();
    }
    private void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = transform.position.y;
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        Assert.IsNotNull(enemy, $"Enemy prefab does not have an Enemy component.");
        enemy.OnCharacterDeath += OnEnemyDeath;
    }

    private void OnEnemyDeath(Character character)
    {
        character.OnCharacterDeath -= OnEnemyDeath;
        StartCoroutine(SpawnEnemyCoroutine());
    }

    IEnumerator SpawnEnemyCoroutine()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnEnemy();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}