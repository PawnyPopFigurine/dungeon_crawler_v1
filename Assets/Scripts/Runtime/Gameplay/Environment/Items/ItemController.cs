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
    }
}