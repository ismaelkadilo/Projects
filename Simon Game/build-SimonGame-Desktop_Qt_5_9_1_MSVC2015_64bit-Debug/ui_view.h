/********************************************************************************
** Form generated from reading UI file 'view.ui'
**
** Created by: Qt User Interface Compiler version 5.9.1
**
** WARNING! All changes made in this file will be lost when recompiling UI file!
********************************************************************************/

#ifndef UI_VIEW_H
#define UI_VIEW_H

#include <QtCore/QVariant>
#include <QtWidgets/QAction>
#include <QtWidgets/QApplication>
#include <QtWidgets/QButtonGroup>
#include <QtWidgets/QHeaderView>
#include <QtWidgets/QLabel>
#include <QtWidgets/QMainWindow>
#include <QtWidgets/QMenuBar>
#include <QtWidgets/QProgressBar>
#include <QtWidgets/QPushButton>
#include <QtWidgets/QSpinBox>
#include <QtWidgets/QStatusBar>
#include <QtWidgets/QToolBar>
#include <QtWidgets/QWidget>

QT_BEGIN_NAMESPACE

class Ui_View
{
public:
    QWidget *centralWidget;
    QPushButton *exitButton;
    QPushButton *redButton;
    QPushButton *blueButton;
    QPushButton *pushButton;
    QProgressBar *progressBar;
    QPushButton *greenButton;
    QPushButton *yellowButton;
    QSpinBox *spinBox;
    QLabel *label;
    QMenuBar *menuBar;
    QToolBar *mainToolBar;
    QStatusBar *statusBar;

    void setupUi(QMainWindow *View)
    {
        if (View->objectName().isEmpty())
            View->setObjectName(QStringLiteral("View"));
        View->resize(430, 383);
        centralWidget = new QWidget(View);
        centralWidget->setObjectName(QStringLiteral("centralWidget"));
        exitButton = new QPushButton(centralWidget);
        exitButton->setObjectName(QStringLiteral("exitButton"));
        exitButton->setGeometry(QRect(320, 270, 91, 41));
        exitButton->setStyleSheet(QLatin1String("QPushButton {\n"
"    border: 2px solid #8f8f91;\n"
"    border-radius: 50px;\n"
"    background-color: qlineargradient(x1: 0, y1: 0, x2: 0, y2: 1,\n"
"                                      stop: 0 #f6f7fa, stop: 1 #dadbde);\n"
"    min-width: 80px;\n"
"}\n"
""));
        redButton = new QPushButton(centralWidget);
        redButton->setObjectName(QStringLiteral("redButton"));
        redButton->setGeometry(QRect(30, 120, 113, 111));
        redButton->setAutoFillBackground(false);
        redButton->setStyleSheet(QLatin1String("\n"
"background-color: rgb(255, 7, 44);"));
        blueButton = new QPushButton(centralWidget);
        blueButton->setObjectName(QStringLiteral("blueButton"));
        blueButton->setGeometry(QRect(280, 120, 111, 111));
        blueButton->setStyleSheet(QStringLiteral("background-color: rgb(17, 0, 255);"));
        pushButton = new QPushButton(centralWidget);
        pushButton->setObjectName(QStringLiteral("pushButton"));
        pushButton->setGeometry(QRect(160, 120, 101, 101));
        pushButton->setStyleSheet(QLatin1String("QPushButton {\n"
"    border: 2px solid #8f8f91;\n"
"    border-radius: 50px;\n"
"    background-color: qlineargradient(x1: 0, y1: 0, x2: 0, y2: 1,\n"
"                                      stop: 0 #f6f7fa, stop: 1 #dadbde);\n"
"    min-width: 80px;\n"
"}\n"
""));
        progressBar = new QProgressBar(centralWidget);
        progressBar->setObjectName(QStringLiteral("progressBar"));
        progressBar->setGeometry(QRect(280, 10, 131, 21));
        progressBar->setStyleSheet(QLatin1String("QProgressBar{\n"
"border: 2x solid grey;\n"
"border-radius: 5px;\n"
"text-align: center;\n"
"}\n"
"\n"
"QProgressBar::chunk{\n"
"background-color:#05b8cc;\n"
"width: 10px;\n"
"margin: 0.0px;\n"
"}\n"
""));
        progressBar->setValue(0);
        greenButton = new QPushButton(centralWidget);
        greenButton->setObjectName(QStringLiteral("greenButton"));
        greenButton->setGeometry(QRect(150, 10, 111, 101));
        greenButton->setStyleSheet(QStringLiteral("background-color: rgb(0,255,0);"));
        yellowButton = new QPushButton(centralWidget);
        yellowButton->setObjectName(QStringLiteral("yellowButton"));
        yellowButton->setGeometry(QRect(150, 230, 113, 101));
        yellowButton->setStyleSheet(QStringLiteral("background-color: rgb(255,255,0);"));
        spinBox = new QSpinBox(centralWidget);
        spinBox->setObjectName(QStringLiteral("spinBox"));
        spinBox->setGeometry(QRect(20, 30, 42, 22));
        spinBox->setValue(1);
        label = new QLabel(centralWidget);
        label->setObjectName(QStringLiteral("label"));
        label->setGeometry(QRect(10, 10, 91, 16));
        View->setCentralWidget(centralWidget);
        menuBar = new QMenuBar(View);
        menuBar->setObjectName(QStringLiteral("menuBar"));
        menuBar->setGeometry(QRect(0, 0, 430, 22));
        View->setMenuBar(menuBar);
        mainToolBar = new QToolBar(View);
        mainToolBar->setObjectName(QStringLiteral("mainToolBar"));
        View->addToolBar(Qt::TopToolBarArea, mainToolBar);
        statusBar = new QStatusBar(View);
        statusBar->setObjectName(QStringLiteral("statusBar"));
        View->setStatusBar(statusBar);

        retranslateUi(View);
        QObject::connect(exitButton, SIGNAL(clicked()), View, SLOT(close()));

        QMetaObject::connectSlotsByName(View);
    } // setupUi

    void retranslateUi(QMainWindow *View)
    {
        View->setWindowTitle(QApplication::translate("View", "Simon Game", Q_NULLPTR));
        exitButton->setText(QApplication::translate("View", "Exit", Q_NULLPTR));
        redButton->setText(QString());
        blueButton->setText(QString());
        pushButton->setText(QApplication::translate("View", "START", Q_NULLPTR));
        greenButton->setText(QString());
        yellowButton->setText(QString());
        label->setText(QApplication::translate("View", "Starting Level", Q_NULLPTR));
    } // retranslateUi

};

namespace Ui {
    class View: public Ui_View {};
} // namespace Ui

QT_END_NAMESPACE

#endif // UI_VIEW_H
