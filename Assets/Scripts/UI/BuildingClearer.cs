using System;
using System.Linq;
using Controllers;
using UnityEngine;
using Utilities;
using UnityEngine.UI;
using Entities;
using TMPro;
using UnityEngine.EventSystems;
using static Managers.GameManager;

namespace UI
{
    public class BuildingClearer : MonoBehaviour
    { 
        [SerializeField] int pixelsPerUnit;
        [SerializeField] private Image buttonImage;
        [SerializeField] private LayerMask _collisionMask;
        
        private Transform _clearButton;
        private Building _selected;
        private int _selectedDestroyCost;
        private Camera _mainCamera;
        private int _numberOfTerrainTilesDeleted;
        private TextMeshProUGUI _nameText, _costText;
        private Vector3 _selectedPosition;

        private const float ScaleSteps = 1.025f;
        // Modifier for building refunds. Currently set to provide players with a 75% refund. 
        private const float BuildingRefundModifier = 0.25f;

        private void Start()
        {
            _clearButton = transform.GetChild(0);

            TextMeshProUGUI[] textFields = transform.GetChild(0).GetComponentsInChildren<TextMeshProUGUI>();

            _nameText = textFields[0];
            _costText = textFields[1];

            Click.OnLeftClick += LeftClick;
            Click.OnRightClick += RightClick;
    
            CameraMovement.OnCameraMove += Clear;
    
            _mainCamera = Camera.main;
            
            Clear();
        }
    
        private void Update()
        {
            if (_selected == null || !_clearButton.gameObject.activeSelf) return;

            // Lerp the button to stay above the building.
            _clearButton.position = Vector3.Lerp(
                _clearButton.position,
                _mainCamera.WorldToScreenPoint(_selectedPosition) + 
                (Vector3.up * pixelsPerUnit), 
                0.5f);
        }
    
        // Clears all selections
        void Clear()
        {
            _clearButton.gameObject.SetActive(false);
            _selected = null;
            _selectedDestroyCost = 0;
        }

        private void LeftClick()
        {
            // TODO: Find a way to stop the mouse from clicking on grid elements through other UI elements.
            Clear();

            // Make sure that we don't bring up the button if we click on a UI element. 
            if (BuildingPlacement.Selected != -1 || _selected != null || EventSystem.current.IsPointerOverGameObject()) 
                return;

            _selected = getBuildingOnClick();

            if (!_selected) return;

            // Make sure the building has not just been spawned (such as when it's just been built)
            if (_selected.HasNeverBeenSelected)
            {
                _selected.HasNeverBeenSelected = false;
                return;
            }

            // Find building position and reposition clearButton to overlay on top of it.  
            Vector3 buildingPosition = _selected.transform.position;
            _clearButton.position = _mainCamera.WorldToScreenPoint(buildingPosition) + (Vector3.up * pixelsPerUnit);
            _clearButton.gameObject.SetActive(true);

            int destructionCost;
            string selectedName = _selected.name;

            // If the selected element is terrain, apply the cost increase algorithm to the destruction cost.
            if (_selected.type == BuildingType.Terrain)
            {
                var range = Enumerable.Range(1, _numberOfTerrainTilesDeleted);
                destructionCost = (int) range.Select(i => 1.0f / Math.Pow(ScaleSteps, i)).Sum();
                selectedName = "Terrain";
            }
            else
            {
                // Set the cost to 25% of the original building cost (player gets 75% back)
                destructionCost = Mathf.FloorToInt(_selected.baseCost * BuildingRefundModifier);
            }

            // Disable the button if the player can't afford to clear the tile (visually changes opacity)
            float opacity = Manager.Wealth >= destructionCost ? 255f : 166f;

            // Change opacity of the text.
            Color oldColor = _costText.color;
            _costText.color = _nameText.color = new Color(oldColor.r, oldColor.g, oldColor.b, opacity);

            // Change opacity of button
            Color oldButtonColor = buttonImage.color;
            buttonImage.color = new Color(oldButtonColor.r, oldButtonColor.g, oldButtonColor.b, opacity / 255f);
                
            // Update UI
            _costText.text = destructionCost.ToString();
            _nameText.text = selectedName;

            _selectedDestroyCost = destructionCost;
            _selectedPosition = _selected.transform.position;
        }

        private Building getBuildingOnClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _mainCamera.nearClipPlane));
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 200f, _collisionMask);

            if (hit.collider == null) return null;

            return hit.collider.GetComponentInParent<Building>();
        }
    
        public void ClearBuilding()
        {
            Building occupant = _selected;
            
            if (_selected.indestructible || !Manager.Spend(_selectedDestroyCost)) return;

            if (_selected.type == BuildingType.Terrain)
            {
                _numberOfTerrainTilesDeleted += Manager.Map.GetCells(_selected).Length;
            }

            occupant.Clear();
            Manager.Buildings.Remove(occupant);
            Clear();
        }
    
        void RightClick()
        {
            Clear();
        }
    }
}

