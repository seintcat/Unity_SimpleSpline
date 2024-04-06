using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineFollowerWithDistance : MonoBehaviour
{
    public SplineManager spline;
    public float distance;
    public float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distance += Time.deltaTime * speed;
        Transform newTransform = spline.LineFollowDistance(transform, distance);
        transform.SetPositionAndRotation(newTransform.position, newTransform.rotation);
    }
}
