using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Managers.GameManager;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine.UIElements;

namespace Controllers
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private float maxScrollSpeed = 0.1f;
        [SerializeField] private float maxDragSpeed = 1f;
        [SerializeField] private float dragAcceleration = 0.1f;
        [SerializeField] private float scrollAccelerationSpeed = 0.1f;
        [SerializeField] private float bounceTime = 0.1f;
        [SerializeField] private PostProcessProfile profile;
        [SerializeField] private PostProcessVolume volume;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float doFAdjustMultiplier = 20f;
        [SerializeField] private bool invertScroll;
        
        private Camera _cam;
        private DepthOfField _depthOfField;
        private CinemachineFreeLook _freeLook;
        private Vector3 _dragDir;
        private Vector3 _vel = Vector3.zero;
        private float _scrollAcceleration;
        private float _scrollAccelerationRef;
        private Vector3 _followVelRef = Vector3.zero;

        private Vector3 _startPos, _startRot;
        private Vector3 _lastDrag;
        private bool _dragging, _rotating;
        private Rect _screenBounds;

        private int _oceanMask;

        public static Action OnCameraMove;

        public static bool IsMoving;

        private void Awake()
        {
            _screenBounds = new Rect(Screen.width / 99f,
                Screen.height / 99f,
                Screen.width - ((Screen.width / 99f) * 2),
                Screen.height - ((Screen.height / 99f) * 2));

            _cam = GetComponent<Camera>();
            profile.TryGetSettings(out _depthOfField);
            _freeLook = GetComponent<CinemachineFreeLook>();
            
            var camTf = _cam.transform;
            _startPos = camTf.position;
            _startRot = camTf.eulerAngles;
            
            _oceanMask = LayerMask.GetMask("Ocean");
        }

        private void Start()
        {
            startPos = _freeLook.Follow.position;
            startHeight = _freeLook.m_Orbits[1].m_Height;
            inMenu = true;
            
            _freeLook.Follow.position = menuPos;
            _freeLook.m_Orbits[1].m_Height = menuOrbitHeight;
        }

        private bool isActivated = false;
        private bool inMenu;
        private Vector3 menuPos = new Vector3(-2.0f, 1.0f, -24.0f);
        private Vector3 startPos;
        private float menuOrbitHeight = 2.0f;
        private float startHeight;
        
        private void Update()
        {
            if (inMenu)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    isActivated = true;
                    inMenu = false;
                }
            } 
            else if (isActivated) 
            {
                _freeLook.Follow.position = Vector3.Lerp(_freeLook.Follow.position, startPos, Time.deltaTime + 0.01f);
                _freeLook.m_Orbits[1].m_Height =
                    Mathf.Lerp(_freeLook.m_Orbits[1].m_Height, startHeight, Time.deltaTime);

                print((startPos - _freeLook.Follow.position).magnitude);
                if ((startPos - _freeLook.Follow.position).magnitude < UnityVectorExtensions.Epsilon + 1)
                {
                    isActivated = false;
                }
            }
            else
            {
                if (Manager.inMenu) return;
            
            // Apply rotation
            if (Input.GetMouseButtonDown(1)) _rotating = true;
            
            if (Input.GetMouseButton(1))
            {
                _freeLook.m_XAxis.m_InputAxisValue = -Input.GetAxis("Mouse X");
            }
            else if (Input.GetMouseButtonUp(1))
            {
                _rotating = false;
                _freeLook.m_XAxis.m_InputAxisValue = 0;
            }
            
            var isOverSurface = Physics.Raycast(
                _cam.ScreenPointToRay(Input.mousePosition),
                out var posHit, 1000f, _oceanMask);

            // Apply position
            if (isOverSurface && Input.GetMouseButtonDown(0))
            {
                 IsMoving = _dragging = true;
                _lastDrag = posHit.point;
            }
            
            if (isOverSurface && Input.GetMouseButton(0))
            {
                _dragDir = _lastDrag - posHit.point;
                _dragDir.y = 0;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                IsMoving = _dragging = false;
            }

            if (_screenBounds.Contains(Input.mousePosition))
            {
                _freeLook.Follow.position += _dragDir * Time.deltaTime * 100;
            }   

            if (!_dragging)
            {
                _dragDir = Vector3.SmoothDamp(
                    _dragDir, Vector3.zero, ref _vel, dragAcceleration);
            }
            
            // Apply scroll 
            var scroll = -Input.mouseScrollDelta.y;
            _scrollAcceleration += scroll * Time.deltaTime;
            _scrollAcceleration = Mathf.SmoothDamp(_scrollAcceleration, 0, 
                ref _scrollAccelerationRef, scrollAccelerationSpeed);
            _scrollAcceleration = Mathf.Clamp(_scrollAcceleration, -maxScrollSpeed, maxScrollSpeed);
            _freeLook.m_YAxis.Value += _scrollAcceleration;

            volume.weight = Mathf.Lerp(1, 0, _freeLook.m_YAxis.Value);
            var doFRay = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(doFRay, out var hit, 100f, layerMask))
            {
                _depthOfField.focusDistance.value = Mathf.MoveTowards(
                    _depthOfField.focusDistance.value, hit.distance, 
                    Time.deltaTime * doFAdjustMultiplier);
            }

            var atLimit = _freeLook.m_YAxis.Value <= 0.01 | _freeLook.m_YAxis.Value >= 0.98;
            if (atLimit && Mathf.Abs(_scrollAcceleration) > 0)
                _freeLook.Follow.position += new Vector3(0, _scrollAcceleration, 0);
            
            // Apply follow
            var followPos = _freeLook.Follow.position;
            var clampedY = Mathf.Clamp(followPos.y, -1, 2);
            followPos = new Vector3(followPos.x, clampedY, followPos.z);

            var newFollowPos =  new Vector3(followPos.x, 1, followPos.z);
            followPos = Vector3.SmoothDamp(
                followPos, newFollowPos, ref _followVelRef, bounceTime);
            _freeLook.Follow.position = followPos;
            }
        }

        public void Center()
        {
            var t = transform;
            t.position = _startPos;
            t.eulerAngles = _startRot;
        }
    }
}
