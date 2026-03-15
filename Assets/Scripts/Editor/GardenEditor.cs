using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Garden))]
public class GardenEditor : Editor
{
    private string _lastReport = string.Empty;
    private MessageType _lastReportType = MessageType.Info;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        if (GUILayout.Button("Verify Garden"))
        {
            var garden = (Garden)target;
            var isValid = garden.VerifyGarden(out _lastReport);
            _lastReportType = isValid ? MessageType.Info : MessageType.Warning;

            EditorUtility.SetDirty(garden);
        }

        if (!string.IsNullOrWhiteSpace(_lastReport))
        {
            EditorGUILayout.HelpBox(_lastReport, _lastReportType);
        }
    }
}
