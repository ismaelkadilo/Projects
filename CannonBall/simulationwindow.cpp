/*
 * simulationwindow.cpp
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

#include "simulationwindow.h"
#include "ui_simulationwindow.h"

#include "tools/solver.h"

#include "sstream"
#include <math.h>
#include <vector>

using namespace constants;
using namespace solver;
using namespace std;

/*
 * public
 */
SimulationWindow::SimulationWindow(
        QWidget *parent,
        Qt::WindowFlags flags) :
    QMainWindow(parent, flags),
    ui(new Ui::SimulationWindow),
    sf::RenderWindow(
        sf::VideoMode(SIMULATION_WINDOW_WIDTH, SIMULATION_WINDOW_HEIGHT),
        "Cannonball Simulator",
        sf::Style::Titlebar),
    world(GRAVITY),
    isP2WBall(false),
    currentScore(0)
{
    // have a different random target distance
    srand(time(nullptr));
    moveTargetToNewRandomLocation();

    // prepare simulation window
    setFramerateLimit(FRAME_RATE);

    // prepare sfml assets
    QResource boxResource(":/assets/JhonsonBall_scaled.png");
    boxTexture.loadFromMemory(boxResource.data(), boxResource.size());

    QResource targetResource(":assets/target.png");
    targetTexture.loadFromMemory(targetResource.data(), targetResource.size());

    QResource arialResource(":assets/arial.ttf");
    arialFont.loadFromMemory(arialResource.data(), arialResource.size());

    // start simulation
    sfmlTimer = new QTimer(this);
    connect(sfmlTimer, &QTimer::timeout, [=]{ if (isOpen()) sfmlUpdate(); });
    sfmlTimer->setInterval(1000/(2*FRAME_RATE));
    sfmlTimer->start();


    // create the store
    storeWindow = new StoreWindow(new StoreModel, this);
    connect(storeWindow, &StoreWindow::cheaterCannonBallToggled,
            [=]{ isP2WBall = true; });
    connect(storeWindow, &StoreWindow::doSolution,
            this, &SimulationWindow::showSolution);
    connect(storeWindow, &StoreWindow::skinSelected, [=](QString resStr){
        QResource resource(resStr);
        boxTexture.loadFromMemory(resource.data(), resource.size());
    });

    // create the sound assets
    booSound = new QSound(":/assets/booSound.wav", this);
    cheerSound = new QSound(":/assets/cheerSound.wav", this);
    shootSound = new QSound(":/assets/shootSound.wav", this);

    backgroundMusic = new QMediaPlayer;

#ifdef WIN32
    // create the music assets in a location accessible via windows DirectShow
    QFile::copy(":/assets/level1.mp3", "./level1.mp3");
    QFile::copy(":/assets/level2.mp3", "./level2.mp3");
    QFile::copy(":/assets/level3.mp3", "./level3.mp3");
#endif

    // create gui
    ui->setupUi(this);

    // initial actions
    ui->pauseRadioButton->click();
}

SimulationWindow::~SimulationWindow()
{
    delete ui;
}

/*
 * protected
 */
void SimulationWindow::sfmlUpdate()
{

    // advance the Box2D world simulation
    world.Step(1/60.f, 8, 3);

    // draw the sfml frame
    clear(sf::Color::White);
    drawBackground();
    drawTarget();
    drawTargetDistance();
    drawBodies();

    display();
}

void SimulationWindow::closeEvent(QCloseEvent *event)
{
    Q_UNUSED(event);

    // close the child processes
    for (QProcess *process : childProcesses) process->close();
}

/*
 * private slots
 */
void SimulationWindow::on_firePushButton_clicked()
{
    // disable the fire button
    ui->firePushButton->setEnabled(false);

    // sound effects
    shootSound->play();

    // Box2D's coordinate system is flipped so that +y is down
    b2Vec2 initialPos(
                ORIGIN_X_PX * PIXEL_TO_METER,
                ORIGIN_Y_PX * PIXEL_TO_METER);
    b2Vec2 initialVel(ui->initialVelocityDoubleSpinbox->value()
                      * cos(ui->initialAngleDoubleSpinBox->value() * DEG2RAD),
                      -1.0 * ui->initialVelocityDoubleSpinbox->value()
                      * sin(ui->initialAngleDoubleSpinBox->value() * DEG2RAD));
    createCannonBall(initialPos, initialVel, isP2WBall);
    isP2WBall = false;
}

void SimulationWindow::on_pauseRadioButton_clicked()
{
    backgroundMusic->stop();
    ui->initialAngleDoubleSpinBox->setEnabled(false);
    ui->initialVelocityDoubleSpinbox->setEnabled(false);
    ui->tutorialTextBrowser->setText(tr(""));
}

void SimulationWindow::on_levelOneRadioButton_clicked()
{
    handleLevelChange(
        false,
        true,
        "level1.mp3",
        "level1Background.jpg",
        tr("<h1>Level 1</h1>"
           "<p>In this level you must find the initial velocity with the angle "
           "fixed at 45 deg such that the cannon ball will hit the target.</p>"
           "<p>Hint: there is only one solution.</p>"));
}

