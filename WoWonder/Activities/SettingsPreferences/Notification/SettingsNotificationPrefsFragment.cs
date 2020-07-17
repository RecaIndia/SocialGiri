using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Preferences;
using Android.Views;
using WoWonder.Helpers.Model;
using WoWonder.Library.OneSignal;
using CheckBoxPreference = Android.Support.V7.Preferences.CheckBoxPreference;

namespace WoWonder.Activities.SettingsPreferences.Notification
{
    public class SettingsNotificationPrefsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region  Variables Basic

        private CheckBoxPreference NotifcationPlaySoundPref;
        private SwitchPreferenceCompat NotifcationPopupPref;
        private readonly Activity ActivityContext; 

        #endregion

        #region General

        public SettingsNotificationPrefsFragment(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {
                // Load the preferences from an XML resource
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs_Notification);

                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                InitComponent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                NotifcationPopupPref = (SwitchPreferenceCompat)FindPreference("notifications_key");
                NotifcationPlaySoundPref = (CheckBoxPreference)FindPreference("checkBox_PlaySound_key");

                //Update Preferences data on Load
                OnSharedPreferenceChanged(MainSettings.SharedData, "notifications_key");
                 
                NotifcationPopupPref.IconSpaceReserved = false; 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    NotifcationPopupPref.PreferenceChange += NotifcationPopupPrefOnPreferenceChange;
                    NotifcationPlaySoundPref.PreferenceChange += NotifcationPlaySoundPrefOnPreferenceChange;
                }
                else
                {
                    NotifcationPopupPref.PreferenceChange -= NotifcationPopupPrefOnPreferenceChange;
                    NotifcationPlaySoundPref.PreferenceChange -= NotifcationPlaySoundPrefOnPreferenceChange;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Events

        private void NotifcationPlaySoundPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var etp = (CheckBoxPreference)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        UserDetails.SoundControl = true;
                    }
                    else
                    {
                        UserDetails.SoundControl = false;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Notification >> Popup 
        private void NotifcationPopupPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        UserDetails.NotificationPopup = true;
                        OneSignalNotification.RegisterNotificationDevice();
                    }
                    else
                    {
                        UserDetails.NotificationPopup = false;
                        OneSignalNotification.Un_RegisterNotificationDevice();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        //On Change 
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {
                if (key.Equals("notifications_key"))
                {
                    var getvalue = MainSettings.SharedData.GetBoolean("notifications_key", true);
                    NotifcationPopupPref.Checked = getvalue;
                    UserDetails.NotificationPopup = getvalue;
                }
                else if (key.Equals("checkBox_PlaySound_key"))
                {
                    bool getvalue = MainSettings.SharedData.GetBoolean("checkBox_PlaySound_key", true);
                    NotifcationPlaySoundPref.Checked = getvalue;
                    UserDetails.SoundControl = getvalue; 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
} 