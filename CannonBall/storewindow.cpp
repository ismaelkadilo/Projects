/*
 * storewindow.cpp
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

#include "storewindow.h"
#include "ui_storewindow.h"

using namespace std;

/*
 * public
 */
StoreWindow::StoreWindow(StoreModel *model, QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::StoreWindow)
{
    ui->setupUi(this);

    // view <-> creditcarddialog
    prompt = new CreditCardDialog(this);
    connect(prompt, &CreditCardDialog::addZcoins,
            this, &StoreWindow::newZCoins);
    connect(this,&StoreWindow::addZCoinsToModel,
            model,&StoreModel::newZCoinsForModel);

    // view -> model
    connect(this, &StoreWindow::needSkinResourceString,
            model, &StoreModel::resourceRequest);
    connect(this, &StoreWindow::init,
            model, &StoreModel::initModel);
    connect(this, &StoreWindow::needNewSkin,
            model, &StoreModel::newSkinNeeded);
    connect(this, &StoreWindow::upgradeCannon,
            model, &StoreModel::doCheater);
    connect(this, &StoreWindow::solutionRequested,
            model, &StoreModel::doSolution);

    // model -> view
    connect(model, &StoreModel::resourceFound,
            this, &StoreWindow::skinResourceStringReciever);
    connect(model, &StoreModel::initInfo,
            this, &StoreWindow::initFromModel);
    connect(model, &StoreModel::cheaterOff,
            this, &StoreWindow::cheaterCannonBallDeactivated);
    connect(model, &StoreModel::notEnoughZCoinsInModel,
            this, &StoreWindow::notEnoughZCoins);
    connect(model, &StoreModel::newSkinFound,
            this, &StoreWindow::skinFoundByModel);
    connect(model, &StoreModel::cheaterOn,
            this, &StoreWindow::cheaterCannonBallActivated);
    connect(model, &StoreModel::solutionRequestAccepted,
            this, &StoreWindow::doSolution);
    connect(model, &StoreModel::noNewSkins,
            this, &StoreWindow::skinNotFoundByModel);
    connect(model, &StoreModel::zcoinsAdded,
            this, &StoreWindow::zCoinsAddedByModel);

    // inventory
    auto *container = new QWidget();
    ui->scrollArea->setWidget(container);
    ui->verticalLayout = new QVBoxLayout(container);
    ui->verticalLayout->setAlignment(Qt::AlignTop);

    // catalog
    auto *catContainer = new QWidget();
    ui->catalogScrollArea->setWidget(catContainer);
    ui->catalogLayout = new QGridLayout(catContainer);
    ui->catalogLayout->setAlignment(Qt::AlignTop);

    selectedButtonIndex = 0;
    updateCoins(STARTINGCOINS);

    // initialize default skin and catalog
    emit init();
}

StoreWindow::~StoreWindow()
{
    for(int i = 0; i < skins.size(); i++) delete skins[i];
    delete prompt;
    delete ui;
}

/*
 * private slots
 */
void StoreWindow::initFromModel(
        vector<Skin> allSkins,
        QString defaultSkinResource,
        int initialZCoins)
{
    for (const Skin &skin : allSkins)
    {
        QLabel *label = new QLabel(tr(""));
        label->setFixedSize(BUTTONWIDTH, BUTTONHEIGHT);
        label->setPixmap(
                    QPixmap::fromImage(
                        skin.skin).scaled(
                            QSize(100,100),
                            Qt::IgnoreAspectRatio,
                            Qt::FastTransformation));
        addCatalogItem(label);
    }
    updateCoins(initialZCoins);
    addSkinToInventory(defaultSkinResource);
}

void StoreWindow::newZCoins(int coins)
{
    emit addZCoinsToModel(coins);
}

void StoreWindow::zCoinsAddedByModel(int coins)
{
    ui->zcoinsAmountLabel->setText(tr("%1").arg(coins));
}

void StoreWindow::notEnoughZCoins()
{
    QMessageBox msgBox;
    msgBox.setWindowTitle(tr("Epic Fail"));
    msgBox.setText(tr("Not Enough ZCoins!!!"));
    msgBox.exec();
}

