/*
 * layerwidget.cpp
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

#include "layerwidget.h"

using namespace constants;

/*
 * helpers
 */
const QString LAYER_WIDGET_STYLE_SHEET = \
        "QCheckBox::indicator\n"
        "{\n"
        "    height: "
        + QString::number(DEFAULT_TOOL_PUSH_BUTTON_ICON_SIZE.height()) +
        "px;\n"
        "    width:"
        + QString::number(DEFAULT_TOOL_PUSH_BUTTON_ICON_SIZE.width()) +
        "px;\n"
        "}\n"
        "\n"
        "QCheckBox::indicator:off\n"
        "{\n"
        "    image: url(':/images/notVisibleIcon.png');\n"
        "}\n"
        "\n"
        "QCheckBox::indicator:on\n"
        "{\n"
        "    image: url(':/images/visibleIcon.png');\n"
        "}\n";

/*
 * public
 */
LayerWidget::LayerWidget(
        EditorWindow *controller,
        QWidget *parent,
        Qt::WindowFlags flags) :
    ImageViewerWidget(controller, parent, flags),
    image(controller->getSpriteSize(), controller->getSpriteImageFormat())
{
    // internal states
    image.fill(controller->getSpriteBackgroundFill());

    // widget settings
    setStyleSheet(styleSheet() + LAYER_WIDGET_STYLE_SHEET);

    // create object hierarchy
    layout = new QHBoxLayout(this);
    layout->setContentsMargins(0, 0, 0, 0);

    checkbox = new QCheckBox(this);
    checkbox->setChecked(true);
    connect(checkbox, &QCheckBox::stateChanged, [this]{
        emit layerVisibilityChanged(isLayerVisible());
    });

    preview = new QLabel(this);
    preview->setFixedSize(DEFAULT_TOOL_PUSH_BUTTON_ICON_SIZE);
    preview->setProperty("textured", true);
    preview->setProperty("boxed", true);
    preview->setScaledContents(true);
    preview->setPixmap(QPixmap::fromImage(image));

    label = new QLabel(tr("New Layer"), this);

    // layout
    layout->addWidget(checkbox);
    layout->addWidget(preview);
    layout->addWidget(label, 1);

    // attach to object hierarchy
    setLayout(layout);
}

bool LayerWidget::isLayerVisible() const
{
    return Qt::CheckState::Checked == checkbox->checkState();
}

QImage LayerWidget::getImage() const
{
    return image;
}

QString LayerWidget::getText() const
{
    return label->text();
}

QImage * LayerWidget::imagePtr()
{
    return &image;
}

/*
 * public slots
 */
void LayerWidget::setLayerVisibility(bool visibility)
{
    if (isLayerVisible() != visibility)
    {
        checkbox->setCheckState(visibility ? Qt::CheckState::Checked
                                           : Qt::CheckState::Unchecked);
        emit layerVisibilityChanged(visibility);
    }
}

void LayerWidget::setImage(QImage img, QRect uptRect)
{
    // update the entire image if the rect wasn't specified
    if (!uptRect.isValid()) uptRect = img.rect();

    image = img;
    preview->setPixmap(QPixmap::fromImage(img));
    emit imageChanged(img, uptRect);
}

void LayerWidget::setText(const QString &text)
{
    if (label->text() != text)
    {
        label->setText(text);
        emit textChanged(text);
    }
}
