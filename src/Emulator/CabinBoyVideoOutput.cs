using UnityEngine;

using CabinBoy.Core;

namespace CabinBoy.Emulator;


public class CabinBoyVideoOutput : UnityGB.IVideoOutput
{
    private Texture2D _texture;
    private Color32[] _pixels;
    private int       _width;
    private int       _height;

    public Texture2D GetTexture() => _texture;


    public void SetSize(int w, int h)
    {
        if (_texture != null && _width == w && _height == h)
            return;

        _width  = w;
        _height = h;

        _texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode   = TextureWrapMode.Clamp;

        _pixels = new Color32[w * h];

        ModLogger.Msg($"CabinBoyVideoOutput sized to {w}×{h}.");
    }


    public void SetPixels(uint[] colors)
    {
        if (_texture == null || _pixels == null)
            return;

        for (int y = 0; y < _height; y++)
        {
            int flippedY = _height - 1 - y;

            for (int x = 0; x < _width; x++)
            {
                uint src = colors[y * _width + x];

                _pixels[flippedY * _width + x] = new Color32(
                    r: (byte)(src >> 16),
                    g: (byte)(src >>  8),
                    b: (byte)(src      ),
                    a: (byte)(src >> 24));
            }
        }

        _texture.SetPixels32(_pixels);
        _texture.Apply(false);
    }
}
