using System;
using Windows.UI.Xaml.Controls.Primitives;

namespace Screenbox.Controls.Interactions
{
    public class ListViewDoubleTappedEventArgs : EventArgs
    {
        public SelectorItem Item { get; }

        public bool Handled { get; set; }

        public ListViewDoubleTappedEventArgs(SelectorItem item)
        {
            Item = item;
        }
    }
}
