using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GroupContact.Resources;
using GroupContact.ViewModels;
using Microsoft.Phone.Tasks;

namespace GroupContact
{
    using System.Collections.ObjectModel;

    public partial class MainPage : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            _isNewPageInstance = true;

            // Set the event handler for when the application data object changes.
            (Application.Current as App).ApplicationDataObjectChanged +=
                          new EventHandler(MainPage_ApplicationDataObjectChanged);

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            // If _isNewPageInstance is true, the page constructor has been called, so
            // state may need to be restored.
            if (_isNewPageInstance)
            {
                //if (!App.ViewModel.IsDataLoaded)
                //{
                //    App.ViewModel.LoadData();

                //}

                // If the application member variable is not empty,
                // set the page's data object from the application member variable.
                if ((Application.Current as App).ApplicationDataObject != null)
                {
                    UpdateApplicationDataUI();
                }
                else
                {
                    // Otherwise, call the method that loads data.
                    statusTextBlock.Text = "getting data...";
                    (Application.Current as App).GetDataAsync();
                }
            }

            // Set _isNewPageInstance to false. If the user navigates back to this page
            // and it has remained in memory, this value will continue to be false.
            _isNewPageInstance = false;
        }

        // The event handler called when the ApplicationDataObject changes.
        void MainPage_ApplicationDataObjectChanged(object sender, EventArgs e)
        {
            // Call UpdateApplicationData on the UI thread.
            Dispatcher.BeginInvoke(() => UpdateApplicationDataUI());
        }
        void UpdateApplicationDataUI()
        {
            // Set the ApplicationData and ApplicationDataStatus members of the ViewModel
            // class to update the UI.
            //dataTextBlock.Text = (Application.Current as App).ApplicationDataObject;
            var app = (Application.Current as App);
            var vm = this.DataContext as MainViewModel;
            if (app.ApplicationDataObject.Queue != null)
            {
                vm.SMSQueue = new ObservableCollection<ItemViewModel>(app.ApplicationDataObject.Queue);
            }
            statusTextBlock.Text = app.ApplicationDataStatus;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var t = e.AddedItems.OfType<PivotItem>().First();
            switch (t.Header.ToString())
            {
                case "Dashboard":
                    BuildDashboardApplicationBar();
                    break;
                case "Smart Group":
                    BuildSmartGroupApplicationBar();
                    break;
                default:
                    break;
            }
        }
        private void BuildDashboardApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
            appBarButton.Text = "help";
            ApplicationBar.Buttons.Add(appBarButton);

            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }
        private void BuildSmartGroupApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.Relative));
            appBarButton.Text = "Add";
            ApplicationBar.Buttons.Add(appBarButton);

            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var fe = e.OriginalSource as FrameworkElement;
            var ivm = fe.DataContext as ItemViewModel;

            SmsComposeTask smsComposeTask = new SmsComposeTask();

            smsComposeTask.To = ivm.LineThree;
            smsComposeTask.Body = ivm.LineTwo;
            smsComposeTask.Show();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}