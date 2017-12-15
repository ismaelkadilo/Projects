/*
 * solver.h
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

#ifndef SOLVER_H
#define SOLVER_H

namespace solver
{
    const float ACCELERATION_GRAVITY = 9.81f;

    double distanceFromAngleVelocity(double angleDeg,
                                     double velocityMeterPerSec);
    double angleFromDistanceVelocity(double distanceInMeter,
                                     double velocityMeterPerSec);
    double velocityFromDistanceAngle(double distanceInMeter,
                                     double angleDeg);
}

#endif // SOLVER_H
