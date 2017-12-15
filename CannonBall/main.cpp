/*
 * main.cpp
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

#include "introwindow.h"
#include "simulationwindow.h"
#include "youtubeplayerwindow.h"

#include <QApplication>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);

    QWidget *w;
    switch (argc)
    {
    case 1:
        w = new IntroWindow;
        w->showMaximized();
        break;
    case 2:
        w = new SimulationWindow;
        w->show();
        break;
    case 3:
        w = new YouTubePlayerWindow;
        w->show();
        break;
    }

    return a.exec();
}
