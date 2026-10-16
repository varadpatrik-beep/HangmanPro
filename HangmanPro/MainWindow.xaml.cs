using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Input;

namespace HangmanPro
{
    public class WordItem
    {
        public string Word { get; set; }
        public string Hint { get; set; }
        public WordItem(string word, string hint) { Word = word; Hint = hint; }
    }

    public enum GameDifficulty
    {
        Könnyű,
        Normál,
        Hardcore
    }

    public partial class MainWindow : Window
    {
        private Dictionary<string, List<WordItem>> wordCategories;
        private string activeCategory;
        private List<WordItem> unusedWords;

        private string currentWord;
        private List<char> guessedLetters;
        private int mistakes;

        private int score = 0;
        private int successfulWords = 0;
        private int currentSessionId = 0;

        private string playerName;
        private UIElement[] hangmanParts;

        private DispatcherTimer timer;
        private int timeLeft;

        private GameDifficulty currentDifficulty;
        private int maxTime;
        private int timePenalty;
        private readonly int selectedDuration;
        private bool gameRunning;

        // BUG-02 FIX: Osztályszintű Random a szóismétlődés ellen
        private Random rnd = new Random();

        public MainWindow(string name, int difficultyIndex, int duration, string category)
        {
            InitializeComponent();
            playerName = name;
            selectedDuration = duration;
            activeCategory = category;

            currentDifficulty = (GameDifficulty)difficultyIndex;
            SetDifficultyParameters();

            PlayerInfoText.Text = $"Játékos: {playerName} ({currentDifficulty})";
            ActiveCategoryText.Text = activeCategory;

            hangmanParts = new UIElement[] { Part1_Head, Part2_Body, Part3_LeftArm, Part4_RightArm, Part5_LeftLeg, Part6_RightLeg };

            InitializeCategories();
            SetupTimer();

            StartNewGame();
        }

        private void SetDifficultyParameters()
        {
            maxTime = selectedDuration;
            timePenalty = currentDifficulty switch
            {
                GameDifficulty.Könnyű => 2,
                GameDifficulty.Normál => 5,
                _ => 10
            };
            TimeProgress.Maximum = Math.Max(1, maxTime);
        }

        private void SetupTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!gameRunning || maxTime == 0) return;
            timeLeft--;
            TimeProgress.Value = Math.Max(0, timeLeft);
            UpdateTimeText();

            if (timeLeft <= maxTime * 0.25) TimeProgress.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F04747"));
            else TimeProgress.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#69E3CC"));

