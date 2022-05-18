#include <misc.h>

Vector2 GetUnityVectorFromB2d(const b2Vec2 &b2vec)
{
    Vector2 vec
    {
        b2vec.x,
        b2vec.y
    };
    return vec;
}