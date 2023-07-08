using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;


public class MapManager : MonoBehaviour
{
    public float[,] flowfield;

    public Tilemap tilemap;

    Dictionary<TileBase, TileProperties> tileData;

    public List<TileMapper> tileMapper;

    [System.Serializable]
    public struct TileMapper {
        public TileBase tile;
        public TileProperties tileProperties;
    }

    // Start is called before the first frame update
    void Start()
    {
        tileData = new Dictionary<TileBase, TileProperties>();
        foreach(var m in tileMapper) {
            tileData[m.tile] = m.tileProperties;
        }

        flowfield = new float[GetWidth(), GetHeight()];
        Debug.Log(GetHeight() + " x " + GetWidth());

        StartCoroutine(MapUpdate());
    }

    IEnumerator MapUpdate() {
        while(true) {
            GenerateFlowField();
            yield return new WaitForSeconds(.1f);
        }
    }

    public int GetWidth() {
        return tilemap.cellBounds.xMax;
    }

    public int GetHeight() {
        return tilemap.cellBounds.yMax;
    }

    public TileProperties GetTileData(Vector3Int tilePosition) {
        TileBase tile = tilemap.GetTile(tilePosition);

        if(tile == null)
            return null;
        else
            return tileData[tile];
    }

    public bool GetPassable(Vector3Int tilePosition) {
        if(tilePosition.y < 0 || tilePosition.y >= GetHeight())
            return false;
        else if(tilePosition.x < 0 || tilePosition.x >= GetWidth())
            return false;

        var tileProperties = GetTileData(tilePosition);
        if(tileProperties) {
            return tileProperties.isPassable;
        }
        else return false;
    }

    public List<Vector3Int> GetPassableNeighbors(Vector3Int pos) {
        var res = new List<Vector3Int>();

        var newPos = new Vector3Int();
        // walrus := ?
        if(GetPassable(newPos = pos + new Vector3Int(0, 1)))
            res.Add(newPos);
        if(GetPassable(newPos = pos + new Vector3Int(1, 0)))
            res.Add(newPos);
        if(GetPassable(newPos = pos + new Vector3Int(-1, 0)))
            res.Add(newPos);
        if(GetPassable(newPos = pos + new Vector3Int(0, -1)))
            res.Add(newPos);

        return res;
    }

    public Vector3 GetDownFlowField(Vector3 position) {
        Vector3Int  tilePosition = tilemap.WorldToCell(position);

        List<Vector3Int> best = new List<Vector3Int>();
        float bestScore = 100f;
        foreach(Vector3Int neighbor in GetPassableNeighbors(tilePosition)) {
            if(flowfield[neighbor.x, neighbor.y] < bestScore) {
                bestScore = flowfield[neighbor.x, neighbor.y];
                best.Clear();
                best.Add(neighbor);
            }
            else if(flowfield[neighbor.x, neighbor.y] == bestScore) {
                best.Add(neighbor);
            }
        }

        int idx = Random.Range(0, best.Count);

        return best[idx] + new Vector3(0.5f, 0.5f, 0);
    }

    public void GenerateFlowField() {

        // 1. Move towards player

        Vector3Int playerPos = new Vector3Int(1, 1, 0);
        
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        float[,] playerFlowField = new float[GetWidth(), GetHeight()];
        for(int i = 0; i < playerFlowField.GetLength(0); i++) {
            for(int j = 0; j < playerFlowField.GetLength(1); j++) {
                playerFlowField[i, j] = -1f;
            }
        }

        frontier.Enqueue(playerPos);
        playerFlowField[playerPos.x, playerPos.y] = 0f;

        while(frontier.Count > 0) {
            Vector3Int current = frontier.Dequeue();
            int currentX = current.x;
            int currentY = current.y;

            foreach(Vector3Int neighbor in GetPassableNeighbors(current)) {
                int neighborX = neighbor.x;
                int neighborY = neighbor.y;
                if(playerFlowField[neighborX, neighborY] == -1) {
                    frontier.Enqueue(neighbor);
                    playerFlowField[neighborX, neighborY] = playerFlowField[currentX, currentY] + 0.5f;
                }
            }
        }

        // 2. Avoid other goons

        
        GameObject[] goons = GameObject.FindGameObjectsWithTag("Goon");
        
        foreach(GameObject goon in goons) {
            Vector3Int goonPosition = tilemap.WorldToCell(goon.transform.position);

            var newPos = new Vector3Int();

            //   X 
            // X . X
            //   X
            if(GetPassable(newPos = goonPosition + new Vector3Int(0, 1)))
                playerFlowField[newPos.x, newPos.y] += 0.7f;
            if(GetPassable(newPos = goonPosition + new Vector3Int(0, -1)))
                playerFlowField[newPos.x, newPos.y] += 0.7f;
            if(GetPassable(newPos = goonPosition + new Vector3Int(1, 0)))
                playerFlowField[newPos.x, newPos.y] += 0.7f;
            if(GetPassable(newPos = goonPosition + new Vector3Int(-1, 0)))
                playerFlowField[newPos.x, newPos.y] += 0.7f;
        }
        

        // update flow field
        flowfield = playerFlowField;
    }

    private void OnDrawGizmos() {

        if(!Application.IsPlaying(gameObject))
            return;

        UnityEditor.Handles.BeginGUI();
        for(int i = 0; i < flowfield.GetLength(0); i++) {
            for(int j = 0; j < flowfield.GetLength(1); j++) {
                // Gizmos.DrawWireCube(new Vector3(i, j, 0), 0.2f*Vector3.one);
                UnityEditor.Handles.Label(new Vector3(i, j, 0) + 0.5f*Vector3.one, flowfield[i, j].ToString());
            }
        }
        Gizmos.DrawWireCube(new Vector3(1.5f, 1.5f), Vector3.one);

        UnityEditor.Handles.EndGUI();
    }

}
