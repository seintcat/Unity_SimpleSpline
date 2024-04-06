using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineFollowerWithRatio : MonoBehaviour
{
    public SplineManager spline;
    public float ratio;
    public float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ratio += Time.deltaTime * speed;
        Transform newTransform = spline.LineFollowRatio(transform, ratio);
        transform.SetPositionAndRotation(newTransform.position, newTransform.rotation);
    }
}
