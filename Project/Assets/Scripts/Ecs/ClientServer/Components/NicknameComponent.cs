using System;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct NicknameComponent : IComplexComponent<NicknameComponent>
    {
        public string Value;

        public void Write(HGlobalWriter writer)
        {
            writer.WriteString(Value);
        }

        public void ReadTo(HGlobalReader reader, ref NicknameComponent result)
        {
            result.Value = reader.ReadString();
        }

        public void CopyRefsTo(ref NicknameComponent to)
        {
        }
    }
}