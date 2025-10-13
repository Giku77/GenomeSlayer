using UnityEngine;

public class ForceScale : MonoBehaviour
{
    public Vector3 scale = new(0.7f, 0.7f, 0.7f);
    void LateUpdate() => transform.localScale = scale;
}
