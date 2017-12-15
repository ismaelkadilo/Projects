#include "model.h"
#include <QMediaPlayer>

Model::Model(QObject *parent) : QObject(parent)
{
    speedupMultiplier = 1000; //set the initial delay on the timer
    failedsound = new QMediaPlayer();
    failedsound->setMedia(QUrl("qrc:/sounds/Wrong Buzzer Sound Effect.mp3"));

    buttonsound = new QMediaPlayer();
    buttonsound->setMedia(QUrl("qrc:/sounds/button.mp3"));

    startsound = new QMediaPlayer();
    startsound->setMedia(QUrl("qrc:/sounds/start.mp3"));

    difficultyCounter = 3;
    startingDifficulty = difficultyCounter/2;
    difficultyLevel = startingDifficulty;
}

void Model::UpdateModel() {
    emit ModelUpdated(difficultyCounter);
    QPushButton* buttonSender = qobject_cast<QPushButton*>(sender()); // retrieve the button you have clicked
    QString buttonText = buttonSender->objectName(); // retrive the text from the button clicked
    if (buttonText == "pushButton"){
        startsound->play(); // sounds when starting game
        strColor = buttonText.toStdString();
        startTimer();
    }
    if (moves.size() > 0){
        if (buttonText == "redButton"){
            CheckMove(1);
        }
        if (buttonText == "blueButton"){
            CheckMove(2);

        }
        if (buttonText == "greenButton"){
            CheckMove(3);
        }
        if (buttonText == "yellowButton"){
            CheckMove(4);
        }
    }
}

void Model::startTimer(){
    difficultyCounter = difficultyLevel * 2 + 1;
    playerMoves = 0;
    computerMoves = 0;
    QTime time = QTime::currentTime();
    qsrand((uint)time.msec());
    timer = new QTimer(this);
    emit GameActive(true);
    timer->start(speedupMultiplier);
    connect(timer, SIGNAL(timeout()), this, SLOT(SendButtonSignal()));
    speedupMultiplier *= .9;
}

void Model::SendButtonSignal(){
    if (difficultyCounter == 1){
        isGameActive = false;
        timer->stop();
        emit GameActive(false);
    }
    if (playerMoves == 0){
        ProgressUpdate(0);
    }
    int r;
    if (difficultyCounter % 2 == 0){
        if (moves.size() <= computerMoves){

            r = GetRandomNumber(1,4);

            moves.push_back(r);
        }
        else{
            r = moves.at(computerMoves);
        }
        emit ButtonUpdated(r, true);
        computerMoves++;
    }
    else{
        emit ButtonUpdated(r, false);
    }
}

int Model::GetRandomNumber(const int Min, const int Max)
{
    return ((qrand() % ((Max + 1) - Min)) + Min);
}

void Model::LowerDifficultyCounter(){
    difficultyCounter--;
}

void Model::StorePreviousMove(int x){
    moves.push_back(x);
}

void Model::CheckMove(int x){
    if (x == moves.at(playerMoves)){
        playerMoves++;
        float temp = 100* (float)playerMoves / (float)(difficultyLevel); //calculate progress percentage.
        progressPercentage = (int)temp;
        emit ProgressUpdate(progressPercentage);
        if (moves.size() == playerMoves){
            difficultyLevel++;
            startTimer();
        }
    }
    else{
        speedupMultiplier = 1000; //reset the timer speed.
        //when the player makes a mistake there will be a sound
        failedsound->play();
        //when the player makes a mistake there will be a "You Lose message"
        difficultyLevel = startingDifficulty;
        QMessageBox msgBox;
        msgBox.setWindowTitle("Game Over");
        QString s = "Oops. You Lose.";
        msgBox.setText(s);
        msgBox.exec();
        GameActive(false);
        moves.clear();
    }
}

void Model::SetStartingDifficulty(int x){
    difficultyCounter = x * 2 + 1;
    difficultyLevel = x;
    startingDifficulty = x;
}

