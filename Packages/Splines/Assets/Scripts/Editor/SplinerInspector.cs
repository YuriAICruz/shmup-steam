using UnityEngine;
using UnityEditor;

namespace Splines
{
    [CustomEditor(typeof(Spliner))]
    public class SplinerInspector : Editor
    {
        private Spliner _self;
        private bool _canEdit;
        private int _selectedIndex;
        private const float _pickSize = 0.06f;
        private const float _handleSize = 0.04f;

        private void Awake()
        {
            _self = target as Spliner;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_self.Curve == null || !_self.Curve.IsInitialized)
            {
                _self.Curve = new Spline(Vector3.left, Vector3.one, Vector3.forward, Vector3.right);

                SceneView.RepaintAll();
            }
            else
            {
                var loop = _self.Curve.GetLoop();
                var loopMod = EditorGUILayout.Toggle("Loop:", loop);
                if (loop != loopMod)
                {
                    Undo.RecordObject(_self, "Set Loop");
                    EditorUtility.SetDirty(_self);

                    _self.Curve.SetLoop(loopMod);
                    SceneView.RepaintAll();
                }

                EditorGUILayout.Space();

                if (_canEdit)
                {
                    EditGui();
                    if (GUILayout.Button("Done"))
                    {
                        _canEdit = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Edit"))
                    {
                        _canEdit = true;
                    }
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Clear"))
                {
                    Clear();
                }
            }
        }

        private void EditGui()
        {
            if (_selectedIndex % 3 == 1 || _selectedIndex % 3 == 2)
            {
                EditorGUILayout.LabelField("Handle");

                var mode = _self.Curve.GetMode(_selectedIndex);
                var modeMod = (SplinePointMode) EditorGUILayout.EnumPopup("Mode:", mode);
                if (mode != modeMod)
                {
                    Undo.RecordObject(_self, "Set Mode");
                    EditorUtility.SetDirty(_self);

                    _self.Curve.SetMode(_selectedIndex, modeMod);

                    SceneView.RepaintAll();
                }

                ShowPosition();
            }
            else
            {
                EditorGUILayout.LabelField("Control Point");

                if (_selectedIndex < _self.Curve.Count() - 1)
                {
                    if (GUILayout.Button("Add Here"))
                    {
                        _self.Curve.Add(_selectedIndex);
                        SceneView.RepaintAll();
                    }
                }

                ShowPosition();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add"))
            {
                _self.Curve.Add();
                SceneView.RepaintAll();
            }
        }

        private void ShowPosition()
        {
            var pt = _self.Curve.GetPoint(_selectedIndex);
            var point = EditorGUILayout.Vector3Field("Position:", pt);
            if (pt != point)
            {
                Undo.RecordObject(_self, "Move Point");
                EditorUtility.SetDirty(_self);

                _self.Curve.SetPoint(_selectedIndex, point);

                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            if (_self.Curve == null) return;

            Draw();
        }

        private void Clear()
        {
            _self.Curve = null;
        }

        private void Draw()
        {
            DrawCurve();

            CreateHandles();
        }

        private void CreateHandles()
        {
            var handleRotation = Tools.pivotRotation == PivotRotation.Local ? _self.transform.rotation : Quaternion.identity;

            for (int i = 0, n = _self.Curve.Count() - 1; i < n; i += 3)
            {
                SetButtonPositionHandle(i, handleRotation, Handles.CubeHandleCap);
                SetButtonPositionHandle(i + 3, handleRotation, Handles.CubeHandleCap);
//                SetPositionHandle(i, handleRotation);
//                SetPositionHandle(i + 3, handleRotation);

                SetButtonPositionHandle(i + 1, handleRotation, Handles.SphereHandleCap);
                SetButtonPositionHandle(i + 2, handleRotation, Handles.SphereHandleCap);
            }
        }

        private void DrawCurve()
        {
            for (int i = 1, n = _self.Curve.Count(); i < n; i += 3)
            {
                Handles.DrawBezier(_self.Curve.GetPoint(i - 1), _self.Curve.GetPoint(i + 2), _self.Curve.GetPoint(i), _self.Curve.GetPoint(i + 1), Color.gray, null, _self.Width);

                Handles.color = Color.gray;
                Handles.DrawLine(_self.Curve.GetPoint(i - 1), _self.Curve.GetPoint(i));
                Handles.DrawLine(_self.Curve.GetPoint(i + 1), _self.Curve.GetPoint(i + 2));
            }
            DrawSteped();
        }

        private void DrawSteped()
        {
            Handles.color = _self.Color;
            var lineStart = _self.Curve.GetPointOnCurveTransformed(0f, _self.transform);
            if (EditorApplication.isPlaying)
                lineStart = _self.Curve.GetPointOnCurve(0f);

            for (int i = 1, n = 50; i <= n; i++)
            {
                var ext = i / (float) n;

                if (ext > _self.Limit)
                    break;

                var lineEnd = _self.Curve.GetPointOnCurveTransformed(ext, _self.transform);
                if (EditorApplication.isPlaying)
                    lineEnd = _self.Curve.GetPointOnCurve(ext);

                Handles.DrawLine(lineStart, lineEnd);
                lineStart = lineEnd;
            }
        }

        private void SetButtonPositionHandle(int index, Quaternion handleRotation, Handles.CapFunction cap)
        {
            var point = _self.Curve.GetPointTransformed(index, _self.transform);
            if (EditorApplication.isPlaying)
                point = _self.Curve.GetPoint(index);
            
            var size = 1; //HandleUtility.GetHandleSize(point);
            Handles.color = Color.white;

            if (Handles.Button(point, handleRotation, size * _handleSize, size * _pickSize, cap))
            {
                _selectedIndex = index;
                Repaint();
            }

            if (_selectedIndex != index) return;

            EditorGUI.BeginChangeCheck();
            point = Handles.PositionHandle(point, handleRotation);

            if (!EditorGUI.EndChangeCheck()) return;

            Undo.RecordObject(_self, "Move Point");
            EditorUtility.SetDirty(_self);
            _self.Curve.SetTransformedPoint(_selectedIndex, point, _self.transform);
        }

        private void SetPositionHandle(int index, Quaternion handleRotation)
        {
            var point = _self.Curve.GetPointTransformed(index, _self.transform);
            if (EditorApplication.isPlaying)
                point = _self.Curve.GetPoint(index);

            EditorGUI.BeginChangeCheck();
            var p = Handles.PositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_self, "Move Point");
                EditorUtility.SetDirty(_self);
                
                _self.Curve.SetTransformedPoint(index, p, _self.transform);
            }
        }
    }
}