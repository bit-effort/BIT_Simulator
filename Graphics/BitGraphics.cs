using BIT_Simulator.SimLog;
using MoonSharp.Interpreter;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BIT_Simulator.Graphics
{
    [MoonSharpUserData]
    internal class BitGraphics
    {
        private Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

        public void draw_rect(int x, int y, int width, int height, uint color)
        {
            Color c = GetColor(color);
            Raylib.DrawRectangle(x, y, width, height, c);
        }

        public void draw_rect_round(int x, int y, int width, int height, int roundness, uint color)
        {
            Color c = GetColor(color);
            Raylib.DrawRectangleRounded(new Rectangle(x, y, width, height), roundness / 100.0f, 32, c);
        }

        public void draw_rect_outline(int x, int y, int width, int height, int thickness, int roundness, uint color)
        {
            Color c = GetColor(color);
            Raylib.DrawRectangleRoundedLinesEx(new Rectangle(x, y, width, height), roundness / 100.0f, 32, thickness, c);
        }

        public void draw_text(string text, int x, int y, int fontSize, string fontFamily, uint color)
        {
            Color c = GetColor(color);
            Raylib.DrawTextEx(BitFont.GetFont(fontFamily), text, new Vector2(x, y), fontSize, 1, c);
        }

        public void load_image(string imagePath)
        {
            if (!_textureCache.ContainsKey(imagePath))
            {
                if (File.Exists(imagePath))
                {
                    _textureCache[imagePath] = Raylib.LoadTexture(imagePath);
                    SIMLOG.Info($"Image loaded: {imagePath}");
                }
                else
                {
                    SIMLOG.Warning($"Image not found: {imagePath}");
                }
            }
        }
        public void unload_image(string imagePath)
        {
            if (_textureCache.ContainsKey(imagePath))
            {
                Raylib.UnloadTexture(_textureCache[imagePath]);
                _textureCache.Remove(imagePath);
            }
        }

        public void draw_image(string imagePath, int x, int y)
        {
            // Check if it's already cached
            if (!_textureCache.ContainsKey(imagePath))
            {
                load_image(imagePath);
            }

            if (_textureCache.TryGetValue(imagePath, out Texture2D tex))
            {
                Raylib.DrawTexture(tex, x, y, Color.White);
            }
        }

        public int get_screen_width()
        {
            return Raylib.GetScreenWidth();
        }
        public int get_screen_height()
        {
            return Raylib.GetScreenHeight();
        }

        public float get_tick()
        {
            return (float)Raylib.GetFrameTime();
        }

        Color GetColor(uint color)
        {
            return new Color((int)((color >> 16) & 0xFF),
                             (int)((color >> 8) & 0xFF),
                             (int)(color & 0xFF), 255);
        }
    }
}
