using System.Collections.Generic;
using System.Linq;
using GamePieces;
using NUnit.Framework;
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
    public static readonly Color ColBackground   = new(0.13f, 0.13f, 0.13f);
    public static readonly Color ColCellEmpty    = new(0.20f, 0.20f, 0.20f);
    public static readonly Color NormalCell      = new(0.28f, 0.56f, 0.90f);
    public static readonly Color PassthroughCell = new(0.20f, 0.80f, 0.45f);
    public static readonly Color SwitchCell      = new(0.8f,  0.5f,  0.3f);
    public static readonly Color ColHover        = new(1.00f, 1.00f, 1.00f, 0.07f);
    public static readonly Color ColGridLine     = new(0.10f, 0.10f, 0.10f);
    public static readonly Color ColGlueEdge     = Color.red;
    private const float GlueThickness            = 3f;

    // Lighter tints drawn on exposed (neighbourless) edges — drawn before glue so red sits on top
    public static readonly Color NormalEdge      = new(0.52f, 0.74f, 1.00f);
    public static readonly Color PassthroughEdge = new(0.45f, 0.98f, 0.68f);
    public static readonly Color SwitchEdge      = new(1.00f, 0.76f, 0.52f);
    private const float EdgeThickness            = 5f;
    #endregion

    #region tracking state
    private Dictionary<Vector2Int, PieceEditorTileData> activeCells = new();
    private Vector2Int originCell;
    private PieceData targetPiece;

    private bool dragAdding;
    private bool isDragging;
    private bool isSilentPiece;
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

    #region unity events
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
        {
            Vector2Int curPos = tile.relativeOffset + center;
            activeCells[curPos] = new PieceEditorTileData
            {
                type           = tile.type,
                glueDirections = tile.glue.ToList()
            };
        }

        // Recalculate sprite data for all loaded tiles
        foreach (var coord in activeCells.Keys.ToList())
            RecalculateTileSprite(coord);

        RecalculateOrigin();
    }

    private void SaveToPiece()
    {
        if (activeCells.Count == 0 || targetPiece == null) return;

        SerializedObject so = new(targetPiece);
        SerializedProperty tilesProp = so.FindProperty("tiles");
        SerializedProperty silentPro = so.FindProperty("isSilentPiece");
        silentPro.boolValue = isSilentPiece;
        tilesProp.ClearArray();

        var sorted = activeCells.OrderBy(c => c.Key.x).ThenBy(c => c.Key.y).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            Vector2Int          cell      = sorted[i].Key;
            Vector2Int          relOffset = cell - originCell;
            PieceEditorTileData data      = sorted[i].Value;

            tilesProp.InsertArrayElementAtIndex(i);
            SerializedProperty tp = tilesProp.GetArrayElementAtIndex(i);
            tp.FindPropertyRelative("relativeOffset").vector2IntValue = relOffset;
            tp.FindPropertyRelative("isOrigin").boolValue             = cell == originCell;
            tp.FindPropertyRelative("type").enumValueIndex            = (int)data.type;
            tp.FindPropertyRelative("tileType").enumValueIndex      = (int)data.spriteType;
            tp.FindPropertyRelative("spriteDirection").enumValueIndex = (int)data.spriteDirection;

            SerializedProperty glueDir = tp.FindPropertyRelative("glue");
            glueDir.ClearArray();
            for (int g = 0; g < data.glueDirections.Count; g++)
            {
                glueDir.InsertArrayElementAtIndex(g);
                glueDir.GetArrayElementAtIndex(g).enumValueIndex = (int)data.glueDirections[g];
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetPiece);
        AssetDatabase.SaveAssets();

        // Debug.Log($"[PieceEditor] Saved {activeCells.Count} tile(s) to '{targetPiece.name}'.");
    }
    #endregion

    #region centroid calc
    private void RecalculateOrigin()
    {
        if (activeCells.Count == 0) return;

        float cx         = activeCells.Average(c => (float)c.Key.x);
        float cy         = activeCells.Average(c => (float)c.Key.y);
        Vector2 centroid = new(cx, cy);

        originCell = activeCells
            .OrderBy(c => Vector2.Distance(new Vector2(c.Key.x, c.Key.y), centroid))
            .ThenBy(c => c.Key.x).ThenBy(c => c.Key.y)
            .First().Key;
    }
    #endregion

    #region sprite type calculation

    /// <summary>
    /// Recalculates the sprite type and facing direction for the tile at <paramref name="coord"/>,
    /// then does the same for each of its four cardinal neighbours.  Call this whenever a tile is
    /// added or removed so every affected cell stays up-to-date.
    /// </summary>
    private void RecalculateNeighborSprites(Vector2Int coord)
    {
        // The tile itself (may have just been removed, so guard inside)
        RecalculateTileSprite(coord);

        // The eight neighbours whose edge-counts may have changed
        RecalculateTileSprite(coord + Vector2Int.up);
        RecalculateTileSprite(coord + Vector2Int.down);
        RecalculateTileSprite(coord + Vector2Int.left);
        RecalculateTileSprite(coord + Vector2Int.right);
        RecalculateTileSprite(coord + Vector2Int.one);
        RecalculateTileSprite(coord + Vector2Int.one * -1);
        RecalculateTileSprite(coord + new Vector2Int(1, -1));
        RecalculateTileSprite(coord + new Vector2Int(-1, 1));
    }

    // based on the eight surrounding neighbors, recalculates the tile.
    private void RecalculateTileSprite(Vector2Int coord)
    {
        if (!activeCells.TryGetValue(coord, out var data)) return;

        bool hasUp = activeCells.ContainsKey(coord + Vector2Int.up);
        bool hasDown = activeCells.ContainsKey(coord + Vector2Int.down);
        bool hasLeft = activeCells.ContainsKey(coord + Vector2Int.left);
        bool hasRight = activeCells.ContainsKey(coord + Vector2Int.right);
        bool hasUpRight = activeCells.ContainsKey(coord + Vector2Int.one);
        bool hasUpLeft = activeCells.ContainsKey(coord + new Vector2Int(-1, 1));
        bool hasDownLeft = activeCells.ContainsKey(coord + Vector2Int.one * -1);
        bool hasDownRight = activeCells.ContainsKey(coord + new Vector2Int(1, -1));

        int cardinalNeighborCount = (hasUp ? 1 : 0)
                                    + (hasDown ? 1 : 0)
                                    + (hasLeft ? 1 : 0)
                                    + (hasRight ? 1 : 0);
        int cornerNeighborCount = (hasUpLeft ? 1 : 0)
                                + (hasUpRight ? 1 : 0)
                                + (hasDownLeft ? 1 : 0)
                                + (hasDownRight ? 1 : 0);

        switch (cardinalNeighborCount)
        {
            case 0:
                data.spriteType      = PieceTileSpriteType.ALL_EDGE;
                data.spriteDirection = TileSpriteDirection.FACES_UP;
                break;

            case 1: 
                data.spriteType = PieceTileSpriteType.TRIPLE_EDGE;
                data.spriteDirection = hasUp ? TileSpriteDirection.FACES_DOWN : 
                                       hasDown ? TileSpriteDirection.FACES_UP :
                                       hasLeft ? TileSpriteDirection.FACES_RIGHT : 
                                       TileSpriteDirection.FACES_LEFT;
                break;

            case 2:
                if ((hasUp && hasDown) || (hasLeft && hasRight))
                {
                    data.spriteType = PieceTileSpriteType.OPPOSITE_EDGE;
                    data.spriteDirection = (hasUp && hasDown) ? TileSpriteDirection.FACES_RIGHT : TileSpriteDirection.FACES_UP;
                }
                else
                {
                    if (hasUp && hasRight)
                    {
                        data.spriteType = hasUpRight ? PieceTileSpriteType.ADJACENT_EDGE_NO_CORNER : PieceTileSpriteType.ADJACENT_EDGE_CORNER;
                        data.spriteDirection = TileSpriteDirection.FACES_DOWN;
                    }
                    else if (hasRight && hasDown)
                    {
                        data.spriteType = hasDownRight ? PieceTileSpriteType.ADJACENT_EDGE_NO_CORNER : PieceTileSpriteType.ADJACENT_EDGE_CORNER;
                        data.spriteDirection = TileSpriteDirection.FACES_LEFT;
                    }
                    else if (hasDown && hasLeft)
                    {
                        data.spriteType = hasDownLeft ? PieceTileSpriteType.ADJACENT_EDGE_NO_CORNER : PieceTileSpriteType.ADJACENT_EDGE_CORNER;
                        data.spriteDirection = TileSpriteDirection.FACES_UP;
                    }
                    else
                    {
                        data.spriteType = hasUpLeft ? PieceTileSpriteType.ADJACENT_EDGE_NO_CORNER : PieceTileSpriteType.ADJACENT_EDGE_CORNER;
                        data.spriteDirection = TileSpriteDirection.FACES_RIGHT;
                    }
                }
                break;

            case 3:
                if (!hasUp)
                {
                    data.spriteType = (hasDownLeft && hasDownRight) ? PieceTileSpriteType.T_SECTION_NO_CORNERS
                                        : hasDownLeft ? PieceTileSpriteType.T_SECTION_CORNER_BOTTOM_RIGHT
                                        : hasDownRight ? PieceTileSpriteType.T_SECTION_CORNER_BOTTOM_LEFT
                                        : PieceTileSpriteType.T_SECTION_BOTH_CORNERS;
                    data.spriteDirection = TileSpriteDirection.FACES_UP;
                }
                else if (!hasRight)
                {
                    data.spriteType = (hasUpLeft && hasDownLeft) ? PieceTileSpriteType.T_SECTION_NO_CORNERS
                                        : hasUpLeft ? PieceTileSpriteType.T_SECTION_CORNER_BOTTOM_RIGHT
                                        : hasDownLeft ? PieceTileSpriteType.T_SECTION_CORNER_BOTTOM_LEFT
                                        : PieceTileSpriteType.T_SECTION_BOTH_CORNERS;
                    data.spriteDirection = TileSpriteDirection.FACES_RIGHT;
                }
                else if (!hasLeft)
                {
                    data.spriteType = (hasUpRight && hasDownRight) ? PieceTileSpriteType.T_SECTION_NO_CORNERS
                                        : hasUpRight ? PieceTileSpriteType.T_SECTION_CORNER_BOTTOM_LEFT
                                        : hasDownRight ? PieceTileSpriteType.T_SECTION_CORNER_BOTTOM_RIGHT
                                        : PieceTileSpriteType.T_SECTION_BOTH_CORNERS;
                    data.spriteDirection = TileSpriteDirection.FACES_LEFT;
                }
                else // !hasDown
                {
                    data.spriteType = (hasUpLeft && hasUpRight) ? PieceTileSpriteType.T_SECTION_NO_CORNERS
                                        : hasUpLeft ? PieceTileSpriteType.T_SECTION_CORNER_BOTTOM_LEFT
                                        : hasUpRight ? PieceTileSpriteType.T_SECTION_CORNER_BOTTOM_RIGHT
                                        : PieceTileSpriteType.T_SECTION_BOTH_CORNERS;
                    data.spriteDirection = TileSpriteDirection.FACES_DOWN;
                }
                break;

            case 4:
                switch (cornerNeighborCount) {
                    case 0:
                        data.spriteType = PieceTileSpriteType.NO_EDGE_FOUR_CORNERS;
                        data.spriteDirection = TileSpriteDirection.FACES_UP;
                        break;
                    case 1:
                        data.spriteType = PieceTileSpriteType.NO_EDGE_THREE_CORNERS;
                        data.spriteDirection = hasUpRight ? TileSpriteDirection.FACES_UP
                                             : hasDownRight ? TileSpriteDirection.FACES_RIGHT
                                             : hasDownLeft ? TileSpriteDirection.FACES_DOWN 
                                             : TileSpriteDirection.FACES_LEFT;
                        break;
                    case 2: 
                        if ((hasUpRight && hasDownLeft) || (hasUpLeft && hasDownLeft))
                        {
                            data.spriteType = PieceTileSpriteType.NO_EDGE_OPPOSITE_CORNER; 
                            data.spriteDirection = hasUpRight ? TileSpriteDirection.FACES_RIGHT : TileSpriteDirection.FACES_UP;
                        }
                        else // adjacent
                        {
                            data.spriteType = PieceTileSpriteType.NO_EDGE_ADJACENT_CORNERS;
                            data.spriteDirection = (hasUpLeft && hasUpRight) ? TileSpriteDirection.FACES_UP
                                                 : (hasUpRight && hasDownRight) ? TileSpriteDirection.FACES_RIGHT
                                                 : (hasDownRight && hasDownLeft) ? TileSpriteDirection.FACES_DOWN
                                                 : TileSpriteDirection.FACES_LEFT;
                        }
                        break;
                    case 3:
                        data.spriteType = PieceTileSpriteType.NO_EDGE_ONE_CORNER;
                        data.spriteDirection = !hasDownLeft ? TileSpriteDirection.FACES_UP
                                             : !hasUpLeft ? TileSpriteDirection.FACES_RIGHT
                                             : !hasUpRight ? TileSpriteDirection.FACES_DOWN 
                                             : TileSpriteDirection.FACES_LEFT;
                        break;
                    case 4:
                        data.spriteType = PieceTileSpriteType.NO_EDGE_NO_CORNER;
                        data.spriteDirection = TileSpriteDirection.FACES_UP;
                        break;
                }
                break;
        }
    }

    #endregion

    #region glue helpers

    /// <summary>
    /// Determines which cardinal edge the mouse is nearest using 45-degree diagonal splits.
    /// The cell is divided into four triangles by its two diagonals (an X pattern):
    ///
    ///   +-------+
    ///   |\ N   /|
    ///   | \   / |
    ///   |W  x  E|
    ///   | /   \ |
    ///   |/  S  \|
    ///   +-------+
    ///
    /// Normalised coords (nx, ny) in [0,1] — ny=0 is the screen-top edge (NORTH).
    ///   NORTH : ny &lt; nx  AND  ny &lt; (1-nx)   → above both diagonals
    ///   SOUTH : ny &gt; nx  AND  ny &gt; (1-nx)   → below both diagonals
    ///   WEST  : ny &gt; nx  AND  ny &lt; (1-nx)   → left triangle
    ///   EAST  : ny &lt; nx  AND  ny &gt; (1-nx)   → right triangle
    /// </summary>
    private static GlueCardinality GetGlueDirectionFromMouse(Rect cellRect, Vector2 mousePos)
    {
        float nx = (mousePos.x - cellRect.x) / cellRect.width;
        float ny = (mousePos.y - cellRect.y) / cellRect.height;

        bool aboveMain = ny < nx;       // above the \ diagonal
        bool aboveAnti = ny < 1f - nx;  // above the / diagonal

        if ( aboveMain &&  aboveAnti) return GlueCardinality.NORTH;
        if (!aboveMain && !aboveAnti) return GlueCardinality.SOUTH;
        if (!aboveMain &&  aboveAnti) return GlueCardinality.WEST;
        return GlueCardinality.EAST;
    }

    private static void ToggleGlue(PieceEditorTileData data, GlueCardinality dir)
    {
        if (data.glueDirections.Contains(dir)) data.glueDirections.Remove(dir);
        else data.glueDirections.Add(dir);
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
        isSilentPiece = GUILayout.Toggle(isSilentPiece, "Silent piece");
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
        Rect  gridArea = new(GridPadding, toolbarH + GridPadding, GridSize * CellSize, GridSize * CellSize);
        Vector2Int center = new(GridSize / 2, GridSize / 2);

        // Background panel
        EditorGUI.DrawRect(
            new Rect(gridArea.x - 2, gridArea.y - 2, gridArea.width + 4, gridArea.height + 4),
            new Color(0.08f, 0.08f, 0.08f));
        EditorGUI.DrawRect(gridArea, ColBackground);

        Event e = Event.current;

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                var  coord    = new Vector2Int(x, y);
                Rect cellRect = new(
                    gridArea.x + x * CellSize,
                    gridArea.y + (GridSize - 1 - y) * CellSize,   // flip Y → positive Y is up
                    CellSize - 1,
                    CellSize - 1);

                bool active   = activeCells.ContainsKey(coord);
                bool isOrigin = active && coord == originCell;
                bool isCenter = coord == center;

                // --- Fill ---
                Color fill = active
                    ? activeCells[coord].type switch
                      {
                          PieceTileType.PASSTHROUGH => PassthroughCell,
                          PieceTileType.SWITCH      => SwitchCell,
                          _                         => NormalCell
                      }
                    : ColCellEmpty;

                EditorGUI.DrawRect(cellRect, fill);

                // --- Grid lines ---
                EditorGUI.DrawRect(new Rect(cellRect.xMax, cellRect.y,    1,              cellRect.height), ColGridLine);
                EditorGUI.DrawRect(new Rect(cellRect.x,    cellRect.yMax, cellRect.width, 1),               ColGridLine);

                // --- Exposed edge highlights (drawn before glue so red sits on top) ---
                if (active)
                {
                    Color edgeColor = activeCells[coord].type switch
                    {
                        PieceTileType.PASSTHROUGH => PassthroughEdge,
                        PieceTileType.SWITCH      => SwitchEdge,
                        _                         => NormalEdge
                    };
                    DrawExposedEdges(cellRect, coord, edgeColor);
                }

                // --- Glue edge bars ---
                if (active)
                    DrawGlueEdges(cellRect, activeCells[coord].glueDirections);

                // --- Label ---
                if (active)
                {
                    Vector2Int rel   = coord - originCell;
                    string     label = isOrigin ? "●" : $"{rel.x},{rel.y}";
                    GUI.Label(cellRect, label, new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize  = isOrigin ? 14 : 9,
                        normal    = { textColor = Color.white }
                    });
                }
                else if (isCenter)
                {
                    GUI.Label(cellRect, "·", new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize  = 9,
                        normal    = { textColor = new Color(0.4f, 0.4f, 0.4f) }
                    });
                }

                // --- Input ---
                if (!cellRect.Contains(e.mousePosition)) continue;

                EditorGUI.DrawRect(cellRect, ColHover);

                if (e.type == EventType.MouseDown)
                {
                    if (e.button == 0)
                    {
                        dragAdding = !active;
                        isDragging = true;

                        if (dragAdding) activeCells[coord] = new PieceEditorTileData { type = PieceTileType.NORMAL };
                        else            activeCells.Remove(coord);

                        RecalculateNeighborSprites(coord);
                    }
                    else if (e.button == 1 && active)
                    {
                        // Cycle tile type: NORMAL → PASSTHROUGH → SWITCH → NORMAL
                        activeCells[coord].type = activeCells[coord].type switch
                        {
                            PieceTileType.NORMAL      => PieceTileType.PASSTHROUGH,
                            PieceTileType.PASSTHROUGH => PieceTileType.SWITCH,
                            _                         => PieceTileType.NORMAL
                        };
                    }

                    RecalculateOrigin();
                    e.Use();
                    Repaint();
                }
                else if (e.type == EventType.MouseDrag && isDragging && e.button == 0)
                {
                    if (dragAdding) { if (!activeCells.ContainsKey(coord)) activeCells[coord] = new PieceEditorTileData { type = PieceTileType.NORMAL }; }
                    else            activeCells.Remove(coord);

                    RecalculateNeighborSprites(coord);
                    RecalculateOrigin();
                    e.Use();
                    Repaint();
                }
                else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.G && active)
                {
                    // Detect which triangular zone (45° diagonals) the mouse is in and toggle that edge's glue
                    GlueCardinality dir = GetGlueDirectionFromMouse(cellRect, e.mousePosition);
                    ToggleGlue(activeCells[coord], dir);
                    e.Use();
                    Repaint();
                }
            }
        }

        if (e.type == EventType.MouseUp) isDragging = false;

        GUILayout.Space(toolbarH + GridPadding * 2 + GridSize * CellSize);
    }

    /// <summary>
    /// Draws a light-tinted bar on each side of the cell that has no active neighbour,
    /// giving a visual indication of exposed piece edges.  Drawn before glue bars so
    /// the red glue overlay is always fully visible on top.
    /// </summary>
    private void DrawExposedEdges(Rect r, Vector2Int coord, Color edgeColor)
    {
        // Grid Y-up: Vector2Int.up → higher y index → screen-top (NORTH in screen space)
        if (!activeCells.ContainsKey(coord + Vector2Int.up))
            EditorGUI.DrawRect(new Rect(r.x,                    r.y,                    r.width,       EdgeThickness), edgeColor);
        if (!activeCells.ContainsKey(coord + Vector2Int.down))
            EditorGUI.DrawRect(new Rect(r.x,                    r.yMax - EdgeThickness, r.width,       EdgeThickness), edgeColor);
        if (!activeCells.ContainsKey(coord + Vector2Int.left))
            EditorGUI.DrawRect(new Rect(r.x,                    r.y,                    EdgeThickness, r.height),      edgeColor);
        if (!activeCells.ContainsKey(coord + Vector2Int.right))
            EditorGUI.DrawRect(new Rect(r.xMax - EdgeThickness, r.y,                    EdgeThickness, r.height),      edgeColor);
    }

    /// <summary>
    /// Draws a bright-red bar flush against each edge that has an active glue.
    /// NORTH = screen-top edge, SOUTH = screen-bottom, WEST = left, EAST = right.
    /// </summary>
    private static void DrawGlueEdges(Rect r, List<GlueCardinality> glues)
    {
        foreach (var g in glues)
        {
            Rect edge = g switch
            {
                GlueCardinality.NORTH => new Rect(r.x,                    r.y,                    r.width,       GlueThickness),
                GlueCardinality.SOUTH => new Rect(r.x,                    r.yMax - GlueThickness, r.width,       GlueThickness),
                GlueCardinality.WEST  => new Rect(r.x,                    r.y,                    GlueThickness, r.height),
                GlueCardinality.EAST  => new Rect(r.xMax - GlueThickness, r.y,                    GlueThickness, r.height),
                _                    => default
            };
            EditorGUI.DrawRect(edge, ColGlueEdge);
        }
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
            "LMB: paint / drag / erase  |  RMB: cycle tile type  |  G (hover active tile): toggle glue edge  |  ● = centroid\n" +
            "blue = normal  |  orange = switch  |  green = passthrough  |  red bar = glue edge  |  light edge = exposed side",
            EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();
    }
    #endregion
}

[System.Serializable]
public class PieceEditorTileData
{
    public PieceTileType          type;
    public List<GlueCardinality>  glueDirections  = new();
    public PieceTileSpriteType    spriteType      = PieceTileSpriteType.ALL_EDGE;
    public TileSpriteDirection    spriteDirection = TileSpriteDirection.FACES_UP;
}