void SimulationWindow::on_levelTwoRadioButton_clicked()
{
    handleLevelChange(
        true,
        false,
        "level2.mp3",
        "level2Background.jpg",
        tr("<h1>Level 2</h1>"
           "<p>In this level you must find the initial angle with the velocity "
           "fixed such that the cannon ball will hit the target.</p>"
           "<p>Hint: there are two solutions.</p>"));
}

void SimulationWindow::on_levelThreeRadioButton_clicked()
{
    handleLevelChange(
        true,
        true,
        "level3.mp3",
        "level3Background.jpg",
        tr("<h1>Level 3</h1>"
           "<p>In this level you must find the initial angle and velocity "
           "such that the cannon ball will hit the target.</p>"
           "<p>Hint: there are an infinite number of solutions.</p>"));
}

void SimulationWindow::on_storePushbutton_clicked()
{
    storeWindow->show();
}

void SimulationWindow::on_youtubePushButton_clicked()
{
    // QWebEngineView and SFML's Render Window both tries to occupy the OpenGL.
    // Have them operate in separate processes.

    // start the same process with the argument "-skip" to skip the intro
    // and "-youtube" to go to youtube.
    QStringList arguments;
    arguments << "-skip" << "-youtube";

    // start up the process
    QProcess *process = new QProcess;
    process->start(QCoreApplication::applicationFilePath(),
                   arguments);

    // record it in the list of child processes
    childProcesses.push_back(process);
}

void SimulationWindow::showSolution()
{
    enum level {zero, one, two, three};
    level currentLevel;
    if (ui->pauseRadioButton->isChecked()) currentLevel = zero;
    if (ui->levelOneRadioButton->isChecked()) currentLevel = one;
    if (ui->levelTwoRadioButton->isChecked()) currentLevel = two;
    if (ui->levelThreeRadioButton->isChecked()) currentLevel = three;

    QString response;
    switch (currentLevel)
    {
    case zero:
        response = "Game is paused, no solution";
        break;
    case one:
        response = tr("Velocity [m/s]: %1").arg(
                    velocityFromDistanceAngle(
                        targetXPosition * PIXEL_TO_METER,
                        ui->initialAngleDoubleSpinBox->value()));
        break;
    case two:
        response = tr("Angle [Deg]: %1").arg(
                    angleFromDistanceVelocity(
                        targetXPosition * PIXEL_TO_METER,
                        ui->initialVelocityDoubleSpinbox->value()));
        break;
    case three:
        response = tr("Angle [Deg]: 45; Velocity [m/s]: %1").arg(
                    velocityFromDistanceAngle(
                        targetXPosition * PIXEL_TO_METER,
                        45.0));
        break;
    default:
        response = tr("Unknown Level");
        break;
    }

    QMessageBox::information(this, "Solution", response);
}

/*
 * private
 */
void SimulationWindow::handleLevelChange(
        bool isAngleControlEnable,
        bool isVelocityControlEnable,
        QString musicFileName,
        QString backgroundFileName,
        QString prompt)
{
    // music
#ifdef WIN32
    backgroundMusic->setMedia(QUrl::fromLocalFile("./" + musicFileName));
#else defined(MACX)
    backgroundMusic->setMedia(QUrl("qrc:/assets/" + musicFileName));
#endif
    backgroundMusic->play();

    // controls
    ui->initialAngleDoubleSpinBox->setEnabled(isAngleControlEnable);
    ui->initialVelocityDoubleSpinbox->setEnabled(isVelocityControlEnable);
    resetInitialInputParameters();

    // prompt
    ui->tutorialTextBrowser->setText(prompt);

    // background
    QResource backgroundResource(":/assets/" + backgroundFileName);
    backgroundTexture.loadFromMemory(backgroundResource.data(),
                                     backgroundResource.size());
}

void SimulationWindow::createCannonBall(b2Vec2 pos, b2Vec2 vel, bool isP2W)
{
    float width = isP2W ? P2W_CANNON_BALL_WIDTH : NORMAL_CANNON_BALL_WIDTH;
    float height = isP2W ? P2W_CANNON_BALL_HEIGHT : NORMAL_CANNON_BALL_HEIGHT;

    b2BodyDef bodyDef;
    bodyDef.position = pos;
    bodyDef.linearVelocity = vel;
    bodyDef.type = b2_dynamicBody;

    b2PolygonShape shape;
    shape.SetAsBox((width/2.0f) * PIXEL_TO_METER,
                   (height/2.0f) * PIXEL_TO_METER);
    b2FixtureDef fixtureDef;
    fixtureDef.density = 0.0f;
    fixtureDef.friction = 0.7f;
    fixtureDef.shape = &shape;

    b2Body *body = world.CreateBody(&bodyDef);
    body->CreateFixture(&fixtureDef);
    body->SetUserData((void *)isP2W);
}

void SimulationWindow::drawBackground()
{
    sf::Sprite background;
    background.setTexture(backgroundTexture);
    draw(background);
}

