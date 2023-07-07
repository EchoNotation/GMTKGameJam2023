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
        Vector3Int playerPos = new Vector3Int(0, 0, 0);
        
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        int[,] playerFlowField = new int[GetHeight(), GetWidth()];
        for(int i = 0; i < playerFlowField.Length; i++) {
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
        for(int i = 0; i < flowfield.Length; i++) {
            for(int j = 0; j < flowfield.GetLength(1); j++) {
                Gizmos.DrawCube(new Vector3(i, j, 0), Vector3.one);
            }
        }
        
    }

}
