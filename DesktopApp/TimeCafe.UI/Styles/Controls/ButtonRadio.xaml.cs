using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;

namespace TimeCafe.UI.Styles.Controls;

class ButtonRadio : RadioButton
{
    public ButtonRadio()
    {
        this.DefaultStyleKey = typeof(ButtonRadio);
        this.PointerEntered += ButtonRadio_PointerEntered;

    }
    public void ChangeCursor(InputCursor cursor)
    {
        this.ProtectedCursor = cursor;
    }

    private void ButtonRadio_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ChangeCursor(InputSystemCursor.Create(InputSystemCursorShape.Hand));
        VisualStateManager.GoToState(this, "PointerOver", true);
    }


}
