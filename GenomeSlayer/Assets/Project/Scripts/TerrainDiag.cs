using UnityEngine;

public class TerrainDiag : MonoBehaviour
{
    public Terrain[] ter;
    void Start()
    {
        var terrains = ter;
        for (int idx = 0; idx < terrains.Length; idx++)
        {
            var t = terrains[idx];
            var td = t.terrainData;
            Debug.Log($"[{idx}] name={t.name}, pos={t.transform.position}, " +
                      $"scale={t.transform.localScale}, " +
                      $"size={td.size} (Width={td.size.x}, Height={td.size.y}, Length={td.size.z}), " +
                      $"heightmapRes={td.heightmapResolution}, alphamapRes={td.alphamapResolution}, " +
                      $"baseMapRes={td.baseMapResolution}");
        }
    }
}