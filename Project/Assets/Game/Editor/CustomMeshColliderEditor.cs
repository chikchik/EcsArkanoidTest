using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolygonCollider2D))]

public class CustomMeshColliderEditor : Editor
{
    private bool _editMode;
    private PolygonCollider2D _target;
    private Vector3 _mouseProjection;
    private int _activePathIndex;
    private int _activeVerticeIndex;
    private int _activeEdgeVerticeIndex;
    private Vector3 _dissectionPoint;
    private Quaternion _rotation;

    private const float _distanceThreshold = 1f;

    public override void OnInspectorGUI()
    {
        _editMode = GUILayout.Toggle(_editMode, "Edit", "Button");
        DrawDefaultInspector();
    }

    public void OnSceneGUI()
    {
        Debug.Log(Event.current.type);
        _target = target as PolygonCollider2D;
        DrawColliderShapes();

        if (!_editMode) return;
        if (Event.current.type == EventType.MouseMove) DetectActiveSelection();
        DrawHandlers();
    }

    private void DrawColliderShapes()
    {
        Handles.color = _editMode ? Color.green : Color.green * 0.7f;
        var pos = _target.transform.position;
        var rotEuler = _target.transform.rotation.eulerAngles;
        _rotation = Quaternion.Euler(new Vector3(0f, rotEuler.y, 0f));
        for (var i = 0; i < _target.pathCount; i++)
        {
            var lineVerts = _target.GetPath(i).Select(vert => _rotation * new Vector3(vert.x, 0f, vert.y) + pos).ToList();
            var first = lineVerts[0];
            lineVerts.Add(first);
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
            EditorGUI.BeginChangeCheck();
            var handlePos = _rotation * new Vector3(path[_activeVerticeIndex].x, 0f, path[_activeVerticeIndex].y) + _target.transform.position;
            handlePos = Handles.FreeMoveHandle(handlePos, _rotation, 0.2f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_target, "Change collider shape");
                var loc = Quaternion.Inverse(_rotation) * (handlePos - _target.transform.position);
                path[_activeVerticeIndex] = new Vector2(loc.x, loc.z);
                _target.SetPath(_activePathIndex, path);
            }
            return;
        }

        var activeEdgeVert = path[_activeEdgeVerticeIndex];
        Handles.DrawLine(_rotation * new Vector3(activeVert.x, 0f, activeVert.y) + _target.transform.position,
            _rotation * new Vector3(activeEdgeVert.x, 0f, activeEdgeVert.y) + _target.transform.position, 10f);
        Handles.DotHandleCap(0, _dissectionPoint, Quaternion.identity, 0.2f, EventType.Repaint);
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

        _mouseProjection = Quaternion.Inverse(_rotation) * (ray.GetPoint(distance) - _target.transform.position);
        var mouse2d = new Vector2(_mouseProjection.x, _mouseProjection.z);
        //var mouse3d = _rotation * new Vector3(mouse2d.x, 0f, mouse2d.y);
        //Handles.DrawWireDisc(_target.transform.position + mouse3d, Vector3.up, 0.5f);
        var nearest = GetNearestVertice(mouse2d, out distance);
        _activePathIndex = nearest.x;
        _activeVerticeIndex = nearest.y;

        var path = _target.GetPath(_activePathIndex);
        Vector2 cross;

        //check edge to prev vert
        var i = _activeVerticeIndex == 0 ? path.Length - 1 : _activeVerticeIndex - 1;
        if (CheckAngle(path[_activeVerticeIndex], path[i], mouse2d, out cross))
        {
            _activeEdgeVerticeIndex = i;
            _dissectionPoint = _rotation * new Vector3(cross.x, 0f, cross.y) + _target.transform.position;
            return;
        }

        //check edge to next vert
        i = _activeVerticeIndex == path.Length - 1 ? 0 : _activeVerticeIndex + 1;
        if (CheckAngle(path[_activeVerticeIndex], path[i], mouse2d, out cross))
        {
            _activeEdgeVerticeIndex = i;
            _dissectionPoint = _rotation * new Vector3(cross.x, 0f, cross.y) + _target.transform.position;
            return;
        }

        _activeEdgeVerticeIndex = -1;
        return;

    }

    private static bool CheckAngle(Vector2 vert, Vector2 edgevert, Vector2 mouse, out Vector2 cross)
    {
        var edge = edgevert - vert;
        var mouseDir = mouse - vert;
        var dot = Vector2.Dot(edge.normalized, mouseDir);
        if (dot < 0 || dot > edge.magnitude)
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
}
