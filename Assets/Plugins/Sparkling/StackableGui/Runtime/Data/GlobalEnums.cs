namespace Sparkling.StackableGui
{
    public enum StackVisibilityMode
    {
        /// <summary>Only the top element is active; all others are hidden.</summary>
        TopOnly,

        /// <summary>All elements in the stack remain visible.</summary>
        AllVisible
    }

    public enum CanvasType
    {
        /// <summary>Bottommost layer, intended for background scenes or panoramas.</summary>
        Background = 1,

        /// <summary>Secondary UI panels behind the main UI.</summary>
        Back = 2,

        /// <summary>Main gameplay UI layer.</summary>
        Middle = 3,

        /// <summary>Popups and dialogs.</summary>
        Front = 4,

        /// <summary>Tooltips and notifications.</summary>
        Over = 5,

        /// <summary>System-level UI such as pause menus and settings.</summary>
        System = 6,

        /// <summary>Topmost layer, intended for loading screens.</summary>
        Loading = 7
    }

    public enum InputBlockingMode
    {
        /// <summary>All canvases receive input.</summary>
        BlockNone,

        /// <summary>Only the topmost canvas with elements receives input.</summary>
        BlockBelowTop,

        /// <summary>No canvas receives input.</summary>
        BlockAll
    }

    public enum StackChangeType
    {
        /// <summary>An element was added to the stack.</summary>
        Pushed,

        /// <summary>An element was removed from the stack.</summary>
        Popped,

        /// <summary>All elements were removed at once via ClearCanvas.</summary>
        Cleared
    }
}