using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Futurice.DataAccess;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Reactive.Linq;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;
        }

        public string Progress { get; set; }


        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var operation = App.Repository.Get<NewsArticle>(new ModelIdentifier("testmodelid"));
            var operationStates = operation.Begin();

            operationStates
                .Select(state => state.Progress)
                .Subscribe(progress => Progress = progress.ToString() + "%");

            DataContext = await operationStates
                .Where(state => state.Result != null)
                .Select(state => state.Result)
                .FirstAsync();
        }
    }
}
