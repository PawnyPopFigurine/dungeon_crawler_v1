using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "LevelData/Item")]
    [System.Serializable]
    public class ItemDefinitionSO : ScriptableObject
    {
        [SerializeField] ItemDefinition _definition;
        public ItemDefinition Definition => _definition;
    }

    [System.Serializable]
    public class ItemDefinition : WeightedListItem
	{
        [SerializeField] string _id;
        public string Id => _id;

        [SerializeField] bool _hideInGame;
        public bool HideInGame => _hideInGame;

        [SerializeField] bool _excludeFromRandom;
        public bool ExcludeFromRandom => _excludeFromRandom;

        [SerializeField] GameObject _itemPrefab;
        public GameObject ItemPrefab => _itemPrefab;

        public void Initialise()
		{
            //do stuff here
		}

        public ItemDefinition CreateCopy()
		{
            ItemDefinition copy = new()
            {
                _id = _id,
                _hideInGame = _hideInGame,
                _itemPrefab = _itemPrefab,
                _excludeFromRandom = _excludeFromRandom,
            };

            copy.SetWeighting(Weighting);

            return copy;
		}
	}
}