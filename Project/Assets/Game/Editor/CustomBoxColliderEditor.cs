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
        var p = targetCollider.transform.position;
        var halfsize = targetCollider.size * 0.5f;
        var lineVertices = new[]
        {
            new Vector3(p.x + halfsize.x, p.y, p.z + halfsize.y),
            new Vector3(p.x - halfsize.x, p.y, p.z + halfsize.y),
            new Vector3(p.x - halfsize.x, p.y, p.z - halfsize.y),
            new Vector3(p.x + halfsize.x, p.y, p.z - halfsize.y),
            new Vector3(p.x + halfsize.x, p.y, p.z + halfsize.y),
        };
        Handles.DrawPolyLine(lineVertices);
        //Handles.DrawWireDisc(targetCollider.transform.position, Vector3.up, targetCollider.radius);
        if (!_editMode) return;

        var handlers = new[]
        {
            (new Vector3(p.x, p.y, p.z + halfsize.y), Vector3.forward),
            (new Vector3(p.x, p.y, p.z - halfsize.y), Vector3.forward),
            (new Vector3(p.x + halfsize.x, p.y, p.z), Vector3.right),
            (new Vector3(p.x - halfsize.x, p.y, p.z), Vector3.right),
        };

        for (int i = 0; i < handlers.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            _handlerSize = HandleUtility.GetHandleSize(handlers[i].Item1) * 0.05f;
            handlers[i].Item1 = Handles.Slider(handlers[i].Item1, handlers[i].Item2, _handlerSize, Handles.DotHandleCap, 0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetCollider, "Change collider size");
                var diff = p - handlers[i].Item1;
                if (handlers[i].Item2 == Vector3.right) targetCollider.size = new Vector2(Mathf.Abs(diff.x) * 2, targetCollider.size.y);
                else targetCollider.size = new Vector2(targetCollider.size.x, Mathf.Abs(diff.z) * 2);
                break;
            }
        }

    }
}
