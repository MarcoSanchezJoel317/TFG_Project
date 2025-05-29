using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] Transform trackedTransform;
    void Update()
    {
        transform.position = trackedTransform.position;
    }
}
