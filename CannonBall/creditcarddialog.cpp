/*
 * creditcardwindow.cpp
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

#include "creditcarddialog.h"
#include "ui_creditcardwindow.h"

/*
 * public
 */
CreditCardDialog::CreditCardDialog(QWidget *parent, Qt::WindowFlags flags) :
    QDialog(parent, flags),
    ui(new Ui::CreditCardWindow)
{
    ui->setupUi(this);
}

CreditCardDialog::~CreditCardDialog()
{
    delete ui;
}

/*
 * public slots
 */
void CreditCardDialog::on_lowButton_clicked()
{
    QMessageBox msgBox;
    msgBox.setWindowTitle(tr("Congratulations!"));
    msgBox.setText(tr("You just bought 1000 ZCoins!!!"));
    msgBox.exec();

    emit addZcoins(1000);
}

void CreditCardDialog::on_midButton_clicked()
{
    QMessageBox msgBox;
    msgBox.setWindowTitle(tr("Congratulations!"));
    msgBox.setText(tr("You just bought 10000 ZCoins!!!"));
    msgBox.exec();

    emit addZcoins(10000);

}

void CreditCardDialog::on_highButton_clicked()
{

    QMessageBox msgBox;
    msgBox.setWindowTitle(tr("Congratulations!"));
    msgBox.setText(tr("You just bought 100000 ZCoins!!!"));
    msgBox.exec();

    emit addZcoins(100000);
}

void CreditCardDialog::on_doneButton_clicked()
{
    close();
}
