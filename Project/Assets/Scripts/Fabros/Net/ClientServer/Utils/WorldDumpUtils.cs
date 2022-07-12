using System;
using System.Globalization;
using Fabros.Ecs.ClientServer.WorldDiff;
using Flow.EcsLite;

namespace Game.Fabros.Net.ClientServer.Utils
{
    public class WorldDumpUtils
    {
        public static string DumpEntity(ComponentsCollection Pool, int entity, EcsWorld world)
        {
            var raw = world.GetRawEntities();
            
            var str = $"entity #{entity} gen={raw[entity].Gen}\n";
            object[] list = null;

            for (int i = 0; i < Pool.Components.Count; ++i)
            {
                var spool = Pool.Components[i];
                var pool = world.GetPoolByType(spool.GetComponentType());
                if (pool == null)
                    continue;
                var tp = spool.GetComponentType();
                var fields = tp.GetFields();
                
                if (!pool.Has(entity))
                    continue;
                    

                var component = pool.GetReadRaw(entity);
                var componentStr = "";
                for (int f = 0; f < fields.Length; ++f)
                {
                    var name = fields[f];
                    var field = tp.GetField(name.Name);
                    
                    var val = field.GetValue(component);
                    if (field.FieldType == typeof(float))
                    {
                        float fl = Convert.ToSingle(val);
                        val = fl.ToString("F2", CultureInfo.InvariantCulture.NumberFormat);
                    }

                    componentStr += $"    {name.Name} = {val}\n";
                }
                str += $"  {tp.Name}\n{componentStr}\n";
            }
            

            return str;
        }
        
        public static string DumpWorld(ComponentsCollection Pool, EcsWorld world)
        {
            int[] entities = null;
            int count = world.GetAllEntities(ref entities);
            string str = "";
            for (int i = 0; i < count; ++i)
            {
                int entity = entities[i];
                str += $"{DumpEntity(Pool, entity, world)}\n\n";
            }

            return str;
        }
    }
}