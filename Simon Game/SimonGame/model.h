#ifndef MODEL_H
#define MODEL_H

#include <QObject>
#include <QMessageBox>
#include <QString>
#include <QPushButton>
#include <QSpinBox>
#include <QTimer>
#include <QDateTime>
#include <iostream>
#include <QMediaPlayer>
using namespace std;

class Model : public QObject
{
    Q_OBJECT
public:
    explicit Model(QObject *parent = nullptr);
    void startTimer();
    QTimer *timer;
    int GetRandomNumber(const int Min, const int Max);
    bool isEvenSignal;
    std::vector<int> moves;


signals:
    void ModelUpdated(int);
    void ButtonUpdated(int, bool);
    void GameActive(bool);
    void ProgressUpdate(int);

public slots:
    void UpdateModel();
    void LowerDifficultyCounter();
    void StorePreviousMove(int x);
    void SetStartingDifficulty(int);
    void SendButtonSignal();

private:
    double speedupMultiplier;
    void CheckMove(int x);
    int difficultyCounter;
    int difficultyLevel;
    int startingDifficulty;
    std::string strColor;
    bool isGameActive;
    int previousDifficulty;
    int progressPercentage;
    int playerMoves;
    int computerMoves;
    QMediaPlayer * failedsound;
    QMediaPlayer * buttonsound;
    QMediaPlayer * startsound;
};

#endif // MODEL_H
