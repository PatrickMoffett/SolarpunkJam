using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float initialDelay = 0f;
    [SerializeField] private float spawnDelay = 2f;
    [SerializeField] private int maxEnemiesSpawned = 5;
    [SerializeField] private bool dontSpawnOnScreen = false;
    [SerializeField] private bool respawnOnDeath = true;

    private bool initialDelayPassed = false;
    private int currentEnemiesSpawned = 0;
    private Coroutine spawnCoroutine;
    public static GameObject _bucket;

    private void OnEnable()
    {
        if (!_bucket)
        {
            _bucket = new GameObject("SpawnedEnemy_Bucket");
        }
        spawnCoroutine = StartCoroutine(SpawnEnemyCoroutine());
    }
    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
    private void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = transform.position.y;
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        if (!_bucket)
        {
            _bucket = new GameObject("SpawnedEnemy_Bucket");
        }
        // add the enemy to the bucket
        enemyObject.transform.parent = _bucket.transform;

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        Assert.IsNotNull(enemy, $"Enemy prefab does not have an Enemy component.");
        enemy.OnCharacterDeath += OnEnemyDeath;

        currentEnemiesSpawned++;
    }

    private void OnEnemyDeath(Character character)
    {
        if (!respawnOnDeath)
        {
            currentEnemiesSpawned--;
        }
        character.OnCharacterDeath -= OnEnemyDeath;
    }

    IEnumerator SpawnEnemyCoroutine()
    {
        yield return new WaitForSeconds(initialDelay);
        while (true)
        { 
            yield return new WaitForSeconds(spawnDelay);
            if (ShouldSpawn())
            {
                SpawnEnemy();
            }
        }
    }

    private bool ShouldSpawn()
    {
        bool underMaxEnemies = currentEnemiesSpawned < maxEnemiesSpawned;
        return dontSpawnOnScreen ? !CheckIfOnScreen() && underMaxEnemies : underMaxEnemies;
    }

    private bool CheckIfOnScreen()
    {
        if (Camera.main == null)
        {
            return false;
        }
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}