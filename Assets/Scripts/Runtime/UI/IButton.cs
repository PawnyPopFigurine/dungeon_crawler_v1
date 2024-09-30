using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace UI
{
	[Serializable]
	public class ButtonData
	{
		[SerializeField]
		private Color _normalColour;
		[SerializeField]
		private Color _disabledColour;
		[SerializeField]
		private Color _highlightColour;
		[SerializeField]
		private Color _selectColour;

		public Color NormalColour => _normalColour;
		public Color DisabledColour => _disabledColour;
		public Color HighlightColour => _highlightColour;
		public Color SelectColour => _selectColour;

		public ButtonData(in ButtonData sourceData)
		{
			CopyFrom(sourceData);
		}

		public void CopyFrom(in ButtonData sourceData)
		{
			_normalColour = sourceData.NormalColour;
			_highlightColour = sourceData.HighlightColour;
			_disabledColour = sourceData.DisabledColour;
			_selectColour = sourceData.SelectColour;
		}
	}

	public interface IButton : IPointerClickHandler, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
	{
		public delegate void ButtonEvent(IButton button);

		public event ButtonEvent OnButtonSelect;
		public event ButtonEvent OnButtonEnter;
		public event ButtonEvent OnButtonExit;
		public event ButtonEvent OnButtonDown;
		public event ButtonEvent OnButtonUp;

		public enum EButtonState
		{
			None,
			Disabled,
			Normal,
			Highlighted,
			Selected
		};

		public void Initialise(in ButtonData buttonData);

		public EButtonState ButtonState { get; }
		public void Enable();
		public void Highlight();
		public void Disable();
		public void Select();
		public void Show(bool bShow = true);
		public void Hide();
	}
}