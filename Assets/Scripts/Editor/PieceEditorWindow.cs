using System.Collections.Generic;
using System.Linq;
using GamePieces;
using UnityEditor;
using UnityEngine;

/// <summary>
/// initial implementation created by claude 4.6 sonnet
/// adjusted by me to make things a bit better 
/// </summary>
public class PieceEditorWindow : EditorWindow
{
    #region grid config
    public static int   GridSize    = 11;       // preferably, odd-sized
    private const float CellSize    = 44f;
    public static float GridPadding = 16f;
    #endregion

    #region visuals 
    public static readonly Color ColBackground = new(0.13f, 0.13f, 0.13f);
    public static readonly Color ColCellEmpty  = new(0.20f, 0.20f, 0.20f);
    public static readonly Color NormalCell = new(0.28f, 0.56f, 0.90f);
    public static readonly Color PassthroughCell = new(0.20f, 0.80f, 0.45f);
    public static readonly Color SwitchCell = new(0.8f, 0.5f, 0.3f);
    public static readonly Color ColHover      = new(1.00f, 1.00f, 1.00f, 0.07f);
    public static readonly Color ColGridLine   = new(0.10f, 0.10f, 0.10f);
    #endregion

    #region tracking state
    private Dictionary<Vector2Int, PieceTileType> activeCells = new();
    private Vector2Int originCell;
    private PieceData targetPiece;

    private bool dragAdding;
    private bool isDragging;
    #endregion

    #region open functions
    [MenuItem("Window/General/PieceEditor")]
    public static void Open()
    {
        var win = GetWindow<PieceEditorWindow>("Piece Editor");
        win.minSize = new Vector2(
            GridSize * CellSize + GridPadding * 2,
            GridSize * CellSize + GridPadding * 2 + 80);
    }

    public static void OpenWithPiece(PieceData piece)
    {
        Open();
        GetWindow<PieceEditorWindow>().LoadPiece(piece);
    }
    #endregion

