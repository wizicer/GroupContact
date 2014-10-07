using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GroupContact.Resources;
using GroupContact.ViewModels;

namespace GroupContact
{
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using Microsoft.Phone.UserData;
    using System.Linq;

    public partial class App : Application
    {
        private static MainViewModel viewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null) viewModel = new MainViewModel();

                return viewModel;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {

        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately
            //if (!App.ViewModel.IsDataLoaded)
            //{
            //    App.ViewModel.LoadData();
            //}
            if (e.IsApplicationInstancePreserved)
            {
                ApplicationDataStatus = "application instance preserved.";
                return;
            }

            // Check to see if the key for the application state data is in the State dictionary.
            if (PhoneApplicationService.Current.State.ContainsKey("ApplicationDataObject"))
            {
                // If it exists, assign the data to the application member variable.
                ApplicationDataStatus = "data from preserved state.";
                ApplicationDataObject = PhoneApplicationService.Current.State["ApplicationDataObject"] as SaveModel;
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // If there is data in the application member variable...
            if (ApplicationDataObject != null)
            {
                // Store it in the State dictionary.
                PhoneApplicationService.Current.State["ApplicationDataObject"] = ApplicationDataObject;

                // Also store it in isolated storage, in case the application is never reactivated.
                SaveDataToIsolatedStorage("myDataFile.txt", ApplicationDataObject);
            }
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            // Ensure that required application state is persisted here.

            // The application will not be tombstoned, so save only to isolated storage.
            if (ApplicationDataObject != null)
            {
                SaveDataToIsolatedStorage("myDataFile.txt", ApplicationDataObject);
            }
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized) return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame) RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset) RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh) return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow =
                    (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }






        // Declare a private variable to store application state.
        private SaveModel _applicationDataObject;

        // Declare an event for when the application data changes.
        public event EventHandler ApplicationDataObjectChanged;

        // Declare a public property to access the application data variable.
        public SaveModel ApplicationDataObject
        {
            get
            {
                return _applicationDataObject;
            }
            set
            {
                if (value != _applicationDataObject)
                {
                    _applicationDataObject = value;
                    //OnApplicationDataObjectChanged(EventArgs.Empty);
                }
            }
        }

        // Create a method to raise the ApplicationDataObjectChanged event.
        protected void OnApplicationDataObjectChanged(EventArgs e)
        {
            EventHandler handler = ApplicationDataObjectChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Declare a public property to store the status of the application data.
        public string ApplicationDataStatus { get; set; }

        public void GetDataAsync()
        {
            // Call the GetData method on a new thread.
            Thread t = new Thread(new ThreadStart(GetData));
            t.Start();
        }

        private async void GetData()
        {
            // Check the time elapsed since data was last saved to isolated storage.
            TimeSpan TimeSinceLastSave = TimeSpan.FromSeconds(0);
            if (IsolatedStorageSettings.ApplicationSettings.Contains("DataLastSavedTime"))
            {
                DateTime dataLastSaveTime = (DateTime)IsolatedStorageSettings.ApplicationSettings["DataLastSavedTime"];
                TimeSinceLastSave = DateTime.Now - dataLastSaveTime;
            }

            // Check to see if data exists in isolated storage and see if the data is fresh.
            // This example uses 30 seconds as the valid time window to make it easy to test. 
            // Real apps will use a larger window.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.FileExists("myDataFile.txt") && TimeSinceLastSave.TotalSeconds < 30)
            {
                //// This method loads the data from isolated storage, if it is available.
                //StreamReader sr = new StreamReader(isoStore.OpenFile("myDataFile.txt", FileMode.Open));
                //string data = sr.ReadToEnd();
                //sr.Close();

                ApplicationDataStatus = "data from isolated storage";
                ApplicationDataObject = await IsolatedStorageOperations.Load<SaveModel>("myDataFile.txt");
            }
            else
            {
                LoadData();
                //// Otherwise, it gets the data from the web. 
                //HttpWebRequest request =
                //    (HttpWebRequest)
                //    WebRequest.Create(new Uri("http://windowsteamblog.com/windows_phone/b/windowsphone/rss.aspx"));
                //request.BeginGetResponse(HandleWebResponse, request);
            }
        }
        public void LoadData()
        {
            Contacts cons = new Contacts();
            cons.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(Contacts_SearchCompleted);
            cons.SearchAsync(String.Empty, FilterKind.None, "Contacts Test #1");
        }
        void Contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            var q = e.Results
                .Where(c => c.Companies.FirstOrDefault().IfNull(null, _ => _.CompanyName) == "老师")
                .Select(c => new ItemViewModel
                {
                    LineOne = c.CompleteName.LastName,
                    LineTwo = string.Format("祝{0}节日快乐", c.CompleteName.Nickname ?? c.CompleteName.LastName),
                    LineThree = c.PhoneNumbers.FirstOrDefault().PhoneNumber,
                });
            ApplicationDataStatus = "data from contacts.";
            if (ApplicationDataObject == null)
            {
                ApplicationDataObject = new SaveModel();
            }
            ApplicationDataObject.Queue = q.ToList();
            OnApplicationDataObjectChanged(EventArgs.Empty);
        }

        //private void HandleWebResponse(IAsyncResult result)
        //{
        //    // Put this in a try block in case the web request was unsuccessful.
        //    try
        //    {
        //        // Get the request from the IAsyncResult.
        //        HttpWebRequest request = (HttpWebRequest)(result.AsyncState);

        //        // Read the response stream from the response.
        //        StreamReader sr = new StreamReader(request.EndGetResponse(result).GetResponseStream());
        //        string data = sr.ReadToEnd();

        //        // Use the Dispatcher to call SetData on the UI thread, passing the retrieved data.
        //        //Dispatcher.BeginInvoke(() => { SetData(data, "web"); });
        //        ApplicationDataStatus = "data from web.";
        //        ApplicationDataObject = data;
        //    }
        //    catch
        //    {
        //        // If the data request fails, alert the user.
        //        ApplicationDataStatus = "Unable to get data from Web.";
        //        ApplicationDataObject = "";
        //    }
        //}

        private void SaveDataToIsolatedStorage(string isoFileName, object value)
        {
            value.Save(isoFileName);
            //IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            //StreamWriter sw = new StreamWriter(isoStore.OpenFile(isoFileName, FileMode.OpenOrCreate));
            //sw.Write(value);
            //sw.Close();
            IsolatedStorageSettings.ApplicationSettings["DataLastSaveTime"] = DateTime.Now;
        }
    }

    public class SaveModel
    {
        public int PivotIndex { get; set; }
        public List<ItemViewModel> Queue { get; set; }
    }
    public static class IsolatedStorageOperations
    {
        public static async Task Save<T>(this T obj, string file)
        {
            await Task.Run(() =>
            {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream stream = null;

                try
                {
                    stream = storage.CreateFile(file);
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stream, obj);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            });
        }

        public static async Task<T> Load<T>(string file)
        {

            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            T obj = Activator.CreateInstance<T>();

            if (storage.FileExists(file))
            {
                IsolatedStorageFileStream stream = null;
                try
                {
                    stream = storage.OpenFile(file, FileMode.Open);
                    XmlSerializer serializer = new XmlSerializer(typeof(T));

                    obj = (T)serializer.Deserialize(stream);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
                return obj;
            }
            await obj.Save(file);
            return obj;
        }
    }
}