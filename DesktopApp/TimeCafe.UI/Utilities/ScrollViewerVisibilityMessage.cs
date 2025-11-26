using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TimeCafe.UI.Utilities;

public enum ShellScrollViewerMode
{
    Default,
    Disabled
}

public class ShellScrollViewerVisibilityMessage(ShellScrollViewerMode mode) : ValueChangedMessage<ShellScrollViewerMode>(mode)
{
}