using System;
using System.Collections.Generic;
using System.ComponentModel; // CancelEventArgs
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace waifu2x_ncnn_vulkan_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            System.Environment.CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var dirInfo = new DirectoryInfo(App.directory);
            var langlist = dirInfo.GetFiles("UILang.*.xaml");
            string[] langcodelist = new string[langlist.Length];
            for (int i = 0; i < langlist.Length; i++)
            {
                var fn_parts = langlist[i].ToString().Split('.');
                langcodelist[i] = fn_parts[1];
            }

            foreach (var langcode in langcodelist)
            {
                MenuItem mi = new MenuItem();
                mi.Tag = langcode;
                mi.Header = langcode;
                mi.Click += new RoutedEventHandler(MenuItem_Style_Click);
                menuLang.Items.Add(mi);
            }
            foreach (MenuItem item in menuLang.Items)
            {
                if (item.Tag.ToString().Equals(CultureInfo.CurrentUICulture.Name))
                {
                    item.IsChecked = true;
                }
            }
            // Load settings from file

            if (Properties.Settings.Default.output_dir != "null")
            { txtDstPath.Text = Properties.Settings.Default.output_dir; }
            txtOutExt.SelectedValue = Properties.Settings.Default.output_format;
            txtOutQuality.Text = Properties.Settings.Default.output_quality.ToString();

            if (System.Text.RegularExpressions.Regex.IsMatch(
                Properties.Settings.Default.gpu_id,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                txtGPU_ID.Text = Properties.Settings.Default.gpu_id;
            }

            txtBlocksize.Text = Properties.Settings.Default.block_size;
            txtScale_ratio.Text = Properties.Settings.Default.scale_ratio;
            txtOutput_width.Text = Properties.Settings.Default.Output_width;
            txtOutput_height.Text = Properties.Settings.Default.Output_heigh;
            txtOutput_width_height.Text = Properties.Settings.Default.Output_width_height;
            txtBlocksize.Text = Properties.Settings.Default.block_size;
            txtThread.Text = Properties.Settings.Default.thread;

            //btnCUDA.IsChecked = true;
            btnDenoise0.IsChecked = true;

            if (Properties.Settings.Default.noise_level == "3")
            { btnDenoise3.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "2")
            { btnDenoise2.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "1")
            { btnDenoise1.IsChecked = true; }
            if (Properties.Settings.Default.noise_level == "0")
            { btnDenoise0.IsChecked = true; }

            btnModeScale.IsChecked = true;

            if (Properties.Settings.Default.mode == "scale")
            { btnModeScale.IsChecked = true; }
            if (Properties.Settings.Default.mode == "noise_scale")
            { btnModeNoiseScale.IsChecked = true; }
            if (Properties.Settings.Default.mode == "noise")
            { btnModeNoise.IsChecked = true; }
            if (Properties.Settings.Default.mode == "auto_scale")
            { btnModeAutoScale.IsChecked = true; }

            txtModel.SelectedValue = Properties.Settings.Default.model;

            if (Properties.Settings.Default.mag_mode == "Scale_ratio_mode")
            { btnScale_ratio.IsChecked = true; }
            if (Properties.Settings.Default.mag_mode == "Width_mode")
            { btnOutput_width.IsChecked = true; }
            if (Properties.Settings.Default.mag_mode == "Height_mode")
            { btnOutput_height.IsChecked = true; }
            if (Properties.Settings.Default.mag_mode == "Width_height_mode")
            { btnOutput_width_height.IsChecked = true; }

            checkTTAmode.IsChecked = Properties.Settings.Default.TTAmode;
            checkSoundBeep.IsChecked = Properties.Settings.Default.SoundBeep;
            checkStore_output_dir.IsChecked = Properties.Settings.Default.store_output_dir;
            checkOutput_no_overwirit.IsChecked = Properties.Settings.Default.output_no_overwirit;
            checkPrecision_fp32.IsChecked = Properties.Settings.Default.Precision_fp32;
            checkAlphachannel_ImageMagick.IsChecked = Properties.Settings.Default.Alphachannel_ImageMagick;
            checkKeep_aspect_ratio.IsChecked = Properties.Settings.Default.Keep_aspect_ratio;
            txtScale_ratio.Text = Properties.Settings.Default.scale_ratio;

        }
        System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();
        // public static StringBuilder param_src= new StringBuilder("");
        public static StringBuilder param_dst = new StringBuilder("");
        public static StringBuilder param_dst_suffix = new StringBuilder("");
        public static StringBuilder param_outformat = new StringBuilder("png");
        public static StringBuilder param_output_quality = new StringBuilder("-quality 100");
        public static StringBuilder param_mag_mode = new StringBuilder("Scale_ratio_mode");
        public static StringBuilder param_denoise = new StringBuilder("");
        public static StringBuilder param_denoise2 = new StringBuilder("");
        public static StringBuilder param_model = new StringBuilder("models-cunet");
        public static StringBuilder param_model2 = new StringBuilder("models-cunet");
        public static StringBuilder param_block = new StringBuilder("100");
        public static StringBuilder param_mode = new StringBuilder("noise_scale");
        public static StringBuilder param_mode2 = new StringBuilder("noise_scale");
        public static StringBuilder param_gpu_id = new StringBuilder("0");
        public static StringBuilder param_thread = new StringBuilder("2");
        public static StringBuilder param_tta = new StringBuilder("");
        public static StringBuilder binary_path = new StringBuilder("");
        public static StringBuilder binary_type = new StringBuilder("");
        public static String[] param_src;
        // public static int scale_ratio;
        public static float scale_ratio_public;
        public static int FileCount = 0;
        public static int output_width_public = 0;
        public static int output_height_public = 0;
        public DateTime starttimea;
        public static bool Cancel = false;
        public static bool Output_no_overwirit;
        public static bool Keep_aspect_ratio = false;
        public static bool Alphachannel_ImageMagick;
        public static bool txtScale_ratio_exponent = false;

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            // Save settings

            // Remember last output path
            if (checkStore_output_dir.IsChecked == true)
            {
                if (txtDstPath.Text.Trim() != "")
                {
                    Properties.Settings.Default.output_dir = txtDstPath.Text;
                }
                else
                {
                    Properties.Settings.Default.output_dir = "null";
                }

            }
            else
            {
                Properties.Settings.Default.output_dir = "null";
            }
            Properties.Settings.Default.output_format = txtOutExt.SelectedValue.ToString();
            Properties.Settings.Default.output_no_overwirit = Convert.ToBoolean(checkOutput_no_overwirit.IsChecked);
            Properties.Settings.Default.model = param_model.ToString().Replace("-m ", "").Replace("-n ", "");
            Properties.Settings.Default.TTAmode = Convert.ToBoolean(checkTTAmode.IsChecked);
            Properties.Settings.Default.SoundBeep = Convert.ToBoolean(checkSoundBeep.IsChecked);
            Properties.Settings.Default.store_output_dir = Convert.ToBoolean(checkStore_output_dir.IsChecked);
            Properties.Settings.Default.Precision_fp32 = Convert.ToBoolean(checkPrecision_fp32.IsChecked);
            Properties.Settings.Default.Alphachannel_ImageMagick = Convert.ToBoolean(checkAlphachannel_ImageMagick.IsChecked);
            Properties.Settings.Default.Keep_aspect_ratio = Convert.ToBoolean(checkKeep_aspect_ratio.IsChecked);
            Properties.Settings.Default.mode = param_mode.ToString();
            Properties.Settings.Default.noise_level = param_denoise.ToString();
            Properties.Settings.Default.mag_mode = param_mag_mode.ToString();

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtScale_ratio.Text,
                @"^\d+(\.\d+)?$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.scale_ratio = txtScale_ratio.Text;
            }
            else
            {
                Properties.Settings.Default.scale_ratio = "2";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtOutput_width.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.Output_width = txtOutput_width.Text;
            }
            else
            {
                Properties.Settings.Default.Output_width = "0";
            }


            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtOutput_height.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.Output_heigh = txtOutput_height.Text;
            }
            else
            {
                Properties.Settings.Default.Output_heigh = "0";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtOutput_width_height.Text,
                @"^\d+x\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.Output_width_height = txtOutput_width_height.Text;
            }
            else
            {
                Properties.Settings.Default.Output_width_height = "1920x1080";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
            txtOutQuality.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.output_quality = int.Parse(txtOutQuality.Text);
            }
            else
            {
                Properties.Settings.Default.output_quality = 100;
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtBlocksize.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.block_size = txtBlocksize.Text;
            }
            else
            {
                Properties.Settings.Default.block_size = "100";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtGPU_ID.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.gpu_id = txtGPU_ID.Text;
            }
            else
            {
                Properties.Settings.Default.gpu_id = "Unspecified";
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtThread.Text,
                @"^\d+$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                Properties.Settings.Default.thread = txtThread.Text;
            }
            else
            {
                Properties.Settings.Default.thread = "2";
            }

            Properties.Settings.Default.Save();

            Cancel = true;
            try
            {
                foreach (var process in Process.GetProcessesByName("waifu2x-ncnn-vulkan"))
                {
                    process.Kill();
                }
            }
            catch { }
        }

        private void OnMenuHelpClick(object sender, RoutedEventArgs e)
        {
            string msg =
                "This is a multilingual graphical user-interface\n" +
                "for the waifu2x-ncnn-vulkan commandline program.\n" +
                "DO NOT rename any subdirectories inside waifu2x-ncnn-vulkan folder\n" +
                "To make a translation, copy one of the bundled xaml file\n" +
                "then edit the copy with a text editor.\n" +
                "Whenever you see a language code like en-US, change it to\n" +
                "the target language code like zh-TW, ja-JP.\n" +
                "The filename needs to be changed too.\n" +
                "Translators: @yoaan, @xperiazu21, @MartBlur,\n" +
                "@martin-varela, @laikal, @flavw";
            MessageBox.Show(msg);
        }

        private void OnMenuVersionClick(object sender, RoutedEventArgs e)
        {
            string msg =
                "Multilingual GUI for waifu2x-ncnn-vulkan\n" +
                "By f11894 and Flavw\n" +
                "Version 2.1.1.2\n" +
                "BuildDate: 2023/02/20\n" +
                "License: MIT License";
            MessageBox.Show(msg);
        }

        private void OnBtnSrc(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Multiselect = true;
            fdlg.Filter = "Graphic Files(*.png;*.jpg;*jpeg;*.gif;*.bmp;*.tif;*.tiff;*.webp)|*.png;*.jpg;*jpeg;*.gif;*.bmp;*.tif;*.tiff;*.webp|All Files(*.*)|*.*";
            if (fdlg.ShowDialog() == true)
            {
                this.txtSrcPath.Text = string.Join("\n", fdlg.FileNames);
                param_src = fdlg.FileNames;
            }
        }

        private void OnSrcClear(object sender, RoutedEventArgs e)
        {
            this.txtSrcPath.Clear();
            param_src = null;
        }

        private void OnBtnDst(object sender, RoutedEventArgs e)
        {
            var dlg = new Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtDstPath.Text = dlg.SelectedPath;
            }
        }

        private void OnDstClear(object sender, RoutedEventArgs e)
        {
            this.txtDstPath.Clear();
        }

        private void MenuItem_Style_Click(object sender, RoutedEventArgs e)
        {
            foreach (MenuItem item in menuLang.Items)
            {
                item.IsChecked = false;
            }
            MenuItem mi = (MenuItem)sender;
            mi.IsChecked = true;
            App.Instance.SwitchLanguage(mi.Tag.ToString());
        }

        private void On_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void On_SrcDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.txtSrcPath.Text = string.Join("\n", fn);
                param_src = (string[])e.Data.GetData(DataFormats.FileDrop);
            }
        }

        private void On_DstDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.txtDstPath.Text = fn[0];
            }
        }

        private void OnSetModeChecked(object sender, RoutedEventArgs e)
        {
            gpDenoise.IsEnabled = true;
            if (btnModeNoise.IsChecked == false)
            {
                gpScale.IsEnabled = true;
                gpScale2.IsEnabled = true;
            }

            param_mode.Clear();
            RadioButton optsrc = sender as RadioButton;
            param_mode.Append(optsrc.Tag.ToString());
            if (btnModeScale.IsChecked == true)
            { gpDenoise.IsEnabled = false; }

            if (btnModeNoise.IsChecked == true)
            {
                gpScale.IsEnabled = false;
                gpScale2.IsEnabled = false;
            }
        }

        private void OnScaleModeChecked(object sender, RoutedEventArgs e)
        {
            param_mag_mode.Clear();
            RadioButton optsrc = sender as RadioButton;
            param_mag_mode.Append(optsrc.Tag.ToString());
            txtOutput_width.IsEnabled = false;
            txtOutput_height.IsEnabled = false;
            txtOutput_width_height.IsEnabled = false;
            txtScale_ratio.IsEnabled = false;
            if (btnScale_ratio.IsChecked == true)
            { txtScale_ratio.IsEnabled = true; }
            if (btnOutput_width.IsChecked == true)
            { txtOutput_width.IsEnabled = true; }
            if (btnOutput_height.IsChecked == true)
            { txtOutput_height.IsEnabled = true; }
            if (btnOutput_width_height.IsChecked == true)
            { txtOutput_width_height.IsEnabled = true; }
        }
        private void OnDenoiseChecked(object sender, RoutedEventArgs e)
        {
            param_denoise.Clear();
            RadioButton optsrc = sender as RadioButton;
            param_denoise.Append(optsrc.Tag.ToString());
        }

        /*private void OnDeviceChecked(object sender, RoutedEventArgs e)
        {
            param_gpu_id.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_gpu_id.Append(optsrc.Tag.ToString());
        }
        */

        private void OnModelChecked(object sender, RoutedEventArgs e)
        {
            ComboBoxItem optsrc = sender as ComboBoxItem;
            param_model.Clear();
            if (optsrc.Tag.ToString() == "realesrgan-x4plus" || optsrc.Tag.ToString() == "realesrnet-x4plus" || optsrc.Tag.ToString() == "realesrgan-x4plus-anime" || optsrc.Tag.ToString() == "realesr-animevideov3")
            {
                param_model.Append("-n ");
                param_model.Append(optsrc.Tag.ToString());
                btnModeScale.IsChecked = true;
                btnModeNoiseScale.IsEnabled = false;
                btnModeNoise.IsEnabled = false;
                btnModeAutoScale.IsEnabled = false;
            }
            else if (optsrc.Tag.ToString() == "models-DF2K" || optsrc.Tag.ToString() == "models-DF2K_JPEG")
            {
                param_model.Append("-m ");
                param_model.Append(optsrc.Tag.ToString());
                btnModeScale.IsChecked = true;
                btnModeNoiseScale.IsEnabled = false;
                btnModeNoise.IsEnabled = false;
                btnModeAutoScale.IsEnabled = false;
            }
            else
            {
                param_model.Append("-m ");
                param_model.Append(optsrc.Tag.ToString());
                btnModeNoiseScale.IsEnabled = true;
                btnModeNoise.IsEnabled = true;
                btnModeAutoScale.IsEnabled = true;
            }
            if (optsrc.Tag.ToString() == "models-pro")
            {
                if (param_denoise.ToString() == "1" || param_denoise.ToString() == "2")
                { btnDenoise0.IsChecked = true; }
                btnDenoise1.IsEnabled = false;
                btnDenoise2.IsEnabled = false;
            }
            else
            {
                btnDenoise1.IsEnabled = true;
                btnDenoise2.IsEnabled = true;
            }
        }
        private async void OnAbort(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            this.btnRun.IsEnabled = true;
            this.btnAbort.IsEnabled = false;
            string[] process_array = new string[6] { "waifu2x-ncnn-vulkan", "waifu2x-ncnn-vulkan-fp32", "realcugan-ncnn-vulkan", "realesrgan-ncnn-vulkan", "realsr-ncnn-vulkan", "magick" };
            try
            {
                for (int i = 0; i < process_array.Length; i++)
                {
                    foreach (var process in Process.GetProcessesByName(process_array[i]))
                    {
                        process.Kill();
                    }
                }

            }
            catch { }
        }
        public void tasks_waifu2x(int maxConcurrency, int FileCount, string mag_mode_local, string mode_local)
        {
            DateTime starttime = DateTime.Now;
            starttimea = starttime;
            string labelstring = FileCount.ToString();
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "0 / " + prgbar.Maximum, DispatcherPriority.Background);

            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrency))
            {
                List<Task> tasks = new List<Task>();
                Task file_t = Task.Factory.StartNew(() => { });
                Task directory_t = Task.Factory.StartNew(() => { });
                foreach (var input in param_src)
                {
                    if (File.Exists(input))
                    {
                        concurrencySemaphore.Wait();
                        file_t = Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                TimeSpan timespent;
                                if (Cancel == false)
                                {
                                    string input_image = input;
                                    string output_final = null;
                                    if (param_dst.ToString().Trim() == "")
                                    {
                                        output_final = System.IO.Directory.GetParent(input_image) + "\\" + System.IO.Path.GetFileNameWithoutExtension(input_image) + param_dst_suffix + "." + param_outformat;
                                    }
                                    else
                                    {
                                        output_final = param_dst + "\\" + System.IO.Path.GetFileNameWithoutExtension(input_image) + param_dst_suffix + "." + param_outformat;
                                    }
                                    if (Output_no_overwirit == true)
                                    {
                                        if (File.Exists(output_final))
                                        {
                                            prgbar.Dispatcher.Invoke(() => prgbar.Value += 1, DispatcherPriority.Background);
                                            timespent = DateTime.Now - starttime;
                                            pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - Time Left: " + new TimeSpan(0, 0, Convert.ToInt32(Math.Round(((timespent.TotalSeconds / prgbar.Value) * (Int32.Parse(labelstring) - prgbar.Value)), MidpointRounding.ToEven))).ToString(@"hh\:mm\:ss"), DispatcherPriority.Background);
                                            return;
                                        }
                                    }
                                    run_waifu2x(input_image, output_final, mag_mode_local, mode_local);
                                    timespent = DateTime.Now - starttime;
                                    if (Cancel == false)
                                    {
                                        prgbar.Dispatcher.Invoke(() => prgbar.Value += 1, DispatcherPriority.Background);
                                        pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - Time Left: " + new TimeSpan(0, 0, Convert.ToInt32(Math.Round(((timespent.TotalSeconds / prgbar.Value) * (Int32.Parse(labelstring) - prgbar.Value)), MidpointRounding.ToEven))).ToString(@"hh\:mm\:ss"), DispatcherPriority.Background);
                                    }
                                }
                            }

                            finally
                            {
                                concurrencySemaphore.Release();
                            }
                        });
                    }
                    if (Directory.Exists(input))
                    {
                        var reg = new Regex(@".+\.(jpe?g|png|bmp|gif|tiff?|webp)$", RegexOptions.IgnoreCase);
                        var Directoryfiles = Directory.GetFiles((input), "*", SearchOption.AllDirectories).Where(f => reg.IsMatch(f)).ToArray();
                        foreach (var Directoryimage in Directoryfiles)
                        {
                            string relative_path = System.IO.Path.GetDirectoryName(Directoryimage).Replace(input, "");
                            string output_dir = null;
                            concurrencySemaphore.Wait();
                            directory_t = Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    TimeSpan timespent;
                                    if (Cancel == false)
                                    {
                                        string input_image = Directoryimage;
                                        string output_final = null;
                                        if (param_dst.ToString().Trim() == "")
                                        {
                                            output_dir = input + param_dst_suffix + relative_path;
                                        }
                                        else
                                        {
                                            output_dir = param_dst + relative_path;
                                        }
                                        output_final = output_dir + "\\" + System.IO.Path.GetFileNameWithoutExtension(input_image) + "." + param_outformat;

                                        if (Output_no_overwirit == true)
                                        {
                                            if (File.Exists(output_final))
                                            {
                                                prgbar.Dispatcher.Invoke(() => prgbar.Value += 1, DispatcherPriority.Background);
                                                timespent = DateTime.Now - starttime;
                                                pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - Time Left: " + new TimeSpan(0, 0, Convert.ToInt32(Math.Round(((timespent.TotalSeconds / prgbar.Value) * (Int32.Parse(labelstring) - prgbar.Value)), MidpointRounding.ToEven))).ToString(@"hh\:mm\:ss"), DispatcherPriority.Background);
                                                return;
                                            }
                                        }

                                        try
                                        {
                                            Directory.CreateDirectory(output_dir);
                                        }
                                        catch
                                        {
                                            MessageBox.Show(@"Failed to create folder!");
                                            return;
                                        }
                                        run_waifu2x(input_image, output_final, mag_mode_local, mode_local);
                                        timespent = DateTime.Now - starttime;
                                        if (Cancel == false)
                                        {
                                            prgbar.Dispatcher.Invoke(() => prgbar.Value += 1, DispatcherPriority.Background);
                                            pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - Time Left: " + new TimeSpan(0, 0, Convert.ToInt32(Math.Round(((timespent.TotalSeconds / prgbar.Value) * (Int32.Parse(labelstring) - prgbar.Value)), MidpointRounding.ToEven))).ToString(@"hh\:mm\:ss"), DispatcherPriority.Background);
                                        }
                                    }
                                }

                                finally
                                {
                                    concurrencySemaphore.Release();
                                }
                            });
                        }
                    }
                }
                tasks.Add(file_t);
                tasks.Add(directory_t);
                Task.WaitAll(tasks.ToArray());
            }
        }

        private void run_waifu2x(string input_image, string output_final, string mag_mode_local, string mode_local)
        {
            string others_param = String.Join(" ",
                param_model2,
                param_block,
                param_tta,
                param_gpu_id);

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
            startInfo.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            string noise_level_temp = null;
            string noise_level_temp2 = null;
            string Magick_resize_option = "";
            string output_temp = null;
            string output_rgb_temp = null;
            string output_alpha_temp = null;
            string input_temp = null;
            string input_rgb_temp = null;
            string input_alpha_temp = null;
            for (int retryCount = 0; retryCount <= 5; retryCount++)
            {
                Guid g = System.Guid.NewGuid();
                string random32 = (g.ToString("N").Substring(0, 32));
                if (binary_type.ToString() == "waifu2x" || binary_type.ToString() == "realcugan")
                {
                    noise_level_temp2 = "-n -1";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(System.IO.Path.GetExtension(input_image), @"\.jpe?g", RegexOptions.IgnoreCase) && param_mode2.ToString() == "auto_scale")
                    {
                        noise_level_temp = "-n -1";
                    }
                    else
                    {
                        noise_level_temp = param_denoise2.ToString();
                    }
                }

                bool AlphaHas = false;
                int Image_Width = 0;
                int Image_Height = 0;
                if (Alphachannel_ImageMagick == true) if (!System.Text.RegularExpressions.Regex.IsMatch(System.IO.Path.GetExtension(input_image), @"\.jpe?g", RegexOptions.IgnoreCase))
                    {
                        try
                        {
                            startInfo.Arguments = "/C .\\ImageMagick\\magick.exe identify -format %A \"" + input_image + "\"";
                            process.StartInfo = startInfo;
                            process.Start();
                            string IsAlphaPixelFormat = process.StandardOutput.ReadToEnd();
                            process.WaitForExit();
                            if (IsAlphaPixelFormat == "Blend")
                            {
                                AlphaHas = true;
                            }
                        }
                        catch
                        {
                            // 画像の情報を調べるのに失敗
                        }
                    }
                try
                {
                    startInfo.Arguments = "/C .\\ImageMagick\\magick.exe identify -format %Wx%H \"" + input_image + "\"";
                    process.StartInfo = startInfo;
                    process.Start();
                    string identify_WxH = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    if (System.Text.RegularExpressions.Regex.IsMatch(
                        identify_WxH,
                        @"^(\d+x\d+)$",
                        System.Text.RegularExpressions.RegexOptions.ECMAScript))
                    {
                        string[] width_height = identify_WxH.Split('x');
                        Image_Width = int.Parse(width_height[0]);
                        Image_Height = int.Parse(width_height[1]);
                    }
                    else
                    {
                        break;
                    }

                }
                catch
                {
                    // 画像の情報を調べるのに失敗
                }
                int scale_ratio_local = (int)Math.Round(scale_ratio_public);
                int output_width_local = output_width_public;
                int output_height_local = output_height_public;
                if (mode_local != "noise" && mag_mode_local == "Scale_ratio_mode" && txtScale_ratio_exponent == false)
                {
                    output_width_local = (int)Math.Round(Image_Width * scale_ratio_public, MidpointRounding.AwayFromZero);
                    output_height_local = (int)Math.Round(Image_Height * scale_ratio_public, MidpointRounding.AwayFromZero);
                    Magick_resize_option = " -resize " + output_width_local + "x" + output_height_local + "! ";
                }

                if (mode_local != "noise" && output_width_local + output_height_local != 0)
                {
                    scale_ratio_local = 2;
                    if (Keep_aspect_ratio == true && mag_mode_local == "Width_height_mode")
                    {
                        while (true)
                        {
                            if (output_width_local <= Image_Width * scale_ratio_local) break;
                            if (output_height_local <= Image_Height * scale_ratio_local) break;
                            scale_ratio_local = scale_ratio_local * 2;
                            if (Cancel == true) return;
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            if (output_width_local <= Image_Width * scale_ratio_local) break;
                            scale_ratio_local = scale_ratio_local * 2;
                            if (Cancel == true) return;
                        }
                        while (true)
                        {
                            if (output_height_local <= Image_Height * scale_ratio_local) break;
                            scale_ratio_local = scale_ratio_local * 2;
                            if (Cancel == true) return;
                        }
                    }
                }
                if (mag_mode_local == "Width_mode")
                { Magick_resize_option = " -resize " + output_width_local + "x "; }
                if (mag_mode_local == "Height_mode")
                { Magick_resize_option = " -resize " + "x" + output_height_local + " "; }
                if (mag_mode_local == "Width_height_mode")
                {
                    if (Keep_aspect_ratio == true)
                    {
                        Magick_resize_option = " -resize " + output_width_local + "x" + output_height_local + " ";
                    }
                    else
                    {
                        Magick_resize_option = " -resize " + output_width_local + "x" + output_height_local + "! ";
                    }
                }

                int r = 1;
                int r2 = 1;
                int run_times = 1;
                string mag_value = "2";

                if (binary_type.ToString() == "realsr")
                {
                    mag_value = "4";
                    r = r * 4;
                }
                else if (binary_type.ToString() == "realesrgan")
                {
                    if (param_model2.ToString().Replace("-n ", "") == "realesr-animevideov3" && r * 2 >= scale_ratio_local)
                    {
                        mag_value = "2";
                        r = r * 2;
                    }
                    else
                    {
                        mag_value = "4";
                        r = r * 4;
                    }
                }
                else if (binary_type.ToString() == "realcugan")
                {
                    if (param_model2.ToString().Replace("-m ", "") == "models-se" && noise_level_temp != "-n 1" && noise_level_temp != "-n 2" && r * 4 <= scale_ratio_local)
                    {
                        mag_value = "4";
                        r = r * 4;
                    }
                    else
                    {
                        mag_value = "2";
                        r = r * 2;
                    }
                }
                else
                {
                    mag_value = "2";
                    r = r * 2;
                }
                if (scale_ratio_public == 1 && mode_local == "noise")
                {
                    r = 1;
                    mag_value = "1";
                    scale_ratio_local = 1;
                }

                // debug code
                /*
                string debug_txt = System.IO.Path.ChangeExtension(output_final, "txt");
                Encoding enc_debug = Encoding.UTF8;
                StreamWriter writer_debug = new StreamWriter(debug_txt, false, enc_debug);
                writer_debug.WriteLine(input_image);
                writer_debug.WriteLine("mag_value " + mag_value.ToString());
                writer_debug.WriteLine("mag_mode_local " + mag_mode_local);
                writer_debug.WriteLine("Magick_resize_option " + Magick_resize_option);
                writer_debug.WriteLine("param_outformat " + param_outformat);
                writer_debug.WriteLine("param_output_quality " + param_output_quality);
                writer_debug.WriteLine("txtScale_ratio_exponent " + txtScale_ratio_exponent.ToString());
                writer_debug.WriteLine("scale_ratio_public " + scale_ratio_public.ToString());
                writer_debug.WriteLine("scale_ratio_local " + scale_ratio_local.ToString());
                writer_debug.WriteLine("Image_Width " + Image_Width);
                writer_debug.WriteLine("Image_Height " + Image_Height);
                writer_debug.WriteLine("output_width_public " + output_width_public.ToString());
                writer_debug.WriteLine("output_height_public " + output_height_public);
                writer_debug.WriteLine("output_width_local " + output_width_local.ToString());
                writer_debug.WriteLine("output_height_local " + output_height_local.ToString());
                writer_debug.WriteLine("binary_type " + binary_type.ToString());
                writer_debug.Close();
                
                */

                if (AlphaHas == true)
                {
                    startInfo.Arguments =
                       "/C .\\ImageMagick\\magick.exe convert \"" + input_image + "\" -channel RGB -separate -combine png24:\"" + System.IO.Path.GetTempPath() + random32 + "-RGB" + r2 + "x.png" + "\" && " +
                          ".\\ImageMagick\\magick.exe convert \"" + input_image + "\" -channel matte -separate +matte png24:\"" + System.IO.Path.GetTempPath() + random32 + "-Alpha" + r2 + "x.png" + "\""
                    ;
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
                for (; run_times <= 10; run_times = run_times + 1)
                {
                    if (Cancel == true) return;
                    output_temp = System.IO.Path.GetTempPath() + random32 + "-" + r + "x.png";
                    output_rgb_temp = System.IO.Path.GetTempPath() + random32 + "-RGB" + r + "x.png";
                    output_alpha_temp = System.IO.Path.GetTempPath() + random32 + "-Alpha" + r + "x.png";
                    input_temp = System.IO.Path.GetTempPath() + random32 + "-" + r2 + "x.png";
                    input_rgb_temp = System.IO.Path.GetTempPath() + random32 + "-RGB" + r2 + "x.png";
                    input_alpha_temp = System.IO.Path.GetTempPath() + random32 + "-Alpha" + r2 + "x.png";
                    if (run_times == 1)
                    {
                        if (AlphaHas == true)
                        {
                            if (scale_ratio_local == 1)
                            {
                                startInfo.Arguments = "/C " + binary_path + " -i \"" + input_rgb_temp + "\" -o \"" + output_rgb_temp + "\" -s " + mag_value + " " + noise_level_temp + " " + others_param;
                            }
                            else
                            {
                                startInfo.Arguments = "/C " + binary_path + " -i \"" + input_rgb_temp + "\" -o \"" + output_rgb_temp + "\" -s " + mag_value + " " + noise_level_temp + " " + others_param + "&& " +
                                                          binary_path + " -i \"" + input_alpha_temp + "\" -o \"" + output_alpha_temp + "\" -s " + mag_value + " " + noise_level_temp2 + " " + others_param;
                            }
                        }
                        else
                        {
                            startInfo.Arguments = "/C " + binary_path + " -i \"" + input_image + "\" -o \"" + output_temp + "\" -s " + mag_value + " " + noise_level_temp + " " + others_param;
                        }
                    }
                    else
                    {
                        if (AlphaHas == true)
                        {
                            startInfo.Arguments = "/C " + binary_path + " -i \"" + input_rgb_temp + "\" -o \"" + output_rgb_temp + "\"  -s " + mag_value + " " + noise_level_temp2 + " " + others_param + "&& " +
                                                          binary_path + " -i \"" + input_alpha_temp + "\" -o \"" + output_alpha_temp + "\" -s " + mag_value + " " + noise_level_temp2 + " " + others_param;
                        }
                        else
                        {
                            startInfo.Arguments = "/C " + binary_path + " -i \"" + input_temp + "\" -o \"" + output_temp + "\" -s " + mag_value + " " + noise_level_temp2 + " " + others_param;
                        }

                    }


                    process.StartInfo = startInfo;
                    process.Start();
                    string stdout = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    if (process.ExitCode != 0) if (Cancel == false)
                        {
                            System.Media.SystemSounds.Beep.Play();
                            CLIOutput.Dispatcher.Invoke(() => CLIOutput.Focus(), DispatcherPriority.Background);
                            CLIOutput.Dispatcher.Invoke(() => this.CLIOutput.AppendText("Error: The process failed and will be retried.\ncmd.exe " + startInfo.Arguments + "\n" + stdout + "\n"), DispatcherPriority.Background);
                            CLIOutput.Dispatcher.Invoke(() => CLIOutput.Select(CLIOutput.Text.Length, 0), DispatcherPriority.Background);
                        }
                    if (scale_ratio_local != 1)
                    {
                        new FileInfo(input_temp).Delete();
                        new FileInfo(input_rgb_temp).Delete();
                        new FileInfo(input_alpha_temp).Delete();
                    }
                    if (r >= scale_ratio_local) break;
                    if (mag_value == "4") r2 = r2 * 4;
                    if (mag_value == "2") r2 = r2 * 2;
                    if (binary_type.ToString() == "realsr")
                    {
                        mag_value = "4";
                        r = r * 4;
                    }
                    else if (binary_type.ToString() == "realesrgan")
                    {
                        if (param_model2.ToString().Replace("-n ", "") == "realesr-animevideov3" && r * 2 >= scale_ratio_local)
                        {
                            mag_value = "2";
                            r = r * 2;
                        }
                        else
                        {
                            mag_value = "4";
                            r = r * 4;
                        }
                    }
                    else if (binary_type.ToString() == "realcugan")
                    {
                        if (param_model2.ToString().Replace("-m ", "") == "models-se" && noise_level_temp != "-n 1" && noise_level_temp != "-n 2" && r * 4 <= scale_ratio_local)
                        {
                            mag_value = "4";
                            r = r * 4;
                        }
                        else
                        {
                            mag_value = "2";
                            r = r * 2;
                        }
                    }
                    else
                    {
                        mag_value = "2";
                        r = r * 2;
                    }
                }
                if (AlphaHas == true)
                {
                    startInfo.Arguments = "/C .\\ImageMagick\\magick.exe convert " + "\"" + output_rgb_temp + "\" " + "\"" + output_alpha_temp + "\" -compose CopyOpacity -composite " + param_output_quality + Magick_resize_option + "  \"" + output_final + "\"";
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                    new FileInfo(output_rgb_temp).Delete();
                    new FileInfo(output_alpha_temp).Delete();
                }
                else
                {
                    if (Cancel == true) return;
                    if (param_outformat.ToString() == "png" && Magick_resize_option.Trim() == "")
                    {
                        try
                        {
                            if (File.Exists(output_final))
                            {
                                File.Delete(output_final);
                            }
                            System.IO.File.Move(output_temp, output_final);
                        }
                        catch
                        { }
                    }
                    else
                    {
                        startInfo.Arguments = "/C .\\ImageMagick\\magick.exe convert " + "\"" + output_temp + "\" " + param_output_quality + Magick_resize_option + "  \"" + output_final + "\"";
                        process.StartInfo = startInfo;
                        process.Start();
                        process.WaitForExit();
                        new FileInfo(output_temp).Delete();
                    }

                }
                if (!File.Exists(output_final))
                {
                    if (retryCount == 5)
                    {
                        System.Media.SystemSounds.Beep.Play();
                        CLIOutput.Dispatcher.Invoke(() => CLIOutput.Focus(), DispatcherPriority.Background);
                        CLIOutput.Dispatcher.Invoke(() => this.CLIOutput.AppendText("Error: Output file could not be found.\ninput " + input_image + "\noutput " + output_final + "\n\n"), DispatcherPriority.Background);
                        CLIOutput.Dispatcher.Invoke(() => CLIOutput.Select(CLIOutput.Text.Length, 0), DispatcherPriority.Background);
                        try
                        {
                            Encoding enc = Encoding.UTF8;
                            StreamWriter writer = new StreamWriter("error_log.txt", true, enc);
                            writer.WriteLine(System.DateTime.Now.ToString());
                            writer.WriteLine("Error: Output file could not be found.");
                            writer.WriteLine("input  " + input_image);
                            writer.WriteLine("output " + output_final + "\r\n");
                            writer.Close();
                        }
                        catch
                        { }
                    }
                }
                else
                {
                    break;
                }
            }
        }
        public void Errormessage(string x)
        {
            System.Media.SystemSounds.Beep.Play();
            MessageBox.Show(@x);
            btnAbort.IsEnabled = false;
            btnRun.IsEnabled = true;
            return;
        }

        async private void OnRun(object sender, RoutedEventArgs e)
        {
            Cancel = false;
            binary_path.Clear();
            binary_type.Clear();
            if (param_model.ToString().Replace("-m ", "") == "models-DF2K" || param_model.ToString().Replace("-m ", "") == "models-DF2K_JPEG")
            {
                if (!File.Exists("realsr-ncnn-vulkan\\realsr-ncnn-vulkan.exe"))
                {
                    MessageBox.Show(@"realsr-ncnn-vulkan\\realsr-ncnn-vulkan.exe is missing!");
                    return;
                }
                binary_path.Append(".\\realsr-ncnn-vulkan\\realsr-ncnn-vulkan.exe ");
                binary_type.Append("realsr");
            }
            else if (param_model.ToString().Replace("-n ", "") == "realesrgan-x4plus" || param_model.ToString().Replace("-n ", "") == "realesrnet-x4plus" || param_model.ToString().Replace("-n ", "") == "realesrgan-x4plus-anime" || param_model.ToString().Replace("-n ", "") == "realesr-animevideov3")
            {
                if (!File.Exists("realesrgan-ncnn-vulkan\\realesrgan-ncnn-vulkan.exe"))
                {
                    MessageBox.Show(@"realesrgan-ncnn-vulkan\\realesrgan-ncnn-vulkan.exe is missing!");
                    return;
                }
                binary_path.Append(".\\realesrgan-ncnn-vulkan\\realesrgan-ncnn-vulkan.exe ");
                binary_type.Append("realesrgan");
            }
            else if (param_model.ToString().Replace("-m ", "") == "models-se" || param_model.ToString().Replace("-m ", "") == "models-pro")
            {
                if (!File.Exists("realcugan-ncnn-vulkan\\realcugan-ncnn-vulkan.exe"))
                {
                    MessageBox.Show(@"realcugan-ncnn-vulkan\\realcugan-ncnn-vulkan.exe is missing!");
                    return;
                }
                binary_path.Append(".\\realcugan-ncnn-vulkan\\realcugan-ncnn-vulkan.exe ");
                binary_type.Append("realcugan");
            }
            else
            {
                binary_type.Append("waifu2x");
                if (checkPrecision_fp32.IsChecked == true)
                {
                    if (!File.Exists("waifu2x-ncnn-vulkan\\waifu2x-ncnn-vulkan-fp32.exe"))
                    {
                        MessageBox.Show(@"waifu2x-ncnn-vulkan\\waifu2x-ncnn-vulkan-fp32.exe is missing!");
                        return;
                    }
                    binary_path.Append(".\\waifu2x-ncnn-vulkan\\waifu2x-ncnn-vulkan-fp32.exe ");
                }
                else
                {
                    if (!File.Exists("waifu2x-ncnn-vulkan\\waifu2x-ncnn-vulkan.exe"))
                    {
                        MessageBox.Show(@"waifu2x-ncnn-vulkan\\waifu2x-ncnn-vulkan.exe is missing!");
                        return;
                    }
                    binary_path.Append(".\\waifu2x-ncnn-vulkan\\waifu2x-ncnn-vulkan.exe ");
                }
            }

            if (!File.Exists("ImageMagick\\Magick.exe"))
            {
                MessageBox.Show(@"ImageMagick is missing!");
                return;
            }

            if (param_src == null)
            {
                MessageBox.Show(@"source images is not found!");
                return;
            }

            if (this.txtDstPath.Text.Trim() != "") if (!Directory.Exists(this.txtDstPath.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(this.txtDstPath.Text);
                    }
                    catch
                    {
                        MessageBox.Show(@"Failed to create folder!");
                        return;
                    }
                }

            // Sets Source
            // The source must be a file or folder that exists

            param_tta.Clear();
            if (checkTTAmode.IsChecked == true)
            {
                param_tta.Append("-x");
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtGPU_ID.Text,
                @"^(\d+)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                param_gpu_id.Clear();
                param_gpu_id.Append("-g ");
                param_gpu_id.Append(txtGPU_ID.Text);
            }
            else
            {
                param_gpu_id.Clear();
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtBlocksize.Text,
                @"^(\d+)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                if (int.Parse(txtBlocksize.Text) % 4 != 0)
                {
                    MessageBox.Show(@"Block size must be a multiple of 4");
                    return;
                }
                else
                {
                    param_block.Clear();
                    param_block.Append("-t ");
                    param_block.Append(txtBlocksize.Text);
                }
            }
            else
            {
                param_block.Clear();
            }

            param_outformat.Clear();
            param_outformat.Append(txtOutExt.Text);

            param_output_quality.Clear();
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                txtOutQuality.Text,
                @"^(\d+)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                txtOutQuality.Text = "100";
            }
            if (param_outformat.ToString() == "webp")
            {
                if (txtOutQuality.Text == "100")
                {
                    param_output_quality.Append("-define webp:lossless=true");
                }
                else
                {
                    param_output_quality.Append("-quality " + txtOutQuality.Text);
                }
            }
            else if (param_outformat.ToString() == "jpg")
            {
                param_output_quality.Append("( +clone -alpha opaque -fill white -colorize 100% ) +swap -geometry +0+0 -compose Over -composite -alpha off -quality " + txtOutQuality.Text);
            }
            else
            {
                param_output_quality.Append("-quality " + txtOutQuality.Text);
            }

            /*
            if (((int.Parse(txtScale_ratio.Text) & int.Parse(txtScale_ratio.Text) - 1)) != 0)
            {
                MessageBox.Show(@"Magnification must be a power of two.");
                return;
            }
            */
            param_mode2.Clear();
            param_mode2.Append(param_mode.ToString());
            if (param_mode2.ToString() == "noise") if (param_model.ToString().Replace("-m ", "") != "models-cunet")
                {
                    MessageBox.Show("\"Denoise only\" is available only for CUnet models.");
                    return;
                }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtThread.Text,
                 @"^(\d+)$",
               System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                param_thread.Clear();
                param_thread.Append(txtThread.Text);
            }
            else
            {
                param_thread.Clear();
            }

            param_model2.Clear();
            param_model2.Append(param_model.ToString());

            param_denoise2.Clear();
            param_denoise2.Append("-n ");
            param_denoise2.Append(param_denoise.ToString());
            output_width_public = 0;
            output_height_public = 0;
            // Set mode

            if (param_mode2.ToString() == "scale")
            {
                param_denoise2.Clear();
                param_denoise2.Append("-n ");
                param_denoise2.Append("-1");
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(
                txtScale_ratio.Text,
                @"^(\d+(\.\d+)?)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                scale_ratio_public = float.Parse(txtScale_ratio.Text);
            }
            else
            {
                MessageBox.Show("Scale_ratio must be a number.");
                return;
            }

            txtScale_ratio_exponent = false;
            if (binary_type.ToString() != "realesrgan" && System.Text.RegularExpressions.Regex.IsMatch(
                txtScale_ratio.Text,
                @"^(2|4|8|16|32|64|128|256|512|1024)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                txtScale_ratio_exponent = true;
            }
            if (binary_type.ToString() == "realesrgan" && System.Text.RegularExpressions.Regex.IsMatch(
                txtScale_ratio.Text,
                @"^(4|16|64|128|512)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                txtScale_ratio_exponent = true;
            }

            if (param_mode2.ToString() != "noise") if (param_mag_mode.ToString() == "Width_mode") if (System.Text.RegularExpressions.Regex.IsMatch(
                txtOutput_width.Text,
                @"^(\d+)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
                    {
                        output_width_public = int.Parse(txtOutput_width.Text);
                    }
                    else
                    {
                        MessageBox.Show("Width must be a number.");
                        return;
                    }

            if (param_mode2.ToString() != "noise") if (param_mag_mode.ToString() == "Height_mode") if (System.Text.RegularExpressions.Regex.IsMatch(
                txtOutput_height.Text,
                @"^(\d+)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
                    {
                        output_height_public = int.Parse(txtOutput_height.Text);
                    }
                    else
                    {
                        MessageBox.Show("Height must be a number.");
                        return;
                    }

            if (param_mode2.ToString() != "noise") if (param_mag_mode.ToString() == "Width_height_mode") if (System.Text.RegularExpressions.Regex.IsMatch(
                txtOutput_width_height.Text,
                @"^(\d+x\d+)$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
                    {
                        string[] width_height = txtOutput_width_height.Text.Split('x');
                        output_width_public = int.Parse(width_height[0]);
                        output_height_public = int.Parse(width_height[1]);
                    }
                    else
                    {
                        MessageBox.Show("Width and Height must be a number.\n\nexample: 1920x1080");
                        return;
                    }

            this.btnRun.IsEnabled = false;
            this.btnAbort.IsEnabled = true;

            param_dst.Clear();
            param_dst.Append(this.txtDstPath.Text);

            param_dst_suffix.Clear();
            if (this.txtDstPath.Text.Trim() == "")
            {
                if (param_model.ToString().Replace("-m ", "") == "models-cunet")
                { param_dst_suffix.Append("(CUnet)"); }
                if (param_model.ToString().Replace("-m ", "") == "models-upconv_7_anime_style_art_rgb")
                { param_dst_suffix.Append("(UpRGB)"); }
                if (param_model.ToString().Replace("-m ", "") == "models-upconv_7_photo")
                { param_dst_suffix.Append("(UpPhoto)"); }
                if (param_model.ToString().Replace("-m ", "") == "models-se")
                { param_dst_suffix.Append("(Real-CUGAN-se)"); }
                if (param_model.ToString().Replace("-m ", "") == "models-pro")
                { param_dst_suffix.Append("(Real-CUGAN-pro)"); }
                if (param_model.ToString().Replace("-n ", "") == "realesrgan-x4plus")
                { param_dst_suffix.Append("(realesrgan-x4plus)"); }
                if (param_model.ToString().Replace("-n ", "") == "realesrnet-x4plus")
                { param_dst_suffix.Append("(realesrnet-x4plus)"); }
                if (param_model.ToString().Replace("-n ", "") == "realesrgan-x4plus-anime")
                { param_dst_suffix.Append("(realesrgan-x4plus-anime)"); }
                if (param_model.ToString().Replace("-n ", "") == "realesr-animevideov3")
                { param_dst_suffix.Append("(realesr-animevideov3)"); }
                if (param_model.ToString().Replace("-m ", "") == "models-DF2K")
                { param_dst_suffix.Append("(DF2K)"); }
                if (param_model.ToString().Replace("-m ", "") == "models-DF2K_JPEG")
                { param_dst_suffix.Append("(DF2K_JPEG)"); }
                param_dst_suffix.Append("(");
                param_dst_suffix.Append(param_mode2.ToString().Replace("-m ", ""));
                param_dst_suffix.Append(")");
                if (param_mode2.ToString() == "noise" || param_mode2.ToString() == "noise_scale" || param_mode2.ToString() == "auto_scale")
                {
                    param_dst_suffix.Append("(");
                    param_dst_suffix.Append("Level");
                    param_dst_suffix.Append(param_denoise.ToString());
                    param_dst_suffix.Append(")");
                }
                if (checkTTAmode.IsChecked == true)
                {
                    param_dst_suffix.Append("(tta)");
                }

                if (param_mode2.ToString() == "scale" || param_mode2.ToString() == "noise_scale" || param_mode2.ToString() == "auto_scale")
                {
                    if (param_mag_mode.ToString() == "Scale_ratio_mode")
                    {
                        param_dst_suffix.Append("(x");
                        param_dst_suffix.Append(txtScale_ratio.Text);
                        param_dst_suffix.Append(")");
                    }
                    if (param_mag_mode.ToString() == "Width_mode")
                    {
                        param_dst_suffix.Append("(width ");
                        param_dst_suffix.Append(output_width_public);
                        param_dst_suffix.Append(")");
                    }
                    if (param_mag_mode.ToString() == "Height_mode")
                    {
                        param_dst_suffix.Append("(height ");
                        param_dst_suffix.Append(output_height_public);
                        param_dst_suffix.Append(")");
                    }
                    if (param_mag_mode.ToString() == "Width_height_mode")
                    {
                        param_dst_suffix.Append("(");
                        if (checkKeep_aspect_ratio.IsChecked == true)
                        {
                            param_dst_suffix.Append("within ");
                        }
                        param_dst_suffix.Append(output_width_public);
                        param_dst_suffix.Append("x");
                        param_dst_suffix.Append(output_height_public);
                        param_dst_suffix.Append(")");
                    }

                }
                if (binary_type.ToString() == "waifu2x" && checkPrecision_fp32.IsChecked == true)
                {
                    param_dst_suffix.Append("(FP32)");
                }
            }
            Keep_aspect_ratio = checkKeep_aspect_ratio.IsChecked.Value;
            Output_no_overwirit = checkOutput_no_overwirit.IsChecked.Value;
            Alphachannel_ImageMagick = checkAlphachannel_ImageMagick.IsChecked.Value;
            if (param_mode2.ToString() == "noise")
            { scale_ratio_public = 1; }

            FileCount = 0;
            var reg = new Regex(@".+\.(jpe?g|png|bmp|gif|tiff?|webp)$", RegexOptions.IgnoreCase);
            foreach (var dropfile in param_src)
            {
                if (File.Exists(dropfile))
                {
                    FileCount += 1;
                }
                if (Directory.Exists(dropfile))
                {
                    await Task.Run(() =>
                    {
                        var Directoryfiles = Directory.GetFiles((dropfile), "*", SearchOption.AllDirectories).Where(f => reg.IsMatch(f)).ToArray();
                        foreach (var Directoryimage in Directoryfiles)
                        {
                            FileCount += 1;
                        }
                    });
                }
            }

            prgbar.Maximum = FileCount;
            prgbar.Value = 0;
            this.CLIOutput.Clear();

            await Task.Run(() => tasks_waifu2x(int.Parse(param_thread.ToString()), FileCount, param_mag_mode.ToString(), param_mode2.ToString()));
            TimeSpan Processing_time;
            Processing_time = DateTime.Now - starttimea;
            while (true)
            {
                await Task.Delay(100);
                if (prgbar.Value == FileCount)
                {
                    pLabel.Dispatcher.Invoke(() => pLabel.Content = "Processing time: " + (Processing_time).ToString(@"hh\:mm\:ss\.fff"), DispatcherPriority.Background);
                    break;
                }
                if (Cancel == true)
                {
                    pLabel.Dispatcher.Invoke(() => pLabel.Content = "has cancelled", DispatcherPriority.Background);
                    break;
                }
            }
            prgbar.Value = 0;
            this.btnRun.IsEnabled = true;
            this.btnAbort.IsEnabled = false;

            if (checkSoundBeep.IsChecked == true)
            { System.Media.SystemSounds.Beep.Play(); }
        }


    }
}
