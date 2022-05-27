#include <iostream>
#include <stdio.h>

#include "structs.h"
#include "box2d.h"
#include "Box2DToUnityWrapper.h"

#include <stdio.h>
#include <string>

using namespace std;

int main()
{
     cout << "Initialize physics World!" << endl;

     FILE* file = fopen("./dbg.log", "wt");


     Vector2 gravity{ 0.0f, 0.0f };
     b2World* world = CreateWorld(gravity);
     b2Body* ground = CreateBody(world, b2_staticBody, Vector2{ 0, -10 }, 0, 0);
     b2Body* box = CreateBody(world, b2_dynamicBody, Vector2{ 0, -5 }, 1, 0.3f);

     b2Body** bodies = new b2Body*[2];

     fprintf(file, "before cast %p - %p\n", box, ground);

     auto bodiesRef = reinterpret_cast<void**>(bodies);

     bodies[0] = box;
     bodies[1] = ground;

     fprintf(file, "after cast %p - %p\n", bodies[0], bodies[1]);

     float timeStep = 1.0f / 60.0f;
     int32 velocityIterations = 6;
     int32 positionIterations = 2;

     for(int32 i = 0; i < 100; i++)
     {
         UpdateWorld(world, timeStep, velocityIterations, positionIterations);
         world = CloneWorld(bodiesRef, 2, world);
         Vector2 position = GetPosition(box);
         float angle = GetAngle(box);
         //fprintf(file, "%4.2f %4.2f %4.2f\n", position.x, position.y, angle);
     }
     fflush(file);

     fclose(file);

     delete bodies;
     bodies = nullptr;
     return 0;
}

