using UnityEditor;
using UnityEngine;

public class FixPivot : MonoBehaviour
{
    [MenuItem("Tools/Center Pivot")]
    static void CenterPivot()
    {
        var meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        Vector3 offset = mesh.bounds.center;
        mesh.RecalculateBounds();
        var vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] -= offset;
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        meshFilter.transform.position += offset;
    }
}
