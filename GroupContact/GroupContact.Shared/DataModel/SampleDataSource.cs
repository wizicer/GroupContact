
// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace GroupContact.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Windows.ApplicationModel.Contacts;
#if WINDOWS_PHONE_APP
    //using Windows.Phone.PersonalInformation;
#endif
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<SampleDataItem>();
        }
        public SampleDataGroup()
        {

        }

        public string UniqueId { get;  set; }
        public string Title { get;  set; }
        public string Subtitle { get;  set; }
        public string Description { get;  set; }
        public string ImagePath { get;  set; }
        [XmlIgnore]
        public ObservableCollection<SampleDataItem> Items { get;  set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
                return;
#if WINDOWS_PHONE_APP

            ContactStore agenda = await ContactManager.RequestStoreAsync();
            IReadOnlyList<Contact> contacts = null;
            contacts = await agenda.FindContactsAsync();


            var q = contacts
                .Where(c => c.JobInfo.FirstOrDefault().IfNotNull(ci => ci.CompanyName == "笨笨老师"))
                .Select(c => new SampleDataItem(c.Id, c.LastName, string.Format("祝{0}节日快乐", c.DisplayName ?? c.LastName), "Assets/DarkGray.png", c.Phones.FirstOrDefault().IfNotNull(cp => cp.Number), "desc"))
                .ToList();


            var group = new SampleDataGroup("hh", "老师", "hello group", "Assets/LightGray.png", "group descirpt");
            foreach (var item in q)
            {
                group.Items.Add(item);

            }
            //group.Items.Add(new SampleDataItem("hll", "title", "subti", "Assets/DarkGray.png", "itemdesc", "comntetahj"));
            this.Groups.Add(group);
#endif

            //Uri dataUri = new Uri("ms-appx:///DataModel/SampleData.json");

            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            //string jsonText = await FileIO.ReadTextAsync(file);
            //JsonObject jsonObject = JsonObject.Parse(jsonText);
            //JsonArray jsonArray = jsonObject["Groups"].GetArray();

            //foreach (JsonValue groupValue in jsonArray)
            //{
            //    JsonObject groupObject = groupValue.GetObject();
            //    SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
            //                                                groupObject["Title"].GetString(),
            //                                                groupObject["Subtitle"].GetString(),
            //                                                groupObject["ImagePath"].GetString(),
            //                                                groupObject["Description"].GetString());

            //    foreach (JsonValue itemValue in groupObject["Items"].GetArray())
            //    {
            //        JsonObject itemObject = itemValue.GetObject();
            //        group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
            //                                           itemObject["Title"].GetString(),
            //                                           itemObject["Subtitle"].GetString(),
            //                                           itemObject["ImagePath"].GetString(),
            //                                           itemObject["Description"].GetString(),
            //                                           itemObject["Content"].GetString()));
            //    }
            //    this.Groups.Add(group);
            //}
        }
    }

    internal static class ExtensionMethods
    {
        public static TRet IfNotNull<T, TRet>(this T obj, Func<T, TRet> action) where T : class
        {
            if (obj != null)
            {
                return action(obj);
            }

            return default(TRet);
        }
    }
}