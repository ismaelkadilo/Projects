/*
 * editorwindow.cpp
 *
 * Anthony Chyr (u0627375)
 * Carlos Enrique Guerra Chan (u0847821)
 * Elliot C Carr-Lee (u0549837)
 * Ismael Kadilo Wa Ngoie (u1120347)
 * Kameron Service (u0963620)
 *
 * CS 3505 Software Practices II Johnson
 * A7: Sprite Editor Implementation
 */

#include "editorwindow.h"

#include "canvaswidget.h"
#include "framelistwidget.h"
#include "framewidget.h"
#include "layerlistwidget.h"
#include "layerwidget.h"

#include <math.h>

using namespace constants;
using namespace std;

/*
 * public
 */
EditorWindow::EditorWindow(
        QSize size,
        ToolManagerWidget *tool,
        QWidget *parent,
        QImage::Format imageFormat,
        Qt::WindowFlags flags) :
    QWidget(parent, flags),
    toolManager(tool)
{
    // initialization
    setSpriteSize(size);
    setSpriteImageFormat(imageFormat);
    setSpriteBackgroundFill(DEFAULT_BACKGROUND_FILL_COLOR);

    // gui
    setAttribute(Qt::WA_DeleteOnClose);
    setWindowTitle(tr("sprite%1.ssp").arg(++numConstructed));
    createGui();

    // internal states that require the gui to exist first
    frameListWidget = new FrameListWidget(this);
    connect(frameListWidget, &FrameListWidget::currentFrameChanged,
            canvas, QOverload<>::of(&CanvasWidget::update));
    connect(frameListWidget, &FrameListWidget::frameImageChanged,
            [=](FrameListWidget *frameList,
                FrameListWidgetItem *frameItm,
                LayerListWidget *layerList,
                LayerListWidgetItem *layerItm,
                QImage img,
                QRect uptRect){
        Q_UNUSED(frameList)
        Q_UNUSED(layerList)
        Q_UNUSED(layerItm)
        Q_UNUSED(img)
        if (getFrameListWidget()->currentItem() == frameItm)
            canvas->imageUpdate(uptRect);
    });
}

EditorWindow::~EditorWindow()
{
    delete frameListWidget;
}

QSize EditorWindow::getSpriteSize() const
{
    return spriteSize;
}

QImage::Format EditorWindow::getSpriteImageFormat() const
{
    return spriteImageFormat;
}

QRgb EditorWindow::getSpriteBackgroundFill() const
{
    return spriteBackgroundFill;
}

QFileInfo EditorWindow::getFileInfo() const
{
    return fileInfo;
}

FrameListWidget * EditorWindow::getFrameListWidget() const
{
    return frameListWidget;
}

FrameWidget * EditorWindow::getCurrentFrameWidget() const
{
    return frameListWidget->currentWidget();
}

LayerListWidget * EditorWindow::getCurrentLayerListWidget() const
{
    FrameWidget * currentFrameWidget = getCurrentFrameWidget();
    return currentFrameWidget ? currentFrameWidget->getLayerListWidget()
                              : Q_NULLPTR;
}

LayerWidget * EditorWindow::getCurrentLayerWidget() const
{
    LayerListWidget * currentLayerListWidget = getCurrentLayerListWidget();
    return currentLayerListWidget ? currentLayerListWidget->currentWidget()
                                  : Q_NULLPTR;
}

ToolManagerWidget * EditorWindow::getToolManager() const
{
    return toolManager;
}

void EditorWindow::ensureSelectedLayer()
{
    // make sure that a frame is selected, add one if needed
    if (!getCurrentFrameWidget())
    {
        FrameListWidget *frameList = getFrameListWidget();
        if (!frameList->count()) frameList->addNewFrame();
        frameList->setCurrentRow(0);
    }

    // make sure that a layer is selected, add one if needed
    if (!getCurrentLayerWidget())
    {
        LayerListWidget *layerList = getCurrentLayerListWidget();
        if (!layerList->count()) layerList->addNewLayer();
        layerList->setCurrentRow(0);
    }
}

/*
 * public slots
 */
void EditorWindow::setSpriteSize(QSize size)
{
    if (size.isValid() && getSpriteSize() != size)
    {
        spriteSize = size;
        emit spriteSizeChanged(spriteSize);
    }
}

void EditorWindow::setSpriteImageFormat(QImage::Format format)
{
    if (getSpriteImageFormat() != format)
    {
        spriteImageFormat = format;
        emit spriteImageFormatChanged(format);
    }
}

void EditorWindow::setSpriteBackgroundFill(QRgb fill)
{
    if (getSpriteBackgroundFill() != fill)
    {
        spriteBackgroundFill = fill;
        emit spriteBackgroundFillChanged(fill);
    }
}

void EditorWindow::setFileInfo(QFileInfo info)
{
    if (getFileInfo() != info)
    {
        fileInfo = info;
        setWindowTitle(info.fileName());
    }
}

/*
 * private
 */
