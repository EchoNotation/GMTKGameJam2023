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

    public struct Mark {
        public Vector3Int position;
        public float time;
        public float validForTime;
    }

    public float markMemoryTime = 10f;
    public float mapUpdateTime = 0.1f;
    public int deathMarkCost = 5;

    List<Mark> marks;

    HashSet<Vector3Int> traps;

    // Start is called before the first frame update
    void Start()
    {
        tileData = new Dictionary<TileBase, TileProperties>();
        foreach(var m in tileMapper) {
            tileData[m.tile] = m.tileProperties;
        }

        flowfield = new int[GetWidth(), GetHeight()];
        Debug.Log(GetHeight() + " x " + GetWidth());

        marks = new List<Mark>();

        traps = new HashSet<Vector3Int>();
        GameObject[] whirlies = GameObject.FindGameObjectsWithTag("Whirly");
        foreach(GameObject whirly in whirlies) {
            traps.Add(tilemap.WorldToCell(whirly.transform.position));
        }

        StartCoroutine(MapUpdate());
    }

    IEnumerator MapUpdate() {
        while(true) {
            GenerateFlowField();
            ProcessMarks();
            yield return new WaitForSeconds(mapUpdateTime);
        }
    }

    public void RegisterDeath(Vector3 position) {
        Vector3Int tilePosition = tilemap.WorldToCell(position);
        Mark mark = new Mark {
            position = tilePosition,
            time = Time.time,
            validForTime = markMemoryTime
        };
        marks.Add(mark);
    }

    private bool IsMarkOld(Mark mark) {
        return mark.time + mark.validForTime < Time.time;
    }

    void ProcessMarks() {
        marks.RemoveAll(IsMarkOld);
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

        if(traps.Contains(tilePosition)) {
            return false;
        }

        var tileProperties = GetTileData(tilePosition);
        if(tileProperties) {
            return tileProperties.isPassable;
        }
        else return false;
    }

    public List<Vector3Int> GetPassableNeighbors(Vector3Int pos) {
        var res = new List<Vector3Int>();

        Vector3Int rightPos = pos + new Vector3Int(1, 0);
        Vector3Int leftPos = pos + new Vector3Int(-1, 0);
        Vector3Int upPos = pos + new Vector3Int(0, 1);
        Vector3Int downPos = pos + new Vector3Int(0, -1);

        Vector3Int rightUpPos = pos + new Vector3Int(1, 1);
        Vector3Int rightDownPos = pos + new Vector3Int(1, -1);
        Vector3Int leftUpPos = pos + new Vector3Int(-1, 1);
        Vector3Int leftDownPos = pos + new Vector3Int(-1, -1);

        if(GetPassable(rightPos))
            res.Add(rightPos);
        if(GetPassable(leftPos))
            res.Add(leftPos);
        if(GetPassable(upPos))
            res.Add(upPos);
        if(GetPassable(downPos))
            res.Add(downPos);

        // move diagnonals
        if(GetPassable(rightUpPos) && GetPassable(rightPos) && GetPassable(upPos))
            res.Add(rightUpPos);
        if(GetPassable(rightDownPos) && GetPassable(rightPos) && GetPassable(downPos))
            res.Add(rightDownPos);
        if(GetPassable(leftUpPos) && GetPassable(leftPos) && GetPassable(upPos))
            res.Add(leftUpPos);
        if(GetPassable(leftDownPos) && GetPassable(leftPos) && GetPassable(downPos))
            res.Add(leftDownPos);

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

        int[,] playerFlowField = new int[GetWidth(), GetHeight()];

        Player player = GameObject.FindObjectOfType<Player>();
        if(!player || player.isDead) {
            flowfield = playerFlowField;
            return;
        }

        Transform playerTransform = player.transform;

        Vector3Int playerPos = tilemap.WorldToCell(playerTransform.position);
        
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        
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

            foreach(Vector3Int neighbor in GetPassableNeighbors(current)) {
                int neighborX = neighbor.x;
                int neighborY = neighbor.y;
                if(playerFlowField[neighborX, neighborY] == -1) {
                    frontier.Enqueue(neighbor);
                    playerFlowField[neighborX, neighborY] = playerFlowField[currentX, currentY] + 1;
                }
            }
        }

        playerFlowField[playerPos.x, playerPos.y] += 1;

        // 2. Avoid other goons

        
        GameObject[] goons = GameObject.FindGameObjectsWithTag("Goon");
        
        foreach(GameObject goon in goons) {
            Vector3Int goonPosition = tilemap.WorldToCell(goon.transform.position);

            //   X 
            // X . X
            //   X
            if(GetPassable(goonPosition))
                playerFlowField[goonPosition.x, goonPosition.y] += 1;
            //if(GetPassable(newPos = goonPosition + new Vector3Int(0, 1)))
            //    playerFlowField[newPos.x, newPos.y] += 1;
            //if(GetPassable(newPos = goonPosition + new Vector3Int(0, -1)))
            //    playerFlowField[newPos.x, newPos.y] += 1;
            //if(GetPassable(newPos = goonPosition + new Vector3Int(1, 0)))
            //    playerFlowField[newPos.x, newPos.y] += 1;
            //if(GetPassable(newPos = goonPosition + new Vector3Int(-1, 0)))
            //    playerFlowField[newPos.x, newPos.y] += 1;
        }
        
        // 3. avoid marked locations

        foreach(Mark mark in marks) {
            playerFlowField[mark.position.x, mark.position.y] += deathMarkCost;
        }

        // update flow field
        flowfield = playerFlowField;
    }

    #if UNITY_EDITOR
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
    #endif
}
