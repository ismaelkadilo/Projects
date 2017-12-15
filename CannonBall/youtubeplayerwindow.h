/*
 * youtubeplayerwindow.h
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

#ifndef YOUTUBEPLAYERWINDOW_H
#define YOUTUBEPLAYERWINDOW_H

#include <QMainWindow>
#include <QWebEngineView>

class YouTubePlayerWindow : public QMainWindow
{
    Q_OBJECT

public:
    YouTubePlayerWindow(
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());

private:
    QWebEngineView *webView;
};

#endif // YOUTUBEPLAYERWINDOW_H
