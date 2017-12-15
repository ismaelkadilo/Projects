#ifndef VIEW_H
#define VIEW_H

#include <QMainWindow>
#include "model.h"

namespace Ui {
class View;
}

class View : public QMainWindow
{
    Q_OBJECT

public:
    explicit View(Model& model, QWidget *parent = 0);
    ~View();
     QColor mColor;

private:
    Ui::View *ui;
    void DisarmRegularButtons();

public slots:
    void UpdateColor(int, bool);
    bool GameStatus(bool);


signals:
    void DifficultyUpdated();
    void StoreMove(int);

private slots:
    void on_yellowButton_pressed();
    void on_yellowButton_released();
    void on_redButton_pressed();
    void on_redButton_released();
    void on_greenButton_pressed();
    void on_greenButton_released();
    void on_blueButton_pressed();
    void on_blueButton_released();
};

#endif // VIEW_H
