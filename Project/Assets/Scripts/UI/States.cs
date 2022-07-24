using System;
using Game.UI.FSM;
using UnityEngine;
using Zenject;

public partial class States : FSM, ITickable 
{
	private bool _isStarted;

	 
	public void StartFrom<T>() where T : FSMState
	{
		if (_isStarted)
			throw new Exception("already started");

		_isStarted = true;
		Switch(this.State<T>());
	}
}