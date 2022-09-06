using System;
using System.Runtime.InteropServices;
using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace Game.Ecs.ClientServer.Components
{
    [ForceJsonSerialize]
    [Serializable]
    public struct GameObjectNameComponent
    {
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] 
        public string Name;
        //public int Id;
    }
}