void StoreWindow::skinClickedByUser()
{
    // figureout which skin was clicked
    int lastSelectedIndex = selectedButtonIndex;
    QObject const *SENDER_OBJ = sender();
    for (int i = 0; i < skins.size(); i++)
    {
        if (SENDER_OBJ == skins[i])
        {
            selectedButtonIndex = i;
            break;
        }
    }

    skins[lastSelectedIndex]->setStyleSheet(BUTTONUNSELECTEDSTYLE);
    skins[selectedButtonIndex]->setStyleSheet(BUTTONSELECTEDSTYLE);

    emit needSkinResourceString(selectedButtonIndex);
}

void StoreWindow::skinFoundByModel(QString resource, int coins)
{
    updateCoins(coins);

    QMessageBox about_box(this);
    about_box.setWindowTitle(tr("New Skin!"));
    about_box.setIconPixmap(QPixmap(resource));
    about_box.setParent(this);
    about_box.exec();

    addSkinToInventory(resource);
}

void StoreWindow::skinNotFoundByModel()
{
    QMessageBox msgBox;
    msgBox.setWindowTitle(tr("Epic Fail"));
    msgBox.setText(tr("You own all the skins!!"));
    msgBox.exec();
}

void StoreWindow::skinResourceStringReciever(QString resource){
    emit resourceString(resource);
}

void StoreWindow::cheaterCannonBallActivated(int coins)
{
    ui->zcoinsAmountLabel->setText(tr("%1").arg(coins));

    QMessageBox msgBox;
    msgBox.setWindowTitle(tr("Success"));
    msgBox.setText(tr("Big Cannon Ball Activated!"));
    msgBox.exec();

    emit cheaterCannonBallToggled(true);
}

void StoreWindow::cheaterCannonBallDeactivated(){
    ui->upgradeCannonButton->setText(tr("Upgrade Cannon"));

    QMessageBox msgBox;
    msgBox.setWindowTitle(tr("Success"));
    msgBox.setText(tr("Big Cannon Ball Dectivated!"));
    msgBox.exec();

    emit cheaterCannonBallToggled(false);
}

void StoreWindow::on_newSkinButton_clicked()
{
    emit needNewSkin();
}

void StoreWindow::on_buyZCoinsButton_clicked()
{
    prompt->show();
}

void StoreWindow::on_upgradeCannonButton_clicked()
{
    emit upgradeCannon();
}

void StoreWindow::on_solutionButton_clicked()
{
    emit solutionRequested();
}


/*
 * private
 */
QPushButton * StoreWindow::createNewButton(int index, QString iconResource)
{
    QPushButton* button = new QPushButton(QIcon(iconResource), tr(""));
    button->setFixedSize(BUTTONWIDTH, BUTTONHEIGHT);
    button->setStyleSheet(BUTTONSELECTEDSTYLE);
    button->setIconSize(button->size());
    button->setObjectName(tr("%1").arg(index));
    return button;
}

void StoreWindow::addCatalogItem(QWidget *catalogWidget)
{
    int current_row = 0;
    int current_column = 0;

    while (0 != ui->catalogLayout->itemAtPosition(current_row, current_column))
    {
        // find the latest item spot
        if(2 == current_column)
        {
            current_column = 0;
            ++current_row;
        }
        else
        {
            ++current_column;
        }
    }

    ui->catalogLayout->addWidget(catalogWidget, current_row, current_column);
}

void StoreWindow::updateCoins(int coins)
{
    ui->zcoinsAmountLabel->setText(tr("%1").arg(coins));
}

void StoreWindow::addSkinToInventory(QString skinResource)
{
    //create a new button, connect it, and add it to the framelist layout
    QPushButton *button = createNewButton(skins.size(), skinResource);

    connect(button, &QPushButton::clicked,
            this, &StoreWindow::skinClickedByUser);
    connect(button, &QPushButton::clicked, [=]{
        emit skinSelected(skinResource);
    });

    skins.push_back(button);

    int lastSelectedIndex = selectedButtonIndex;
    selectedButtonIndex = skins.size() - 1;
    skins[lastSelectedIndex]->setStyleSheet(BUTTONUNSELECTEDSTYLE);
    skins[selectedButtonIndex]->setStyleSheet(BUTTONSELECTEDSTYLE);

    ui->verticalLayout->addWidget(button);
}
