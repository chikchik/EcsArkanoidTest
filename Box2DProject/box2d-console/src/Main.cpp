#include <iostream>
#include <stdio.h>

#include "structs.h"
#include "box2d.h"
#include "Box2DToUnityWrapper.h"
#include "distance_joint.h"
#include "joint.h"

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

b2Body** CreateBoxes(int boxCount, b2World* world, Vector2 startPos, float offset)
{
    b2Body** bodies = new b2Body*[boxCount];
    for (int i = 0; i < boxCount; ++i)
    {
        bodies[i] = CreateBox(world, startPos);
        startPos.x += startPos.x + offset;
        startPos.y += startPos.y + offset;
    }
    return bodies;
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

     

     b2Body** boxes = CreateBoxes(3, world, Vector2 {10, 10}, 5);

     float timeStep = 1.0f / 60.0f;
     int32 velocityIterations = 6;
     int32 positionIterations = 2;
     bool flag = true;
     Vector2 scale{ 1, 1 };
     Vector2 force{ 0, -200 };

     cout << boxes[1] << endl << boxes[2] << endl;

     b2Body* b1 = boxes[1];
     b2Body* b2 = boxes[2];

     b2Joint* j = CreateJoint(world, b2JointType::e_mouseJoint, boxes[1], boxes[2], true);
     SetJointStiffness(j, 10);
     SetJointMaxLength(j, 50);


     SetLinearVelocity(boxes[1], force);
     SetAngularVelocity(boxes[1], 20);

     auto bodiesRef = reinterpret_cast<void**>(boxes);


     for(int32 i = 0; i < 1000; i++)
     {
         UpdateWorld(world, timeStep, velocityIterations, positionIterations);
         world = CloneWorld(bodiesRef, 2, world);
         Vector2 position = GetPosition(boxes[2]);
         float angle = GetAngle(boxes[2]);

         
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

         fprintf(file, "%4.2f %4.2f %4.2f\n", position.x, position.y, angle);
     }
     fflush(file);

     fclose(file);

     delete boxes;
     boxes = nullptr;
     DestroyWorld(world);
     return 0;
}

