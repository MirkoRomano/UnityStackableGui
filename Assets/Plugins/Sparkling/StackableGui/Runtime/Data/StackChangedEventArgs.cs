namespace Sparkling.StackableGui
{
    public readonly struct StackChangedEventArgs
    {
        /// <summary>The type of operation that triggered the event.</summary>
        public readonly StackChangeType ChangeType;

        /// <summary>The element involved in the operation. Null when <see cref="ChangeType"/> is <see cref="StackChangeType.Cleared"/>.</summary>
        public readonly IStackableUIElement Element;

        /// <summary>Stack size after the operation.</summary>
        public readonly int NewStackSize;

        /// <summary>Stack size before the operation.</summary>
        public readonly int OldStackSize;

        public StackChangedEventArgs(StackChangeType changeType, IStackableUIElement element, int oldStackSize, int newStackSize)
        {
            ChangeType = changeType;
            Element = element;
            NewStackSize = newStackSize;
            OldStackSize = oldStackSize;
        }
    }
}