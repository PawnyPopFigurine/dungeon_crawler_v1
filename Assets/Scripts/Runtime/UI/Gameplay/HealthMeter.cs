using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.UI
{
    public class HealthMeter : MonoBehaviour
    {
        [SerializeField] List<GameObject> _healthIcons;

        public void UpdateIconsForHealthValue(int health)
		{
            for(int iconIndex = _healthIcons.Count - 1; iconIndex >= 0; --iconIndex)
			{
                if(null != _healthIcons[iconIndex])
				{
                    _healthIcons[iconIndex].SetActive(iconIndex < health);
				}
			}
		}
    }
}