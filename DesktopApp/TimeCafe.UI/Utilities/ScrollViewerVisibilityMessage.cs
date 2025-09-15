using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TimeCafe.UI.Utilities;

public enum ShellScrollViewerMode
{
    Default,
    Disabled
}

public class ShellScrollViewerVisibilityMessage : ValueChangedMessage<ShellScrollViewerMode>
{
    public ShellScrollViewerVisibilityMessage(ShellScrollViewerMode mode) : base(mode) { }
}