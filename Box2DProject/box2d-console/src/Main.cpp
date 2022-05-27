#include <iostream>

#include "structs.h"
#include "box2d.h"
#include "Box2DToUnityWrapper.h"

using namespace std;

int main()
{
    cout << "Initialize physics World!" << endl;

     Vector2 gravity{ 0.0f, 0.0f };
     b2World* world = CreateWorld(gravity);
     b2Body* ground = CreateBody(world, b2_staticBody, Vector2{ 0, -10 }, 0, 0);
     b2Body* box = CreateBody(world, b2_dynamicBody, Vector2{ 0, -5 }, 1, 0.3f);

     float timeStep = 1.0f / 60.0f;
     int32 velocityIterations = 6;
     int32 positionIterations = 2;

     for(int32 i = 0; i < 500; i++)
     {
         UpdateWorld(world, timeStep, velocityIterations, positionIterations);
         Vector2 position = GetPosition(box);
         float angle = GetAngle(box);

         printf("%4.2f %4.2f %4.2f\n", position.x, position.y, angle);
     }

     
     return 0;
}

