#include <iostream>
#include <stdio.h>

#include "structs.h"
#include "box2d.h"
#include "Box2DToUnityWrapper.h"
#include "distance_joint.h"
#include "joint.h"
#include "mouse_joint.h"

#include <stdio.h>
#include <string>

using namespace std;

b2Body* CreateBox(b2World* world, Vector2 pos)
{
    b2Body* box = CreateBody(world, b2_dynamicBody, pos, 0, 1);

    AddFixtureToBody(box,
        CreateBoxShape(Vector2{ 1, 1 }),
        1,
        0, 0, 1, false, b2Filter());
    return box;
}

void CreateBoxes(void** arrayOldRefs, int boxCount, b2World* world, Vector2 startPos, float offset)
{
    for (int i = 0; i < boxCount; ++i)
    {
        arrayOldRefs[i] = (void*)CreateBox(world, startPos);
        startPos.x += startPos.x + offset;
        startPos.y += startPos.y + offset;
    }
}

void callbackCircle(Vector2, float, Box2dColor)
{

}

void callbackSegment(Vector2, Vector2, Box2dColor)
{

}

void callbackTransform(Vector2, Vector2, Box2dColor)
{

}

void callbackPolygon(Vector2[b2_maxPolygonVertices], int32, Box2dColor)
{

}

int main()
{
     cout << "Initialize physics World!" << endl;

     FILE* file = fopen("./dbg.log", "wt");


     Vector2 gravity{ 0.0f, 0.0f };
     b2World* world = CreateWorld(gravity);
     b2Body* ground = CreateBody(world, b2_staticBody, Vector2{ 0, 0 }, 0, 0);

     AddFixtureToBody(ground,
         CreateBoxShape(Vector2{ 1, 1 }),
         1,
         0, 0, 1, false, b2Filter());

     void** arrayOldRefs = new void* [4];

     CreateBoxes(arrayOldRefs, 3, world, Vector2 {10, 10}, 5);

     float timeStep = 1.0f / 60.0f;
     int32 velocityIterations = 6;
     int32 positionIterations = 2;
     bool flag = true;
     Vector2 scale{ 1, 1 };
     Vector2 force{ 0, -200 };

     cout << arrayOldRefs[1] << endl << arrayOldRefs[2] << endl;

     b2Body* b1 = (b2Body*)arrayOldRefs[1];
     b2Body* b2 = (b2Body*)arrayOldRefs[2];

     b2Joint* j = CreateJoint(world, b2JointType::e_distanceJoint,
         (b2Body*)arrayOldRefs[1], (b2Body*)arrayOldRefs[2], true);
     arrayOldRefs[3] = (void*)j;
     cout << GetJointMaxLength(j) << endl;



     SetLinearVelocity((b2Body*)arrayOldRefs[1], force);
     SetAngularVelocity((b2Body*)arrayOldRefs[1], 20);


     b2Draw* debugDraw = CreateBox2dDebugDraw(callbackCircle,
         callbackCircle, callbackSegment, callbackTransform, callbackPolygon);
     SetDebugDraw(world, debugDraw);


     for(int32 i = 0; i < 500; i++)
     {
         Vector2 position = GetJointAnchorA((b2Joint*)arrayOldRefs[3]);
         float angle = GetAngle((b2Body*)arrayOldRefs[2]);

         fprintf(file, "%4.2f %4.2f %4.2f\n", position.x, position.y, angle);
         //fprintf(file, "%p\n", arrayOldRefs[3]);
         fflush(file);

         UpdateWorld(world, timeStep, velocityIterations, positionIterations);
         b2World* cloneWorld = CloneWorld(arrayOldRefs, 3, world);
         DestroyWorld(world);
         world = cloneWorld;

         //fprintf(file, "%p\n", arrayOldRefs[3]);
         //fflush(file);
         SetDebugDraw(world, debugDraw);

         
         //ApplyForceToCenter(box, force, true);
         //ApplyTorque(box, 2000, true);


         //GetMass(box);
         //GetInertia(box);
         //SetFixedRotation(box, false);
         //IsFixedRotation(box);
         //IsAwake(box);
         //IsSleepingAllowed(box);
         //SetSleepingAllowed(box, false);
         //IsBullet(box);
         //GetType(box);
         //SetType(box, b2BodyType::b2_dynamicBody);
         //SetGravityScale(box, 1);
         //GetGravityScale(box);
         //SetAwake(box, flag);
         //ApplyTorque(box, 50.0f, flag);
         //ApplyAngularImpulse(box, 1, flag);
         //cout << dt << endl;
         //cout << position.x << " " << position.y << " " << angle << endl;

         
     }

     fclose(file);

     delete arrayOldRefs;
     arrayOldRefs = nullptr;
     DestroyWorld(world);
     return 0;
}

