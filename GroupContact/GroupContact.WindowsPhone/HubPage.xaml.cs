using GroupContact.Common;
using GroupContact.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Xml.Serialization;
using System.Diagnostics;
using Windows.Data.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using System.Collections.ObjectModel;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace GroupContact
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public HubPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            this.DefaultViewModel["Groups"] = this.GroupsViewModel;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public ObservableCollection<ContactGroup> GroupsViewModel { get; } = new ObservableCollection<ContactGroup>();

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //var dict = ApplicationData.Current.RoamingSettings.Values;
            //var data = dict["data"] as byte[];
            //if (data == null)
            //{
            //    var group = await SampleDataSource.GetGroupsAsync();
            //    this.DefaultViewModel["Groups"] = group;
            //}
            //else
            //{
            //    var x = new XmlSerializer(typeof(IEnumerable<SampleDataGroup>));
            //    using (var ms = new MemoryStream(data))
            //    {
            //        var group = x.Deserialize(ms) as IEnumerable<SampleDataGroup>;
            //        this.DefaultViewModel["Groups"] = group;
            //    }
            //}

            List<ContactGroup> data = null;
            try
            {
                data = await XmlIO.ReadObjectFromXmlFileAsync<List<ContactGroup>>("data.xml");
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("file not found");
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("format not right");
            }

            Debug.WriteLine(data);
            if (data == null)
            {
                //var group = await SampleDataSource.GetGroupsAsync();
                //this.DefaultViewModel["Groups"] = group.ToList();
            }
            else
            {
                this.GroupsViewModel.Clear();
                foreach (var item in data)
                {
                    this.GroupsViewModel.Add(item);
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private async void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            //var group = this.DefaultViewModel["Groups"] as List<SampleDataGroup>;
            //await XmlIO.SaveObjectToXml(group.First(), "data.xml");


            //var dict = ApplicationData.Current.RoamingSettings.Values;
            //var x = new XmlSerializer(typeof(SampleDataGroup));
            //using (var ms = new MemoryStream())
            //{
            //    var group = this.DefaultViewModel["Groups"];
            //    x.Serialize(ms, group);
            //    dict["data"] = ms.ToArray();
            //}
        }

        /// <summary>
        /// Shows the details of a clicked group in the <see cref="GroupPage"/>.
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Details about the click event.</param>
        private void GroupSection_ItemClick(object sender, ItemClickEventArgs e)
        {
            var group = ((ContactGroup)e.ClickedItem);
            if (!Frame.Navigate(typeof(GroupPage), group))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Defaults about the click event.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void AppBarAddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddGroupDialog();
            var ret = await dlg.ShowAsync();

            if (ret == ContentDialogResult.Primary)
            {
                var group = await GetDataAsync(dlg.Message, dlg.Criterion);
                this.GroupsViewModel.Add(group);
                await XmlIO.SaveObjectToXml(this.GroupsViewModel.ToList(), "data.xml");
            }

        }

        private async Task<ContactGroup> GetDataAsync(string message, string groupName)
        {
#if WINDOWS_PHONE_APP

            ContactStore agenda = await ContactManager.RequestStoreAsync();
            IReadOnlyList<Contact> contacts = null;
            contacts = await agenda.FindContactsAsync();


            var q = contacts
                .Where(c => c.JobInfo.FirstOrDefault()?.CompanyName == groupName)
                .Select(c => new MessageItem
                {
                    Id = c.Id,
                    LastName = c.LastName,
                    FirstName = c.FirstName,
                    DisplayName = c.DisplayName,
                    RawContent = message,
                    Telephone = c.Phones.FirstOrDefault()?.Number,
                })
                .ToList();


            var group = new ContactGroup { Name = groupName, Items = q };
            return group;
#endif

        }
    }
    public class ContactGroup
    {
        public string Name { get; set; }
        public List<MessageItem> Items { get; set; }
    }
    public class MessageItem
    {
        public string Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string DisplayName { get; set; }
        public string Telephone { get; set; }
        [XmlIgnore]
        public string Content
        {
            get
            {
                return this.RawContent.FormatToken(new Dictionary<string, string>
                {
                    [nameof(this.LastName)] = this.LastName,
                    [nameof(this.FirstName)] = this.FirstName,
                    [nameof(this.DisplayName)] = this.DisplayName,
                    [nameof(this.Telephone)] = this.Telephone,
                });
            }
        }
        public string RawContent { get; set; }
        public bool DoneSent { get; set; }
    }
}