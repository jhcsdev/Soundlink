using UnityEditor;
using UnityEngine;
using GamePieces;
using System.Collections.Generic;

[CustomEditor(typeof(PieceData))]
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
        float previewCellSize = 16f;
        float previewSize = PieceEditorWindow.GridSize * previewCellSize;

        Rect gridArea = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false));

        gridArea.x = (EditorGUIUtility.currentViewWidth - previewSize) * 0.5f;

        Dictionary<Vector2Int, PieceTileType> lookupByOffset = new();
        Vector2Int originLocation = Vector2Int.zero;
        foreach (PieceTileData tile in ((PieceData)target).tiles)
        {
            lookupByOffset[tile.relativeOffset] = tile.type;
        }

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
                Color fill = lookupByOffset.TryGetValue(adjustedCoord, out PieceTileType t)
                    ? (
                        t == PieceTileType.PASSTHROUGH ? PieceEditorWindow.PassthroughCell :
                        t == PieceTileType.SWITCH ? PieceEditorWindow.SwitchCell :
                        PieceEditorWindow.NormalCell
                    ) : PieceEditorWindow.ColCellEmpty;

                EditorGUI.DrawRect(cellRect, fill);
                EditorGUI.DrawRect(new Rect(cellRect.xMax, cellRect.y, 1, cellRect.height), PieceEditorWindow.ColGridLine);
                EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.yMax, cellRect.width, 1), PieceEditorWindow.ColGridLine);
            }
        }
    }
}