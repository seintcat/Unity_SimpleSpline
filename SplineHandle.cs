#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(SplineManager)), CanEditMultipleObjects]
public class SplineHandle : Editor
{
    SplineManager handleObj;
    List<SplineVertex> vertexes;
    Vector3 targetPos;
    GUIStyle textStyle = new GUIStyle();

    void OnSceneGUI()
    {
        handleObj = target as SplineManager;
        if(handleObj.vertexes == null || handleObj.vertexes.Count < 1)
        {
            return;
        }

        vertexes = handleObj.vertexes;
        targetPos = handleObj.transform.position;
        Handles.color = handleObj.lineColor;
        textStyle.normal.textColor = handleObj.textColor;
        if (handleObj.vertexes.Count == 0)
        {
            return;
        }

        for (int j = 0; j < handleObj.vertexes.Count; j++)
        {
            Handles.Label(vertexes[j].position + targetPos, j.ToString() + "P", textStyle);
        }

        if(handleObj.editAll)
        {
            for(int i = 0; i < vertexes.Count; ++i)
            {
                SplineUtil.DrawHandles(i, vertexes, targetPos, textStyle, handleObj);
            }
        }
        else
        {
            int index = handleObj.editingVertex;
            SplineUtil.DrawHandles(index, vertexes, targetPos, textStyle, handleObj);
        }

        if (vertexes.Count > 1)
        {
            if (handleObj.splineMode == SplineMode.Straight)
            {
                for(int i = 0; i < vertexes.Count - 1; ++i)
                {
                    Handles.DrawPolyLine(vertexes[i].position + targetPos, vertexes[i + 1].position + targetPos);
                }
                if (handleObj.isLoop && vertexes.Count > 2)
                {
                    Handles.DrawPolyLine(vertexes[vertexes.Count - 1].position + targetPos, vertexes[0].position + targetPos);
                }
                //Handles.DrawDottedLines(posArray.ToArray(), 1f);
            }
            else if (handleObj.splineMode == SplineMode.Bezier)
            {
                for (int i = 0; i < vertexes.Count - 1; ++i)
                {
                    SplineUtil.DrawBezier(i, i + 1, vertexes, targetPos, (ISplineObject)handleObj);
                }
                if (handleObj.isLoop && vertexes.Count > 2)
                {
                    SplineUtil.DrawBezier(vertexes.Count - 1, 0, vertexes, targetPos, (ISplineObject)handleObj);
                }
            }
        }
    }
}
#endif