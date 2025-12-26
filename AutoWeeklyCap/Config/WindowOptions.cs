using System;
using System.Numerics;

namespace AutoWeeklyCap.Config;

[Serializable]
public class WindowOptions
{
    public Vector2 Size;
    public Vector2 Position;
    public bool Pin = false;
}
