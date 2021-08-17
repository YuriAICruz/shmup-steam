using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Splines
{
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class SplineInspectorEditor : Editor
    {
        private bool _canEdit;
        private int _selectedIndex;
        private const float _pickSize = 0.06f;
        private const float _handleSize = 0.04f;
        private float _handlsScale = 1;

        private Spline _spline;

        private MonoBehaviour _self;

        private void Start()
        {
            CheckFields();
        }

        private void CheckFields()
        {
            _self = target as MonoBehaviour;

            if (_self == null) return;

            foreach (var fieldInfo in _self.GetType().GetFields())
            {
                try
                {
                    if (fieldInfo.FieldType == typeof(Spline))
                    {
                        _spline = (Spline) fieldInfo.GetValue(_self);
                    }
                }
                catch (System.Exception e)
                {
#if DEV_MODE
                    Debug.Log(e.ToString());
#endif
                }
            }

            if (_spline != null)
            {
                GenerateBaseSpline();
            }
        }

        private void GenerateBaseSpline()
        {
            if (_spline.Count() == 0)
            {
                _spline.Add();
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_spline == null)
            {
                CheckFields();
                return;
            }

            InspectorGui();
        }

        public void InspectorGui()
        {
            if (_spline.Count() == 0)
            {
                GenerateBaseSpline();
                return;
            }

            try
            {
                var loop = _spline.GetLoop();

                var loopMod = EditorGUILayout.Toggle("Loop:", loop);
                if (loop != loopMod)
                {
                    Undo.RecordObject(_self, "Set Loop");
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    EditorUtility.SetDirty(_self);

                    _spline.SetLoop(loopMod);
                    SceneView.RepaintAll();
                }
            }
            catch (Exception e)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spline Editor");

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

            EditorGUILayout.Space();
            _handlsScale = EditorGUILayout.FloatField("Handle Scale", _handlsScale);
        }

        private void EditGui()
        {
            if (_selectedIndex % 3 == 1 || _selectedIndex % 3 == 2)
            {
                EditorGUILayout.LabelField("Handle");

                var mode = _spline.GetMode(_selectedIndex);
                var modeMod = (SplinePointMode) EditorGUILayout.EnumPopup("Mode:", mode);
                if (mode != modeMod)
                {
                    Undo.RecordObject(_self, "Set Mode");
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    EditorUtility.SetDirty(_self);

                    _spline.SetMode(_selectedIndex, modeMod);

                    SceneView.RepaintAll();
                }

                ShowPosition();
            }
            else
            {
                EditorGUILayout.LabelField("Control Point");

                if (_selectedIndex < _spline.Count() - 1)
                {
                    if (GUILayout.Button("Add Here"))
                    {
                        _spline.Add(_selectedIndex);
                        SceneView.RepaintAll();
                    }
                }

                ShowPosition();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add"))
            {
                _spline.Add();
                SceneView.RepaintAll();
            }
        }

        private void ShowPosition()
        {
            var pt = _spline.GetPoint(_selectedIndex);
            var point = EditorGUILayout.Vector3Field("Position:", pt);
            if (pt != point)
            {
                Undo.RecordObject(_self, "Move Point");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorUtility.SetDirty(_self);

                _spline.SetPoint(_selectedIndex, point);

                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            if (_spline == null) return;

            Draw();
        }

        private void Clear()
        {
            _spline = null;
        }

        private void Draw()
        {
            DrawCurve();

            CreateHandles();
        }

        private void CreateHandles()
        {
            var handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? _self.transform.rotation
                : Quaternion.identity;

            for (int i = 0, n = _spline.Count() - 1; i < n; i += 3)
            {
                if (i == 0)
                    SetButtonPositionHandle(i, handleRotation, Handles.CubeHandleCap, Color.blue);
                else
                    SetButtonPositionHandle(i, handleRotation, Handles.CubeHandleCap, Color.white);

                if (i + 3 == n)
                    SetButtonPositionHandle(i + 3, handleRotation, Handles.CubeHandleCap, Color.red);
                else
                    SetButtonPositionHandle(i + 3, handleRotation, Handles.CubeHandleCap, Color.white);
//                SetPositionHandle(i, handleRotation);
//                SetPositionHandle(i + 3, handleRotation);

                SetButtonPositionHandle(i + 1, handleRotation, Handles.SphereHandleCap, Color.white);
                SetButtonPositionHandle(i + 2, handleRotation, Handles.SphereHandleCap, Color.white);
            }
        }

        private void DrawCurve()
        {
            for (int i = 1, n = _spline.Count(); i < n; i += 3)
            {
                if (!EditorApplication.isPlaying)
                    Handles.DrawBezier(
                        _spline.GetPointTransformed(i - 1, _self.transform),
                        _spline.GetPointTransformed(i + 2, _self.transform),
                        _spline.GetPointTransformed(i, _self.transform),
                        _spline.GetPointTransformed(i + 1, _self.transform),
                        Color.green,
                        null,
                        2
                    );
                else
                    Handles.DrawBezier(
                        _spline.GetPoint(i - 1),
                        _spline.GetPoint(i + 2),
                        _spline.GetPoint(i),
                        _spline.GetPoint(i + 1),
                        Color.green,
                        null,
                        2
                    );

                Handles.color = Color.gray;
                if (!EditorApplication.isPlaying)
                {
                    Handles.DrawLine(_spline.GetPointTransformed(i - 1, _self.transform),
                        _spline.GetPointTransformed(i, _self.transform));
                    Handles.DrawLine(_spline.GetPointTransformed(i + 1, _self.transform),
                        _spline.GetPointTransformed(i + 2, _self.transform));
                }
                else
                {
                    Handles.DrawLine(_spline.GetPoint(i - 1), _spline.GetPoint(i));
                    Handles.DrawLine(_spline.GetPoint(i + 1), _spline.GetPoint(i + 2));
                }
            }
        }

        private void SetButtonPositionHandle(int index, Quaternion handleRotation, Handles.CapFunction cap, Color color)
        {
            var point = _spline.GetPointTransformed(index, _self.transform);
            if (EditorApplication.isPlaying)
                point = _spline.GetPoint(index);

            var size = HandleUtility.GetHandleSize(point) * 4;

            Handles.color = color;

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
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.SetDirty(_self);
            _spline.SetTransformedPoint(_selectedIndex, point, _self.transform);
        }

        private void SetPositionHandle(int index, Quaternion handleRotation)
        {
            var point = _spline.GetPointTransformed(index, _self.transform);
            if (EditorApplication.isPlaying)
                point = _spline.GetPoint(index);

            EditorGUI.BeginChangeCheck();
            var p = Handles.PositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_self, "Move Point");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorUtility.SetDirty(_self);
                _spline.SetTransformedPoint(index, p, _self.transform);
            }
        }
    }
}