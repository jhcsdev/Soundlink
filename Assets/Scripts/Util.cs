

using UnityEngine;

public static class Util
{
    public static Vector2 Vec2IntToVec2(Vector2Int v) => new(v.x, v.y);
    public static Vector2Int Vec2ToVec2Int(Vector2 v) => new((int)v.x, (int)v.y);
}