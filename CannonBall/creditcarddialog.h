/*
 * creditcardwindow.h
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

#ifndef CREDITCARDWINDOW_H
#define CREDITCARDWINDOW_H

#include <QDialog>
#include <QMessageBox>

namespace Ui {
class CreditCardWindow;
}

class CreditCardDialog : public QDialog
{
    Q_OBJECT

public:
    explicit CreditCardDialog(
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags = Qt::WindowFlags());
    ~CreditCardDialog();

private slots:
    void on_lowButton_clicked();
    void on_midButton_clicked();
    void on_highButton_clicked();
    void on_doneButton_clicked();

signals:
    void addZcoins(int);

private:
    Ui::CreditCardWindow *ui;
};

#endif // CREDITCARDWINDOW_H
