﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace WinterRose.Monogame.Imgui.Data;

/// <summary>
///     Contains the GUIRenderer's texture data element.
/// </summary>
public class TextureData
{
    public IntPtr? FontTextureID;
    public Dictionary<IntPtr, Texture2D> Loaded;
    public int TextureID;

    public TextureData()
    {
        Loaded = new Dictionary<IntPtr, Texture2D>();
    }

    public int GetNextTextureId()
    {
        return TextureID++;
    }
}