using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SplineUtil
{
#if UNITY_EDITOR
    public static void DrawBezier(int startIndex, int endIndex, List<SplineVertex> vertexes, Vector3 targetPos, ISplineObject handleObj)
    {
        Handles.DrawBezier(
            vertexes[startIndex].position + targetPos,
            vertexes[endIndex].position + targetPos,
            vertexes[startIndex].startTangent + vertexes[startIndex].position + targetPos,
            vertexes[endIndex].endTangent + vertexes[endIndex].position + targetPos,
            handleObj.lineColor,
            new Texture2D(1, 1),
            10f);
    }

    public static void DrawHandles(int index, List<SplineVertex> vertexes, Vector3 targetPos, GUIStyle textStyle, ISplineObject handleObj)
    {
        if (index > -1 && index < vertexes.Count)
        {
            EditorGUI.BeginChangeCheck();
            SplineVertex vertex = vertexes[index];
            Vector3 localVertexPos = vertexes[index].position + targetPos;

            if (handleObj.handleMode == SplineVertexHandleMode.Move)
            {
                if (handleObj.splineMode == SplineMode.Bezier)
                {
                    Handles.Label(vertexes[index].startTangent + localVertexPos, index.ToString() + " Start", textStyle);
                    Handles.DrawPolyLine(localVertexPos, vertexes[index].startTangent + localVertexPos);
                    Vector3 newTargetStartTangent = Handles.PositionHandle(vertexes[index].startTangent + localVertexPos, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        vertex.startTangent = newTargetStartTangent - localVertexPos;
                        handleObj.SetVertex(index, vertex);
                    }

                    Handles.Label(vertexes[index].endTangent + localVertexPos, index.ToString() + " End", textStyle);
                    Handles.DrawPolyLine(localVertexPos, vertexes[index].endTangent + localVertexPos);
                    Vector3 newTargetEndTangent = Handles.PositionHandle(vertexes[index].endTangent + localVertexPos, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        vertex.endTangent = newTargetEndTangent - localVertexPos;
                        handleObj.SetVertex(index, vertex);
                    }
                }

                Vector3 newPos = Handles.PositionHandle(localVertexPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    vertex.position = newPos - targetPos;
                    handleObj.SetVertex(index, vertex);
                }
            }
            else if (handleObj.handleMode == SplineVertexHandleMode.Rotate)
            {
                Quaternion newRot = Handles.RotationHandle(Quaternion.Euler(vertexes[index].rotation.eulerAngles), localVertexPos);
                if (EditorGUI.EndChangeCheck())
                {
                    vertex.rotation = newRot;
                    handleObj.SetVertex(index, vertex);
                }
                Handles.PositionHandle(localVertexPos, Quaternion.Euler(vertexes[index].rotation.eulerAngles));
            }
        }
    }
#endif
}

public interface ISplineObject
{
    public Color lineColor { get; }
    public SplineVertexHandleMode handleMode { get; }
    public SplineMode splineMode { get; }

    public void SetVertex(int index, SplineVertex vertex);
}