using System;
using Game.UI.FSM;
using Zenject;

namespace Game.UI
{
	public partial class States : FSM.FSM, ITickable 
	{
		private bool _isStarted;
		/*
	private DiContainer container;

	public States(DiContainer container)
	{
		this.container = container;
	}*/
	 
		public void StartFrom<T>() where T : FSMState
		{
			if (_isStarted)
				throw new Exception("already started");

			_isStarted = true;
			Switch(this.State<T>());
		}

		protected override FSMState ResolveState(Type type)
		{
			return null;
		}
	}
}