int EditorWindow::numConstructed = 0;

void EditorWindow::createGui()
{
    // create object hierarchy
    scrollArea = new QScrollArea(this);
    scrollArea->setWidgetResizable(true);

    scrollAreaWidgetContents = new QWidget(scrollArea);

    canvas = new CanvasWidget(this, scrollAreaWidgetContents);

    zoomSpinBox = new QSpinBox(this);
    zoomSpinBox->setRange(ZOOM_MIN_PERCENT, ZOOM_MAX_PERCENT);
    zoomSpinBox->setValue(DEFAULT_ZOOM_PERCENT);
    zoomSpinBox->setSingleStep(DEFAULT_ZOOM_STEP_PERCENT);
    zoomSpinBox->setSuffix("%");
    connect(zoomSpinBox, QOverload<int>::of(&QSpinBox::valueChanged),
            [this](int p){ canvas->setFixedSize(getSpriteSize() * p / 100); });

    double numSteps = 100.0 / (log10(zoomSpinBox->maximum()) -
                               log10(zoomSpinBox->minimum()));
    auto toLogScale = [=](int v)->double{ return numSteps * log10(v); };
    auto toLinearScale = [=](int v)->double{ return pow(10.0, v / numSteps); };
    zoomSlider = new QSlider(Qt::Orientation::Horizontal, this);
    zoomSlider->setRange(toLogScale(zoomSpinBox->minimum()),
                         toLogScale(zoomSpinBox->maximum()));
    zoomSlider->setValue(toLogScale(zoomSpinBox->value()));
    zoomSlider->setTickPosition(QSlider::TicksBelow);
    zoomSlider->setTickInterval((toLogScale(zoomSpinBox->maximum()) -
                                 toLogScale(zoomSpinBox->minimum())) / 2);
    connect(zoomSpinBox, QOverload<int>::of(&QSpinBox::valueChanged),
            [=](int v){ zoomSlider->blockSignals(true); // stop infinite loop
                        zoomSlider->setValue(toLogScale(v));
                        zoomSlider->blockSignals(false); });
    connect(zoomSlider, &QSlider::valueChanged,
            [=](int v){ zoomSpinBox->setValue(toLinearScale(v)); });

    auto makeZoomPushButton = [&](const QString &iconPath,
                                  const QString &text,
                                  function<void()> cmd)->QPushButton *{
        QPushButton *pushButton = new QPushButton(this);
        pushButton->setFixedSize(DEFAULT_TOOL_PUSH_BUTTON_SIZE);
        pushButton->setFlat(true);
        pushButton->setIcon(QIcon(iconPath));
        pushButton->setIconSize(DEFAULT_TOOL_PUSH_BUTTON_ICON_SIZE);
        pushButton->setToolTip(text);
        connect(pushButton, &QPushButton::clicked, cmd);
        return pushButton;
    };
    zoomOutPushButton = makeZoomPushButton(
                ":/images/zoomOutIcon.png", tr("Zoom In"),
                [this]{ zoomSpinBox->stepDown(); });
    zoomInPushButton = makeZoomPushButton(
                ":/images/zoomInIcon.png", tr("Zoom Out"),
                [this]{ zoomSpinBox->stepUp(); });
    zoomNormalPushButton = makeZoomPushButton(
                ":/images/zoomNormalIcon.png", tr("Original"),
                [this]{ zoomSpinBox->setValue(DEFAULT_ZOOM_PERCENT); });
    zoomFitPushButton = makeZoomPushButton(
                ":/images/zoomFitIcon.png", tr("Fit"),
                [this]{ int w = 100 * (scrollArea->width() - 2)
                                    / getSpriteSize().width();
                        int h = 100 * (scrollArea->height() - 2)
                                    / getSpriteSize().height();
                        zoomSpinBox->setValue(min(w, h)); });

    mainLayout = new QGridLayout(this);
    mainLayout->setHorizontalSpacing(0);

    scrollAreaLayout = new QGridLayout(scrollAreaWidgetContents);
    scrollAreaLayout->setMargin(0);

    // layouts
    mainLayout->addWidget(scrollArea, 0, 0, 1, 7);
    mainLayout->addWidget(zoomOutPushButton, 1, 0, 1, 1);
    mainLayout->addWidget(zoomSlider, 1, 1, 1, 1);
    mainLayout->addWidget(zoomInPushButton, 1, 2, 1, 1);
    mainLayout->addWidget(zoomNormalPushButton, 1, 3, 1, 1);
    mainLayout->addWidget(zoomFitPushButton, 1, 4, 1, 1);
    mainLayout->addWidget(zoomSpinBox, 1, 5, 1, 1);
    mainLayout->setColumnStretch(6, 1);

    scrollAreaLayout->addWidget(canvas, 0, 0, 1, 1);

    // attach to object hierarchy
    scrollArea->setWidget(scrollAreaWidgetContents);

    // further initializations
    emit zoomSpinBox->valueChanged(zoomSpinBox->value());
}
