using UnityEditor;
using UnityEngine;
using GamePieces;
using System.Collections.Generic;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

[CustomEditor(typeof(PieceData), true)]
[CanEditMultipleObjects]
public class PieceDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open in Piece Editor", GUILayout.Height(28)))
            PieceEditorWindow.OpenWithPiece((PieceData)target);

        EditorGUILayout.Space(6);

        DrawGridPreview();

        EditorGUILayout.Space(6);

        DrawDefaultInspector();
    }

    private void DrawGridPreview()
    {
        const float previewCellSize  = 16f;
        const float glueThickness    = 2f;
        float       previewSize      = PieceEditorWindow.GridSize * previewCellSize;

        Rect gridArea = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false));
        gridArea.x = (EditorGUIUtility.currentViewWidth - previewSize) * 0.5f;

        // Build lookup: relative offset → PieceTileData
        Dictionary<Vector2Int, PieceTileData> lookup = new();
        foreach (PieceTileData tile in ((PieceData)target).tiles)
            lookup[tile.relativeOffset] = tile;

        // Border + background
        EditorGUI.DrawRect(
            new Rect(gridArea.x - 2, gridArea.y - 2, gridArea.width + 4, gridArea.height + 4),
            new Color(0.08f, 0.08f, 0.08f));
        EditorGUI.DrawRect(gridArea, PieceEditorWindow.ColBackground);

        Vector2Int center = new(PieceEditorWindow.GridSize / 2, PieceEditorWindow.GridSize / 2);

        for (int x = 0; x < PieceEditorWindow.GridSize; x++)
        {
            for (int y = 0; y < PieceEditorWindow.GridSize; y++)
            {
                Rect cellRect = new(
                    gridArea.x + x * previewCellSize,
                    gridArea.y + (PieceEditorWindow.GridSize - 1 - y) * previewCellSize,
                    previewCellSize - 1,
                    previewCellSize - 1);

                Vector2Int adjustedCoord = new Vector2Int(x, y) - center;
                bool active = lookup.TryGetValue(adjustedCoord, out PieceTileData data);

                Color fill = active
                    ? data.type switch
                      {
                          PieceTileType.PASSTHROUGH => PieceEditorWindow.PassthroughCell,
                          PieceTileType.SWITCH      => PieceEditorWindow.SwitchCell,
                          _                         => PieceEditorWindow.NormalCell
                      }
                    : PieceEditorWindow.ColCellEmpty;

                EditorGUI.DrawRect(cellRect, fill);
                EditorGUI.DrawRect(new Rect(cellRect.xMax, cellRect.y,    1,              cellRect.height), PieceEditorWindow.ColGridLine);
                EditorGUI.DrawRect(new Rect(cellRect.x,    cellRect.yMax, cellRect.width, 1),               PieceEditorWindow.ColGridLine);

                // Glue edge bars
                if (!active || data.glue == null) continue;
                foreach (GlueCardinality g in data.glue)
                {
                    Rect edge = g switch
                    {
                        GlueCardinality.NORTH => new Rect(cellRect.x,                      cellRect.y,                      cellRect.width, glueThickness),
                        GlueCardinality.SOUTH => new Rect(cellRect.x,                      cellRect.yMax - glueThickness,   cellRect.width, glueThickness),
                        GlueCardinality.WEST  => new Rect(cellRect.x,                      cellRect.y,                      glueThickness,  cellRect.height),
                        GlueCardinality.EAST  => new Rect(cellRect.xMax - glueThickness,   cellRect.y,                      glueThickness,  cellRect.height),
                        _                    => default
                    };
                    EditorGUI.DrawRect(edge, PieceEditorWindow.ColGlueEdge);
                }
            }
        }
    }

    private PieceData item { get { return target as PieceData; } }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        var icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Icons/basic_square_icon.png");
        if (icon == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);

        Type t = GetType("UnityEditor.SpriteUtility");
        if (t != null)
        {
            MethodInfo method = t.GetMethod("RenderStaticPreview", new Type[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
            if (method != null)
            {
                object ret = method.Invoke(null, new object[] { icon, Color.white, width, height }); // null = static method
                if (ret is Texture2D)
                    return ret as Texture2D;
            }
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }

    private static Type GetType(string TypeName)
    {
        var type = Type.GetType(TypeName);
        if(type!=null)
            return type;

        if(TypeName.Contains("."))
        {
            var assemblyName = TypeName.Substring(0,TypeName.IndexOf('.'));
            var assembly = Assembly.Load(assemblyName);
            if(assembly==null)
                return null;
            type=assembly.GetType(TypeName);
            if(type!=null)
                return type;
        }

        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach(var assemblyName in referencedAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            if(assembly!=null)
            {
                type=assembly.GetType(TypeName);
                if(type!=null)
                    return type;
            }
        }
        return null;
    }
}