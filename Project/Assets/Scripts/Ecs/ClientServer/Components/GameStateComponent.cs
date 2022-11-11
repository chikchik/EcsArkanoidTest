using System;

namespace Game.Ecs.ClientServer.Components
{
    public enum GameState
    {
        None,
        Init,
        Play,
        End
    }

    [Serializable]
    public struct GameStateComponent
    {
        public GameState Value;
    }
}