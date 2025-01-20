#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(SplineManager)), CanEditMultipleObjects]
public class SplineManagerEditor : Editor
{
    List<SplineVertex> vertexes;
    Vector3 targetPos;
    GUIStyle textStyle = new GUIStyle();

    void OnSceneGUI()
    {
        SplineManager manager = target as SplineManager;
        if(manager.vertexes == null || manager.vertexes.Count < 1)
        {
            return;
        }

        vertexes = manager.vertexes;
        targetPos = manager.transform.position;
        Handles.color = manager.lineColor;
        textStyle.normal.textColor = manager.textColor;
        if (manager.vertexes.Count == 0)
        {
            return;
        }

        for (int j = 0; j < manager.vertexes.Count; j++)
        {
            Handles.Label(vertexes[j].position + targetPos, j.ToString() + "P", textStyle);
        }

        if(manager.editAll)
        {
            for(int i = 0; i < vertexes.Count; ++i)
            {
                SplineUtil.DrawHandles(i, vertexes, targetPos, textStyle, manager);
            }
        }
        else
        {
            int index = manager.editingVertex;
            SplineUtil.DrawHandles(index, vertexes, targetPos, textStyle, manager);
        }

        if (vertexes.Count > 1)
        {
            if (manager.splineMode == SplineMode.Straight)
            {
                for(int i = 0; i < vertexes.Count - 1; ++i)
                {
                    Handles.DrawPolyLine(vertexes[i].position + targetPos, vertexes[i + 1].position + targetPos);
                }
                if (manager.isLoop && vertexes.Count > 2)
                {
                    Handles.DrawPolyLine(vertexes[vertexes.Count - 1].position + targetPos, vertexes[0].position + targetPos);
                }
                //Handles.DrawDottedLines(posArray.ToArray(), 1f);
            }
            else if (manager.splineMode == SplineMode.Bezier || manager.splineMode == SplineMode.OneTangent)
            {
                for (int i = 0; i < vertexes.Count - 1; ++i)
                {
                    SplineUtil.DrawBezier(i, i + 1, vertexes, targetPos, manager);
                }
                if (manager.isLoop && vertexes.Count > 2)
                {
                    SplineUtil.DrawBezier(vertexes.Count - 1, 0, vertexes, targetPos, manager);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SplineManager manager = target as SplineManager;

        if (manager.vertexAdding.Count > 0 && GUILayout.Button("Make spline vertex per GameObject"))
        {
            foreach(Transform _transform in manager.vertexAdding)
            {
                manager.vertexes.Add(new SplineVertex()
                {
                    position = _transform.position,
                    rotation = _transform.rotation,
                    startTangent = _transform.rotation * Vector3.forward,
                    endTangent = _transform.rotation * Vector3.back
                });
            }
            manager.vertexAdding.Clear();
        }
    }
}
#endif