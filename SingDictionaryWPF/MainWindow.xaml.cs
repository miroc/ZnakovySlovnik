using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Reactive.Linq;
using System.Reactive;


namespace SingDictionaryWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// TODO: REFACTORING, move all functionality into MVVM class
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, SingDictionaryWPF.AboutWindow.OnWindowClosing
    {

        #region fields

        private const string SWITCH_VIDEO_TO_SIDE = "Pohľad z boku";
        private const string SWITCH_VIDEO_TO_FRONT = "Pohľad spredu";

        private Dictionary d;
        private ObservableCollection<Word> currentWordsSel = new ObservableCollection<Word>();
        private ObservableCollection<Word> filteredSelectedWords = new ObservableCollection<Word>();
        private Word selectedWord;
        private Video selectedVideo;
        private String selectedCategory;
        private String selectedSubcategory;
        private BrushConverter brushConverter = new BrushConverter();

        private string[] categories= { "--vyber--", "Slovenský jazyk", "Literatúra", "Štylistika" };
        public string[] Categories { get { return categories; } set { categories = value; } }

        private string[] subcategories = { "--vyber--", "Lexika", "Syntax", "Fonetika - Fonológia", "Morfológia" };
        public string[] Subcategories { get { return subcategories; } set { subcategories = value; } }
        

        public String SelectedCategory { get { return selectedCategory; } set { selectedCategory = value; } }
        public String SelectedSubcategory { get { return selectedSubcategory; } set { selectedSubcategory = value; } }

        public ObservableCollection<Word> CurrentWordsSel { get { return currentWordsSel; } set { currentWordsSel = value; } }
        public ObservableCollection<Word> FilteredSelectedWords { get { return filteredSelectedWords; } set { filteredSelectedWords = value; } }

        public Word SelectedWord
        {
            get { return selectedWord; }
            set {
                if (value != this.selectedWord)
                    {selectedWord = value;NotifyPropertyChanged("SelectedWord"); }                
            }
        }

        public Video SelectedVideo
        {
            get { return selectedVideo; }
            set
            {
                
                if (value != this.selectedVideo)
                { selectedVideo = value; NotifyPropertyChanged("SelectedVideo"); }
            }
        }

        private List<ToggleButton> alphabetButtons = new List<ToggleButton>();

        //RX framework for autocomplete
        IDisposable keyPressSubscription = null;
        
        ICommand _changeRegionCommand;
        public ICommand ChangeRegionCommand { get { return _changeRegionCommand; } }

        

        #endregion

        #region windowInitialization
        public MainWindow()
        {   
            
            InitializeComponent();
            
            //load dictionary in a different thread
            BackgroundWorker worker = new BackgroundWorker();            
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {                
                d = Dictionary.Instance;                    
            };
            worker.RunWorkerCompleted += (s, e) => {
                if (e.Error != null)
                {
                    MessageBox.Show("Slovník sa nedá načítať. Aplikácia sa ukončí" + Environment.NewLine + Environment.NewLine + "Chyba: " + e.Error.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown(1);
                }
                else
                {
                    this.comboBoxCategory.SelectedIndex = 0;
                    this.comboBoxSubcategory.SelectedIndex = 0;
                    loadWordsUsingLambda(x => true);//load all
                }
            };
            worker.RunWorkerAsync();


            //kedze nevyuzivame MVVM architekturu, nabindujeme si data Context na hlavne okno
            //abu sme potom mohli vyuzivat jeho properties
            this.DataContext = this;
            
            AddControls();
            LoadWMP();

            
        }

        /**
         * add alphabet buttons
         * */
        private void AddControls()
        {           
            //init Icommands
            _changeRegionCommand=  new SimpleDelegateCommand((x) => ChangeRegion(x));

            
            //init Buttons
            for (char c = 'A'; c <= 'Z'; c++)
            {
                //alphabet.Add(c.ToString());
                ToggleButton b = new ToggleButton();
                
                b.Content = c.ToString();
                b.Name = "button" + c;
                b.Width = 20.0;
                b.Margin = new Thickness(1.0);

                b.Click += new RoutedEventHandler(buttonAlphabetClick);
                this.AlphabetWrap.Children.Add(b);
                alphabetButtons.Add(b);
            }            

            //init RX search
            // Create a stream of keyPressEvents
            var textChangedEvents = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                (TextChangedEventHandler ev) => { textBoxSearch.TextChanged += ev; },
                (TextChangedEventHandler ev) => { textBoxSearch.TextChanged -= ev; });

            // Throttle the events so that we don't get more than one
            // event every 1500 milliseconds. This will hold back the event
            // until there is a momentary pause in key presses.
            keyPressSubscription = textChangedEvents
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOnDispatcher()
                .Subscribe(x=>PerformSearch()); //TODO skarede!...
        }              

        private void LoadWMP()
        {
            mediaPlayer.enableContextMenu = false;
            
            mediaPlayer.settings.volume = 0;
            mediaPlayer.settings.mute = true;
            mediaPlayer.uiMode = "mini";
            //mediaPlayer.
        }
        
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {            
           // this.comboBoxCategory.SelectedIndex = 0;
            //this.comboBoxSubcategory.SelectedIndex = 0;
            //Console.WriteLine("s");
        }
        #endregion

        #region changeSelectedVideo

        //changes region video, called by ChangeRegionCommand
        private object ChangeRegion(object x)
        {
            SelectedVideo = SelectedWord.VideoDict[x as string]["front"];
            buttonSwitchView.Content = SWITCH_VIDEO_TO_SIDE;
            mediaPlayer.URL = SelectedVideo.FullPath;

            //MessageBox.Show(x.ToString());
            return null;
        }

        //vyber slova z wordListu
        private void wordsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Word w = this.wordsListBox.SelectedItem as Word;
            if (w!=null){
                try
                {
                    SelectedWord = w;
                    SelectedVideo = w.VideoDict["BA"]["front"];
                    mediaPlayer.URL = SelectedVideo.FullPath;
                    buttonSwitchView.Content = SWITCH_VIDEO_TO_SIDE;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Video k slovu '"+ w +"' sa nedá načítať.", "Chyba",MessageBoxButton.OK,MessageBoxImage.Error);
                }
                
            }            
        }

        //Prepinanie pohľadov
        private void button1_Click(object sender, RoutedEventArgs e)
        {    
            if (selectedVideo!= null)
            {
                try
                {
                    if (selectedVideo.orientation == "front")
                    {
                        if (selectedWord.VideoDict[selectedVideo.version]["side"] != null)
                        {
                            SelectedVideo = selectedWord.VideoDict[selectedVideo.version]["side"];
                            //mediaPlayer.Source = SelectedVideo.Uri;
                            mediaPlayer.URL = SelectedVideo.FullPath;

                            buttonSwitchView.Content = SWITCH_VIDEO_TO_FRONT;
                        }
                    }
                    else
                    {
                        if (selectedWord.VideoDict[selectedVideo.version]["front"] != null)
                        {
                            SelectedVideo = selectedWord.VideoDict[selectedVideo.version]["front"];
                            //mediaPlayer.Source = SelectedVideo.Uri;
                            mediaPlayer.URL = SelectedVideo.FullPath;
                            buttonSwitchView.Content = SWITCH_VIDEO_TO_SIDE;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Video k slovu '" + selectedWord.Name + "' sa nedá načítať.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }                
            }            
        }

        #endregion

        #region changeSelectedWords

        private void comboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //aktivacia/deaktivacia podkategorie
            if (String.IsNullOrEmpty(selectedCategory)) return;
            //no category
            if (selectedCategory == categories[0]){ 
                labelSubcategory.IsEnabled = comboBoxSubcategory.IsEnabled =  false;
                comboBoxSubcategory.SelectedIndex = 0;
                loadWordsUsingLambda(x => true);
                return;
            }            

            //vybrana konkretna kategoria
            comboBoxSubcategory.IsEnabled = labelSubcategory.IsEnabled = selectedCategory == categories[1] ? true : false;
            alphabetButtons.ForEach(x => x.IsChecked = false);
            loadWordsUsingLambda(x => x.Category == SelectedCategory);               
        }

        private void comboBoxSubcategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(selectedSubcategory) && selectedSubcategory != subcategories[0])
            {
                loadWordsUsingLambda(x => x.Subcategory == selectedSubcategory);
            }
            else if (!String.IsNullOrEmpty(selectedCategory) && selectedCategory != categories[0] )
            {
                loadWordsUsingLambda(x => x.Category == SelectedCategory);
            }         
        }

        private void loadWordsUsingLambda(Func<Word, bool> lambda)
        {
            //if search was performed cancel it
            CancelSearch();
            addListToWordsSelection(CurrentWordsSel, d.Words.Where(lambda).OrderBy(x => x.Name).ToList<Word>());            
        }

        private void loadWordsStartingWith(String letter)
        {           
            loadWordsUsingLambda(x => x.FirstLetter.Equals(letter, StringComparison.CurrentCultureIgnoreCase));
            this.comboBoxCategory.SelectedIndex = this.comboBoxSubcategory.SelectedIndex = 0;
        }


        //Vyber slov podla abecedy
        protected void buttonAlphabetClick(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton) sender;
            //The ?? operator returns the left-hand operand if it is not null, or else it returns the right operand.
            if (button.IsChecked ?? false)
            {
                alphabetButtons.ForEach(x => x.IsChecked = button == x);
                string content = button.Content.ToString();
                loadWordsStartingWith(content);
            }
            else
            {
                loadWordsUsingLambda(x => true);   
            }
        }
                
        //fires INotifyPropertyChange
        private void addListToWordsSelection(ObservableCollection<Word> oCollection,List<Word> list)
        {
            oCollection.Clear();
            foreach (Word wrd in list)
            {
                oCollection.Add(wrd);
            }
        }

        #endregion

        #region notifyPropertyChange
        //INotifyPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {            
            if (PropertyChanged != null)
            {                
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region search

        private System.Windows.Data.Binding bindToSelection = new System.Windows.Data.Binding("CurrentWordsSel");
        private System.Windows.Data.Binding bindToFilteredSelection = new System.Windows.Data.Binding("FilteredSelectedWords");
                
        //do search
        public void PerformSearch()
        {
            if (textBoxSearch.Text == "hľadaj") { return; }
            else if (textBoxSearch.Text == "")
            {
                CancelSearch(false);
                return;
            }
            var searchedText = textBoxSearch.Text.ToLower();

            addListToWordsSelection(FilteredSelectedWords, currentWordsSel.Where(x => x.Name.Contains(searchedText)).ToList());
            wordsListBox.SetBinding(ListBox.ItemsSourceProperty, bindToFilteredSelection);

        }
        //change binding to original
        public void CancelSearch(bool resetTextBox = true)
        {            
            wordsListBox.SetBinding(ListBox.ItemsSourceProperty, bindToSelection);
            if (resetTextBox)
            {
                textBoxSearch.Foreground = brushConverter.ConvertFromString("#FFB0B0B0") as SolidColorBrush;
                textBoxSearch.Text = "hľadaj";
            }
        }

        private void textBoxSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (textBoxSearch.Text == "hľadaj")
            {
                textBoxSearch.Foreground = brushConverter.ConvertFromString("Black") as SolidColorBrush;
                textBoxSearch.Text = "";
            }
            
            //MessageBox.Show("textBoxSearch_PreviewMouseDown");
        }
       
        private void textBoxSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxSearch.Text == "")
            {
                textBoxSearch.Foreground = brushConverter.ConvertFromString("#FFB0B0B0") as SolidColorBrush;
                textBoxSearch.Text = "hľadaj";
            }
        }


        
        private void textBoxSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && wordsListBox.Items.Count > 0)
            {
                e.Handled = true;
                wordsListBox.SelectedIndex=0;                
                ((ListBoxItem)wordsListBox.ItemContainerGenerator.ContainerFromItem(wordsListBox.SelectedItem)).Focus();
                
            }
        }

        #endregion

        #region About

        private AboutWindow aboutWindow;
        private void buttonAbout_Click(object sender, RoutedEventArgs e)
        {
            if (aboutWindow == null)
            {
                aboutWindow = new AboutWindow(this);
                aboutWindow.Left = this.Left +  this.Width / 2.0 - aboutWindow.Width / 2.0;
                aboutWindow.Top = this.Top + this.Height/ 2.0 - aboutWindow.Height/ 2.0;
                aboutWindow.Show();
            }
            else
            {
                aboutWindow.Activate();
            }
        }
        //callback on closing
        public void onWindowClosing()
        {
            aboutWindow = null;
        }

        #endregion

    }

}
