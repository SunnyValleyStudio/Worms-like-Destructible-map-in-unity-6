using UnityEngine;

public class ModifiableTexture
{
    private Texture2D m_texture;
    private Sprite m_sprite;
    private Vector2 m_pivot;
    private float m_pixelsPerUnit;
    public Texture2D Texture => m_texture;
    public Sprite Sprite => m_sprite;

    public static ModifiableTexture CreateFromSprite(Sprite sprite)
    {
        Rect rect = sprite.rect;
        Texture2D texture = new((int)rect.width,(int)rect.height
        ,TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color32[] pixels = sprite.texture.GetPixels32();
        texture.SetPixels32(pixels);
        texture.Apply();

        Vector2 normalizaedPivot = new(
            sprite.pivot.x / rect.width,
            sprite.pivot.y / rect.height);
        
        return new ModifiableTexture(texture, normalizaedPivot, sprite.pixelsPerUnit);
    }
    
    private ModifiableTexture(Texture2D texture, Vector2 pivot, float pixelsPerUnit)
    {
        this.m_texture = texture;
        this.m_pivot = pivot;
        this.m_pixelsPerUnit = pixelsPerUnit;
        RecreateSprite();
    }

    private void RecreateSprite()
    {
        m_sprite = Sprite.Create(
            m_texture, 
            new Rect(0, 0, m_texture.width, m_texture.height), 
            m_pivot,
            m_pixelsPerUnit,
            0,
            SpriteMeshType.FullRect,
            Vector4.zero,
            false);
    }

    public Vector2Int WorldToTexturePosition(Vector2 worldPosition, Transform transform)
    {
        Vector2 localPosition = transform.InverseTransformPoint(worldPosition);
        int x = Mathf.FloorToInt(localPosition.x * m_pixelsPerUnit + m_sprite.pivot.x);
        int y = Mathf.FloorToInt(localPosition.y * m_pixelsPerUnit + m_sprite.pivot.y);
        return new Vector2Int(x, y);
    }

    public bool IsValidTexturePosition(Vector2Int texturePosition)
    {
        return texturePosition.x >= 0 && texturePosition.y >= 0 
                                      && texturePosition.x < m_texture.width
                                      &&  texturePosition.y < m_texture.height;
    }

    public bool SetPixel(Vector2Int texturePosition, Color color)
    {
        if (!IsValidTexturePosition(texturePosition))
            return false;
        m_texture.SetPixel(texturePosition.x, texturePosition.y, color);
        return true;
    }

    public void ApplyChanges()
    {
        m_texture.Apply();
    }
}