            if (timeLeft <= 0)
            {
                timer.Stop();
                mistakes = hangmanParts.Length;
                MistakesText.Text = $"{mistakes} / {hangmanParts.Length}";
                UpdateHangman(forceShowAll: true);
                CheckLoss(timeout: true);
            }
        }

        private void UpdateTimeText()
        {
            TimeText.Text = maxTime == 0 ? "∞" : TimeSpan.FromSeconds(Math.Max(0, timeLeft)).ToString(@"mm\:ss");
        }

        private void InitializeCategories()
        {
            wordCategories = new Dictionary<string, List<WordItem>>
            {
                { "Általános IT", new List<WordItem> {
                    new WordItem("EGÉR", "Ezzel mozgatod a kurzort"),
                    new WordItem("MONITOR", "A számítógép képernyője"),
                    new WordItem("BILLENTYŰZET", "Ezen gépelünk"),
                    new WordItem("NYOMTATÓ", "Papírra viszi a dokumentumot"),
                    new WordItem("PROGRAM", "Utasítások sorozata a számítógép számára"),
                    new WordItem("JELSZÓ", "Belépéskor kérhetik"),
                    new WordItem("BÖNGÉSZŐ", "Weboldalakat nyitsz meg vele"),
                    new WordItem("FÁJL", "Elmentett dokumentum vagy adat"),
                    new WordItem("MAPPA", "Fájlok rendszerezésére szolgál"),
                    new WordItem("KÉPERNYŐ", "Ezen látod a programokat"),
                    new WordItem("LAPTOP", "Hordozható számítógép"),
                    new WordItem("ADATBÁZIS", "Rendezett adatok gyűjteménye"),
                    new WordItem("INTERNET", "Világszintű számítógépes hálózat"),
                    new WordItem("LETÖLTÉS", "Fájl átvitele a gépedre"),
                    new WordItem("FELTÖLTÉS", "Fájl elküldése egy szerverre"),
                    new WordItem("ALKALMAZÁS", "Telefonon vagy számítógépen futó program"),
                    new WordItem("FRISSÍTÉS", "Új verzió telepítése"),
                    new WordItem("MAPPANÉV", "Egy könyvtár elnevezése"),
                    new WordItem("KÓDSOR", "Programozás során írt egyetlen sor"),
                    new WordItem("HÁLÓZAT", "Összekapcsolt eszközök rendszere"),
                    new WordItem("SZERVER", "Szolgáltatásokat nyújtó számítógép"),
                    new WordItem("VÍRUSIRTÓ", "Kártékony programok ellen véd"),
                    new WordItem("PROCESSZOR", "A számítógép egyik központi egysége"),
                    new WordItem("MEMÓRIA", "Gyors, ideiglenes adattároló"),
                    new WordItem("WEBOLDAL", "Böngészőben megjelenő internetes oldal")
                }},
                // BUG-05 FIX: Nehézség névütközés miatt átnevezve "Profi IT & Hálózat"-ra
                { "Profi IT & Hálózat", new List<WordItem> {
                    new WordItem("ROUTER", "Hálózati forgalmat irányító eszköz"),
                    new WordItem("TŰZFAL", "Szűri a hálózati forgalmat"),
                    new WordItem("PROTOKOLL", "Kommunikációs szabályrendszer"),
                    new WordItem("ALHÁLÓZAT", "Egy hálózat kisebb logikai része"),
                    new WordItem("TITKOSÍTÁS", "Adatok védelme kódolással"),
                    new WordItem("JOGOSULTSÁG", "Meghatározza, mit érhetsz el"),
                    new WordItem("VIRTUÁLIS GÉP", "Szoftveresen létrehozott számítógép"),
                    new WordItem("KONFIGURÁCIÓ", "Rendszer beállításainak összessége"),
                    new WordItem("ADATKÖZPONT", "Szerverek elhelyezésére szolgáló létesítmény"),
                    new WordItem("FORRÁSKÓD", "Program ember által olvasható leírása"),
                    new WordItem("KERETRENDSZER", "Szoftverfejlesztést segítő alap"),
                    new WordItem("PARANCS SOR", "Szöveges felület utasítások kiadásához"),
                    new WordItem("HIBAKERESÉS", "Programhibák felkutatása"),
                    new WordItem("VERZIÓKEZELÉS", "Kódbeli változások követése"),
                    new WordItem("ADATCSOMAG", "Hálózaton továbbított adategység"),
                    new WordItem("IP CÍM", "Eszköz hálózati azonosítója"),
                    new WordItem("DOMAIN", "Webes név, például egy honlap címe"),
                    new WordItem("TERHELÉSELOSZTÁS", "Kérések szétosztása szerverek között"),
                    new WordItem("KÉTFAKTOROS", "Bejelentkezés két külön ellenőrzéssel"),
                    new WordItem("MENTÉS", "Adatok biztonsági másolata"),
                    new WordItem("TÁROLÓ", "Adatok megőrzésére használt rendszer"),
                    new WordItem("VÉGPONT", "Hálózat egyik csatlakozó eszköze"),
                    new WordItem("NAPLÓZÁS", "Események folyamatos rögzítése"),
                    new WordItem("LEKÉRDEZÉS", "Adatok kiválasztása feltételek szerint"),
                    new WordItem("KÓDOLÁS", "Információ meghatározott formára alakítása")
                }},
                { "Autószerelés & Alkatrészek", new List<WordItem> {
                    new WordItem("MOTOR", "Az autó hajtását biztosító egység"),
                    new WordItem("FÉK", "Lassítja és megállítja a járművet"),
                    new WordItem("KERÉK", "Ezen gördül az autó"),
                    new WordItem("OLAJ", "A motor kenéséhez szükséges"),
                    new WordItem("SZŰRŐ", "Felfogja a szennyeződéseket"),
                    new WordItem("AKKUMULÁTOR", "Indításhoz és áramellátáshoz fontos"),
                    new WordItem("GYERTYA", "Benzinmotornál szikrát ad"),
                    new WordItem("KIPUFOGÓ", "Elvezeti az égésterméket"),
                    new WordItem("VÁLTÓ", "Sebességfokozatot választasz vele"),
                    new WordItem("KUPLUNG", "Összeköti és szétválasztja a hajtást"),
                    new WordItem("HŰTŐ", "Segít szabályozni a motor hőmérsékletét"),
                    new WordItem("GENERÁTOR", "Menet közben áramot termel"),
                    new WordItem("ÖNINDÍTÓ", "Beindítja a motort"),
                    new WordItem("LENGÉSCSILLAPÍTÓ", "Csökkenti a futómű kilengéseit"),
                    new WordItem("LÉGSZŰRŐ", "A motorba jutó levegőt tisztítja"),
                    new WordItem("OLAJSZŰRŐ", "A kenőanyag szennyeződéseit fogja fel"),
                    new WordItem("FÉKBETÉT", "A féktárcsához szorul lassításkor"),
                    new WordItem("FÉKTÁRCSA", "A fékrendszer forgó eleme"),
                    new WordItem("VEZÉRMŰSZÍJ", "A motor mozgó részeit szinkronizálja"),
                    new WordItem("HENGERFEJ", "A motor hengerei fölötti elem"),
                    new WordItem("FUTÓMŰ", "A kerekek felfüggesztése"),
                    new WordItem("GUMIABRONCS", "A keréken lévő rugalmas külső"),
                    new WordItem("ÜZEMANYAG", "Ezzel működik a belső égésű motor"),
                    new WordItem("KORMÁNY", "Ezzel irányítod az autót"),
                    new WordItem("HŰTŐFOLYADÉK", "Megakadályozza a motor túlmelegedését")
                }},
                { "Életmód & Egyéb", new List<WordItem> {
                    new WordItem("KÁVÉ", "Sokan ezzel indítják a reggelt"),
                    new WordItem("REGGELI", "A nap első étkezése"),
                    new WordItem("VACSORA", "Esti étkezés"),
                    new WordItem("KÖNYV", "Olvasnivaló lapokból vagy képernyőn"),
                    new WordItem("ZENE", "Dallam és ritmus együtt"),
                    new WordItem("FILM", "Moziban vagy otthon nézheted"),
                    new WordItem("SZÍNHÁZ", "Élő előadások helyszíne"),
                    new WordItem("KIRÁNDULÁS", "Természetben tett hosszabb séta"),
                    new WordItem("UTAZÁS", "Elindulás egy másik helyre"),
                    new WordItem("BŐRÖND", "Utazáskor ebbe pakolsz"),
                    new WordItem("NAPSZEMÜVEG", "Védi a szemed az erős fénytől"),
                    new WordItem("ESERNYŐ", "Eső ellen tartod a fejed fölé"),
                    new WordItem("SPORTCIPŐ", "Edzéshez is viselhető lábbeli"),
                    new WordItem("KERÉKPÁR", "Kétkerekű, pedállal hajtott jármű"),
                    new WordItem("FUTÁS", "Gyorsabb mozgás, mint a séta"),
                    new WordItem("ÚSZÁS", "Mozgás a vízben"),
                    new WordItem("FŐZÉS", "Étel elkészítése hő segítségével"),
                    new WordItem("SÜTEMÉNY", "Édes, sütőben készülő finomság"),
                    new WordItem("GYÜMÖLCS", "Például alma vagy körte"),
                    new WordItem("ZÖLDSÉG", "Például répa vagy paprika"),
                    new WordItem("TÁRSASJÁTÉK", "Többen játszhatjátok az asztalnál"),
                    new WordItem("FÉNYKÉP", "Egy pillanat megörökítése"),
                    new WordItem("AJÁNDÉK", "Ünnepre is adhatod valakinek"),
                    new WordItem("SZÜLETÉSNAP", "Évente egyszer ünnepeljük"),
                    new WordItem("BARÁTSÁG", "Két ember közti közeli kapcsolat")
                }}
            };
        }

        private void StartNewGame()
        {
            currentSessionId++;

            score = 0;
            successfulWords = 0;
            UpdateScore();

            timer.Stop();
            timeLeft = maxTime;
            TimeProgress.Value = maxTime == 0 ? 1 : maxTime;
            TimeProgress.Visibility = maxTime == 0 ? Visibility.Hidden : Visibility.Visible;
            TimeProgress.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#69E3CC"));
            UpdateTimeText();

            if (maxTime != 0) timer.Start();

            // BUG-03 FIX: Csak akkor töltjük újra az unusedWords listát, ha null. Így nem keveri vissza egyből a már kitalált szavakat.
            if (unusedWords == null)
            {
                unusedWords = new List<WordItem>(wordCategories[activeCategory]);
            }

            LoadNextWord();
        }

        private void LoadNextWord()
        {
            if (unusedWords == null || unusedWords.Count == 0)
            {
                unusedWords = new List<WordItem>(wordCategories[activeCategory]);
            }

            // BUG-02 FIX: Itt már az osztályszintű rnd-t használjuk
            int index = rnd.Next(unusedWords.Count);
            WordItem selectedItem = unusedWords[index];

            unusedWords.RemoveAt(index);
            currentWord = selectedItem.Word;

            if (currentDifficulty == GameDifficulty.Hardcore)
            {
                HintText.Text = "🔒 Hardcore mód: A tippek ki vannak kapcsolva!";
                HintText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F04747"));
            }
            else
            {
                HintText.Text = $"💡 Tipp: {selectedItem.Hint}";
                HintText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5CA84"));
            }

            guessedLetters = new List<char>();
            mistakes = 0;
            gameRunning = true;

            MessageText.Text = "Válassz egy betűt a kezdéshez!";
            MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#69E3CC"));

            MistakesText.Text = "0 / 6";
            WordInfoText.Text = $"{currentWord.Replace(" ", "").Length} betű  •  Kitalált szavak eddig: {successfulWords}";

            UpdateWordDisplay();
            HideHangman();
            GenerateKeyboard();
        }

        private void GenerateKeyboard()
        {
            KeyboardPanel.Children.Clear();
            string hungarianAlphabet = "AÁBCDEÉFGHIÍJKLMNOÓÖŐPQRSTUÚÜŰVWXYZ";
            Style btnStyle = (Style)FindResource("KeyboardBtnStyle");

            foreach (char c in hungarianAlphabet)
            {
                Button btn = new Button { Content = c.ToString(), Style = btnStyle };
                btn.Click += Letter_Click;
                KeyboardPanel.Children.Add(btn);
            }
        }

        private void Letter_Click(object sender, RoutedEventArgs e)
        {
            // BUG-04 FIX: Sorrendi hiba javítása. Az ellenőrzés legelőször fut le, megakadályozva a felesleges kivételeket.
            if (!gameRunning) return;

            Button btn = (Button)sender;
            char letter = char.Parse(btn.Content.ToString());

            btn.IsEnabled = false;
            guessedLetters.Add(letter);

            if (currentWord.Contains(letter.ToString()))
            {
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43B581"));

                score += 10 + ((int)currentDifficulty * 5);
                UpdateScore();
                UpdateWordDisplay();
                CheckWin();
            }
            else
            {
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F04747"));
                mistakes++;
                MistakesText.Text = $"{mistakes} / {hangmanParts.Length}";

                if (maxTime != 0) timeLeft = Math.Max(0, timeLeft - timePenalty);
                TimeProgress.Value = timeLeft;
                UpdateTimeText();
                UpdateHangman();
                CheckLoss();
            }
        }

        private void UpdateScore()
        {
            ScoreText.Text = score.ToString();
        }

        private void UpdateWordDisplay()
        {
            WordDisplay.Inlines.Clear();
            string[] digraphs = { "CS", "DZ", "GY", "LY", "NY", "SZ", "TY", "ZS" };

            for (int i = 0; i < currentWord.Length; i++)
            {
                char c = currentWord[i];

                if (c == ' ')
                {
                    WordDisplay.Inlines.Add(new Run("   "));
                    continue;
                }

                bool isDigraphStart = (i < currentWord.Length - 1) && Array.Exists(digraphs, d => d == currentWord.Substring(i, 2));

                Border letterBorder = new Border
                {
                    BorderThickness = new Thickness(0, 0, 0, 3),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(2, 0, 2, 4),
                    Margin = new Thickness(0, 0, 8, 0)
                };

                TextBlock letterText = new TextBlock
                {
                    FontSize = 32,
                    FontWeight = FontWeights.Bold
                };

                if (isDigraphStart)
                {
                    char c1 = currentWord[i];
                    char c2 = currentWord[i + 1];
                    string t1 = guessedLetters.Contains(c1) ? c1.ToString() : "_";
                    string t2 = guessedLetters.Contains(c2) ? c2.ToString() : "_";

                    letterBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#69E3CC"));

                    letterText.Text = $"{t1} {t2}";
                    letterText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#91F1DF"));

                    i++;
                }
                else
                {
                    string t = guessedLetters.Contains(c) ? c.ToString() : "_";
                    letterBorder.BorderBrush = Brushes.Transparent;

                    letterText.Text = t;
                    letterText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFF"));
                }

                letterBorder.Child = letterText;
                WordDisplay.Inlines.Add(new InlineUIContainer(letterBorder) { BaselineAlignment = BaselineAlignment.Center });
            }
        }

        private void HideHangman()
        {
            foreach (var part in hangmanParts)
            {
                part.Visibility = Visibility.Hidden;
                part.Opacity = 0;
            }
        }

        private void UpdateHangman(bool forceShowAll = false)
        {
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4));

            if (forceShowAll)
            {
                foreach (var part in hangmanParts)
                {
                    part.Visibility = Visibility.Visible;
                    part.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }
            }
            else if (mistakes > 0 && mistakes <= hangmanParts.Length)
            {
                UIElement currentPart = hangmanParts[mistakes - 1];
                currentPart.Visibility = Visibility.Visible;
                currentPart.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

        private async void CheckWin()
        {
            bool isWon = true;
            foreach (char c in currentWord)
            {
                if (c != ' ' && !guessedLetters.Contains(c))
                {
                    isWon = false;
                    break;
                }
            }

            if (isWon)
            {
                gameRunning = false;
                successfulWords++;

                score += 50 + ((int)currentDifficulty * 20);
                UpdateScore();

                MessageText.Text = "🎉 Helyes! Jön a következő...";
                MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43B581"));
                DisableKeyboard();

                int capturedSession = currentSessionId;

                await Task.Delay(1500);

                if (currentSessionId == capturedSession && (maxTime == 0 || timeLeft > 0))
                {
                    LoadNextWord();
                }
            }
        }

        private void CheckLoss(bool timeout = false)
        {
            if (mistakes >= hangmanParts.Length || (maxTime != 0 && timeLeft <= 0))
            {
                timer.Stop();
                gameRunning = false;

                string reason = timeout || (maxTime != 0 && timeLeft <= 0) ? "⏳ Lejárt az idő!" : "💀 Vesztettél!";
                MessageText.Text = $"{reason}\nA szó: {currentWord}\nJáték vége! Kitalált szavak: {successfulWords}  •  Összpontszám: {score}";

                MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F04747"));
                DisableKeyboard();
            }
        }

        private void DisableKeyboard()
        {
            foreach (Button btn in KeyboardPanel.Children)
            {
                btn.IsEnabled = false;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            gameRunning = false;
            currentSessionId++;

            LoginWindow login = new LoginWindow();
            login.Show();
            Close();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text)) return;

            char inputChar = e.Text.ToUpper(new System.Globalization.CultureInfo("hu-HU"))[0];

            foreach (Button btn in KeyboardPanel.Children)
            {
                if (btn.Content.ToString() == inputChar.ToString() && btn.IsEnabled)
                {
                    btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                }
            }
        }
    }
}