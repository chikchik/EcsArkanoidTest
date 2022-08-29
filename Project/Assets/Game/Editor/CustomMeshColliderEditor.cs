using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolygonCollider2D))]

public class CustomMeshColliderEditor : Editor
{
    private bool _editMode;
    private bool _removeMode;
    private PolygonCollider2D _target;
    private int _activePathIndex;
    private int _activeVerticeIndex;
    private int _activeEdgeVerticeIndex;
    private Vector2 _dissectionPoint;
    private Quaternion _rotation;

    private const float _distanceThreshold = 1f;
    private const float _verticeSelectTreshold = 0.15f;

    public override void OnInspectorGUI()
    {
        _editMode = GUILayout.Toggle(_editMode, "Edit", "Button");
        DrawDefaultInspector();
    }

    public void OnSceneGUI()
    {
        _target = target as PolygonCollider2D;
        if (_target == null) return;

        var rotEuler = _target.transform.rotation.eulerAngles;
        _rotation = Quaternion.Euler(new Vector3(0f, rotEuler.y, 0f));
        DrawColliderShapes();

        if (!_editMode) return;
        if (Event.current.type == EventType.Layout) HandleUtility.AddDefaultControl(0);
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftControl) _removeMode = true;
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl) _removeMode = false;
        DetectActiveSelection();
        SceneView.RepaintAll();
        DrawHandlers();
    }

    private void DrawColliderShapes()
    {
        Handles.color = _editMode ? Color.green : Color.green * 0.7f;
        for (var i = 0; i < _target.pathCount; i++)
        {
            var lineVerts = _target.GetPath(i).Select(FromColliderSpace).ToList();
            lineVerts.Add(lineVerts[0]);
            Handles.DrawPolyLine(lineVerts.ToArray());
        }
    }

    private void DrawHandlers()
    {
        if (_activePathIndex == -1) return;

        var path = _target.GetPath(_activePathIndex);
        var activeVert = path[_activeVerticeIndex];
        if (_activeEdgeVerticeIndex == -1)
        {
            if (Event.current.type == EventType.MouseDown && _removeMode)
            {
                RemoveVertice();
                return;
            }
            EditorGUI.BeginChangeCheck();
            var handlePos = FromColliderSpace(activeVert);
            handlePos = Handles.FreeMoveHandle(handlePos, _rotation, HandleUtility.GetHandleSize(handlePos) * 0.05f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_target, "Change collider shape");
                path[_activeVerticeIndex] = ToColliderSpace(handlePos);
                _target.SetPath(_activePathIndex, path);
            }
            return;
        }

        var activeEdgeVert = path[_activeEdgeVerticeIndex];
        Handles.DrawLine(FromColliderSpace(activeVert), FromColliderSpace(activeEdgeVert), 3f);
        var dissection = FromColliderSpace(_dissectionPoint);
        Handles.DotHandleCap(0, dissection, Quaternion.identity, HandleUtility.GetHandleSize(dissection) * 0.05f, EventType.Repaint);

        if (Event.current.type == EventType.MouseDown && !_removeMode)
        {
            DissectEdge();
            Event.current = null;
        }
    }

    private void DissectEdge()
    {
        var path = _target.GetPath(_activePathIndex);
        var newPath = new Vector2[path.Length + 1];
        var shift = 0;
        var ind = Mathf.Min(_activeEdgeVerticeIndex, _activeVerticeIndex);
        var newPos = ind + 1;
        if ((_activeVerticeIndex == 0 && _activeEdgeVerticeIndex == path.Length - 1) || (_activeVerticeIndex == path.Length - 1 && _activeEdgeVerticeIndex == 0))
        {
            newPos = -1; //special case, add vertice at end
        }

        for (var i = 0; i < path.Length; i++)
        {
            if (i == newPos)
            {
                newPath[i] = _dissectionPoint;
                shift = 1;
            }
            newPath[i + shift] = path[i];
        }

        if (newPos == -1) newPath[newPath.Length - 1] = _dissectionPoint;

        Undo.RecordObject(_target, "Add collider vertice");
        _target.SetPath(_activePathIndex, newPath);
    }

    private void RemoveVertice()
    {
        var shift = 0;
        var path = _target.GetPath(_activePathIndex);
        var newPath = new Vector2[path.Length - 1];
        for (var i = 0; i < path.Length; i++)
        {
            if (i == _activeVerticeIndex)
            {
                shift = -1;
                continue;
            }

            newPath[i + shift] = path[i];
        }
        Undo.RecordObject(_target, "Delete collider vertice");
        _target.SetPath(_activePathIndex, newPath);
    }

    private void DetectActiveSelection()
    {
        var plane = new Plane(Vector3.up, _target.transform.position);
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (!plane.Raycast(ray, out var distance))
        {
            _activePathIndex = -1;
            return;
        }

        var mouse2d = ToColliderSpace(ray.GetPoint(distance));
        var nearest = GetNearestVertice(mouse2d, out distance);
        _activePathIndex = nearest.x;
        _activeVerticeIndex = nearest.y;

        var path = _target.GetPath(_activePathIndex);

        //check edge to prev vert
        var i = _activeVerticeIndex == 0 ? path.Length - 1 : _activeVerticeIndex - 1;
        if (CheckAngle(path[_activeVerticeIndex], path[i], mouse2d, out _dissectionPoint))
        {
            _activeEdgeVerticeIndex = i;
            return;
        }

        //check edge to next vert
        i = _activeVerticeIndex == path.Length - 1 ? 0 : _activeVerticeIndex + 1;
        if (CheckAngle(path[_activeVerticeIndex], path[i], mouse2d, out _dissectionPoint))
        {
            _activeEdgeVerticeIndex = i;
            return;
        }

        _activeEdgeVerticeIndex = -1;

    }

    private static bool CheckAngle(Vector2 vert, Vector2 edgevert, Vector2 mouse, out Vector2 cross)
    {
        var edge = edgevert - vert;
        var mouseDir = mouse - vert;
        var dot = Vector2.Dot(edge.normalized, mouseDir);
        if (dot < _verticeSelectTreshold || dot > edge.magnitude)
        {
            cross = Vector2.zero;
            return false;
        }

        cross = (edge.normalized * dot) + vert;
        return true;
    }

    private Vector2Int GetNearestVertice(Vector2 point, out float distance)
    {
        var nearest = new Vector2Int(-1, -1);
        distance = float.MaxValue;
        for (var i = 0; i < _target.pathCount; i++)
        {
            var path = _target.GetPath(i);
            for (var j = 0; j < path.Length; j++)
            {
                var tempDistance = (point - path[j]).magnitude;
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    nearest = new Vector2Int(i, j);
                }
            }
        }

        return nearest;
    }

    private Vector2 ToColliderSpace(Vector3 vector)
    {
        var loc = Quaternion.Inverse(_rotation) * (vector - _target.transform.position);
        return new Vector2(loc.x, loc.z);
    }

    private Vector3 FromColliderSpace(Vector2 vector)
    {
        var res = _rotation * new Vector3(vector.x, 0f, vector.y) + _target.transform.position;
        return res;
    }
}
