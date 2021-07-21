using System;
using System.Linq;
using UnityEngine;
using Utilities;
using UnityEngine.UI;
using Entities;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using static Managers.GameManager;

namespace Controllers
{
    public class Clear : MonoBehaviour
    {
        public static int TerrainClearCount { get; set; }
        private const int TerrainBaseCost = 5;
        private const float TerrainCostScale = 1.025f;
        public static int RuinsClearCount { get; set; }
        private const int RuinsBaseCost = 20;
        private const float RuinsCostScale = 1.25f;
        
        private const float RefundPercentage = 0.50f;
        
                
        private class SelectedBuildingConfig
        {
            public bool IsRefund;
            public int DestructionCost;
            public string BuildingName;
        }
        
        [SerializeField] private int yOffset;
        [SerializeField] private Image buttonImage;
        [SerializeField] private TextMeshProUGUI nameText, costText;
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] [ColorUsage(false, true)] private Color hoverColor;
        [SerializeField] [ColorUsage(false, true)] private Color selectColor;
        [SerializeField] private float raycastInterval;

        private Canvas _canvas;
        private Camera _cam;
        private OutlinePostProcess _outline;
        private Building _hoveredBuilding, _selectedBuilding;
        private SelectedBuildingConfig _config;
        private float _timeSinceRaycast;

        public static Action<Building> OnClear;
        
        private Building HoveredBuilding
        {
            get => _hoveredBuilding;
            set {
                // Deselection should only happen when the un-hovered building
                // is not the same as the selected building
                if (_hoveredBuilding && !SelectedBuilding)
                {
                    _hoveredBuilding.selected = false;
                }
                _hoveredBuilding = value;
                if (!_hoveredBuilding) return;
                _hoveredBuilding.selected = true;
                SetHighlightColor(hoverColor);
            }
        }
        
        private Building SelectedBuilding
        {
            get => _selectedBuilding;
            set
            {
                if (_selectedBuilding) _selectedBuilding.selected = false;

                _selectedBuilding = value;
                _canvas.enabled = _selectedBuilding;
                if (!_selectedBuilding) return;
                
                _selectedBuilding.selected = true;
                SetHighlightColor(selectColor);
                
                // Find building position and reposition clearButton to overlay on top of it.  
                RepositionClearButton();

                // Get UI config information
                _config = GetClearButtonConfiguration();

                // Set button opacity (based on whether the player can afford to destroy a building) and text
                SetButtonOpacity(_config.IsRefund || Manager.Wealth >= _config.DestructionCost ? 255f : 166f);
            
                nameText.text = _config.BuildingName;
                if (_config.BuildingName == "Guild Hall") costText.text = "Cost: Don't";
                else costText.text = (_config.IsRefund ? "Refund: " : "Cost: ") + _config.DestructionCost;
            }
        }
        
        private void DeselectBuilding()
        {
            SelectedBuilding = null;
        }
        
        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            _cam = Camera.main;
            if (_cam) _cam.GetComponentInChildren<PostProcessVolume>().profile.TryGetSettings(out _outline);
            
            Click.OnLeftClick += LeftClick;
            Click.OnRightClick += DeselectBuilding;

            ClickOnButtonDown.OnUIClick += DeselectBuilding;
            //CameraMovement.OnCameraMove += DeselectBuilding;
            Shade.OnShadeOpened += () => HoveredBuilding = null;
        }

        private void Update()
        {
            // Don't hover new buildings while a building is selected, the camera is moving, or in the UI
            if (SelectedBuilding || CameraMovement.IsMoving || IsSelectionDisabled())
            {
                HoveredBuilding = null;
                return;
            }

            // Time-slice the hovered building check
            _timeSinceRaycast += Time.deltaTime;
            if (!(_timeSinceRaycast >= raycastInterval)) return;
            _timeSinceRaycast = 0f;
            
            HoveredBuilding = SelectHoveredBuilding();
        }

        private void LateUpdate()
        {
            // Keep track of clear button position above selected building
            if (!SelectedBuilding) return;
            transform.position = _cam.WorldToScreenPoint(SelectedBuilding.transform.position) + (Vector3.up * yOffset);
        }

        // Returns the building the cursor is hovering over if exists
        private Building SelectHoveredBuilding()
        {
            Ray ray = _cam.ScreenPointToRay(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, _cam.nearClipPlane));
            Physics.Raycast(ray, out RaycastHit hit, 200f, collisionMask);

            return hit.collider ? hit.collider.GetComponentInParent<Building>() : null;
        }
        
        private void LeftClick()
        {
            if (Click.PlacingBuilding) return;

            // Make sure that we don't bring up the button if we click on a UI element. 
            if (IsSelectionDisabled()) return;
            
            if (SelectedBuilding) // Deselect current building if already selected
            {
                DeselectBuilding();
                return;
            }

            SelectedBuilding = SelectHoveredBuilding();
        }

        private void SetHighlightColor(Color color)
        {
            _outline.color.value = color;
        }

        private static bool IsSelectionDisabled()
        {
            return BuildingPlacement.Selected != BuildingPlacement.Deselected || EventSystem.current.IsPointerOverGameObject();
        }
        
        private SelectedBuildingConfig GetClearButtonConfiguration()
        {
            SelectedBuildingConfig config = new SelectedBuildingConfig();
            
            if (SelectedBuilding.IsRuin)
            {
                config.BuildingName = "Ruin";
                config.IsRefund = false;
                //Scaled cost
                config.DestructionCost = (int) (Enumerable
                    .Range(RuinsClearCount, SelectedBuilding.SectionCount)
                    .Sum(i => Mathf.Pow(RuinsCostScale, i)) * RuinsBaseCost);
            }
            else if (SelectedBuilding.type == BuildingType.Terrain)
            {
                config.BuildingName = "Terrain";
                config.IsRefund = false;
                //Scaled cost
                config.DestructionCost = (int) (Enumerable
                    .Range(TerrainClearCount, SelectedBuilding.SectionCount)
                    .Sum(i => Mathf.Pow(TerrainCostScale, i)) * TerrainBaseCost);
            }
            else
            {
                config.IsRefund = true;
                config.DestructionCost = Mathf.FloorToInt(SelectedBuilding.ScaledCost * RefundPercentage);
                config.BuildingName = SelectedBuilding.name;
            }

            return config;
        }

        private void SetButtonOpacity(float opacity)
        {
            // Set for button text
            Color oldColor = costText.color;
            costText.color = nameText.color = new Color(oldColor.r, oldColor.g, oldColor.b, opacity);

            // Set for button colour
            Color oldButtonColor = buttonImage.color;
            buttonImage.color = new Color(oldButtonColor.r, oldButtonColor.g, oldButtonColor.b, opacity / 255f);
        }

        private void RepositionClearButton()
        {
            Vector3 buildingPosition = SelectedBuilding.transform.position;
            transform.position = _cam.WorldToScreenPoint(buildingPosition) + (Vector3.up * yOffset);
            _canvas.enabled = true;
        }

        public void ClearBuilding()
        {
            if (
                !SelectedBuilding  ||
                _config == null ||
                SelectedBuilding.indestructible || 
                !Manager.Spend(_config.DestructionCost * (_config.IsRefund ? -1 : 1))
            ) return;
            
            
            if (SelectedBuilding.IsRuin) RuinsClearCount += SelectedBuilding.SectionCount;
            if (SelectedBuilding.type == BuildingType.Terrain) TerrainClearCount += SelectedBuilding.SectionCount;
            
            OnClear.Invoke(SelectedBuilding);

            SelectedBuilding.Clear();
            DeselectBuilding();
        }
    }
}

