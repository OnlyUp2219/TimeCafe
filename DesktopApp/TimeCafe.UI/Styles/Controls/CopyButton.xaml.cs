using Microsoft.UI.Xaml.Media.Animation;
using TimeCafe.UI.Utilities.Helpers;

namespace TimeCafe.UI.Styles.Controls;

public sealed class CopyButton : Button
{
    public CopyButton()
    {
        this.DefaultStyleKey = typeof(CopyButton);
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetTemplateChild("CopyToClipboardSuccessAnimation") is Storyboard _storyBoard)
        {
            _storyBoard.Begin();
            UIHelper.AnnounceActionForAccessibility(this, "Copied to clipboard", "CopiedToClipboardActivityId");
        }
    }

    protected override void OnApplyTemplate()
    {
        Click -= CopyButton_Click;
        base.OnApplyTemplate();
        Click += CopyButton_Click;
    }
}