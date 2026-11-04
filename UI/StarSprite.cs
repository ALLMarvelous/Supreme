using UnityEngine;

namespace Supreme.UI;

internal static class StarSprite
{
    private const float DarkToneRed = 196f / 255f;
    private const float LightToneRed = 1f;

    private static Sprite? cached;

    internal static Sprite Recolor(Sprite source, Color light, Color dark)
    {
        if (cached != null) return cached;

        var pixels = ReadPixels(source.texture);
        var colors = pixels.GetPixels();
        for (var i = 0; i < colors.Length; i++)
        {
            var color = colors[i];
            var tone = Mathf.InverseLerp(DarkToneRed, LightToneRed, color.r);
            var mapped = Color.Lerp(dark, light, tone);
            mapped.a = color.a;
            colors[i] = mapped;
        }

        pixels.SetPixels(colors);
        pixels.Apply(false);
        pixels.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        var rect = source.rect;
        var pivot = new Vector2(source.pivot.x / rect.width, source.pivot.y / rect.height);
        cached = Sprite.Create(pixels, rect, pivot, source.pixelsPerUnit);
        cached.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        return cached;
    }

    private static Texture2D ReadPixels(Texture2D texture)
    {
        var target = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB);
        var previous = RenderTexture.active;
        try
        {
            Graphics.Blit(texture, target);
            RenderTexture.active = target;

            var readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false)
            {
                filterMode = texture.filterMode,
                wrapMode = TextureWrapMode.Clamp
            };
            readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            return readable;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
        }
    }
}