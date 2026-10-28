using UnityEngine;

public class LockRotationMinimap : MonoBehaviour
{
    public Vector3 rotacaoFixa = Vector3.zero;

    void LateUpdate()
    {
        transform.eulerAngles = rotacaoFixa;
    }
}
