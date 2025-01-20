using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SplineManager : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Editor")]
    public int editingVertex;
    public bool editAll;
    public Color lineColor = Color.black;
    public Color textColor = Color.black;
    public SplineVertexHandleMode handleMode;
    public List<Transform> vertexAdding;
#endif

    [Header("Core")]
    public bool accurateDistance;
    public bool isLoop;
    public bool followerClamping;
    public SplineMode splineMode;
    public List<SplineVertex> vertexes;

    private List<float> splineDistances;

    // Start is called before the first frame update
    void Start()
    {
        if(accurateDistance)
        {
            SetDistance();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform OneLineFollowRatio(Transform _transform, int index, float ratio)
    {
        int start = index;
        int end = index + 1;
        if (index == vertexes.Count - 1 && isLoop)
        {
            end = 0;
        }

        _transform.position = transform.position;

        if (splineMode == SplineMode.Straight)
        {
            _transform.position += Vector3.Lerp(vertexes[start].position, vertexes[end].position, ratio);
        }
        else if(splineMode == SplineMode.Bezier || splineMode == SplineMode.OneTangent)
        {
            List<Vector3> CasteljauList = new List<Vector3>() { 
                vertexes[start].position, 
                vertexes[start].startTangent + vertexes[start].position, 
                vertexes[end].endTangent + vertexes[end].position, 
                vertexes[end].position };
            while (CasteljauList.Count > 1)
            {
                for(int i = 0; i < CasteljauList.Count - 1; ++i)
                {
                    CasteljauList[i] = Vector3.Lerp(CasteljauList[i], CasteljauList[i + 1], ratio);
                }
                CasteljauList.RemoveAt(CasteljauList.Count - 1);
            }
            _transform.position += CasteljauList[0];
        }

        _transform.rotation = Quaternion.Lerp(vertexes[start].rotation.normalized, vertexes[end].rotation.normalized, ratio);
        return _transform;
    }
    public Transform LineFollowRatio(Transform _transform, float ratio)
    {
        Dictionary<int, float> lineDict = GetAllLineDistance();
        float distance = 0f;
        foreach (float oneLineDistance in lineDict.Values)
        {
            distance += oneLineDistance;
        }

        if (followerClamping)
        {
            ratio = Mathf.Clamp(ratio, 0, 1);
        }
        else
        {
            ratio -= Mathf.Floor(ratio);
        }
        bool inputNonZero = ratio != 0;

        distance *= ratio;
        int index = 0;
        int vertexesCount = vertexes.Count;
        if(!isLoop)
        {
            vertexesCount -= 1;
        }
        for (int i = 0; i < vertexesCount; ++i)
        {
            if (distance < lineDict[i])
            {
                index = i;
                break;
            }
            else
            {
                distance -= lineDict[i];
            }
        }
        if(inputNonZero && index == 0 && distance == 0)
        {
            index = vertexesCount - 1;
            distance = lineDict[index];
        }

        return OneLineFollowRatio(_transform, index, distance / lineDict[index]);
    }
    public Transform LineFollowDistance(Transform _transform, float distance)
    {
        Dictionary<int, float> lineDict = GetAllLineDistance();
        float allDistance = 0f;
        foreach (float oneLineDistance in lineDict.Values)
        {
            allDistance += oneLineDistance;
        }

        if (followerClamping)
        {
            distance = Mathf.Clamp(distance, 0, allDistance);
        }
        else
        {
            if (distance > allDistance)
            {
                int tmpValue = (int)(distance / allDistance);
                distance -= tmpValue * allDistance;
            }
        }
        bool inputNonZero = distance != 0;

        int index = 0;
        int vertexesCount = vertexes.Count;
        if (!isLoop)
        {
            vertexesCount -= 1;
        }
        for (int i = 0; i < vertexesCount; ++i)
        {
            if (distance < lineDict[i])
            {
                index = i;
                break;
            }
            else
            {
                distance -= lineDict[i];
            }
        }
        if (inputNonZero && index == 0 && distance == 0)
        {
            index = vertexesCount - 1;
            distance = lineDict[index];
        }

        return OneLineFollowRatio(_transform, index, distance / lineDict[index]);
    }

    private Dictionary<int, float> GetAllLineDistance()
    {
        Dictionary<int, float> lineDict = new Dictionary<int, float>();
        int vertexesCount = vertexes.Count;
        if (!isLoop)
        {
            vertexesCount -= 1;
        }
        for (int i = 0; i < vertexesCount; ++i)
        {
            lineDict.Add(i, GetLineDistance(i));
        }
        return lineDict;
    }
    private float GetLineDistance(int index)
    {
        int start = index;
        int end = index + 1;

        if(index == vertexes.Count - 1 && isLoop)
        {
            end = 0;
        }
        else if(index > vertexes.Count - 2)
        {
            return 0f;
        }

        if (splineMode == SplineMode.Straight)
        {
            return (vertexes[start].position - vertexes[end].position).magnitude;
        }
        else if(splineMode == SplineMode.Bezier || splineMode == SplineMode.OneTangent)
        {
            if (accurateDistance)
            {
                return splineDistances[start];
            }
            else
            {
                List<Vector3> CasteljauList = new List<Vector3>() {
                vertexes[start].position,
                vertexes[start].startTangent + vertexes[start].position,
                vertexes[end].endTangent + vertexes[end].position,
                vertexes[end].position };
                List<Vector3> lerpBezierLine = new List<Vector3>() {
                vertexes[start].position,
                vertexes[start].startTangent + vertexes[start].position,
                vertexes[end].endTangent + vertexes[end].position,
                vertexes[end].position };
                //List<float> CasteljauDistances = new List<float>();
                float distancePredict = float.MaxValue;
                while (CasteljauList.Count > 1)
                {
                    List<Vector3> newLerpBezierLinePart = new List<Vector3>();
                    List<Vector3> newLerpBezierLine = new List<Vector3>();

                    // Make Casteljau Bezier dot
                    for (int i = 0; i < CasteljauList.Count - 1; ++i)
                    {
                        CasteljauList[i] = Vector3.Lerp(CasteljauList[i], CasteljauList[i + 1], 0.5f);
                        newLerpBezierLinePart.Add(CasteljauList[i]);
                    }
                    CasteljauList.RemoveAt(CasteljauList.Count - 1);

                    // Calculate last Bezier path distance
                    float newDistancePredict = 0f;
                    for (int i = 0; i < lerpBezierLine.Count - 1; ++i)
                    {
                        newDistancePredict += (lerpBezierLine[i] - lerpBezierLine[i + 1]).magnitude;
                    }
                    if (newDistancePredict < distancePredict)
                    {
                        distancePredict = newDistancePredict;
                    }

                    // Make advanced Bezier path
                    int middle = lerpBezierLine.Count / 2;
                    if ((lerpBezierLine.Count % 2) == 1)
                    {
                        for (int i = 0; i < middle; ++i)
                        {
                            newLerpBezierLine.Add(lerpBezierLine[i]);
                        }
                        foreach (Vector3 pos in newLerpBezierLinePart)
                        {
                            newLerpBezierLine.Add(pos);
                        }
                        for (int i = lerpBezierLine.Count - 1; i > middle; --i)
                        {
                            newLerpBezierLine.Add(lerpBezierLine[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < middle - 1; ++i)
                        {
                            newLerpBezierLine.Add(lerpBezierLine[i]);
                        }
                        foreach (Vector3 pos in newLerpBezierLinePart)
                        {
                            newLerpBezierLine.Add(pos);
                        }
                        for (int i = lerpBezierLine.Count - 1; i > middle; --i)
                        {
                            newLerpBezierLine.Add(lerpBezierLine[i]);
                        }
                    }
                    lerpBezierLine.Clear();
                    lerpBezierLine = newLerpBezierLine;
                }
                return distancePredict;
            }
        }

        return 0f;
    }

    public void SetVertex(int index, SplineVertex vertex)
    {
        vertexes[index] = vertex;
    }

    private void SetDistance()
    {
        splineDistances = new List<float>();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if(handleMode != SplineVertexHandleMode.None && Tools.current != Tool.None)
        {
            Tools.current = Tool.None;
        }
#endif
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        foreach(SplineVertex vertex in vertexes)
        {
            Gizmos.color = lineColor;
            Gizmos.DrawRay(transform.position + vertex.position, vertex.rotation * Vector3.forward);
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position + vertex.position, vertex.rotation * Vector3.up);
        }
    }
#endif
}

#if UNITY_EDITOR
public enum SplineVertexHandleMode
{
    None = 0,
    MoveVertex = 1,
    MoveTangent = 2,
    Rotate = 3
}
#endif

[Serializable]
public struct SplineVertex
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 startTangent;
    public Vector3 endTangent;
}

public enum SplineMode
{
    Straight = 0,
    Bezier = 1,
    OneTangent = 2,
}

