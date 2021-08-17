using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems.Gameplay.LevelDesign
{
    public class CameraFollowPoints : MonoBehaviour
    {
        private bool _idle;
        private bool _isAvailable;
        private Vector3 _initialPosition;

        public bool Idle
        {
            get { return _idle; }
            private set
            {
                if (_idle == value) return;

                _idle = value;
                if (_idle)
                {
                    _isAvailable = true;
                    transform.position = Vector3.one * -999;
                }
            }
        }

        public bool IsAvailable => _isAvailable;

        [Inject] private CameraManager _cameraManager;
        [Inject] private SignalBus _signalBus;
        private SectorSettings _sector;
        private Transform _parent;


        private void ResetPoint()
        {
            Idle = true;
            _isAvailable = true;
            transform.position = _initialPosition;
            transform.SetParent(_parent);
        }

        public void Setup(SectorSettings sector)
        {
            _sector = sector;

            _parent = transform.parent;
            _initialPosition = transform.position;

            _signalBus.Subscribe<LevelStarted>(ResetPoint);
            _signalBus.Subscribe<SectorChange>(SetSector);

            Idle = true;
        }

        private void SetSector(SectorChange sector)
        {
            if (sector.currentSectorSettings.uid != _sector.uid)
            {
                _idle = true;
                return;
            }

            FollowCamera();
        }

        public void Pick()
        {
            _isAvailable = false;
        }

        public void Release()
        {
            _isAvailable = true;
        }

        public void FollowCamera()
        {
            Idle = false;
            transform.SetParent(_cameraManager.transform);
        }
    }
}