/*
 * simulationwindow.h
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

#ifndef SIMULATIONWINDOW_H
#define SIMULATIONWINDOW_H

#include "constants.h"
#include "storewindow.h"

#include <SFML/Graphics.hpp>
#include <Box2D/Box2D.h>

#include <QtWidgets>
#include <QSound>
#include <QMediaPlayer>

namespace Ui {
class SimulationWindow;
}

class SimulationWindow : public QMainWindow, public sf::RenderWindow
{
    Q_OBJECT

public:
    explicit SimulationWindow(
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());
    ~SimulationWindow();

protected:
    void sfmlUpdate();

    void closeEvent(QCloseEvent *event) override;

private slots:
    void on_firePushButton_clicked();
    void on_levelOneRadioButton_clicked();
    void on_levelTwoRadioButton_clicked();
    void on_levelThreeRadioButton_clicked();
    void on_storePushbutton_clicked();
    void on_pauseRadioButton_clicked();
    void on_youtubePushButton_clicked();

    void showSolution();

private:
    Ui::SimulationWindow *ui;
    StoreWindow *storeWindow;

    QTimer *sfmlTimer;
    QList<QProcess *> childProcesses;

    b2World world;

    sf::Texture boxTexture, targetTexture, backgroundTexture;
    sf::Font arialFont;

    QSound *cheerSound, *booSound, *shootSound;
    QMediaPlayer *backgroundMusic;

    float targetXPosition;
    bool isP2WBall;
    int currentScore;

    void handleLevelChange(
            bool isAngleControlEnable,
            bool isVelocityControlEnable,
            QString musicFileName,
            QString backgroundFileName,
            QString prompt);

    void createCannonBall(b2Vec2 pos, b2Vec2 vel, bool isP2W);

    void drawBackground();
    void drawTarget();
    void drawTargetDistance();
    void drawBodies();

    void resetInitialInputParameters();
    void moveTargetToNewRandomLocation();

    float getCannonBallWidth(bool isP2W);
    float getCannonBallHeight(bool isP2W);
    bool isbelowXAxis(float yPosition);
    bool isHitTarget(float xPosition, bool isP2W);
};

#endif // SIMULATIONWINDOW_H
