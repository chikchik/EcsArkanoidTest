using Fabros.Ecs.ClientServer.WorldDiff;
using Game.Ecs.ClientServer.Components;

namespace Game.ClientServer
{
    public class SharedComponents
    {
        public static ComponentsCollection CreateComponentsPool()
        {
            /*
             * общий код для клиента и сервера
             * CreateComponentsPool необходим, чтобы компоненты правильно передавались через сеть
             * если компонента в списке нет, то он не будет попадать в diff
            */

            var collection = new ComponentsCollection();
            ComponentsCollectionUtils.AddComponents(collection);

            return collection;
        }

    }
}