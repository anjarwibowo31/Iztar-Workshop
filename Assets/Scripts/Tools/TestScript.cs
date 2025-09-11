using Sirenix.OdinInspector;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private string layer;

    [Button]
    private void CheckLayer()
    {
        Debug.Log(LayerMask.NameToLayer(layer));
    }
} 
