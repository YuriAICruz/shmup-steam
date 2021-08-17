using System;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;
using Zenject;

namespace Graphene.LevelEditor
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class EditorCamera : MonoBehaviour
    {
        private Camera _camera;
        
        public float farClipPlane => _camera.farClipPlane;

        private void Awake()
        {
            GetCamera();
        }

        private void Start()
        {
            GetCamera();
        }


        private void GetCamera()
        {
            if (_camera) return;

            _camera = GetComponent<Camera>();
        }

        public Vector3 ScreenToWorldPoint(Vector3 pos)
        {
            GetCamera();
            
            return _camera.ScreenToWorldPoint(pos);
        }

        public Ray ScreenPointToRay(Vector3 position)
        {
            GetCamera();
            
            return _camera.ScreenPointToRay(position);
        }

        public void Pan(Vector3 offset)
        {
            transform.position += offset;
        }
    }
}