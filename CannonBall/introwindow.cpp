/*
 * introwindow.cpp
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

#include <QCoreApplication>
#include <QProcess>

/*
 * public
 */
IntroWindow::IntroWindow(
        QWidget *parent,
        Qt::WindowFlags flags) :
    QMainWindow(parent, flags),
    webView(new QWebEngineView(this))
{
    setWindowTitle(tr("Intro"));
    setAttribute(Qt::WA_DeleteOnClose, true);
    setCentralWidget(webView);
    webView->load(QUrl("qrc:/assets/intro.webm"));
}

IntroWindow::~IntroWindow()
{
    // QWebEngineView and SFML's Render Window both tries to occupy the OpenGL.
    // Have them operate in separate processes where intro starts the rest of
    // the application after it closes.

    // start the same process with the argument "-skip" to skip the intro
    QStringList arguments;
    arguments << "-skip";

    QProcess *simulationWindowProcess = new QProcess;
    simulationWindowProcess->start(QCoreApplication::applicationFilePath(),
                                   arguments);
}
