using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JZK.Gameplay;

[CustomEditor(typeof(RoomController))]

public class RoomControllerEditor : Editor
{
    RoomController _target;

	private void Awake()
	{
		_target = (RoomController)target;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
	}
}
