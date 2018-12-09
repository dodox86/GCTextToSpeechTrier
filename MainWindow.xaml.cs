using Google.Cloud.TextToSpeech.V1;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GCTextToSpeechTrier
{
    public partial class MainWindow : Window
    {
        private static string EnvironmentVariableName = "GOOGLE_APPLICATION_CREDENTIALS";

        private string defaultCredentialsPath = @".\credentials.json";
        private string languageCode = "en-US";
        private SsmlVoiceGender ssmiGender = SsmlVoiceGender.Neutral;

        public MainWindow()
        {
            InitializeComponent();

            string path = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            CredentialFilePathTextBox.Text = string.IsNullOrEmpty(path) ? defaultCredentialsPath : path;
        }

        /// <summary>
        /// set credential file path to GOOGLE_APPLICATION_CREDENTIALS environment variable. 
        /// </summary>
        /// <param name="path">Credential file path (usually it may be *.json)</param>
        private void SetCredentialsPath(string path)
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableName, path);
        }

        private void Request(string text)
        {
            // Instantiate a client
            var client = TextToSpeechClient.Create();

            // Set the text input to be synthesized.
            var input = new SynthesisInput
            {
                //Text = "Hello, World!"
                Text = text,
            };

            // Build the voice request, select the language code ("en-US"),
            // and the SSML voice gender ("neutral").
            var voice = new VoiceSelectionParams
            {
                LanguageCode = languageCode,
                SsmlGender = ssmiGender,
            };

            // Select the type of audio file you want returned.
            var config = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the Text-to-Speech request, passing the text input
            // with the selected voice parameters and audio file type
            var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = input,
                Voice = voice,
                AudioConfig = config
            });

            // Write the binary AudioContent of the response to an MP3 file.
            using (Stream output = File.Create("sample.mp3"))
            {
                response.AudioContent.WriteTo(output);
                Debug.WriteLine($"Audio content written to file 'sample.mp3'");
            }
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RequestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetCredentialsPath(CredentialFilePathTextBox.Text);

                Request(ContentTextBox.Text);
                MessageBox.Show("Done.", Title, MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"caused Exception: {ex.Message}");
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string fileName = null;
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                // Need to be a regular file.
                if (File.Exists(files[0]))
                {
                    fileName = files[0];
                }
            }

            if (string.IsNullOrEmpty(fileName))
            {
                e.Handled = true;
                return;
            }

            if (sender is GroupBox)
            {
                var control = (GroupBox)sender;
                if (control.Name == CredentialGroupBox.Name)
                {
                    CredentialFilePathTextBox.Text = fileName;
                    SetCredentialsPath(CredentialFilePathTextBox.Text);
                }
                else if (control.Name == SoundFileGroupBox.Name)
                {
                    SoundFilePathTextBox.Text = fileName;
                }
            }
            else if (sender is TextBox)
            {
                var control = (TextBox)sender;
                if (control.Name == ContentTextBox.Name)
                {
                    ContentTextBox.Text = File.ReadAllText(fileName);
                }
                e.Handled = true;
            }
        }

        private string ChooseFilePath(string title, string filter)
        {
            string path = null;
            var dialog = new OpenFileDialog();
            dialog.Title = title;
            dialog.Filter = filter;
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                path = dialog.FileName;
            }
            return path;
        }

        private void CredentialFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            string path = ChooseFilePath("Credential file path", "All Files(*.*)|*.json");
            if (!string.IsNullOrEmpty(path))
            {
                CredentialFilePathTextBox.Text = path;
            }
        }

        private void SoundFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            string path = ChooseFilePath("Sound file path", "All Files(*.*)|*.mp3");
            if (!string.IsNullOrEmpty(path))
            {
                SoundFilePathTextBox.Text = path;
            }
        }
    }
}
