using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.ClientServer.Physics;
using Game.ClientServer.Physics.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Game.View;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Client
{
    public static class Box2DServices
    {
        public static void abc()
        {
            
        }
    }
    public static class ClientBox2DServices 
    {
        public static void CreateBody(EcsWorld world, int entity, Collider2D collider)
        {
            var go = collider.gameObject;
            var rigidBody = go.GetComponent<Rigidbody2D>();
            if (!rigidBody) 
                return;
            

            entity.EntityReplace<PositionComponent>(world).value = go.transform.position;
            entity.EntityReplace<RotationComponent>(world).value = - go.transform.eulerAngles.y * Mathf.Deg2Rad;

            ref var rigidBodyDefinitionComponent =
                ref entity.EntityAddComponent<RigidbodyDefinitionComponent>(world);
            
            if (go.TryGetComponent(out Box2dPhysicsExtend rigidbodyExtend))
            {
                rigidBodyDefinitionComponent.Density = rigidbodyExtend.Density;
                rigidBodyDefinitionComponent.RestitutionThreshold = rigidbodyExtend.RestitutionThreshold;
                rigidBodyDefinitionComponent.LinearDamping = rigidbodyExtend.LinearDamping;
                rigidBodyDefinitionComponent.AngularDamping = rigidbodyExtend.AngularDamping;
            }

            var type = rigidBody.bodyType;
            var b2Type = BodyType.Static;
            if (type == RigidbodyType2D.Dynamic)
                b2Type = BodyType.Dynamic;
            else if (type == RigidbodyType2D.Kinematic)
                b2Type = BodyType.Kinematic;

            rigidBodyDefinitionComponent.BodyType = b2Type;
            var material = rigidBody.sharedMaterial;
            if (material)
            {
                rigidBodyDefinitionComponent.Friction = material.friction;
                rigidBodyDefinitionComponent.Restitution = material.bounciness;
            }

            rigidBodyDefinitionComponent.IsTrigger = collider.isTrigger;
            
            
            if (collider is BoxCollider2D)
            {
                ref var boxColliderComponent = ref entity.EntityAddComponent<BoxColliderComponent>(world);
                boxColliderComponent.Size = go.transform.lossyScale;
            }

            if (collider is CircleCollider2D)
            {
                ref var circleColliderComponent = ref entity.EntityAddComponent<CircleColliderComponent>(world);
                circleColliderComponent.Radius = collider.transform.lossyScale.x / 2;
            }

            if (collider is PolygonCollider2D)
            {
                ref var polygonColliderComponent = ref entity.EntityAddComponent<PolygonColliderComponent>(world);
                PhysicsShapeGroup2D physicsShapeGroup2D = new PhysicsShapeGroup2D();
                var shapes = collider.GetShapes(physicsShapeGroup2D);

                polygonColliderComponent.Anchors = new int[shapes];
                polygonColliderComponent.Vertices = new List<Vector2>();
                var vertices = new List<Vector2>();
                for (int i = 0; i < shapes; i++)
                {
                    physicsShapeGroup2D.GetShapeVertices(i, vertices);
                    polygonColliderComponent.Anchors[i] = vertices.Count - 1;
                    foreach (var vector2 in vertices)
                    {
                        polygonColliderComponent.Vertices.Add(vector2);
                    }
                }
            }
            
            if (Application.isPlaying)
            {
                Object.DestroyImmediate(collider);
                Object.DestroyImmediate(rigidBody);
            }
        }
    }
}