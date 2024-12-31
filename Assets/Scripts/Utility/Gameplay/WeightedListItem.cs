using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class WeightedListItem
    {
        [SerializeField] int _weighting = 10;
        public int Weighting => _weighting;

        public void SetWeighting(int newWeighting)
		{
            _weighting = newWeighting;
		}
    }
}