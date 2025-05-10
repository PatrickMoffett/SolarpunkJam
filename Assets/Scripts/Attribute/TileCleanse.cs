using Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCleanse : MonoBehaviour
{
    [SerializeField] private Tile _grassTile;
    [SerializeField] private Tile _dirtTile;
    [SerializeField] private GameEvent _tileCleanseEvent;
    private Tilemap _tilemap;
    
    private void Start()
    {
        //_tileCleanseEvent.OnGameEvent += tileCleanse;
        //tileCleanse(transform, new Vector2Int(4, 1));
    }
    //Vector2 range, Transform loc
    public void tileCleanse(Vector2Int range)
    {
        //Vector2Int range = new Vector2Int(4, 1);
        Transform loc = gameObject.transform;
        int count = 0;
        GameObject tilemapObject = GameObject.Find("Tilemap");
        if (tilemapObject != null)
        {
            _tilemap = tilemapObject.GetComponent<Tilemap>();
            if(_tilemap == null)
            {
                Debug.LogError("Tilemap component not found on Tilemap object.");
                return;
            }
        }
        else
        {
            Debug.LogError("Tilemap object not found in the scene.");
        }
        Vector2Int position = ServiceLocator.Instance.Get<TileMapManager>().GetTileCoordAtWorldPosition(loc.position);
        Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);
        StartCoroutine(cleanseSpread(position, tilePosition, range));
    }
    //coroutine that waits for a delay time, then sets the tile to grass based on the distance

    IEnumerator cleanseSpread(Vector2Int position, Vector3Int tilePosition, Vector2Int range)
    {
        yield return new WaitForSeconds(.25f);
        float delay = 0.5f;
        float delayScalar = 2f;
        int count = 0; 
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<(Vector3Int,int)> queue = new Queue<(Vector3Int cell, int distance)>();
        queue.Enqueue((tilePosition,0));
        visited.Add(tilePosition);
        int max = range.x > range.y ? range.x : range.y;
        int lastDistanceProcessed = 0;
        int maxX = position.x + range.x;
        int minX = position.x - range.x;
        int maxY = position.y;
        int minY = position.y - range.y;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if(lastDistanceProcessed != current.Item2)
            {
                yield return new WaitForSeconds(delay);
                delay *= delayScalar;
                lastDistanceProcessed = current.Item2;
            }
            Tile tile = _tilemap.GetTile<Tile>(current.Item1);
            if (tile != null && tile != _grassTile)
            {
                if (current.Item1.y == position.y)
                {
                    _tilemap.SetTile(current.Item1, _grassTile);
                }
                else
                {
                    _tilemap.SetTile(current.Item1, _dirtTile);
                }
            }
            if (current.Item2 < max)
            {
                Vector3Int[] Neighbors = new Vector3Int[3];
                Neighbors[0] = new Vector3Int(current.Item1.x - 1, current.Item1.y, 0);
                Neighbors[1] = new Vector3Int(current.Item1.x + 1, current.Item1.y, 0);
                Neighbors[2] = new Vector3Int(current.Item1.x, current.Item1.y - 1, 0);

                for (int i = 0; i < Neighbors.Length; i++)
                {
                    if (Neighbors[i].x > maxX || Neighbors[i].x < minX || Neighbors[i].y > maxY || Neighbors[i].y < minY)
                    {
                        continue;
                    }
                    if (!visited.Contains(Neighbors[i]) && _tilemap.GetTile<Tile>(Neighbors[i]) != null)
                    {
                        visited.Add(Neighbors[i]);
                        queue.Enqueue((Neighbors[i], current.Item2 + 1));
                    }
                }
            }
        }
    }
}


