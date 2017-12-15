/*
 * solver.cpp
 *
 * Anthony Chyr (u0627375)
 * Carlos Enrique Guerra Chan (u0847821)
 * Elliot C Carr-Lee (u0549837)
 * Ismael Kadilo Wa Ngoie (u1120347)
 * Jared Earl (u1120117)
 * Kameron Service (u0963620)
 * Wesley Barth (u0488618)
 *
 * CS 3505 Software Practices II Johnson
 * A8: An Agile Educational Application
 */

#define _USE_MATH_DEFINES

#include "solver.h"

#include "math.h"

using namespace std;

const float DEG2RAD = (float)M_PI / 180.0f;
const float RAD2DEG = 180.0f / (float)M_PI;

double solver::distanceFromAngleVelocity(
        double angleDeg, double velocityMeterPerSec)
{
    return velocityMeterPerSec * velocityMeterPerSec
            * sin(2 * angleDeg * DEG2RAD) / ACCELERATION_GRAVITY;
}

double solver::angleFromDistanceVelocity(
        double distanceInMeter, double velocityMeterPerSec)
{
    return asin(distanceInMeter * ACCELERATION_GRAVITY
                / (velocityMeterPerSec * velocityMeterPerSec)) * 0.5 * RAD2DEG;
}

double solver::velocityFromDistanceAngle(
        double distanceInMeter, double angleDeg)
{
    return sqrt(distanceInMeter * ACCELERATION_GRAVITY
                / sin(2 * angleDeg * DEG2RAD));
}