void SimulationWindow::drawTarget()
{
    sf::Sprite target;
    target.setTexture(targetTexture);
    target.setTextureRect(sf::Rect<int>(0,
                                        0,
                                        TARGET_SPRITE_WIDTH,
                                        TARGET_SPRITE_HEIGHT));
    target.setScale(sf::Vector2f(TARGET_DISPLAY_WIDTH/TARGET_SPRITE_WIDTH,
                                 TARGET_DISPLAY_HEIGHT/TARGET_SPRITE_HEIGHT));
    target.setOrigin(TARGET_SPRITE_WIDTH/2.0f, TARGET_SPRITE_HEIGHT/2.0f);
    target.setPosition(targetXPosition,
                       SIMULATION_WINDOW_HEIGHT - TARGET_DISPLAY_HEIGHT);
    target.setRotation(0.0f);
    draw(target);
}

void SimulationWindow::drawTargetDistance()
{
    stringstream targetDistanceSS;
    targetDistanceSS << "Distance: " << targetXPosition * PIXEL_TO_METER << "m";

    sf::Text targetDistanceText;
    targetDistanceText.setFont(arialFont);
    targetDistanceText.setString(targetDistanceSS.str());
    targetDistanceText.setCharacterSize(24);
    targetDistanceText.setFillColor(sf::Color::Red);
    targetDistanceText.setStyle(sf::Text::Bold);
    draw(targetDistanceText);
}

void SimulationWindow::drawBodies()
{
    vector<b2Body *> b2BodyToDestory;
    int bodyCount = 0;
    for (b2Body *bodyIterator = world.GetBodyList();
         bodyIterator != 0;
         bodyIterator = bodyIterator->GetNext())
    {
        bool isP2W = (bool)bodyIterator->GetUserData();
        float xPos = bodyIterator->GetPosition().x;
        float yPos = bodyIterator->GetPosition().y;

        sf::Sprite sprite;

        switch (bodyIterator->GetType())
        {
        case b2_dynamicBody:
            sprite.setTexture(boxTexture);
            sprite.setTextureRect(sf::Rect<int>(0,
                                                0,
                                                CANNONBALL_SPRITE_WIDTH,
                                                CANNONBALL_SPRITE_HEIGHT));
            sprite.setScale(sf::Vector2f(
                    getCannonBallWidth(isP2W)/CANNONBALL_SPRITE_WIDTH,
                    getCannonBallHeight(isP2W)/CANNONBALL_SPRITE_HEIGHT));
            sprite.setOrigin(getCannonBallWidth(isP2W)/2.0f,
                             getCannonBallHeight(isP2W)/2.0f);

            if (isbelowXAxis(yPos))
            {
                // determine whether the ball hit the target
                currentScore += isHitTarget(xPos, isP2W)
                        ? HIT_SCORE_CHANGE : MISS_SCORE_CHANGE;
                ui->currentScoreLabel->setText(tr("%1").arg(currentScore));
                (isHitTarget(xPos, isP2W) ? cheerSound : booSound)->play();

                // destroy the body and find a new target
                b2BodyToDestory.push_back(bodyIterator);
                moveTargetToNewRandomLocation();

                // renable the fire button
                ui->firePushButton->setEnabled(true);
            }

            ++bodyCount;
            break;
        }

        sprite.setPosition(bodyIterator->GetPosition().x * METER_TO_PIXEL,
                           bodyIterator->GetPosition().y * METER_TO_PIXEL);
        sprite.setRotation(bodyIterator->GetAngle() * RAD2DEG);

        draw(sprite);
    }

    // clear the bodies to destroy
    for (b2Body *body : b2BodyToDestory)
    {
        world.DestroyBody(body);
    }
}

void SimulationWindow::resetInitialInputParameters()
{
    ui->initialAngleDoubleSpinBox->setValue(45.0);
    ui->initialVelocityDoubleSpinbox->setValue(15.0);
}

void SimulationWindow::moveTargetToNewRandomLocation()
{
    targetXPosition = MIN_TARGET_DISTANCE_PX
            + (rand() % (MAX_TARGET_DISTANCE_PX -  MIN_TARGET_DISTANCE_PX + 1));
}

float SimulationWindow::getCannonBallWidth(bool isP2W)
{
    return isP2W ? P2W_CANNON_BALL_WIDTH : NORMAL_CANNON_BALL_WIDTH;
}

float SimulationWindow::getCannonBallHeight(bool isP2W)
{
    return isP2W ? P2W_CANNON_BALL_HEIGHT : NORMAL_CANNON_BALL_HEIGHT;
}

bool SimulationWindow::isbelowXAxis(float yPosition)
{
    return yPosition > ORIGIN_Y_PX * PIXEL_TO_METER;
}

bool SimulationWindow::isHitTarget(float xPosition, bool isP2W)
{
    return 2 * getCannonBallWidth(isP2W) >
            abs(xPosition * METER_TO_PIXEL - targetXPosition);
}
