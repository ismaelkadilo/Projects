/*
 * toolmanagerwidget.cpp
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

#include "toolmanagerwidget.h"

using namespace constants;
using namespace toolmanager;

/*
 * helpers
 */
void setButtonColor(QPushButton *pushButton, const QColor &color)
{
    pushButton->setStyleSheet(
                "background: " + color.name(QColor::HexArgb) + ";\n");
    pushButton->style()->unpolish(pushButton);
    pushButton->style()->polish(pushButton);
}

/*
 * public
 */
ToolManagerWidget::ToolManagerWidget(
        QWidget *parent,
        Qt::WindowFlags flags) :
    QWidget(parent, flags),
    mousePressHandler(NO_OP_MOUSE_EVENT_HANLDER),
    mouseMoveHandler(NO_OP_MOUSE_EVENT_HANLDER),
    mouseReleaseHandler(NO_OP_MOUSE_EVENT_HANLDER)
{
    // gui
    const QSize COLOR_PICKER_AREA_SIZE = DEFAULT_TOOL_PUSH_BUTTON_SIZE * 2.5;

    auto makeColorPickerPushButton = \
            [&](QPoint topLeftPos,
                void(ToolManagerWidget::*cmd)(),
                const QString &toolTip)->QPushButton *{
        QPushButton *pb = new QPushButton(this);
        pb->setGeometry(QRect(topLeftPos, COLOR_PICKER_AREA_SIZE * 2 / 3));
        pb->setProperty("boxed", true);
        pb->setToolTip(toolTip);
        connect(pb, &QPushButton::clicked, this, cmd);
        return pb;
    };
    brushColorPushButton = makeColorPickerPushButton(
                QPoint(COLOR_PICKER_AREA_SIZE.width()/3,
                       COLOR_PICKER_AREA_SIZE.height()/3),
                &ToolManagerWidget::pickBrushColor,
                tr("Brush Color"));
    penColorPushButton = makeColorPickerPushButton(
                QPoint(0, 0),
                &ToolManagerWidget::pickPenColor,
                tr("Pen Color"));

    penWidthPushButton = new QPushButton(
                QIcon(":/images/penWidthIcon.png"), "", this);
    penWidthPushButton->setFlat(true);
    penWidthPushButton->setGeometry(
                QRect(QPoint(0, COLOR_PICKER_AREA_SIZE.height()),
                      QSize(COLOR_PICKER_AREA_SIZE.width(),
                            COLOR_PICKER_AREA_SIZE.height()/2)));
    penWidthPushButton->setToolTip(tr("Pen Width"));
    connect(penWidthPushButton, &QPushButton::clicked,
            this, &ToolManagerWidget::pickPenWidth);

    // widget settings
    setFixedSize(COLOR_PICKER_AREA_SIZE
                 + QSize(0, penWidthPushButton->height()));

    // internal states
    setBrush(DEFAULT_BRUSH);
    setPen(DEFAULT_PEN);
}

void ToolManagerWidget::handleMousePress(QMouseEvent *event,
                                   EditorWindow *controller,
                                   CanvasWidget *canvas)
{
    getMousePressHandler()(event, controller, canvas, this);
}

void ToolManagerWidget::handleMouseMove(QMouseEvent *event,
                                  EditorWindow *controller,
                                  CanvasWidget *canvas)
{
    getMouseMoveHandler()(event, controller, canvas, this);
}

void ToolManagerWidget::handleMouseRelease(QMouseEvent *event,
                                     EditorWindow *controller,
                                     CanvasWidget *canvas)
{
    getMouseReleaseHandler()(event, controller, canvas, this);
}

MouseEventHandler ToolManagerWidget::getMousePressHandler() const
{
    return mousePressHandler;
}

MouseEventHandler ToolManagerWidget::getMouseMoveHandler() const
{
    return mouseMoveHandler;
}

MouseEventHandler ToolManagerWidget::getMouseReleaseHandler() const
{
    return mouseReleaseHandler;
}

void ToolManagerWidget::setMouseEventHandler(
        MouseEventHandler mousePressClosure,
        MouseEventHandler mouseMoveClosure,
        MouseEventHandler mouseReleaseClosure)
{
    mousePressHandler = mousePressClosure;
    mouseMoveHandler = mouseMoveClosure;
    mouseReleaseHandler = mouseReleaseClosure;
}

void ToolManagerWidget::configurePainter(QPainter *painter) const
{
    painter->setBrush(toolManagerBrush);
    painter->setPen(toolManagerPen);
}

QRect ToolManagerWidget::addPenWidth(QRect rect) const
{
    int radius = (getPen().width() / 2) + 2;
    return rect.normalized().adjusted(-radius, -radius, +radius, +radius);
}

QBrush ToolManagerWidget::getBrush() const
{
    return toolManagerBrush;
}

QPen ToolManagerWidget::getPen() const
{
    return toolManagerPen;
}

/*
 * public slots
 */
void ToolManagerWidget::pickBrushColor()
{
    QBrush brush = getBrush();
    brush.setColor(QColorDialog::getColor(
                       brush.color(), this, tr("Brush Color"),
                       QColorDialog::ShowAlphaChannel));
    setBrush(brush);
}

void ToolManagerWidget::pickPenColor()
{
    QPen pen = getPen();
    pen.setColor(QColorDialog::getColor(
                     pen.color(), this, tr("Pen Color"),
                     QColorDialog::ShowAlphaChannel));
    setPen(pen);
}

void ToolManagerWidget::pickPenWidth()
{
    QPen pen = getPen();
    pen.setWidth(QInputDialog::getInt(
                     this, tr("Pen Width"), tr("Pen width in pixels"),
                     pen.width(), MIN_PEN_WIDTH, MAX_PEN_WIDTH));
    setPen(pen);
}

void ToolManagerWidget::setBrush(const QBrush &BRUSH)
{
    if (BRUSH != toolManagerBrush)
    {
        toolManagerBrush = BRUSH;
        setButtonColor(brushColorPushButton, BRUSH.color());
        brushColorPushButton->raise();
    }
}

void ToolManagerWidget::setPen(const QPen &PEN)
{
    if (PEN != toolManagerPen)
    {
        toolManagerPen = PEN;
        setButtonColor(penColorPushButton, PEN.color());
        penColorPushButton->raise();

        penWidthPushButton->setText(QString::number(PEN.width()) + tr(" px"));
    }
}
