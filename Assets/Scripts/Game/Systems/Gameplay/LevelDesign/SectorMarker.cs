using System;
using System.Collections;
using System.Collections.Generic;
using Graphene.Game.Systems;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public class SectorMarker : MonoBehaviour
    {
        public SectorSettings _sectorSettings;

        [HideInInspector] public int uid;

        [Inject] private CameraManager _cameraManager;
        [Inject] private SignalBus _signalBus;
        [Inject] private LevelManager _levelManager;
        private bool _running = true;
        private bool _waiting;

        private void Awake()
        {
            uid = _sectorSettings.uid = gameObject.GetInstanceID();
            _sectorSettings.position = transform.position;

            _signalBus.Subscribe<LevelStarted>(ResetSector);

            for (int i = 0; i < _sectorSettings.cameraFollowPoints.Length; i++)
            {
                _sectorSettings.cameraFollowPoints[i].Setup(_sectorSettings);
            }
        }

        private void ResetSector(LevelStarted data)
        {
            if (data.hasSector && data.sectorId == _sectorSettings.uid)
            {
                _levelManager.SetSector(_sectorSettings);
                _running = false;
            }
            else
            {
                _running = data.sectorPosition.y <= transform.position.y;
            }
        }

        private void Update()
        {
            if (_waiting && _sectorSettings.waitToEnd && _sectorSettings.spawner.Ended())
            {
                _cameraManager.SetSpeed(_sectorSettings.endCameraSpeed);
                _waiting = false;
            }

            if (!_running || !_cameraManager.InBorders(transform.position)) return;

            _levelManager.SetSector(_sectorSettings);

            if (_running && _sectorSettings.waitToEnd)
            {
                _waiting = true;
            }
            
            _running = false;
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.gray;

            var cam = FindObjectOfType<CameraManager>();
            var p = new Vector3(0, Screen.height, cam.transform.position.z);
            var screenHeight = cam.BoundSize().y;
            var position = transform.position - Vector3.up * screenHeight;
            Gizmos.DrawLine(transform.position + Vector3.left * 30, transform.position + Vector3.right * 30);
            Gizmos.DrawLine(position + Vector3.left * 30, position + Vector3.right * 30);

            var size = 0.5f;
            Gizmos.color = Color.yellow;

            if (_sectorSettings == null) return;

            for (int i = 0; i < _sectorSettings.cameraFollowPoints.Length; i++)
            {
                if (_sectorSettings.cameraFollowPoints[i])
                    Gizmos.DrawSphere(_sectorSettings.cameraFollowPoints[i].transform.position, size);
            }
#endif
        }
    }
}