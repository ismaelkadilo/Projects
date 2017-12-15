/*
 * youtubeplayerwindow.cpp
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

#include "youtubeplayerwindow.h"

/*
 * public
 */
YouTubePlayerWindow::YouTubePlayerWindow(
        QWidget *parent,
        Qt::WindowFlags flags) :
    QMainWindow(parent, flags),
    webView(new QWebEngineView(this))
{
    setWindowTitle(tr("YouTube"));
    setAttribute(Qt::WA_DeleteOnClose, true);
    setCentralWidget(webView);
    webView->load(QUrl("qrc:/assets/youtubePlayer.html"));
}