    #region unity events (enable, selectionchange, gui render)
    private void OnEnable()
    {
        if (Selection.activeObject is PieceData p) LoadPiece(p);
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is PieceData p) { LoadPiece(p); Repaint(); }
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawGrid();
        DrawFooter();
    }
    #endregion

    #region data mutation + retrieval
    private void LoadPiece(PieceData piece)
    {
        SaveToPiece();

        targetPiece = piece;
        activeCells.Clear();

        Vector2Int center = new(GridSize / 2, GridSize / 2);
        foreach (var tile in piece.tiles)
            activeCells.Add(tile.relativeOffset + center, tile.type);

        RecalculateOrigin();
    }

    // todo - add saving type
    private void SaveToPiece()
    {
        if (activeCells.Count == 0 || targetPiece == null) return;

        SerializedObject so = new(targetPiece);
        SerializedProperty tilesProp = so.FindProperty("tiles");
        tilesProp.ClearArray();

        var sorted = activeCells.OrderBy(c => c.Key.x).ThenBy(c => c.Key.y).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            Vector2Int cell = sorted[i].Key;
            Vector2Int relOffset = cell - originCell;

            tilesProp.InsertArrayElementAtIndex(i);
            SerializedProperty tp = tilesProp.GetArrayElementAtIndex(i);
            tp.FindPropertyRelative("relativeOffset").vector2IntValue = relOffset;
            tp.FindPropertyRelative("isOrigin").boolValue = cell == originCell;
            tp.FindPropertyRelative("type").enumValueIndex = (int)sorted[i].Value;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetPiece);
        AssetDatabase.SaveAssets();

        Debug.Log($"[PieceEditor] Saved {activeCells.Count} tile(s) to '{targetPiece.name}'.");
    }
    #endregion

    #region centroid calc
    private void RecalculateOrigin()
    {
        if (activeCells.Count == 0) return;

        float cx = activeCells.Average(c => (float)c.Key.x);
        float cy = activeCells.Average(c => (float)c.Key.y);
        Vector2 centroid = new(cx, cy);

        originCell = activeCells
            .OrderBy(c => Vector2.Distance(new Vector2(c.Key.x, c.Key.y), centroid))
            .ThenBy(c => c.Key.x).ThenBy(c => c.Key.y)  // deterministic tie-break
            .First().Key;
    }
    #endregion

    #region drawing
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        targetPiece = (PieceData)EditorGUILayout.ObjectField(
            targetPiece, typeof(PieceData), false, GUILayout.Width(220));

        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(48)))
        {
            activeCells.Clear();
            Repaint();
        }

        GUILayout.FlexibleSpace();

        GUI.enabled = targetPiece != null && activeCells.Count > 0;
        if (GUILayout.Button("save to piece", EditorStyles.toolbarButton, GUILayout.Width(110)))
            SaveToPiece();
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawGrid()
    {
        float toolbarH = EditorStyles.toolbar.fixedHeight;
        Rect gridArea = new(GridPadding, toolbarH + GridPadding, GridSize * CellSize, GridSize * CellSize);

        // Background panel
        EditorGUI.DrawRect(
            new Rect(gridArea.x - 2, gridArea.y - 2, gridArea.width + 4, gridArea.height + 4),
            new Color(0.08f, 0.08f, 0.08f));
        EditorGUI.DrawRect(gridArea, ColBackground);

        Event e = Event.current;
        Vector2Int center = new(GridSize / 2, GridSize / 2);

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                // Flip Y so that positive Y is upward (like Unity's world space)
                var  coord    = new Vector2Int(x, y);
                Rect cellRect = new(
                    gridArea.x + x * CellSize,
                    gridArea.y + (GridSize - 1 - y) * CellSize,
                    CellSize - 1,
                    CellSize - 1);

                bool active = activeCells.ContainsKey(coord);
                bool isOrigin = active && coord == originCell;
                bool isCenter = coord == center;

                Color fill;
                if (activeCells.ContainsKey(coord))
                {
                    PieceTileType t = activeCells[coord];
                    fill = t == PieceTileType.PASSTHROUGH ? PassthroughCell :
                           t == PieceTileType.SWITCH ? SwitchCell : 
                           NormalCell;
                        
                }
                else fill = ColCellEmpty;

                EditorGUI.DrawRect(cellRect, fill);

                // Grid lines (right + bottom edge)
                EditorGUI.DrawRect(new Rect(cellRect.xMax, cellRect.y,    1,            cellRect.height), ColGridLine);
                EditorGUI.DrawRect(new Rect(cellRect.x,    cellRect.yMax, cellRect.width, 1),             ColGridLine);

                // Label
                if (active)
                {
                    Vector2Int rel   = coord - originCell;
                    string label = isOrigin ? "●" : $"{rel.x},{rel.y}";
                    var style = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize  = isOrigin ? 14 : 9,
                        normal    = { textColor = Color.white }
                    };
                    GUI.Label(cellRect, label, style);
                }
                else if (isCenter)
                {
                    var style = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize  = 9,
                        normal    = { textColor = new Color(0.4f, 0.4f, 0.4f) }
                    };
                    GUI.Label(cellRect, "·", style);
                }

                // Input
                if (cellRect.Contains(e.mousePosition))
                {
                    EditorGUI.DrawRect(cellRect, ColHover);

                    if (e.type == EventType.MouseDown)
                    {
                        if (e.button == 0)
                        {
                            // Toggle: remember whether this drag is adding or removing
                            dragAdding = !active;
                            isDragging = true;

                            if (dragAdding) activeCells.Add(coord, PieceTileType.NORMAL);
                            else activeCells.Remove(coord);
                        }
                        else if (e.button == 1)
                        {
                            if (activeCells.ContainsKey(coord))
                            {
                                // todo - add more tile types? need to update here. this is also really bad code, can probably do some int enum shenanigans?
                                if (activeCells[coord] == PieceTileType.NORMAL) activeCells[coord] = PieceTileType.PASSTHROUGH;
                                else if (activeCells[coord] == PieceTileType.PASSTHROUGH) activeCells[coord] = PieceTileType.SWITCH;
                                else activeCells[coord] = PieceTileType.NORMAL;
                            }
                        }
                        RecalculateOrigin();
                        e.Use();
                        Repaint();
                    }
                    else if (e.type == EventType.MouseDrag && isDragging && e.button == 0)
                    {
                        if (dragAdding) { if (!activeCells.ContainsKey(coord)) activeCells.Add(coord, PieceTileType.NORMAL); }
                        else            activeCells.Remove(coord);

                        RecalculateOrigin();
                        e.Use();
                        Repaint();
                    }
                }
            }
        }

        // Release drag
        if (e.type == EventType.MouseUp) isDragging = false;

        // Reserve space so footer sits below the grid
        GUILayout.Space(toolbarH + GridPadding * 2 + GridSize * CellSize);
    }

    private void DrawFooter()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (activeCells.Count > 0)
        {
            Vector2Int relOrigin = originCell - new Vector2Int(GridSize / 2, GridSize / 2);
            EditorGUILayout.LabelField(
                $"Tiles: {activeCells.Count} | Origin offset from center: {relOrigin}",
                EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.LabelField("No tiles painted yet.", EditorStyles.miniLabel);
        }

        EditorGUILayout.LabelField(
            "LMB: paint / drag / erase | RMB: change PieceTile 'type' | ● = centroid\n" + 
            "blue = normal piece, orange = switch, green = passthrough",
            EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();
    }
    #endregion
}