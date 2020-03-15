using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Configuration;
using Ookii.Dialogs.Wpf;

namespace ImageSlideshow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timerImageChangeA;
        private DispatcherTimer timerImageReloadA;
        private DispatcherTimer timerImageChangeB;
        private DispatcherTimer timerImageReloadB;

        private DispatcherTimer timerSliderStartA;
        private DispatcherTimer timerSliderStartB;

        private DispatcherTimer timerSliderChangeA;
        private DispatcherTimer timerSliderChangeB;
        private int SliderChangeTimeA;
        private int SliderChangeTimeB;

        //Special Event und Zeitgeber
        private DispatcherTimer timerTimewatch;
        private DispatcherTimer timerClock;
        private DispatcherTimer timerSpecialEventCountdown;

        string time_now;
        string time_now_check;
        int time_Countdown_sec;
        bool firstrun_specialevant = true;

        //Imagecontrols
        private Image[] ImageControlsA;
        private List<ImageSource> ImagesA = new List<ImageSource>();
        private Image[] ImageControlsB;
        private List<ImageSource> ImagesB = new List<ImageSource>();

        private bool animationon = false;

        private static string[] ValidImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        private static string[] TransitionEffects = new[] { "Fade" };
        private bool hptmenuopen;
        private bool specialeventopen = false;

        // -> Aus App.config ->
        private string TransitionTypeA, strImagePathA = "";
        private string TransitionTypeB, strImagePathB = "";

        private int CurrentSourceIndexA, CurrentCtrlIndexA, EffectIndexA = 0, IntervalTimeA = 1;
        private int ScanNewTimeA = 1;
        private int CurrentSourceIndexB, CurrentCtrlIndexB, EffectIndexB = 0, IntervalTimeB = 1;
        private int ScanNewTimeB = 1;

        private int SpecialEventTimeStartH;
        private int SpecialEventTimeStartM;
        private string SpecialEventTimeStart;
        private int SpecialEventCountdownDauer;
        //Animationsdauer/ Typ
        private int taduration = Convert.ToInt32(ConfigurationManager.AppSettings["taduration"]);
        private string animationtype = (ConfigurationManager.AppSettings["animationtype"]);
        //Startwerte
        private int taoben = Convert.ToInt32(ConfigurationManager.AppSettings["taoben"]);
        private int taunten = Convert.ToInt32(ConfigurationManager.AppSettings["taunten"]);
        private int taleft = Convert.ToInt32(ConfigurationManager.AppSettings["taleft"]);
        private int taright = Convert.ToInt32(ConfigurationManager.AppSettings["taright"]);
        //Verschiebung
        private int taleftna = Convert.ToInt32(ConfigurationManager.AppSettings["taleftna"]);
        private int tarightna = Convert.ToInt32(ConfigurationManager.AppSettings["tarightna"]);
        private int taleftnb = Convert.ToInt32(ConfigurationManager.AppSettings["taleftnb"]);
        private int tarightnb = Convert.ToInt32(ConfigurationManager.AppSettings["tarightnb"]);

        //------------------------------ STARTSEQUENZ ------------------------------------------------------------------------------

        private string slidernr = "slider1";

        public MainWindow()
        {
            InitializeComponent();
        }

        //**************************************************************************************************************
        //-----------------------Form Laden----------------------------------------------------------------------
        //************************************************************************************************************
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Load_Settings();

            PlaySlideShowA();
            PlaySlideShowB();
            
            Load_Animation();
            Load_Timers();
            Load_Menuevalue();

            string versionnr = "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "   © 2015/16";

            labelVersionNr.Content = versionnr;

            //Slider B Verschieben am Start
            int taleftnn = -taleftnb;
            int tarightnn = tarightnb;

            myImage1b.Margin = new Thickness(taleftnn, taoben, tarightnn, taunten);
            myImage2b.Margin = new Thickness(taleftnn, taoben, tarightnn, taunten);

            timerSliderStartA = new DispatcherTimer();
            timerSliderStartA.Interval = new TimeSpan(0, 0, 0, 1, 0);
            timerSliderStartA.Tick += new EventHandler(timerSliderChangeA_Tick);

            timerSliderStartB = new DispatcherTimer();
            timerSliderStartB.Interval = new TimeSpan(0, 0, 0, 1, 0);
            timerSliderStartB.Tick += new EventHandler(timerSliderChangeB_Tick);

            timerClock = new DispatcherTimer();
            timerClock.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerClock.Tick += new EventHandler(timerClock_Tick);

            timerTimewatch = new DispatcherTimer();
            timerTimewatch.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerTimewatch.Tick += new EventHandler(TimeWatch_Tick);

            timerSpecialEventCountdown = new DispatcherTimer();
            timerSpecialEventCountdown.Interval = new TimeSpan(0, 0, 1);
            timerSpecialEventCountdown.Tick += new EventHandler(Countdown_Tick);

            // Timer einschalten
            timerSliderStartA.IsEnabled = true;
            timerSliderStartB.IsEnabled = true;

            timerTimewatch.IsEnabled = true;
            timerClock.IsEnabled = true;

            timerImageChangeA.IsEnabled = true;
            timerImageReloadA.IsEnabled = true;
            timerImageChangeB.IsEnabled = true;
            timerImageReloadB.IsEnabled = true;

            timerSliderStartA.IsEnabled = false;
            Console.WriteLine("Slideshow A Started");
            timerSliderStartB.IsEnabled = false;
            Console.WriteLine("Slideshow B Started");
        }

        private void Load_Settings()
        {
            //Animationsdauer/ Typ
            taduration = Convert.ToInt32(ConfigurationManager.AppSettings["taduration"]);
            animationtype = ConfigurationManager.AppSettings["animationtype"];

            SliderChangeTimeA = Convert.ToInt32(ConfigurationManager.AppSettings["SliderChangeTimeA"]);
            SliderChangeTimeB = Convert.ToInt32(ConfigurationManager.AppSettings["SliderChangeTimeB"]);

            IntervalTimeA = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalTimeA"]);
            ScanNewTimeA = Convert.ToInt32(ConfigurationManager.AppSettings["ReloadTimeA"]);

            IntervalTimeB = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalTimeB"]);
            ScanNewTimeB = Convert.ToInt32(ConfigurationManager.AppSettings["ReloadTimeB"]);

            strImagePathA = ConfigurationManager.AppSettings["ImagePathA"];
            strImagePathB = ConfigurationManager.AppSettings["ImagePathB"];

            SpecialEventTimeStartH = Convert.ToInt32(ConfigurationManager.AppSettings["SpecialEventTimeStartH"]);
            SpecialEventTimeStartM = Convert.ToInt32(ConfigurationManager.AppSettings["SpecialEventTimeStartM"]);
            SpecialEventCountdownDauer = Convert.ToInt32(ConfigurationManager.AppSettings["SpecialEventCountdown"]);

            sliderIntervalA.Value = IntervalTimeA;
            sliderIntervalB.Value = IntervalTimeB;
            sliderScanNewA.Value = ScanNewTimeA;
            sliderScanNewB.Value = ScanNewTimeB;
            sliderSliderChangeA.Value = SliderChangeTimeA;
            sliderSliderChangeB.Value = SliderChangeTimeB;

            sliderAnimation.Value = taduration;

            label_SliderSelectFolderA.Content = TrimPath(strImagePathA);
            label_SliderSelectFolderB.Content = TrimPath(strImagePathB);

            SpecialEventTimeStart = string.Format("{0:00}:{1:00}", SpecialEventTimeStartH, SpecialEventTimeStartM);
            slider_SpecialEventH.Value = Convert.ToInt32(SpecialEventTimeStartH);
            slider_SpecialEventM.Value = Convert.ToInt32(SpecialEventTimeStartM);
            time_Countdown_sec = SpecialEventCountdownDauer * 60;

            slider_SpecialEventCountdownDauer.Value = Convert.ToInt32(SpecialEventCountdownDauer);           

            Console.WriteLine("Settings Loaded");
        }

        private void Load_Menuevalue()
        {
            sliderIntervalA.Value = SliderChangeTimeA;
            sliderIntervalB.Value = SliderChangeTimeB;
            sliderScanNewA.Value = ScanNewTimeA;
            sliderScanNewB.Value = ScanNewTimeB;
            sliderSliderChangeA.Value = SliderChangeTimeA;
            sliderSliderChangeB.Value = SliderChangeTimeB;

            sliderAnimation.Value = taduration;

            label_SliderSelectFolderA.Content = TrimPath(strImagePathA);
            label_SliderSelectFolderB.Content = TrimPath(strImagePathB);

            slider_SpecialEventH.Value = Convert.ToInt32(SpecialEventTimeStartH);
            slider_SpecialEventM.Value = Convert.ToInt32(SpecialEventTimeStartM);

            slider_SpecialEventCountdownDauer.Value = Convert.ToInt32(SpecialEventCountdownDauer);

            Console.WriteLine("Menü Loaded");
        }

        private void Load_Timers()
        {
            //Slider change to X
            timerSliderChangeA = new DispatcherTimer();
            timerSliderChangeA.Interval = new TimeSpan(0, SliderChangeTimeA, 0);
            timerSliderChangeA.Tick += new EventHandler(timerSliderChangeA_Tick);

            timerSliderChangeB = new DispatcherTimer();
            timerSliderChangeB.Interval = new TimeSpan(0, SliderChangeTimeB, 0);
            timerSliderChangeB.Tick += new EventHandler(timerSliderChangeB_Tick);

            //Slider A
            ImageControlsA = new[] { myImage1a, myImage2a };

            LoadImageFolderA(strImagePathA);

            timerImageChangeA = new DispatcherTimer();
            timerImageChangeA.Interval = new TimeSpan(0, 0, IntervalTimeA);
            timerImageChangeA.Tick += new EventHandler(timerImageChangeA_Tick);

            timerImageReloadA = new DispatcherTimer();
            timerImageReloadA.Interval = new TimeSpan(0, ScanNewTimeA, 0);
            timerImageReloadA.Tick += new EventHandler(ReLoadImageFolderA);

            //Slider B
            ImageControlsB = new[] { myImage1b, myImage2b };

            LoadImageFolderB(strImagePathB);

            timerImageChangeB = new DispatcherTimer();
            timerImageChangeB.Interval = new TimeSpan(0, 0, IntervalTimeB);
            timerImageChangeB.Tick += new EventHandler(timerImageChangeB_Tick);

            timerImageReloadB = new DispatcherTimer();
            timerImageReloadB.Interval = new TimeSpan(0, ScanNewTimeB, 0);
            timerImageReloadB.Tick += new EventHandler(ReLoadImageFolderB);

            Console.WriteLine("Timers Started");
        }

        private void Load_Animation()
        {
            switch (animationtype)
            {

                case "slideleftright":
                    taleftnb = taleftnb * (-1);
                    tarightnb = tarightnb * (-1);
                    break;

                case "slideleftleft":
                    taleftnb = Math.Abs(taleftnb);
                    tarightnb = Math.Abs(tarightnb);
                    break;
            }
        }

        public static string TrimPath(string path)
        {
            int someArbitaryNumber = 15;
            string directory = path;
            if (directory.Length > someArbitaryNumber)
            {
                return String.Format(@"{0}...{1}",
                    directory.Substring(0, someArbitaryNumber-2), directory.Substring(directory.Length-10,10));
            }
            else
            {
                return path;
            }
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            time_now = string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            time_now_check = string.Format("{0:00}:{1:00}", DateTime.Now.Hour, DateTime.Now.Minute);

            label_Uhrzeit.Content = time_now;
        }

        //**************************************************************************************************************
        //-----------------------Special Event----------------------------------------------------------------------
        //************************************************************************************************************

        private void TimeWatch_Tick(object sender, EventArgs e)
        {
            if (time_now_check == SpecialEventTimeStart && specialeventopen == false)
            {
                specialeventopen = true;
                timerTimewatch.IsEnabled = false;
                timerSpecialEventCountdown.IsEnabled = true;
                Console.WriteLine("SpezialEvent Started");
                ThicknessAnimation tasp = new ThicknessAnimation();
                tasp.From = gridSpecialEvent.Margin;
                tasp.To = new Thickness(0, 0, 0, 0);
                tasp.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
                gridSpecialEvent.BeginAnimation(Grid.MarginProperty, tasp);
            }
        }


        private void Countdown_Tick(object sender, EventArgs e)
        {
            if (firstrun_specialevant == true)
            {
                firstrun_specialevant = false;

                timerClock.IsEnabled = false;
                timerTimewatch.IsEnabled = false;
                time_Countdown_sec = time_Countdown_sec - (Convert.ToInt32(DateTime.Now.Second) - 1);
            }
            

            time_now = string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            time_now_check = string.Format("{0:00}:{1:00}", DateTime.Now.Hour, DateTime.Now.Minute);

            time_Countdown_sec = time_Countdown_sec - 1;

            TimeSpan span_timecountdownleft = new TimeSpan(0, 0, time_Countdown_sec);
            int normalizedHours = span_timecountdownleft.Hours;
            int normalizedMinutes = span_timecountdownleft.Minutes;
            int normalizedSeconds = span_timecountdownleft.Seconds;

            if (time_Countdown_sec < -120)
            {
                timerSpecialEventCountdown.IsEnabled = false;
                timerClock.IsEnabled = true;

                SpecialEvent_Over();
            }
            else
            {
                if (time_Countdown_sec > -1)
                label_UhrzeitCountdown.Content = string.Format("{0:00}:{1:00}:{2:00}", normalizedHours, normalizedMinutes, normalizedSeconds); 
                label_Uhrzeit.Content = time_now;
            }
        }

        private void SpecialEvent_Over()
        {         
            timerTimewatch.IsEnabled = true;
            timerSpecialEventCountdown.IsEnabled = false;
            
            ThicknessAnimation tasp = new ThicknessAnimation();
            tasp.From = gridSpecialEvent.Margin;
            tasp.To = new Thickness(0, -1500, 0, 1500);
            tasp.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
            gridSpecialEvent.BeginAnimation(Grid.MarginProperty, tasp);

            Console.WriteLine("SpezialEvent Stopped");
            specialeventopen = false;
        }

        //**************************************************************************************************************
        //-----------------------Vollbild----------------------------------------------------------------------
        //************************************************************************************************************
        private void checkBoxVollbild_Checked(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
        }

        private void checkBoxVollbild_Unchecked(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
            ResizeMode = ResizeMode.CanResize;
        }


        //**************************************************************************************************************
        //-----------------------Mausposition (Menüzugriff) etc----------------------------------------------------------------------
        //************************************************************************************************************
        // QuickMenü & Hauptmenü
        private void grd_MouseEnter(object sender, MouseEventArgs e)
        {
            Point mouse = Mouse.GetPosition(Application.Current.MainWindow);
            int mousepY = Convert.ToInt16(mouse.Y);
            int mousepX = Convert.ToInt16(mouse.X);

            int gridheight = Convert.ToInt16(grd.ActualHeight);
            int gridwidth = Convert.ToInt16(grd.ActualWidth);

            if (gridheight - 40 < mousepY && animationon == false)
            {
                animationon = true;
                ThicknessAnimation taquickmenu = new ThicknessAnimation();
                taquickmenu.From = QuickSettingsGrid.Margin;
                taquickmenu.To = new Thickness(0, 35, 0, 0);
                taquickmenu.Duration = new Duration(TimeSpan.FromMilliseconds(150));
                QuickSettingsGrid.BeginAnimation(Grid.MarginProperty, taquickmenu);
                animationon = false;
            }
            
            if (gridheight - 60 > mousepY && animationon == false) 
            {
                animationon = true;
                ThicknessAnimation taquickmenu = new ThicknessAnimation();
                taquickmenu.From = QuickSettingsGrid.Margin;
                taquickmenu.To = new Thickness(0, 0, 0, -35);
                taquickmenu.Duration = new Duration(TimeSpan.FromMilliseconds(150));
                QuickSettingsGrid.BeginAnimation(Grid.MarginProperty, taquickmenu);
                animationon = false;
            }
            //Hauptmenü
            if (gridheight - 40 > mousepX && mousepX < 50 && animationon == false && hptmenuopen == false)
            {
                animationon = true;
                ThicknessAnimation tahauptmenu = new ThicknessAnimation();
                tahauptmenu.From = MainSettingsGrid.Margin;
                tahauptmenu.To = new Thickness(0, 0, 250, 0);
                tahauptmenu.Duration = new Duration(TimeSpan.FromMilliseconds(150));
                MainSettingsGrid.BeginAnimation(Grid.MarginProperty, tahauptmenu);
                hptmenuopen = true;
                animationon = false;
            }

            if (gridheight - 40 > mousepX && mousepX > 300 && animationon == false && hptmenuopen == true)
            {
                animationon = true;
                ThicknessAnimation tahauptmenu = new ThicknessAnimation();
                tahauptmenu.From = MainSettingsGrid.Margin;
                tahauptmenu.To = new Thickness(-250, 0, 0, 0);
                tahauptmenu.Duration = new Duration(TimeSpan.FromMilliseconds(150));
                MainSettingsGrid.BeginAnimation(Grid.MarginProperty, tahauptmenu);
                hptmenuopen = false;
                animationon = false;
            }
        }

        //Speichern
        private void Save_Settings(string value, string key)
        {
             Configuration config = ConfigurationManager.OpenExeConfiguration(
             System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (config.AppSettings.Settings[key] != null)
                {
                    config.AppSettings.Settings.Remove(key);
                }
                config.AppSettings.Settings.Add(key, value);
                config.Save(ConfigurationSaveMode.Modified);                              
        }

        //Ordner Auswählen
        private void button_SelectFolderA_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
            // dlg.SelectedPath = Properties.Settings.Default.StoreFolder;
            dlg.ShowNewFolderButton = true;
            dlg.ShowDialog();
            string path = dlg.SelectedPath;

            string key = "ImagePath";
            //Laden der AppSettings
            Configuration config = ConfigurationManager.OpenExeConfiguration(
                                    System.Reflection.Assembly.GetExecutingAssembly().Location);
            //Überprüfen ob Key existiert
            if (config.AppSettings.Settings[key] != null)
            {
                //Key existiert. Löschen des Keys zum "überschreiben"
                config.AppSettings.Settings.Remove(key);
            }
            //Anlegen eines neuen KeyValue-Paars
            config.AppSettings.Settings.Add(key, path);
            //Speichern der aktualisierten AppSettings
            config.Save(ConfigurationSaveMode.Modified);

            strImagePathA = path;
            label_SliderSelectFolderA.Content = TrimPath(strImagePathA);
            ReLoadImageFolderA(timerSliderChangeA, e); // Call your method here.
        }

        private void button_SelectFolderB_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
            // dlg.SelectedPath = Properties.Settings.Default.StoreFolder;
            dlg.ShowNewFolderButton = true;
            dlg.ShowDialog();
            string path = dlg.SelectedPath;

            string key = "ImagePathB";
            //Laden der AppSettings
            Configuration config = ConfigurationManager.OpenExeConfiguration(
                                    System.Reflection.Assembly.GetExecutingAssembly().Location);
            //Überprüfen ob Key existiert
            if (config.AppSettings.Settings[key] != null)
            {
                //Key existiert. Löschen des Keys zum "überschreiben"
                config.AppSettings.Settings.Remove(key);
            }
            //Anlegen eines neuen KeyValue-Paars
            config.AppSettings.Settings.Add(key, path);
            //Speichern der aktualisierten AppSettings
            config.Save(ConfigurationSaveMode.Modified);

            strImagePathB = path;        

            label_SliderSelectFolderB.Content = TrimPath(strImagePathB);
            ReLoadImageFolderB(timerSliderChangeB, e); // Call your method here.
        }      

        //Animationstyp Settings
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = comboBox.SelectedIndex;

            switch (i)
            {

                case 0:
                    animationtype = "slideleftright";
                    string keya = "animationtype";
                    //Laden der AppSettings
                    Configuration configa = ConfigurationManager.OpenExeConfiguration(
                                            System.Reflection.Assembly.GetExecutingAssembly().Location);
                    //Überprüfen ob Key existiert
                    if (configa.AppSettings.Settings[keya] != null)
                    {
                        //Key existiert. Löschen des Keys zum "überschreiben"
                        configa.AppSettings.Settings.Remove(keya);
                    }
                    //Anlegen eines neuen KeyValue-Paars
                    configa.AppSettings.Settings.Add(keya, animationtype);
                    //Speichern der aktualisierten AppSettings
                    configa.Save(ConfigurationSaveMode.Modified);
                    break;

                case 1:
                    animationtype = "slideleftleft";
                    string keyb = "animationtype";
                    //Laden der AppSettings
                    Configuration configb = ConfigurationManager.OpenExeConfiguration(
                                            System.Reflection.Assembly.GetExecutingAssembly().Location);
                    //Überprüfen ob Key existiert
                    if (configb.AppSettings.Settings[keyb] != null)
                    {
                        //Key existiert. Löschen des Keys zum "überschreiben"
                        configb.AppSettings.Settings.Remove(keyb);
                    }
                    //Anlegen eines neuen KeyValue-Paars
                    configb.AppSettings.Settings.Add(keyb, animationtype);
                    //Speichern der aktualisierten AppSettings
                    configb.Save(ConfigurationSaveMode.Modified);
                    break;
            }
            Load_Animation();

        }

        //**************************************************************************************************************
        //-----------------------Infoscreen aktivieren----------------------------------------------------------------------
        //************************************************************************************************************
        private void checkBoxInfobox_Checked(object sender, RoutedEventArgs e)
        {
            switch (slidernr)
            {
                case "slider1":
                    timerSliderChangeA.IsEnabled = true;
                    timerSliderChangeB.IsEnabled = false;
                    break;
                case "slider2":
                    timerSliderChangeA.IsEnabled = false;
                    timerSliderChangeB.IsEnabled = true;
                    break;
                default:
                    timerSliderChangeA.IsEnabled = true;
                    timerSliderChangeB.IsEnabled = false;
                    break;
            }
        }

        private void checkBoxInfobox_Unchecked(object sender, RoutedEventArgs e)
        {
            timerSliderChangeA.IsEnabled = false;
            timerSliderChangeB.IsEnabled = false;
        }

        //**************************************************************************************************************
        //-----------------------Aktualisieren des Sliders A----------------------------------------------------------------------
        //************************************************************************************************************
        private void ReLoadImageFolderA(object sender, EventArgs e)
        {
            string folderA = strImagePathA;
            ErrorText.Visibility = Visibility.Collapsed;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (!System.IO.Path.IsPathRooted(folderA))
                folderA = System.IO.Path.Combine(Environment.CurrentDirectory, folderA);
            if (!System.IO.Directory.Exists(folderA))
            {
                ErrorText.Text = "The specified folder does not exist: " + Environment.NewLine + folderA;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            Random randa = new Random();
            var sources = from file in new System.IO.DirectoryInfo(folderA).GetFiles().AsParallel()
                          where ValidImageExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase)
                          orderby randa.Next()
                          select CreateImageSourceA(file.FullName, true);
            ImagesA.Clear();
            ImagesA.AddRange(sources);
            sw.Stop();
            Console.WriteLine("Total time to load {0} images: {1}ms", ImagesA.Count, sw.ElapsedMilliseconds);
        }

        //**************************************************************************************************************
        //-----------------------Aktualisieren des Sliders B----------------------------------------------------------------------
        //************************************************************************************************************
        private void ReLoadImageFolderB(object sender, EventArgs e)
        {
            string folderB = strImagePathB;
            ErrorText.Visibility = Visibility.Collapsed;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (!System.IO.Path.IsPathRooted(folderB))
                folderB = System.IO.Path.Combine(Environment.CurrentDirectory, folderB);
            if (!System.IO.Directory.Exists(folderB))
            {
                ErrorText.Text = "The specified folder does not exist: " + Environment.NewLine + folderB;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            Random randb = new Random();
            var sources = from fileB in new System.IO.DirectoryInfo(folderB).GetFiles().AsParallel()
                          where ValidImageExtensions.Contains(fileB.Extension, StringComparer.InvariantCultureIgnoreCase)
                          orderby randb.Next()
                          select CreateImageSourceB(fileB.FullName, true);
            ImagesB.Clear();
            ImagesB.AddRange(sources);
            sw.Stop();
            Console.WriteLine("Total time to load {0} images: {1}ms", ImagesB.Count, sw.ElapsedMilliseconds);
        }

        //**************************************************************************************************************
        //-----------------------Wechsel zu Slider X----------------------------------------------------------------------
        //************************************************************************************************************
        private void timerSliderChangeB_Tick(object sender, EventArgs e)
        {
            radioButtonSlider1.IsChecked = true;
            timerSliderChangeA.IsEnabled = true;
            timerSliderChangeB.IsEnabled = false;
            timerImageChangeA.IsEnabled = true;
            timerImageChangeB.IsEnabled = false;
            timerImageReloadA.IsEnabled = true;
            timerImageReloadB.IsEnabled = false;
        }

        private void timerSliderChangeA_Tick(object sender, EventArgs e)
        {
            radioButtonSlider2.IsChecked = true;
            timerSliderChangeA.IsEnabled = false;
            timerSliderChangeB.IsEnabled = true;
            timerImageChangeA.IsEnabled = false;
            timerImageChangeB.IsEnabled = true;
            timerImageReloadA.IsEnabled = false;
            timerImageReloadB.IsEnabled = true;
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            if (slidernr == "slider1")
            {
                int taleftnn = -taleftna;
                int tarightnn = tarightna;
                int taleftnnb = taleftnb;
                int tarightnnb = -tarightnb;

                ThicknessAnimation ta1a = new ThicknessAnimation();
                ThicknessAnimation ta2a = new ThicknessAnimation();
                ThicknessAnimation ta1b = new ThicknessAnimation();
                ThicknessAnimation ta2b = new ThicknessAnimation();
                //your first place
                ta1a.From = myImage1a.Margin;
                ta2a.From = myImage1a.Margin;
                ta1b.From = myImage1b.Margin;
                ta2b.From = myImage1b.Margin;
                //this move your grid 1000 over from left side
                //you can use -1000 to move to left side
                ta1a.To = new Thickness(taleftnn, taoben, tarightnn, taunten);
                ta2a.To = new Thickness(taleftnn, taoben, tarightnn, taunten);
                ta1b.To = new Thickness(taleft, taoben, taright, taunten);
                ta2b.To = new Thickness(taleft, taoben, taright, taunten);
                //time the animation playes
                ta1a.Duration = new Duration(TimeSpan.FromMilliseconds(taduration));
                ta2a.Duration = new Duration(TimeSpan.FromMilliseconds(taduration));
                ta1b.Duration = new Duration(TimeSpan.FromMilliseconds(taduration));
                ta2b.Duration = new Duration(TimeSpan.FromMilliseconds(taduration));
                //dont need to use story board but if you want pause,stop etc use story board
                myImage1a.BeginAnimation(Grid.MarginProperty, ta1a);
                myImage2a.BeginAnimation(Grid.MarginProperty, ta2a);
                myImage1b.BeginAnimation(Grid.MarginProperty, ta1b);
                myImage2b.BeginAnimation(Grid.MarginProperty, ta2b);


                slidernr = "slider2";
            }

        }

        //------------------------------------------------------------------------------------------------------------------

        private void radioButtonSlider1_Checked(object sender, RoutedEventArgs e)
        {
            if (slidernr == "slider2")
            {
                int taleftnn = taleftna;
                int tarightnn = -tarightna;
                int taleftnnb = -taleftnb;
                int tarightnnb = tarightnb;

                ThicknessAnimation ta1a = new ThicknessAnimation();
                ThicknessAnimation ta2a = new ThicknessAnimation();
                ThicknessAnimation ta1b = new ThicknessAnimation();
                ThicknessAnimation ta2b = new ThicknessAnimation();
                //your first place
                ta1a.From = myImage1a.Margin;
                ta2a.From = myImage1a.Margin;
                ta1b.From = myImage1b.Margin;
                ta2b.From = myImage1b.Margin;
                //this move your grid 1000 over from left side
                //you can use -1000 to move to left side
                ta1a.To = new Thickness(taleft, taoben, taright, taunten);
                ta2a.To = new Thickness(taleft, taoben, taright, taunten);
                ta1b.To = new Thickness(taleftnnb, taoben, tarightnnb, taunten);
                ta2b.To = new Thickness(taleftnnb, taoben, tarightnnb, taunten);
                //time the animation playes
                ta1a.Duration = new Duration(TimeSpan.FromMilliseconds(taduration));
                ta2a.Duration = new Duration(TimeSpan.FromMilliseconds(taduration));
                ta1b.Duration = new Duration(TimeSpan.FromMilliseconds(taduration));
                ta2b.Duration = new Duration(TimeSpan.FromMilliseconds(taduration));
                //dont need to use story board but if you want pause,stop etc use story board
                myImage1a.BeginAnimation(Grid.MarginProperty, ta1a);
                myImage2a.BeginAnimation(Grid.MarginProperty, ta2a);
                myImage1b.BeginAnimation(Grid.MarginProperty, ta1b);
                myImage2b.BeginAnimation(Grid.MarginProperty, ta2b);

                slidernr = "slider1";
            }
        }

        //**************************************************************************************************************
        //-----------------------Hauptmenü speichern routine----------------------------------------------------------------------
        //************************************************************************************************************
        private void sliderIntervalA_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderChangeTimeA = Convert.ToInt16(sliderIntervalA.Value);
            if (timerImageChangeA != null)
            {
                timerImageChangeA.Interval = new TimeSpan(0, SliderChangeTimeA, 0);
                Save_Settings(Convert.ToString(SliderChangeTimeA), "IntervalTimeA");
            }           
        }

        private void sliderIntervalB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderChangeTimeB = Convert.ToInt16(sliderIntervalA.Value);
            if (timerImageChangeB != null)
            {
                timerImageChangeB.Interval = new TimeSpan(0, SliderChangeTimeB, 0);
                Save_Settings(Convert.ToString(SliderChangeTimeB), "IntervalTimeB");
            }
        }

        private void sliderScanNewA_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScanNewTimeA = Convert.ToInt16(sliderScanNewA.Value);
            if (timerImageReloadA != null)
            {
                timerImageReloadA.Interval = new TimeSpan(0, ScanNewTimeA, 0);
                Save_Settings(Convert.ToString(ScanNewTimeA), "ReloadTimeA");
            }
        }

        private void sliderScanNewB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScanNewTimeB = Convert.ToInt16(sliderScanNewB.Value);
            if (timerImageReloadB != null)
            {
                timerImageReloadB.Interval = new TimeSpan(0, ScanNewTimeB, 0);
                Save_Settings(Convert.ToString(ScanNewTimeB), "ReloadTimeB");
            }
        }

        private void sliderSliderChangeA_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderChangeTimeA = Convert.ToInt16(sliderSliderChangeA.Value);
            if (timerSliderChangeA != null)
            {
                timerSliderChangeA.Interval = new TimeSpan(0, SliderChangeTimeA, 0);
                Save_Settings(Convert.ToString(SliderChangeTimeA), "SliderChangeTimeA");
            }
        }

        private void sliderSliderChangeB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderChangeTimeB = Convert.ToInt16(sliderSliderChangeB.Value);
            if (timerSliderChangeB != null)
            {
                timerSliderChangeB.Interval = new TimeSpan(0, SliderChangeTimeB, 0);
                Save_Settings(Convert.ToString(SliderChangeTimeB), "SliderChangeTimeB");
            }
        }

        private void sliderAnimation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            taduration = Convert.ToInt16(sliderAnimation.Value);
            Save_Settings(Convert.ToString(taduration), "taduration");
        }

        private void slider_SpecialEventH_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SpecialEventTimeStartH = Convert.ToInt16(slider_SpecialEventH.Value);
            Save_Settings(Convert.ToString(SpecialEventTimeStartH), "SpecialEventTimeStartH");
            SpecialEventTimeStart = string.Format("{0:00}:{1:00}", SpecialEventTimeStartH, SpecialEventTimeStartM);
        }

        private void slider_SpecialEventM_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SpecialEventTimeStartM = Convert.ToInt16(slider_SpecialEventM.Value);
            Save_Settings(Convert.ToString(SpecialEventTimeStartM), "SpecialEventTimeStartM");
            SpecialEventTimeStart = string.Format("{0:00}:{1:00}", SpecialEventTimeStartH, SpecialEventTimeStartM);
        }

        private void slider_SpecialEventCountdownDauer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SpecialEventCountdownDauer = Convert.ToInt16(slider_SpecialEventCountdownDauer.Value);
            time_Countdown_sec = Convert.ToInt16(slider_SpecialEventCountdownDauer.Value) * 60;
            if (timerSpecialEventCountdown != null)
            {
                timerSpecialEventCountdown.Interval = new TimeSpan(0, 0, SpecialEventCountdownDauer);
                Save_Settings(Convert.ToString(SpecialEventCountdownDauer), "SpecialEventCountdown");
            }
        }

        //**************************************************************************************************************
        //-----------------------Erstes laden Slider A----------------------------------------------------------------------
        //************************************************************************************************************
        private void LoadImageFolderA(string folderA)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (!System.IO.Path.IsPathRooted(folderA))
                folderA = System.IO.Path.Combine(Environment.CurrentDirectory, folderA);
            if (!System.IO.Directory.Exists(folderA))
            {
                ErrorText.Text = "The specified folder does not exist: " + Environment.NewLine + folderA;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            Random r = new Random();
            var sources = from file in new System.IO.DirectoryInfo(folderA).GetFiles().AsParallel()
                          where ValidImageExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase)
                          orderby r.Next()
                          select CreateImageSourceA(file.FullName, true);
            ImagesA.Clear();
            ImagesA.AddRange(sources);
            sw.Stop();
            Console.WriteLine("Total time to load {0} images (A): {1}ms", ImagesA.Count, sw.ElapsedMilliseconds);
        }

        //**************************************************************************************************************
        //-----------------------Erstes laden Slider B----------------------------------------------------------------------
        //************************************************************************************************************
        private void LoadImageFolderB(string folderB)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (!System.IO.Path.IsPathRooted(folderB))
                folderB = System.IO.Path.Combine(Environment.CurrentDirectory, folderB);
            if (!System.IO.Directory.Exists(folderB))
            {
                ErrorText.Text = "The specified folder does not exist: " + Environment.NewLine + folderB;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            Random rb = new Random();
            var sourcesb = from file in new System.IO.DirectoryInfo(folderB).GetFiles().AsParallel()
                          where ValidImageExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase)
                          orderby rb.Next()
                          select CreateImageSourceB(file.FullName, true);
            ImagesB.Clear();
            ImagesB.AddRange(sourcesb);
            sw.Stop();
            Console.WriteLine("Total time to load {0} images (B): {1}ms", ImagesB.Count, sw.ElapsedMilliseconds);
        }

        //**************************************************************************************************************
        //-----------------------Slider A----------------------------------------------------------------------
        //************************************************************************************************************
        private ImageSource CreateImageSourceA(string file, bool forcePreLoad)
        {
            if (forcePreLoad)
            {
                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(file, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                src.Freeze();
                return src;
            }
            else
            {
                var src = new BitmapImage(new Uri(file, UriKind.Absolute));
                src.Freeze();
                return src;
            }
        }


        private void timerImageChangeA_Tick(object sender, EventArgs e)
        {
            PlaySlideShowA();
        }


        private void PlaySlideShowA()
        {
            try
            {
                if (ImagesA.Count == 0)
                    return;
                var oldCtrlIndex = CurrentCtrlIndexA;
                CurrentCtrlIndexA = (CurrentCtrlIndexA + 1) % 2;
                CurrentSourceIndexA = (CurrentSourceIndexA + 1) % ImagesA.Count;

                Image imgFadeOut = ImageControlsA[oldCtrlIndex];
                Image imgFadeIn = ImageControlsA[CurrentCtrlIndexA];
                ImageSource newSource = ImagesA[CurrentSourceIndexA];
                imgFadeIn.Source = newSource;

                TransitionTypeA = TransitionEffects[EffectIndexA].ToString();

                Storyboard StboardFadeOut = (Resources[string.Format("{0}Out", TransitionTypeA.ToString())] as Storyboard).Clone();
                StboardFadeOut.Begin(imgFadeOut);
                Storyboard StboardFadeIn = Resources[string.Format("{0}In", TransitionTypeA.ToString())] as Storyboard;
                StboardFadeIn.Begin(imgFadeIn);
            }

#pragma warning disable CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
            catch (Exception ex) { }
#pragma warning restore CS0168 // Variable ist deklariert, wird jedoch niemals verwendet

        }

        //**************************************************************************************************************
        //-----------------------Slider B----------------------------------------------------------------------
        //************************************************************************************************************
        private ImageSource CreateImageSourceB(string fileB, bool forcePreLoad)
        {
            if (forcePreLoad)
            {
                var srcB = new BitmapImage();
                srcB.BeginInit();
                srcB.UriSource = new Uri(fileB, UriKind.Absolute);
                srcB.CacheOption = BitmapCacheOption.OnLoad;
                srcB.EndInit();
                srcB.Freeze();
                return srcB;
            }
            else
            {
                var srcB = new BitmapImage(new Uri(fileB, UriKind.Absolute));
                srcB.Freeze();
                return srcB;
            }
        }


        private void timerImageChangeB_Tick(object sender, EventArgs e)
        {
            PlaySlideShowB();                       
        }


        private void PlaySlideShowB()
        {
            try
            {
                if (ImagesB.Count == 0)
                    return;
                var oldCtrlIndexB = CurrentCtrlIndexB;
                CurrentCtrlIndexB = (CurrentCtrlIndexB + 1) % 2;
                CurrentSourceIndexB = (CurrentSourceIndexB + 1) % ImagesB.Count;

                Image imgFadeOutB = ImageControlsB[oldCtrlIndexB];
                Image imgFadeInB = ImageControlsB[CurrentCtrlIndexB];
                ImageSource newSource = ImagesB[CurrentSourceIndexB];
                imgFadeInB.Source = newSource;

                TransitionTypeB = TransitionEffects[EffectIndexB].ToString();

                Storyboard StboardFadeOut = (Resources[string.Format("{0}Out", TransitionTypeB.ToString())] as Storyboard).Clone();
                StboardFadeOut.Begin(imgFadeOutB);
                Storyboard StboardFadeIn = Resources[string.Format("{0}In", TransitionTypeB.ToString())] as Storyboard;
                StboardFadeIn.Begin(imgFadeInB);
            }

#pragma warning disable CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
            catch (Exception ex) { }
#pragma warning restore CS0168 // Variable ist deklariert, wird jedoch niemals verwendet

        }
    }
}
