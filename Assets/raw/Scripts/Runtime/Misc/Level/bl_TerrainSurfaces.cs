using MFPSEditor;
using System.Collections.Generic;
using UnityEngine;

public class bl_TerrainSurfaces : MonoBehaviour
{
    public List<TerrainSurface> terrainSurfaces = new();
    [Space]
    public TerrainLayer[] terrainLayers = new TerrainLayer[0];
    public Terrain terrain;

    public string GetSurfaceTag(Vector3 position)
    {
        int layerID = GetDominantTerrainLayer(position, terrain);
        return layerID >= terrainSurfaces.Count ? string.Empty : terrainSurfaces[layerID].Tag;
    }

    public int GetDominantTerrainLayer(Vector3 worldPos, Terrain terrain)
    {
        // Get the terrain data and position
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        // Calculate which splat map cell the worldPos falls within (ignoring y)
        int mapX = (int)((worldPos.x - terrainPos.x) / terrainData.size.x * terrainData.alphamapWidth);
        int mapZ = (int)((worldPos.z - terrainPos.z) / terrainData.size.z * terrainData.alphamapHeight);

        // Get the splat data for this cell as a 1x1xN 3D array (where N = number of textures)
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        // Determine the most dominant layer
        int dominantLayer = 0;
        float maxMix = 0f;

        for (int i = 0; i < splatmapData.GetLength(2); i++)
        {
            if (splatmapData[0, 0, i] > maxMix)
            {
                maxMix = splatmapData[0, 0, i];
                dominantLayer = i;
            }
        }

        return dominantLayer;
    }

    private void OnValidate()
    {
        if (!TryGetComponent(out terrain)) return;

        terrainLayers = terrain.terrainData.terrainLayers;
        for (int i = 0; i < terrainLayers.Length; i++)
        {
            if (!terrainSurfaces.Exists(x => x.terrainLayer == terrainLayers[i]))
            {
                terrainSurfaces.Add(new TerrainSurface()
                {
                    terrainLayer = terrainLayers[i],
                    MainTexture = terrainLayers[i].diffuseTexture,
                    Tag = "Generic",
                });
            }
        }
    }

    [System.Serializable]
    public class TerrainSurface
    {
        public string Tag;
        [SpritePreview] public Texture2D MainTexture;
        public TerrainLayer terrainLayer;
    }
}