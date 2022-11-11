using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputLoginComponent : IInputComponent, IComplexComponent<InputLoginComponent>
    {
        public string Nickname;

        public void Write(HGlobalWriter writer)
        {
            writer.WriteString(Nickname);
        }

        public void ReadTo(HGlobalReader reader, ref InputLoginComponent result)
        {
            result.Nickname = reader.ReadString();
        }

        public void CopyRefsTo(ref InputLoginComponent to)
        {
        }
    }
}