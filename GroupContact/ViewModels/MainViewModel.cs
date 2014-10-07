using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GroupContact.Resources;
using Microsoft.Phone.UserData;
using System.Linq;
using System.Text;

namespace GroupContact.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ItemViewModel> smsQueue;

        public MainViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
            this.Groups = new ObservableCollection<ItemViewModel>();
            this.SMSQueue = new ObservableCollection<ItemViewModel>();
            this.PivotIndex = 3;
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public ObservableCollection<ItemViewModel> Groups { get; private set; }
        public ObservableCollection<ItemViewModel> Contacts { get; private set; }

        public ObservableCollection<ItemViewModel> SMSQueue
        {
            get
            {
                return this.smsQueue;
            }
            set
            {
                if (this.smsQueue != value)
                {
                    this.smsQueue = value;
                    this.NotifyPropertyChanged("SMSQueue");
                }
            }
        }

        public int PivotIndex { get; set; }
        public bool IsDataLoaded
        {
            get;
            private set;
        }

        ///// <summary>
        ///// Creates and adds a few ItemViewModel objects into the Items collection.
        ///// </summary>
        //public void LoadData()
        //{
        //    Contacts cons = new Contacts();

        //    //Identify the method that runs after the asynchronous search completes.
        //    cons.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(Contacts_SearchCompleted);

        //    //Start the asynchronous search.
        //    cons.SearchAsync(String.Empty, FilterKind.None, "Contacts Test #1");



        //    //this.SMSQueue.Add(new ItemViewModel { LineOne = "name", LineTwo = "content", LineThree = "13501123" });
        //    //this.SMSQueue.Add(new ItemViewModel { LineOne = "name", LineTwo = "content", LineThree = "13501123" });



        //    this.IsDataLoaded = true;
        //}
        //void Contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        //{
        //    this.Contacts = new ObservableCollection<ItemViewModel>(e.Results
        //        .Select(c => new ItemViewModel
        //        {
        //            LineOne = c.CompleteName.LastName,
        //            LineTwo = c.Companies.FirstOrDefault().IfNull("", _ => _.CompanyName)
        //        }));
        //    NotifyPropertyChanged("Contacts");

        //    this.SMSQueue = new ObservableCollection<ItemViewModel>(e.Results
        //        .Where(c => c.Companies.FirstOrDefault().IfNull(null, _ => _.CompanyName) == "笨笨老师")
        //        .Select(c => new ItemViewModel
        //        {
        //            LineOne = c.CompleteName.LastName,
        //            LineTwo = string.Format("祝{0}节日快乐，身体健康，天天开心！学生梁爽携还未满月的女儿发来祝福", c.CompleteName.Nickname?? c.CompleteName.LastName),
        //            LineThree = c.PhoneNumbers.FirstOrDefault().PhoneNumber,
        //        }));
        //    NotifyPropertyChanged("SMSQueue");
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public static class Extensions
    {
        public static T IfNull<T, P>(this P source, T valueIfNull, Func<P, T> elseAction)
        {
            if (source == null)
            {
                return valueIfNull;
            }
            else
            {
                return elseAction(source);
            }
        }
    }

}