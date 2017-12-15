/*
 * constants.h
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

#ifndef CONSTANTS_H
#define CONSTANTS_H

#define _USE_MATH_DEFINES
#include "math.h"

#include <SFML/Graphics.hpp>
#include <Box2D/Box2D.h>

namespace constants
{
const float METER_TO_PIXEL = 30.0f;
const float PIXEL_TO_METER = 1.0f / METER_TO_PIXEL;

const unsigned int SIMULATION_WINDOW_WIDTH = 800;
const unsigned int SIMULATION_WINDOW_HEIGHT = 600;

const float ACCELERATION_GRAVITY = 9.81f;
const b2Vec2 GRAVITY(0.0f, ACCELERATION_GRAVITY);
const int FRAME_RATE = 60;

const float CANNONBALL_SPRITE_WIDTH = 128.0f;
const float CANNONBALL_SPRITE_HEIGHT = 128.0f;

const float NORMAL_CANNON_BALL_WIDTH = 48.0f;
const float NORMAL_CANNON_BALL_HEIGHT = 48.0f;

const float P2W_CANNON_BALL_WIDTH = 2.0f * NORMAL_CANNON_BALL_WIDTH;
const float P2W_CANNON_BALL_HEIGHT = 2.0f * NORMAL_CANNON_BALL_HEIGHT;

const float TARGET_SPRITE_WIDTH = 640.0f;
const float TARGET_SPRITE_HEIGHT = 160.0f;

const float TARGET_DISPLAY_WIDTH = 80.0f;
const float TARGET_DISPLAY_HEIGHT = 20.0f;

const int MIN_TARGET_DISTANCE_PX = 50;
const int MAX_TARGET_DISTANCE_PX = \
        SIMULATION_WINDOW_WIDTH - (int)TARGET_DISPLAY_WIDTH / 2;

const float ORIGIN_X_PX = MIN_TARGET_DISTANCE_PX;
const float ORIGIN_Y_PX = \
        SIMULATION_WINDOW_HEIGHT - TARGET_DISPLAY_HEIGHT / 2;

const float DEG2RAD = (float)M_PI / 180.0f;
const float RAD2DEG = 180.0f / (float)M_PI;

const int HIT_SCORE_CHANGE = 10;
const int MISS_SCORE_CHANGE = -10;
}

#endif // CONSTANTS_H
