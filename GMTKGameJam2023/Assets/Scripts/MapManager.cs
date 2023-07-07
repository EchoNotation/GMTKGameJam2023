using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;


public class MapManager : MonoBehaviour
{
    public int[,] flowfield;

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

        flowfield = new int[GetHeight(), GetWidth()];
        Debug.Log(GetHeight() + " x " + GetWidth());

        GenerateFlowField();
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
        if(tilePosition.x < 0 || tilePosition.x < GetHeight())
            return false;
        else if(tilePosition.y < 0 || tilePosition.y < GetWidth())
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

    public void GenerateFlowField() {
        Vector3Int playerPos = new Vector3Int(0, 1, 0);
        
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        int[,] playerFlowField = new int[GetHeight(), GetWidth()];
        for(int i = 0; i < playerFlowField.GetLength(0); i++) {
            for(int j = 0; j < playerFlowField.GetLength(1); j++) {
                playerFlowField[i, j] = -1;
            }
        }

        frontier.Enqueue(playerPos);
        playerFlowField[playerPos.x, playerPos.y] = 0;

        while(frontier.Count > 0) {
            Vector3Int current = frontier.Dequeue();
            int currentX = current.x;
            int currentY = current.y;

            Debug.Log(current);

            foreach(Vector3Int neighbor in GetPassableNeighbors(current)) {
                int neighborX = neighbor.x;
                int neighborY = neighbor.y;
                if(playerFlowField[neighborX, neighborY] != -1) {
                    frontier.Enqueue(neighbor);
                    playerFlowField[neighborX, neighborY] = playerFlowField[currentX, currentY] + 1;
                }
            }
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
                UnityEditor.Handles.Label(new Vector3(j, i, 0) + 0.5f*Vector3.one, flowfield[i, j].ToString());
            }
        }

        UnityEditor.Handles.EndGUI();
    }

}
