using Services;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCleanse : MonoBehaviour
{
    [SerializeField] private Tile _grassTile;
    [SerializeField] private Tile _dirtTile;
    private Tilemap _tilemap;
    public void tileCleanse(Transform loc)
    {
        int count = 0;
        GameObject tilemapObject = GameObject.Find("Tilemap");
        if (tilemapObject)
        {
            _tilemap = tilemapObject.GetComponent<Tilemap>();
        }
        else
        {
            Debug.LogError("Tilemap object not found in the scene.");
        }
        Vector2Int position = ServiceLocator.Instance.Get<TileMapManager>().GetTileCoordAtWorldPosition(loc.position);
        Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);
        _tilemap.SetTile(tilePosition, _grassTile);
        for (int x = -2; x <= 2; x++)
        {
            if (x == 0) continue;
            Vector3Int neighborPosition = new Vector3Int(position.x + x, position.y, 0);
            Tile tile = _tilemap.GetTile<Tile>(neighborPosition);
            if (tile != null)
            {
                _tilemap.SetTile(neighborPosition, _grassTile);
                count++;
            }
        }
    }
}


