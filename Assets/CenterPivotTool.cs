using UnityEditor;
using UnityEngine;

public static class CenterPivotTool
{
    [MenuItem("Tools/Center Pivot (Selected Only)")]
    static void CenterPivot()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("Сначала выберите объект.");
            return;
        }

        var meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogWarning("На объекте нет MeshFilter.");
            return;
        }

        // Копируем меш — оригинальный ассет остаётся нетронутым
        Mesh originalMesh = meshFilter.sharedMesh;
        Mesh meshCopy = Object.Instantiate(originalMesh);
        meshCopy.name = originalMesh.name + "_Centered";

        // Сдвигаем вершины так, чтобы центр bounds оказался в нуле
        Vector3 offset = meshCopy.bounds.center;
        Vector3[] vertices = meshCopy.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] -= offset;
        meshCopy.vertices = vertices;
        meshCopy.RecalculateBounds();

        // Присваиваем копию только выбранному объекту
        meshFilter.mesh = meshCopy;

        // Компенсируем смещение, чтобы модель не визуально не прыгнула
        // (с учётом масштаба объекта)
        go.transform.position += offset * go.transform.localScale.x;

        Debug.Log($"Pivot отцентрирован для '{go.name}'. Оригинальный меш не изменён.");
    }
}

public static class MeshFinder
{
    [MenuItem("Tools/Find mesh 'model_0'")]
    static void FindMesh()
    {
        // 1. Ищем среди ассетов проекта
        string[] guids = AssetDatabase.FindAssets("t:ModelImporter");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath(path))
                if (obj is Mesh m && m.name == "model_0")
                    Debug.Log($"Меш найден в ассете: {path}", obj);
        }

        // 2. Ищем среди объектов сцены (включая неактивные)
        var filters = Object.FindObjectsByType<MeshFilter>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mf in filters)
            if (mf.sharedMesh != null && mf.sharedMesh.name == "model_0")
                Debug.Log($"Меш используется объектом: {mf.gameObject.name}", mf.gameObject);
    }
}