using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoxCollider2D))]
public class CustomBoxColliderEditor : Editor
{
    private bool _editMode;
    private float _handlerSize;

    public override void OnInspectorGUI()
    {
        _editMode = GUILayout.Toggle(_editMode, "Edit", "Button");
        DrawDefaultInspector();
    }

    public void OnSceneGUI()
    {
        var targetCollider = target as BoxCollider2D;
        if (!targetCollider.enabled) return;

        Handles.color = _editMode ? Color.green : Color.green * 0.7f;
        var pos = targetCollider.transform.position;
        var rot = targetCollider.transform.rotation;
        var halfsize = targetCollider.size * 0.5f;
        var lineVertices = new[]
        {
            rot * new Vector3(halfsize.x, 0f, halfsize.y) + pos,
            rot * new Vector3(-halfsize.x, 0f, halfsize.y) + pos,
            rot * new Vector3(-halfsize.x, 0f, -halfsize.y) + pos,
            rot * new Vector3(halfsize.x, 0f, -halfsize.y) + pos,
            rot * new Vector3(halfsize.x, 0f, halfsize.y) + pos,
        };
        Handles.DrawPolyLine(lineVertices);
        if (!_editMode) return;

        var handlers = new[]
        {
            (rot * new Vector3(0f, 0f, halfsize.y) + pos, Vector3.forward),
            (rot * new Vector3(0f, 0f, -halfsize.y) + pos, Vector3.forward),
            (rot * new Vector3(halfsize.x, 0f, 0f) + pos, Vector3.right),
            (rot * new Vector3(-halfsize.x, 0f, 0f) + pos, Vector3.right),
        };

        for (int i = 0; i < handlers.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            _handlerSize = HandleUtility.GetHandleSize(handlers[i].Item1) * 0.05f;
            handlers[i].Item1 = Handles.Slider(handlers[i].Item1, rot * handlers[i].Item2, _handlerSize, Handles.DotHandleCap, 0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetCollider, "Change collider size");
                var diff = pos - handlers[i].Item1;
                targetCollider.size = handlers[i].Item2 == Vector3.right ? new Vector2(diff.magnitude * 2, targetCollider.size.y) : new Vector2(targetCollider.size.x, diff.magnitude * 2);
                break;
            }
        }

    }
}
