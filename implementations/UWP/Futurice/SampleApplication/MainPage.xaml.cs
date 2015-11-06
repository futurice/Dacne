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
using System.Threading;
using System.Diagnostics;
using Reactive.Bindings;

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

        public ReadOnlyReactiveProperty<string> Progress { get; private set; }


        public string Error { get; set; }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var states = App.Repository.Get<NewsArticle>(new ModelIdentifier("testmodelid"), CancellationToken.None)
                            .ObserveOn(SynchronizationContext.Current);

            // Option A

            states.SubscribeStateChange(
                onResult: result => DataContext = result.Title,
                onProgress: progress => DataContext = progress.ToString() + "%",
                onError: error => Error = error.ToString()
            );

            Progress = states.Select(state => state.Progress.ToString() + "%").ToReadOnlyReactiveProperty();

            // Option B
            /*
            states
                .Select(state => state.Progress)
                .Subscribe(progress => Progress = progress.ToString() + "%");

            DataContext = await states
                .Where(state => state.Result != null)
                .Select(state => state.Result)
                .FirstAsync();
            */
            
            // Option C
            //DataContext = await states.AwaitResultAsync();
        }
    }

    //public static RepostioryExtensions

    
}
