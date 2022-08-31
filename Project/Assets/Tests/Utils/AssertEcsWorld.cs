using System.Collections;
using System.Linq;
using NUnit.Framework;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;

namespace Tests.Runtime.utils
{
    public static class AssertEcsWorld
    {
        public static void DiffsAreEqual(WorldDiff d1, WorldDiff d2)
        {
            Assert.AreEqual(d1.CreatedEntities, d2.CreatedEntities);
            Assert.AreEqual(d1.CreatedEntitiesGen, d2.CreatedEntitiesGen);
            Assert.AreEqual(d1.RemovedEntities, d2.RemovedEntities);
            Assert.AreEqual(d1.Groups.Count, d2.Groups.Count);
            
            for (var i = 0; i < d1.Groups.Count; i++)
            {
                var g1 = d1.Groups[i];
                var g2 = d2.Groups[i];
                Assert.AreEqual(g1.ChangedEntities, g2.ChangedEntities);
                Assert.AreEqual(g1.RemovedFromEntities, g2.RemovedFromEntities);
                Assert.AreEqual(g1.Data, g2.Data);
            }
            
            Assert.AreEqual(d1.InnerWorldEntity, d2.InnerWorldEntity);
            Assert.AreEqual(d1.InnerWorldsDiff.Count, d2.InnerWorldsDiff.Count);

            for (int i = 0; i < d1.InnerWorldsDiff.Count; ++i)
            {
                var innerD1 = d1.InnerWorldsDiff[i];
                var innerD2 = d2.InnerWorldsDiff[i];
                DiffsAreEqual(innerD1, innerD2);
            }
        }
        
        private static void StructsDeepAreEqual<T>(T t1, T t2)
        {
            var eq = StructuralComparisons.StructuralEqualityComparer.Equals(t1, t2);
            Assert.IsTrue(eq);
        }

        public static void FilterIsCorrect<T>(EcsWorld world) where T:struct
        {
            Assert.AreEqual(world.GetPool<T>().GetEntities(), world.Filter<T>().End().GetEntities());
        }
        
        public static void FilterIsCorrect<T1, T2>(EcsWorld world) where T1:struct where T2:struct
        {
            var intersection = world.GetPool<T1>().GetEntities().ToArray();
            intersection = intersection.Intersect(world.GetPool<T2>().GetEntities()).ToArray();
            
            var entities = world.Filter<T1>().Inc<T2>().End().GetEntities();

            Assert.AreEqual(intersection, entities);
        }
        
        public static void FilterIsCorrect<T1, T2, T3>(EcsWorld world) where T1:struct where T2:struct where T3:struct
        {
            var intersection = world.GetPool<T1>().GetEntities().ToArray();
            intersection = intersection.Intersect(world.GetPool<T2>().GetEntities()).ToArray();
            intersection = intersection.Intersect(world.GetPool<T3>().GetEntities()).ToArray();
            
            var entities = world.Filter<T1>().Inc<T2>().Inc<T3>().End().GetEntities();

            Assert.AreEqual(intersection, entities);
        }


        public static void WorldsAreNotEqual(ComponentsCollection collection, EcsWorld srcWorld, EcsWorld destWorld)
        {
            Assert.Throws<AssertionException>(()=>WorldsAreEqual(collection, srcWorld, destWorld));
        }
        
        public static void WorldsAreEqual(ComponentsCollection collection, EcsWorld srcWorld, EcsWorld destWorld)
        {
            //var w1str = LeoDebug.e2s(srcWorld);
            //var w2str = LeoDebug.e2s(destWorld);
        
            var d1 = WorldDiff.BuildDiff(collection, new EcsWorld(), srcWorld);
            var d2 = WorldDiff.BuildDiff(collection, new EcsWorld(), destWorld);
            DiffsAreEqual(d1, d2);
     
        
            //check that filters are not broken internally
            FilterIsCorrect<ComponentA>(destWorld);
            FilterIsCorrect<ComponentB>(destWorld);
            FilterIsCorrect<ComponentC>(destWorld);
            FilterIsCorrect<ComponentA, ComponentB>(destWorld);
            FilterIsCorrect<ComponentA, ComponentC>(destWorld);
            FilterIsCorrect<ComponentB, ComponentA>(destWorld);
            FilterIsCorrect<ComponentB, ComponentC>(destWorld);
            FilterIsCorrect<ComponentC, ComponentA>(destWorld);
            FilterIsCorrect<ComponentA, ComponentB, ComponentC>(destWorld);
        }
    }
}