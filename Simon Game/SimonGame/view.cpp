
#include "view.h"
#include "ui_view.h"
#include "model.h"
#include <QTimer>
#include "QDebug"
#include <QMessageBox>
#include <QObject>


View::View(Model& model, QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::View)
{
    ui->setupUi(this);
    View::DisarmRegularButtons();
    connect(ui->redButton, &QPushButton::clicked,
            &model, &Model::UpdateModel);
    connect(ui->blueButton, &QPushButton::clicked,
            &model, &Model::UpdateModel);
    connect(ui->greenButton, &QPushButton::clicked,
            &model, &Model::UpdateModel);
    connect(ui->yellowButton, &QPushButton::clicked,
            &model, &Model::UpdateModel);
    connect(ui->pushButton, &QPushButton::clicked,
            &model, &Model::UpdateModel);
    connect(&model, &Model::ProgressUpdate,
            ui->progressBar, &QProgressBar::setValue);
    connect(&model, &Model::ButtonUpdated,
            this, &View::UpdateColor);
    connect(this, &View::DifficultyUpdated,
            &model, &Model::LowerDifficultyCounter);
    connect(this, &View::StoreMove,
            &model, &Model::StorePreviousMove);
    connect(&model, &Model::GameActive,
            this, &View::GameStatus);
    connect(ui->spinBox, static_cast<void(QSpinBox::*)(int)>(&QSpinBox::valueChanged),
            &model, &Model::SetStartingDifficulty);
}

void View::UpdateColor(int x, bool isEvenSignal){

    if (isEvenSignal){
        if (x == 1){
            ui->redButton->setStyleSheet(
            QString::fromUtf8("background-color: red;"));
        }
        else if (x == 2){
            ui->blueButton->setStyleSheet(
            QString::fromUtf8("background-color: blue;"));
        }
        else if (x == 3){
            ui->greenButton->setStyleSheet(
            QString::fromUtf8("background-color: green;"));
        }
        else if (x == 4){
            ui->yellowButton->setStyleSheet(
            QString::fromUtf8("background-color: yellow;"));
        }
        //StoreMove(x);
    }
    else {
        ui->yellowButton->setStyleSheet(
        QString::fromUtf8("background-color: ;"));
        ui->redButton->setStyleSheet(
        QString::fromUtf8("background-color ;"));
        ui->blueButton->setStyleSheet(
        QString::fromUtf8("background-color: ;"));
        ui->greenButton->setStyleSheet(
        QString::fromUtf8("background-color: ;"));
    }
    emit DifficultyUpdated();
}

bool View::GameStatus(bool y){
    if (y){
        DisarmRegularButtons();
        ui->pushButton->setEnabled(false);
    }
    else{
        ui->redButton->setEnabled(true);
        ui->blueButton->setEnabled(true);
        ui->greenButton->setEnabled(true);
        ui->yellowButton->setEnabled(true);
        ui->pushButton->setEnabled(true);
        ui->spinBox->setEnabled(true);
    }
    return y;
}

void View::DisarmRegularButtons(){
    ui->redButton->setEnabled(false);
    ui->blueButton->setEnabled(false);
    ui->greenButton->setEnabled(false);
    ui->yellowButton->setEnabled(false);
}

View::~View()
{
    delete ui;
}


void View::on_yellowButton_pressed()
{
    ui->yellowButton->setStyleSheet(
    QString::fromUtf8("background-color: yellow;"));
}

void View::on_yellowButton_released()
{
    ui->yellowButton->setStyleSheet(
    QString::fromUtf8("background-color: ;"));
}

void View::on_redButton_pressed()
{
    ui->redButton->setStyleSheet(
    QString::fromUtf8("background-color: red;"));
}


void View::on_redButton_released()
{
    ui->redButton->setStyleSheet(
    QString::fromUtf8("background-color: ;"));
}

void View::on_greenButton_pressed()
{
    ui->greenButton->setStyleSheet(
    QString::fromUtf8("background-color: green;"));
}

void View::on_greenButton_released()
{
    ui->greenButton->setStyleSheet(
    QString::fromUtf8("background-color: ;"));
}

void View::on_blueButton_pressed()
{
    ui->blueButton->setStyleSheet(
    QString::fromUtf8("background-color: blue;"));
}

void View::on_blueButton_released()
{
    ui-> blueButton->setStyleSheet(
    QString::fromUtf8("background-color: ;"));
}


