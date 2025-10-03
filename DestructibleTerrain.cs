using System.Collections.Generic;
using UnityEngine;

public class DestructibleTerrain : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer m_spriteRenderer;
    private ModifiableTexture m_modifiableTexture;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_modifiableTexture = ModifiableTexture.CreateFromSprite(m_spriteRenderer.sprite);
        m_spriteRenderer.sprite = m_modifiableTexture.Sprite;
    }

    public void RemoveTerrainAt(Vector2 worldPosition, float radius)
    {
        float pixelSize = 1 / m_modifiableTexture.Sprite.pixelsPerUnit;
        int radiusInPixel = Mathf.RoundToInt(radius/pixelSize);
        List<Vector2Int> affectedPixelAsOffset = GetCircleOffsets(radiusInPixel);
        
        Vector2Int circleCenterInPixelSpace 
            = m_modifiableTexture.WorldToTexturePosition(worldPosition, m_spriteRenderer.transform);
        ModifyTextureAt(circleCenterInPixelSpace, Color.clear, affectedPixelAsOffset);
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
