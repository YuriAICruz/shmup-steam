using System;
using Graphene.Game.Systems.Gameplay;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Systems
{
    public enum CameraMovementType
    {
        ScrollUp = 0,
        Fixed = 1,
        FollowShip = 2
    }
    
    public class CameraManager : MonoBehaviour
    {
        public class Factory : PlaceholderFactory<CameraManager>
        {
            private readonly DiContainer _container;

            private CameraManager _mainCamera;

            public Factory(DiContainer container)
            {
                _container = container;
            }
            
            public CameraManager Create(CameraMovementType type, Transform root)
            {
                if (_mainCamera) return _mainCamera;
                
                var cam = _container.InstantiatePrefabResource($"Cameras/{type}").GetComponent<CameraManager>();
                
                cam.transform.SetParent(root);

                _mainCamera = cam;

                _container.Bind<CameraManager>().FromInstance(_mainCamera);
                
                return cam;
            }
        }
        
        public Vector3 direction = Vector3.up;
        public float transitionSpeed = 3;
        public float borders = 0;
        public float safe = 0;

        [Inject] private SignalBus _signalBus;

        private Camera _camera;
        private Bounds _bounds;
        private Bounds _boundsWithBorders;
        private Vector3 _size;
        private Vector3 _currentPosition;
        private Vector3 _initialPosition;

        private bool _init;
        private bool _running;
        private float _speed;
        private float _targetSpeed;
        private Bounds _boundsSafe;

        public enum BorderType
        {
            Center = 0,
            OffsetOut = 1,
            SafeArea = 2
        }

        private void Awake()
        {
            if (!_init)
            {
                Initialize();
                ResetCamera(new LevelStarted(false));
            }

            _signalBus.Subscribe<LevelStarted>(ResetCamera);
            _signalBus.Subscribe<LevelEnded>(PauseCamera);
            _signalBus.Subscribe<LevelCompleted>(PauseCamera);
            _signalBus.Subscribe<SectorChange>(UpdateSector);
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _initialPosition = transform.position;

            _camera = GetComponent<Camera>();

            CreateBounds();

            _init = true;
        }

        public void Setup(Vector3 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            _init = false;
        }
        
        private void PauseCamera()
        {
            _running = false;
        }

        private void CreateBounds()
        {
            _bounds = new Bounds(Vector3.zero, Vector3.zero);
            _boundsWithBorders = new Bounds(Vector3.zero, Vector3.zero);
            _boundsSafe = new Bounds(Vector3.zero, Vector3.zero);
            SetBoundCenter();

            UpdateBoundSizes();
        }

        private void BoundSizes()
        {
            var z = transform.position.z;
            _size =
                _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, z)) -
                _camera.ScreenToWorldPoint(new Vector3(0, 0, z));

            _size.x = Mathf.Abs(_size.x);
            _size.y = Mathf.Abs(_size.y);
            _size.z = 2;
            
            var _sizeOut = _size;
            _sizeOut.x = Mathf.Abs(_size.x) + borders;
            _sizeOut.y = Mathf.Abs(_size.y) + borders;
            
            var _sizeSafe = _size;
            _sizeSafe.x = Mathf.Abs(_size.x) - safe;
            _sizeSafe.y = Mathf.Abs(_size.y) - safe;
            
            _bounds.size = _size;
            _boundsWithBorders.size = _size;
            _boundsSafe.size = _size;
        }

        private void UpdateBoundSizes()
        {
            var z = transform.position.z;
            _size =
                _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, z)) -
                _camera.ScreenToWorldPoint(new Vector3(0, 0, z));

            _size.x = Mathf.Abs(_size.x);
            _size.y = Mathf.Abs(_size.y);
            _size.z = 2;
            _bounds = new Bounds(_bounds.center, _size);

            var _sizeOut = _size;
            _sizeOut.x = Mathf.Abs(_size.x) + borders;
            _sizeOut.y = Mathf.Abs(_size.y) + borders;
            _boundsWithBorders = new Bounds(_bounds.center, _sizeOut);


            var _sizeSafe = _size;
            _sizeSafe.x = Mathf.Abs(_size.x) - safe;
            _sizeSafe.y = Mathf.Abs(_size.y) - safe;
            _boundsSafe = new Bounds(_bounds.center, _sizeSafe);
        }

        private void SetBoundCenter()
        {
            var z = -transform.position.z;
            _bounds.center = _camera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, z));
            _boundsWithBorders.center = _bounds.center;
            _boundsSafe.center = _bounds.center;
        }

        private void Update()
        {
            if (!_running) return;

            _speed = Mathf.Lerp(_speed, _targetSpeed, Time.deltaTime * transitionSpeed);

            _currentPosition += LastMovement();
            transform.position = _currentPosition;

            BoundSizes();
            SetBoundCenter();
        }

        private void UpdateSector(SectorChange data)
        {
            _targetSpeed = data.currentSectorSettings.cameraSpeed;
        }

        public void SetSpeed(float speed)
        {
            _targetSpeed = speed;
        }

        public bool InBorders(Vector3 currentPosition, BorderType border = BorderType.Center)
        {
            switch (border)
            {
                case BorderType.Center:
                    return _bounds.Contains(currentPosition);
                case BorderType.OffsetOut:
                    return _boundsWithBorders.Contains(currentPosition);
                case BorderType.SafeArea:
                    return _boundsSafe.Contains(currentPosition);
                default:
                    throw new ArgumentOutOfRangeException(nameof(border), border, null);
            }
        }

        public Vector3 ClosestPoint(Vector3 currentPosition, BorderType border = BorderType.Center)
        {
            switch (border)
            {
                case BorderType.Center:
                    return _bounds.ClosestPoint(currentPosition);
                case BorderType.OffsetOut:
                    return _boundsWithBorders.ClosestPoint(currentPosition);
                case BorderType.SafeArea:
                    return _boundsSafe.ClosestPoint(currentPosition);
                default:
                    throw new ArgumentOutOfRangeException(nameof(border), border, null);
            }
        }

        public Vector2 BoundSize(BorderType border = BorderType.Center)
        {
            switch (border)
            {
                case BorderType.Center:
                    return _bounds.size;
                case BorderType.OffsetOut:
                    return _boundsWithBorders.size;
                case BorderType.SafeArea:
                    return _boundsSafe.size;
                default:
                    throw new ArgumentOutOfRangeException(nameof(border), border, null);
            }
        }

        public Vector3 LastMovement()
        {
            return direction * (_speed * Time.deltaTime);
        }

        private void ResetCamera(LevelStarted data)
        {
            if (!_init)
            {
                Initialize();
            }

            _currentPosition = _initialPosition;

            if (data.hasSector)
            {
                var z = -transform.position.z;

                transform.position = Vector3.back * z;

                var pos = _camera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, 0, z));

                _currentPosition.y = pos.y + data.sectorPosition.y;
            }

            transform.position = _currentPosition;

            _running = true;

            SetBoundCenter();
        }

        public Vector2 GetDirectionToMouse(Vector3 position)
        {
            var mouse = UnityEngine.Input.mousePosition;
            mouse.z = position.z - transform.position.z;
            var mousePosition = _camera.ScreenToWorldPoint(mouse);

            var dir = mousePosition - position;
            dir.z = 0;
            dir.Normalize();

            return (Vector2) dir;
        }

        public Vector2 ScreenPosition(Vector3 pos)
        {
            var sPos = _camera.WorldToScreenPoint(pos);

            return new Vector2(sPos.x / Screen.width, sPos.y / Screen.height);
        }

        public Vector2 ScreenToWorldNormalized(Vector2 pos, float border = 0)
        {
            var sPos = _camera.ScreenToWorldPoint(
                new Vector3(
                    (pos.x  + border * Mathf.Sign(-pos.x)) * Screen.width,
                    (pos.y + border * Mathf.Sign(-pos.y)) * Screen.height,
                    transform.position.z
                )
            );

            return sPos;
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
                CreateBounds();
            }

            SetBoundCenter();
            UpdateBoundSizes();

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);

            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(_boundsWithBorders.center, _boundsWithBorders.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_boundsSafe.center, _boundsSafe.size);
        }
#endif
    }
}