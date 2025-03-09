using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class ItemController : MonoBehaviour
    {
        [SerializeField] string _definitionId;
        public string DefinitionId => _definitionId;

        bool _initialised;

        int _itemIndex;
        public int ItemIndex => _itemIndex;

        public void Initialise()
        {
            if (_initialised)
            {
                return;
            }

            _initialised = true;

            SetCallbacks();
        }

        public void SetCallbacks()
        {


        }

        public void UpdateController(float deltaTime)
        {

        }

        public void OnLevelPlacement()
        {

        }

        public void ResetController()
        {

        }

        public void OnCollected()
		{
            ItemPoolingSystem.Instance.ClearController(this);
		}

        public void SetItemIndex(int itemIndex)
        {
            _itemIndex = itemIndex;
        }
    }
}