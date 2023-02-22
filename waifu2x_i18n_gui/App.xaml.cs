using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace waifu2x_ncnn_vulkan_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    public partial class App : Application
    {
        public static String directory;
        public static App Instance;
        public event EventHandler LangChangedEvent;

        #region Constructor
        public App()
        {
            Instance = this;
            directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            SetLangResDictionary(GetLangFilePath(CultureInfo.CurrentCulture.Name));

        }
        #endregion

        public void SwitchLanguage(string langcode)
        {
            if (CultureInfo.CurrentCulture.Name.Equals(langcode))
            {
                return;
            }
            var ci = new CultureInfo(langcode);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            SetLangResDictionary(GetLangFilePath(langcode));
            if (LangChangedEvent != null)
            {
                LangChangedEvent(this, new EventArgs());
            }
        }

        private string GetLangFilePath(string langCode)
        {
            string xmlname = "UILang." + langCode + ".xaml";
            var lang_fn = Path.Combine(directory, xmlname);
            if (!File.Exists(lang_fn))
            {
                xmlname = "UILang.en-US.xaml";
                lang_fn = Path.Combine(directory, xmlname);
            }
            return lang_fn;
        }

        private void SetLangResDictionary(string inFile)
        {
            if (File.Exists(inFile))
            {
                var langDict = new ResourceDictionary();
                langDict.Source = new Uri(inFile);
                int langDictId = -1;
                for (int i = 0; i < Resources.MergedDictionaries.Count(); i++)
                {
                    var md = Resources.MergedDictionaries[i];
                    if (md.Contains("ResourceDictionaryName"))
                    {
                        if (md["ResourceDictionaryName"].ToString().StartsWith("waifu2xui-"))
                        {
                            langDictId = i;
                            break;
                        }
                    }
                }
                if (langDictId <= -1)
                {
                    Resources.MergedDictionaries.Add(langDict);
                }
                else
                {
                    Resources.MergedDictionaries[langDictId] = langDict;
                }
            }
        }
    }
}
