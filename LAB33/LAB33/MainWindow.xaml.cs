using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LAB33
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker worker;
        private List<int> currentGeneratedValues = new List<int>(); 

        public MainWindow()
        {
            worker = new BackgroundWorker();
            InitializeComponent();
            worker.DoWork += new DoWorkEventHandler(GenerateNumbers);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(NumbersReady);
            worker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgress);
            worker.WorkerReportsProgress = true;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = new GeneratorSettings();
            try
            {
                userSettings.RangeFrom = TransformInputToInt(rangeFrom.Text);
            } catch (Exception ex)
            {
                MessageBox.Show("Error while reading 'range from' value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                userSettings.RangeTo = TransformInputToInt(rangeTo.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while reading 'range to' value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                userSettings.NumberOfElements = TransformInputToInt(elementsNumber.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while reading 'number of elements' value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            startButton.IsEnabled = false;
            worker.RunWorkerAsync(userSettings);
        }

        private void OnClickNew(object sender, RoutedEventArgs e)
        {
            currentGeneratedValues.Clear();
            startButton.IsEnabled = true;
            UpdateContentText();
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aleksandra Front :)", "Author", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        private void OnClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private int TransformInputToInt(string value)
        {
            if (int.TryParse(value, out int parsedValue))
            {
                return parsedValue;
            }
            else
            {
                value = value.Replace(" ", String.Empty);

                while (value.StartsWith('0') && value.Length >= 2)
                {
                    value = value.Substring(1);
                }

                if (int.TryParse(value, out int secondParseTry))
                {
                    return secondParseTry;
                }
                else
                {
                    throw new Exception("Error while reading entered values");
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OnLoadClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            var path = string.Empty;
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
            }

            var lines = File.ReadAllLines(path).ToList();
            var filtered = lines.Where(line => int.TryParse(line, out int parsed)).ToList();

            currentGeneratedValues.Clear();

            currentGeneratedValues.AddRange(filtered.Select(line =>
            {
                int.TryParse(line, out int parsed);
                return parsed;
            }));
            UpdateContentText();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "txt files (*.txt)|*.txt";
            var path = string.Empty;
            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;
                File.WriteAllLines(path, currentGeneratedValues.Select(number => number.ToString()));
            }
        }

        private void GenerateNumbers(Object sender, DoWorkEventArgs e)
        {
            BackgroundWorker w = sender as BackgroundWorker;
            var userSettings = e.Argument as GeneratorSettings;
            var generator = new NumbersGenerator();
            e.Result = generator.Generate(userSettings.NumberOfElements, userSettings.RangeFrom, userSettings.RangeTo, worker);
        }

        private void NumbersReady(Object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                statusText.Text = "Cancelled";
            }
            else
            {
                statusText.Text = "Successed";
                var res = (List<int>)e.Result;
                currentGeneratedValues.AddRange(res);
            }
            progressBar.Value = 0;
            UpdateContentText();
        }

        private void UpdateContentText()
        {
            contentText.Text = string.Join("\n", currentGeneratedValues);
        }

        private void UpdateProgress(Object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            statusText.Text = "generating numbers: " + e.ProgressPercentage.ToString() + "%";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var res = MessageBox.Show("Do you want to exit application?", "Application exit", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (res == MessageBoxResult.No || res == MessageBoxResult.None)
            {
                e.Cancel = true;
            }
        }
    }
}
