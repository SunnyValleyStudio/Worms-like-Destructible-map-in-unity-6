using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.U2D;

public class DestructibleTerrain : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer m_spriteRenderer;
    private ModifiableTexture m_modifiableTexture;
    
    [SerializeField]
    private TilemapColliderGenerator m_colliderGenerator;

    private TilemapColliderGenerator m_nonChunkedCollder;

    [SerializeField]
    private Grid m_grid;

    [SerializeField]
    private ChunkManager m_chunkManager;

    [SerializeField]
    private Vector2Int m_chunkSize = new(300, 300);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_modifiableTexture = ModifiableTexture.CreateFromSprite(m_spriteRenderer.sprite);
        m_spriteRenderer.sprite = m_modifiableTexture.Sprite;
        float pixelSize = 1 / m_modifiableTexture.Sprite.pixelsPerUnit;
        m_grid.cellSize = new Vector2(pixelSize, pixelSize);
        
        //Vector2 size = m_modifiableTexture.Sprite.bounds.size;
        Vector2 bottomLeftPixel = -m_modifiableTexture.Sprite.pivot;
        
        Vector2Int chunkGridSize = SplitTextureIntoChunks(
            m_modifiableTexture.Texture.width,
            m_modifiableTexture.Texture.height, m_chunkSize);

        bool[][] pixels = m_modifiableTexture.GetPixelsState();

        PrepareColliderChunks(chunkGridSize,
            m_chunkSize,
            bottomLeftPixel,
            pixels);

        // Vector2 bottomLeftWorld = m_spriteRenderer.transform.TransformPoint(bottomLeftLocal);
        //
        // m_nonChunkedCollder = Instantiate(m_colliderGenerator
        //     , bottomLeftWorld
        //     , Quaternion.identity, 
        //     m_grid.transform);
        // m_nonChunkedCollder.PrepareCollider(m_modifiableTexture.GetPixelsState());
    }

    private void PrepareColliderChunks(Vector2Int chunkGridSize, 
        Vector2Int mChunkSize, Vector2 bottomLeftLocal, bool[][] pixels)
    {
        for (int x = 0; x < chunkGridSize.x; x++)
        {
            for (int y = 0; y < chunkGridSize.y; y++)
            {
                Vector3Int offset = new Vector3Int(x*mChunkSize.x, y*mChunkSize.y, 0);
                Vector3 bottomLeftCorner = (Vector3)bottomLeftLocal + offset;
                bottomLeftCorner.Scale(m_grid.cellSize);
                bottomLeftCorner = m_spriteRenderer.transform.TransformPoint(bottomLeftCorner);
                
                TilemapColliderGenerator colliderGenerator = 
                    Instantiate(m_colliderGenerator, bottomLeftCorner
                        , Quaternion.identity, m_grid.transform);
                colliderGenerator.gameObject.name = $"Chunk_{x}_{y}";
                m_chunkManager.AddChunk(colliderGenerator);
                bool[][] chunkPixels = SliceArray(pixels, offset.y, offset.x, mChunkSize.y, mChunkSize.x);
                colliderGenerator.PrepareCollider(chunkPixels);
            }
        }
    }

    private bool[][] SliceArray(bool[][] pixels, int startRow, int startCol, int numRows, int numCols)
    {
        int sourceWidth = pixels.Length;
        int sourceHeight = pixels[0].Length;

        int actualWidth = Mathf.Min(numRows, sourceWidth - startRow);
        int actualHeight = Mathf.Min(numCols, sourceHeight - startCol);
        
        actualWidth = Mathf.Max(0, actualWidth);
        actualHeight = Mathf.Max(0, actualHeight);

        bool[][] result = new bool[actualWidth][];
        for (int row = 0; row < actualWidth; row++)
        {
            result[row] = new bool[actualHeight];
            for (int col = 0; col < actualHeight; col++)
            {
                result[row][col] = pixels[startRow + row][startCol + col];
            }
        }
        return result;
    }

    private Vector2Int SplitTextureIntoChunks(int width, int height, Vector2Int mChunkSize)
    {
        int chunkCountRight = Mathf.CeilToInt((float)width / mChunkSize.x);
        int chunkCountUp = Mathf.CeilToInt((float)height / mChunkSize.y);
        return new(chunkCountRight, chunkCountUp);
    }

    public void RemoveTerrainAt(Vector2 worldPosition, float radius)
    {
        float pixelSize = 1 / m_modifiableTexture.Sprite.pixelsPerUnit;
        int radiusInPixel = Mathf.RoundToInt(radius/pixelSize);
        List<Vector2Int> affectedPixelAsOffset = GetCircleOffsets(radiusInPixel);
        
        Vector2Int circleCenterInPixelSpace 
            = m_modifiableTexture.WorldToTexturePosition(worldPosition, m_spriteRenderer.transform);
        ModifyTextureAt(circleCenterInPixelSpace, Color.clear, affectedPixelAsOffset);
        //m_nonChunkedCollder.DestroyCollider(worldPosition,affectedPixelAsOffset );
        List<TilemapColliderGenerator> chunksToModify = m_chunkManager.GetClosestChunks(worldPosition);
        foreach (var chunk in chunksToModify)
        {
            chunk.DestroyCollider(worldPosition,affectedPixelAsOffset);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if (m_chunkManager != null)
            {
                Vector3 chunkSize = new(m_chunkSize.x * m_grid.cellSize.x,
                    m_chunkSize.y * m_grid.cellSize.y);
                Vector3 halfSize = chunkSize / 2f;
                m_chunkManager.DrawGizmos(chunkSize,halfSize);
            }
        }
    }

    private void ModifyTextureAt(Vector2Int circleCenterInPixelSpace, Color color, List<Vector2Int> affectedPixelAsOffset)
    {
        foreach (Vector2Int offset in affectedPixelAsOffset)
        {
            Vector2Int pos = circleCenterInPixelSpace + offset;
            m_modifiableTexture.SetPixel(pos, color);
        }

        m_modifiableTexture.ApplyChanges();
    }

    private List<Vector2Int> GetCircleOffsets(int radiusInPixel)
    {
        List<Vector2Int> affectedPixelAsOffset = new List<Vector2Int>();
        for (int x = -radiusInPixel; x <= radiusInPixel; x++)
        {
            for (int y = -radiusInPixel; y <= radiusInPixel; y++)
            {
                if (x * x + y * y <= radiusInPixel * radiusInPixel)
                {
                    affectedPixelAsOffset.Add(new Vector2Int(x, y));
                }
            }
        }
        return affectedPixelAsOffset;
    }
}
