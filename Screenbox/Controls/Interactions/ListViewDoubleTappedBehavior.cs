#nullable enable

using System.Windows.Input;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Xaml.Interactivity;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Screenbox.Controls.Interactions
{
    internal class ListViewDoubleTappedBehavior : Trigger<ListViewBase>
    {
        public event TypedEventHandler<ListViewDoubleTappedBehavior, ListViewDoubleTappedEventArgs>? ContextRequested;

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(ListViewDoubleTappedBehavior),
            new PropertyMetadata(null));

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            //AssociatedObject.ContextRequested += OnContextRequested;
            AssociatedObject.DoubleTapped += OnDoubleTapped;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            //AssociatedObject.ContextRequested -= OnContextRequested;
            AssociatedObject.DoubleTapped -= OnDoubleTapped;
        }

        /*
        private void OnContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            if (args.OriginalSource is not SelectorItem element) return;
            ListViewDoubleTappedEventArgs eventArgs = new(element);
            ContextRequested?.Invoke(this, eventArgs);
            if (eventArgs.Handled) return;

            Interaction.ExecuteActions(AssociatedObject, Actions, element.Content);
            if (Command == null) return;
            if (Command is MenuCommand { Items: { } } menuCommand)
            {
                SetMenuCommandDataContext(menuCommand.Items, element.Content);
            }

            Command.ShowAt(element);
            args.Handled = true;
        }*/

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is not FrameworkElement element ||
                element.FindAscendantOrSelf<SelectorItem>() is not { } item) return;
            ListViewDoubleTappedEventArgs eventArgs = new(item);
            ContextRequested?.Invoke(this, eventArgs);
            if (eventArgs.Handled) return;

            Interaction.ExecuteActions(AssociatedObject, Actions, element.DataContext);
            if (Command == null) return;

            if (Command.CanExecute(element.DataContext))
            {
                Command.Execute(element.DataContext);
            }

            e.Handled = true;
        }

    }
}
