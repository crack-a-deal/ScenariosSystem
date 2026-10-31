using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CounterConditionConverterWindow : EditorWindow
{
    // ---- приватные поля условий (доступ через рефлексию) ----

    private static readonly BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly FieldInfo SourceObjectsField = typeof(CounterCondition).GetField("_objects", Flags);
    private static readonly FieldInfo SourceStateField = typeof(CounterCondition).GetField("_requiredState", Flags);
    private static readonly FieldInfo SourceCountField = typeof(CounterCondition).GetField("_requiredCount", Flags);

    private static readonly FieldInfo TargetElementsField = typeof(PerElementCounterCondition).GetField("_elements", Flags);
    private static readonly FieldInfo TargetStateField = typeof(PerElementCounterCondition).GetField("_requiredState", Flags);
    private static readonly FieldInfo TargetCountField = typeof(PerElementCounterCondition).GetField("_requiredCount", Flags);

    private static readonly FieldInfo ElementObjectField = typeof(CounterElement).GetField("_object", Flags);
    private static readonly FieldInfo ElementDisableSelfField = typeof(CounterElement).GetField("_disableSelfOnComplete", Flags);
    private static readonly FieldInfo ElementDisableListField = typeof(CounterElement).GetField("_disableOnComplete", Flags);
    private static readonly FieldInfo ElementEnableListField = typeof(CounterElement).GetField("_enableOnComplete", Flags);

    // ---- черновики конвертации ----

    private class ElementDraft
    {
        public ScenarioStateProvider Object;
        public GameObject Copy; // первая «копия» (включится при выполнении)
        public List<GameObject> DisableAlso = new(); // гайка и т.п.
        public bool Include = true;
    }

    private class FoundCondition
    {
        public Component Owner;          // компонент, держащий SerializeReference
        public string PropertyPath;      // путь до поля условия
        public string Label;            // "GameObject (Component) / путь"
        public ScenarioObjectState RequiredState;
        public int RequiredCount;
        public bool Convert = true;
        public List<ElementDraft> Drafts = new();
    }

    private readonly List<FoundCondition> _found = new();
    private Vector2 _scroll;
    private string _suffixes = "_table, _copy";

    [MenuItem("Tools/Сценарии/Конвертер CounterCondition → PerElement")]
    public static void Open()
    {
        GetWindow<CounterConditionConverterWindow>("Конвертер условий");
    }

    private void OnEnable()
    {
        // Ранний диагноз: если правили имена полей — сразу видно, чего не хватает.
        FieldInfo[] required =
        {
            SourceObjectsField, SourceStateField, SourceCountField,
            TargetElementsField, TargetStateField, TargetCountField,
            ElementObjectField, ElementDisableSelfField, ElementDisableListField, ElementEnableListField
        };

        FieldInfo missing = required.FirstOrDefault(field => field == null);

        if (missing != null)
            Debug.LogError($"[Конвертер] Не найдено поле {missing.Name}. Проверьте, что PerElementCounterCondition и CounterCondition соответствуют ожидаемым именам полей.");
    }

    // ================== СКАНИРОВАНИЕ ==================

    private void ScanStage()
    {
        _found.Clear();

        // Текущая сцена ИЛИ открытый Prefab Mode
        StageHandle stage = StageUtility.GetCurrentStageHandle();
        MonoBehaviour[] components = stage.FindComponentsOfType<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component == null || EditorUtility.IsPersistent(component))
                continue;

            try
            {
                CollectConditions(component);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Конвертер] Пропускаю {component.GetType().Name} на '{component.gameObject.name}': {exception.Message}");
            }
        }

        AutoMatchCopies();
        Repaint();
    }

    /// <summary>Рекурсивно обходит свойства компонента и ищет ManagedReference типа CounterCondition.</summary>
    private void CollectConditions(Component owner)
    {
        SerializedObject serializedObject = new SerializedObject(owner);
        SerializedProperty iterator = serializedObject.GetIterator();

        while (iterator.Next(true))
        {
            if (iterator.propertyType == SerializedPropertyType.ManagedReference &&
                iterator.managedReferenceValue is CounterCondition condition)
            {
                _found.Add(CreateDraft(owner, iterator.propertyPath, condition));
            }
        }
    }

    private static FoundCondition CreateDraft(Component owner, string propertyPath, CounterCondition source)
    {
        var objects = SourceObjectsField.GetValue(source) as List<ScenarioStateProvider>;

        var found = new FoundCondition
        {
            Owner = owner,
            PropertyPath = propertyPath,
            RequiredState = (ScenarioObjectState)SourceStateField.GetValue(source),
            RequiredCount = (int)SourceCountField.GetValue(source),
            Label = $"{owner.gameObject.name} ({owner.GetType().Name})\n{propertyPath}"
        };

        if (objects != null)
        {
            foreach (ScenarioStateProvider scenarioObject in objects)
                found.Drafts.Add(new ElementDraft { Object = scenarioObject });
        }

        return found;
    }

    // ================== АВТОПОДБОР КОПИЙ ==================

    /// <summary>«Bolt_01» + суффикс → «Bolt_01_table». Неоднозначные имена — правьте вручную в окне.</summary>
    private void AutoMatchCopies()
    {
        string[] suffixes = _suffixes
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(suffix => suffix.Trim())
            .Where(suffix => suffix.Length > 0)
            .ToArray();

        if (suffixes.Length == 0)
            return;

        var byName = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

        foreach (Transform transform in StageUtility.GetCurrentStageHandle().FindComponentsOfType<Transform>())
        {
            if (!byName.ContainsKey(transform.name))
                byName[transform.name] = transform.gameObject;
        }

        foreach (FoundCondition found in _found)
        {
            foreach (ElementDraft draft in found.Drafts)
            {
                if (draft.Object == null || draft.Copy != null)
                    continue;

                string objectName = draft.Object.gameObject.name;

                foreach (string suffix in suffixes)
                {
                    if (byName.TryGetValue(objectName + suffix, out GameObject copy))
                    {
                        draft.Copy = copy;
                        break;
                    }
                }
            }
        }
    }

    // ================== КОНВЕРТАЦИЯ ==================

    private static void Apply(FoundCondition found)
    {
        var converted = new PerElementCounterCondition();

        TargetStateField.SetValue(converted, found.RequiredState);
        TargetCountField.SetValue(converted, found.RequiredCount);

        var elements = new List<CounterElement>();

        foreach (ElementDraft draft in found.Drafts.Where(draft => draft.Include))
        {
            var element = new CounterElement();

            ElementObjectField.SetValue(element, draft.Object);
            ElementDisableSelfField.SetValue(element, true);
            ElementDisableListField.SetValue(element, new List<GameObject>(draft.DisableAlso));
            ElementEnableListField.SetValue(element, draft.Copy != null
                ? new List<GameObject> { draft.Copy }
                : new List<GameObject>());

            elements.Add(element);
        }

        TargetElementsField.SetValue(converted, elements);

        var serializedObject = new SerializedObject(found.Owner);
        SerializedProperty property = serializedObject.FindProperty(found.PropertyPath);

        Undo.RecordObject(found.Owner, "Convert CounterCondition → PerElement");
        property.managedReferenceValue = converted;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(found.Owner);
        EditorSceneManager.MarkSceneDirty(found.Owner.gameObject.scene);

        Debug.Log($"[Конвертер] {found.Label} — сконвертировано ({elements.Count} элементов).");
    }

    // ================== GUI ==================

    private void OnGUI()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();

        _suffixes = EditorGUILayout.TextField("Суффиксы копий", _suffixes);

        if (GUILayout.Button("Сканировать сцену", GUILayout.Width(160)))
            ScanStage();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "Находит все CounterCondition в открытой сцене (или Prefab Mode) и готовит из их объектов элементы PerElementCounterCondition. " +
            "Копии подбираются по имени: Bolt_01 → Bolt_01_table (суффиксы выше, через запятую). " +
            "Гайки и другие объекты для совместного выключения добавьте кнопкой «+ выключить вместе» — поле «Выключить вместе с элементом».",
            MessageType.Info);

        if (_found.Count == 0)
        {
            EditorGUILayout.HelpBox("Пока ничего не найдено — откройте сцену со шагами и нажмите «Сканировать сцену».", MessageType.None);
            return;
        }

        EditorGUILayout.Space(2);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (FoundCondition found in _found)
            DrawFound(found);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);

        if (GUILayout.Button("Конвертировать отмеченные", GUILayout.Height(30)))
        {
            foreach (FoundCondition found in _found.Where(found => found.Convert).ToList())
                Apply(found);

            ScanStage(); // старых CounterCondition больше нет — список опустеет
        }
    }

    private void DrawFound(FoundCondition found)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        found.Convert = EditorGUILayout.ToggleLeft("Конвертировать", found.Convert, GUILayout.Width(120));
        EditorGUILayout.LabelField(found.Label, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(
            $"Состояние: {found.RequiredState} · Нужно: {found.RequiredCount} · Элементов: {found.Drafts.Count}",
            EditorStyles.miniLabel);

        if (GUILayout.Button("Переподобрать копии", GUILayout.Width(190)))
        {
            foreach (ElementDraft draft in found.Drafts)
                draft.Copy = null;

            AutoMatchCopies();
        }

        foreach (ElementDraft draft in found.Drafts)
            DrawDraft(draft);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4);
    }

    private static void DrawDraft(ElementDraft draft)
    {
        EditorGUILayout.BeginHorizontal();

        draft.Include = EditorGUILayout.Toggle(draft.Include, GUILayout.Width(16));

        string objectName = draft.Object != null ? draft.Object.gameObject.name : "(пусто)";
        EditorGUILayout.LabelField(objectName, GUILayout.Width(150));

        draft.Copy = (GameObject)EditorGUILayout.ObjectField(
            "копия", draft.Copy, typeof(GameObject), true);

        EditorGUILayout.EndHorizontal();

        // «выключить вместе с элементом» — гайка и т.п.
        for (int i = draft.DisableAlso.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);

            draft.DisableAlso[i] = (GameObject)EditorGUILayout.ObjectField(
                "выключить тоже", draft.DisableAlso[i], typeof(GameObject), true);

            if (GUILayout.Button("×", GUILayout.Width(22)))
                draft.DisableAlso.RemoveAt(i);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);

        if (GUILayout.Button("+ выключить вместе (гайка и т.п.)", GUILayout.Width(240)))
            draft.DisableAlso.Add(null);

        EditorGUILayout.EndHorizontal();
    }
}