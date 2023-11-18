using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Monaco.Helpers;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Monaco
{
    public sealed partial class Editor : UserControl
    {
        public delegate void LoadedEventHandler(object sender, object args);

        public new event LoadedEventHandler Loaded;

        public delegate void TextChangedEventHandler(object sender, string args);

        public event TextChangedEventHandler TextChanged;

        public delegate void LinkClickedEventHandler(object sender, Uri args);

        public event LinkClickedEventHandler LinkClicked;


        public Editor()
        {
            this.InitializeComponent();
            string path = "file:///" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Source\\index.html").Replace("\\", "/");
            webView.Source = new Uri(path);

            this.ActualThemeChanged += Editor_ActualThemeChanged;


            webView.CoreWebView2Initialized += (s, a) =>
            {
                var c = s.CoreWebView2.Settings;
                c.AreDevToolsEnabled = false;
                c.IsBuiltInErrorPageEnabled = false;
                c.IsStatusBarEnabled = false;
                c.IsWebMessageEnabled = false;

                s.CoreWebView2.NavigationCompleted += async (s, a) =>
                {
                    await s.ExecuteScriptAsync("editor.getModel().onDidChangeContent((event) => {\r\n  console.log(\"TEXTCHANGED\")\r\n});");
                    RefreshTheme();

                    // Make the initial flash less visible
                    webView.Opacity = 1;
                    Loaded(this, null);
                };

                s.CoreWebView2.WebMessageReceived += async (s, a) =>
                {
                    if (a.WebMessageAsJson == @"""TEXTCHANGED""")
                    {
                        string text = await GetTextAsync();
                        TextChanged(this, text);
                    }
                };

                s.CoreWebView2.NewWindowRequested += (s, a) =>
                {
                    LinkClicked(this, new Uri(a.Uri));
                    a.Handled = true;
                };
            };


            Loaded += (s, a) => { };
            TextChanged += (s, a) => { };

            // Open links in the default browser instead of WebView2
            LinkClicked += async (s, a) => 
            {
                if(LinkClicked.GetInvocationList().Length < 2)
                {
                    await Launcher.LaunchUriAsync(a);
                }
            };
        }

        private void Editor_ActualThemeChanged(FrameworkElement sender, object args)
        {
            RefreshTheme();
        }

        private async void RefreshTheme()
        {
            string themeStr = this.ActualTheme == ElementTheme.Light ? "vs" : "vs-dark";
            await webView.ExecuteScriptAsync($"monaco.editor.setTheme(\"{themeStr}\")");
        }

        public async void SetLanguage(Language language)
        {
            string id = LanguageCoverter.LanguageEnumToString(language);
            await webView.ExecuteScriptAsync("var model = editor.getModel();" +
                                             $"monaco.editor.setModelLanguage(model, \"{id}\")");
        }

        /// <summary>
        /// monaco.editor.getValue()
        /// </summary>
        public async Task<string> GetTextAsync()
        {
            string str = await webView.CoreWebView2.ExecuteScriptAsync("editor.getValue()");
            return str.ToCSharpString();
        }
        /// <summary>
        /// monaco.editor.setValue(str)
        /// </summary>
        public async void SetText(string str)
        {
            await webView.CoreWebView2.ExecuteScriptAsync($"editor.setValue(\"{str}\")");
        }

        string file = string.Empty;
        public async Task OpenFileAsync(string path, bool detectLanguage = true)
        {
            if (File.Exists(path))
            {
                file = path;
                string fileString = File.ReadAllText(path);
                if(detectLanguage)
                {
                    var res = await webView.CoreWebView2.ExecuteScriptAsync($"var model = monaco.editor.createModel(String.raw`{fileString}`, undefined, monaco.Uri.file(String.raw`{path}`));" +
                                                                    "editor.setModel(model)");
                }
                else
                {
                    await webView.CoreWebView2.ExecuteScriptAsync($"editor.setValue({fileString})");
                }
            }
            else throw new FileNotFoundException();
        }

        public async Task<bool> SaveAsync()
        {
            if(file != string.Empty)
            {
                try
                {
                    string text = await GetTextAsync();
                    await File.WriteAllTextAsync(file, text, Encoding.UTF8);
                    return true;
                }
                catch { }
            }

            return false;
        }

        public async Task<StorageFile> SaveAsAsync(object filePickerTarget)
        {
            var filePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(filePickerTarget);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            var file = await filePicker.PickSaveFileAsync();

            if(file != null)
            {
                string text = await GetTextAsync();
                await File.WriteAllTextAsync(file.Path, text);
                return file;
            }

            return null;
        }
    }
}
