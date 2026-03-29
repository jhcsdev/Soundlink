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
    public static readonly Color ColBackground   = new(0.13f, 0.13f, 0.13f);
    public static readonly Color ColCellEmpty    = new(0.20f, 0.20f, 0.20f);
    public static readonly Color NormalCell      = new(0.28f, 0.56f, 0.90f);
    public static readonly Color PassthroughCell = new(0.20f, 0.80f, 0.45f);
    public static readonly Color SwitchCell      = new(0.8f,  0.5f,  0.3f);
    public static readonly Color ColHover        = new(1.00f, 1.00f, 1.00f, 0.07f);
    public static readonly Color ColGridLine     = new(0.10f, 0.10f, 0.10f);
    public static readonly Color ColGlueEdge     = Color.red;
    private const float GlueThickness            = 3f;
    #endregion

    #region tracking state
    private Dictionary<Vector2Int, PieceEditorTileData> activeCells = new();
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
            tp.FindPropertyRelative("spriteType").enumValueIndex      = (int)data.spriteType;
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

        Debug.Log($"[PieceEditor] Saved {activeCells.Count} tile(s) to '{targetPiece.name}'.");
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

        // The four neighbours whose edge-counts may have changed
        RecalculateTileSprite(coord + Vector2Int.up);
        RecalculateTileSprite(coord + Vector2Int.down);
        RecalculateTileSprite(coord + Vector2Int.left);
        RecalculateTileSprite(coord + Vector2Int.right);
    }

    /// <summary>
    /// Counts the occupied cardinal neighbours of <paramref name="coord"/> and sets
    /// <see cref="PieceEditorTileData.spriteType"/> and <see cref="PieceEditorTileData.spriteDirection"/>
    /// accordingly.
    ///
    /// Neighbour layout used to determine direction (Unity Y-up, same as grid):
    ///
    ///         [UP]
    ///   [LEFT] [coord] [RIGHT]
    ///         [DOWN]
    ///
    /// "Edge" means an exposed side with no neighbour.
    ///
    ///   0 neighbours → ALL_EDGE   (isolated tile, direction irrelevant → FACES_UP)
    ///   4 neighbours → NO_EDGE    (fully interior,  direction irrelevant → FACES_UP)
    ///   1 neighbour  → TRIPLE_EDGE;  the one neighbour sits on the BOTTOM side when FACES_UP,
    ///                                so rotate to put that neighbour at the "bottom" of the sprite.
    ///   3 neighbours → SINGLE_EDGE; the one exposed edge is the TOP side when FACES_UP,
    ///                                so rotate to put that gap at the "top".
    ///   2 neighbours (opposite)    → DOUBLE_OPPOSING_EDGE; horizontal pair → FACES_RIGHT.
    ///   2 neighbours (adjacent)    → DOUBLE_CORNER_EDGE;   the TWO EXPOSED edges are top+right
    ///                                when FACES_UP, so we rotate until the open corner matches.
    /// </summary>
    private void RecalculateTileSprite(Vector2Int coord)
    {
        if (!activeCells.TryGetValue(coord, out var data)) return;

        bool hasUp    = activeCells.ContainsKey(coord + Vector2Int.up);
        bool hasDown  = activeCells.ContainsKey(coord + Vector2Int.down);
        bool hasLeft  = activeCells.ContainsKey(coord + Vector2Int.left);
        bool hasRight = activeCells.ContainsKey(coord + Vector2Int.right);

        int neighborCount = (hasUp    ? 1 : 0)
                          + (hasDown  ? 1 : 0)
                          + (hasLeft  ? 1 : 0)
                          + (hasRight ? 1 : 0);

        switch (neighborCount)
        {
            case 0:
                data.spriteType      = PieceTileSpriteType.ALL_EDGE;
                data.spriteDirection = TileSpriteDirection.FACES_UP;
                break;

            case 4:
                data.spriteType      = PieceTileSpriteType.NO_EDGE;
                data.spriteDirection = TileSpriteDirection.FACES_UP;
                break;

            // ── 3 neighbours: one exposed edge ──────────────────────────────────────
            // The sprite shows its single edge on TOP when FACES_UP, so we point the
            // direction toward the missing neighbour.
            case 3:
                data.spriteType = PieceTileSpriteType.SINGLE_EDGE;
                data.spriteDirection =
                    !hasUp    ? TileSpriteDirection.FACES_UP    :
                    !hasRight ? TileSpriteDirection.FACES_RIGHT :
                    !hasDown  ? TileSpriteDirection.FACES_DOWN  :
                                TileSpriteDirection.FACES_LEFT;
                break;

            // ── 1 neighbour: three exposed edges ────────────────────────────────────
            // The sprite leaves the BOTTOM closed (neighbour there) when FACES_UP, so
            // we rotate until the single neighbour sits at the "bottom" of the sprite.
            case 1:
                data.spriteType = PieceTileSpriteType.TRIPLE_EDGE;
                data.spriteDirection =
                    hasDown  ? TileSpriteDirection.FACES_UP    :   // neighbour below  → bottom closed
                    hasLeft  ? TileSpriteDirection.FACES_RIGHT :   // neighbour left   → rotate so left = bottom
                    hasUp    ? TileSpriteDirection.FACES_DOWN  :   // neighbour above  → rotate so top  = bottom
                               TileSpriteDirection.FACES_LEFT;     // neighbour right  → rotate so right = bottom
                break;

            // ── 2 neighbours ─────────────────────────────────────────────────────────
            case 2:
                if ((hasUp && hasDown) || (hasLeft && hasRight))
                {
                    // Opposing neighbours → straight corridor
                    data.spriteType      = PieceTileSpriteType.DOUBLE_OPPOSING_EDGE;
                    data.spriteDirection = (hasUp && hasDown)
                        ? TileSpriteDirection.FACES_UP    // vertical corridor
                        : TileSpriteDirection.FACES_RIGHT; // horizontal corridor
                }
                else
                {
                    // Adjacent neighbours → corner piece.
                    // FACES_UP  exposes top  + right  → neighbours are DOWN  + LEFT
                    // FACES_RIGHT exposes right + bottom → neighbours are LEFT  + UP
                    // FACES_DOWN  exposes bottom + left  → neighbours are UP   + RIGHT
                    // FACES_LEFT  exposes left  + top    → neighbours are RIGHT + DOWN
                    data.spriteType = PieceTileSpriteType.DOUBLE_CORNER_EDGE;
                    data.spriteDirection =
                        (hasDown  && hasLeft)  ? TileSpriteDirection.FACES_UP    :
                        (hasLeft  && hasUp)    ? TileSpriteDirection.FACES_RIGHT :
                        (hasUp    && hasRight) ? TileSpriteDirection.FACES_DOWN  :
                                                 TileSpriteDirection.FACES_LEFT;  // hasRight && hasDown
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
        else                                   data.glueDirections.Add(dir);
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
        float toolbarH    = EditorStyles.toolbar.fixedHeight;
        Rect  gridArea    = new(GridPadding, toolbarH + GridPadding, GridSize * CellSize, GridSize * CellSize);
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
            "blue = normal  |  orange = switch  |  green = passthrough  |  red bar = glue edge",
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