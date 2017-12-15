/*
 * introwindow.h
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

#ifndef INTROWINDOW_H
#define INTROWINDOW_H

#include <QMainWindow>
#include <QWebEngineView>

class IntroWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit IntroWindow(
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());
    ~IntroWindow();

private:
    QWebEngineView *webView;
};

#endif // INTROWINDOW_H
