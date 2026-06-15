using UnityEngine;

namespace CabinBoy.UI;


public class GameBoyScreenRenderer
{
    private readonly int       _w;
    private readonly int       _h;
    private readonly Color32[] _buffer;
    private readonly Color32[] _texPixels;
    private readonly Texture2D _texture;

    public GameBoyScreenRenderer(int width, int height)
    {
        _w         = width;
        _h         = height;
        _buffer    = new Color32[width * height];
        _texPixels = new Color32[width * height];

        _texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode   = TextureWrapMode.Clamp;
    }

    public void Clear(Color32 color)
    {
        for (int i = 0; i < _buffer.Length; i++)
            _buffer[i] = color;
    }

    public void SetPixel(int x, int y, Color32 color)
    {
        if ((uint)x >= (uint)_w || (uint)y >= (uint)_h)
            return;

        _buffer[y * _w + x] = color;
    }

    public void FillRect(int x, int y, int w, int h, Color32 color)
    {
        int x1 = Mathf.Max(x, 0);
        int y1 = Mathf.Max(y, 0);
        int x2 = Mathf.Min(x + w, _w);
        int y2 = Mathf.Min(y + h, _h);

        for (int py = y1; py < y2; py++)
        {
            int row = py * _w;
            for (int px = x1; px < x2; px++)
                _buffer[row + px] = color;
        }
    }

    public void DrawHLine(int x, int y, int length, Color32 color)
    {
        if ((uint)y >= (uint)_h)
            return;

        int x2  = Mathf.Min(x + length, _w);
        int row = y * _w;

        for (int px = Mathf.Max(x, 0); px < x2; px++)
            _buffer[row + px] = color;
    }

    public void DrawVLine(int x, int y, int length, Color32 color)
    {
        if ((uint)x >= (uint)_w)
            return;

        int y2 = Mathf.Min(y + length, _h);

        for (int py = Mathf.Max(y, 0); py < y2; py++)
            _buffer[py * _w + x] = color;
    }


    public void DrawText(int x, int y, string text, Color32 color)
    {
        BitmapFont.DrawString(_buffer, _w, _h, x, y, text, color);
    }


    public void DrawTextCentered(int y, string text, Color32 color)
    {
        if (string.IsNullOrEmpty(text))
            return;

        int textW = BitmapFont.MeasureWidth(text);
        int x     = (_w - textW) / 2;
        DrawText(x, y, text, color);
    }


    public Texture2D GetTexture()
    {
        for (int y = 0; y < _h; y++)
        {
            int srcRow = y        * _w;
            int dstRow = (_h-1-y) * _w;

            for (int x = 0; x < _w; x++)
            {
                int flippedX = _w - 1 - x;
                _texPixels[dstRow + flippedX] = _buffer[srcRow + x];
            }
        }

        _texture.SetPixels32(_texPixels);
        _texture.Apply(false);
        return _texture;
    }
}
