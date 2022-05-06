#include <iostream>
#include "include/box2d/box2d.h"
#include "PhysicsCppDll.h"

using namespace std;

int main() {
    cout << "Initialize physics World!" << endl;

    // Vector2 gravity(0.0f, -9.8f);
    // b2World* world = CreateWorld(gravity);
    // b2Body* ground = CreateBody(world, b2_staticBody, Vector2(0, -10), Vector2(50, 9), 0, 0, 0);
    // b2Body* box = CreateBody(world, b2_dynamicBody, Vector2(0, 4), Vector2(1, 1), 12, 1, 0.3f);

    // float timeStep = 1.0f / 60.0f;
    // int32 velocityIterations = 6;
    // int32 positionIterations = 2;

    // for(int32 i = 0; i < 500; i++)
    // {
    //     UpdateWorld(world, timeStep, velocityIterations, positionIterations);
    //     Vector2 position = GetPosition(box);
    //     float angle = GetAngle(box);

    //     printf("%4.2f %4.2f %4.2f\n", position.x, position.y, angle);
    // }

    return 0;
}

