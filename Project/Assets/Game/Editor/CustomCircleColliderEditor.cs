using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CircleCollider2D))]
public class CustomCircleColliderEditor : Editor
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
            var targetCollider = target as CircleCollider2D;
            if (!targetCollider.enabled) return;

            Handles.color = _editMode ? Color.green : Color.green * 0.7f;
            Handles.DrawWireDisc(targetCollider.transform.position, Vector3.up, targetCollider.radius);
            if (!_editMode) return;

            var p = targetCollider.transform.position;
            var handlers = new[]
            {
                (new Vector3(p.x, p.y, p.z + targetCollider.radius), Vector3.forward),
                (new Vector3(p.x, p.y, p.z - targetCollider.radius), Vector3.back),
                (new Vector3(p.x + targetCollider.radius, p.y, p.z), Vector3.left),
                (new Vector3(p.x - targetCollider.radius, p.y, p.z), Vector3.right),
            };

            for(int i=0; i<handlers.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                _handlerSize = HandleUtility.GetHandleSize(handlers[i].Item1) * 0.05f;
                handlers[i].Item1 = Handles.Slider(handlers[i].Item1, handlers[i].Item2, _handlerSize, Handles.DotHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetCollider, "Change collider radius");
                    targetCollider.radius = (p - handlers[i].Item1).magnitude;
                    break;
                }
            }

    }
}
