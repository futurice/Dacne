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
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApplication
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IObservable<OperationState<NewsArticle>> _states;

        public MainPage()
        {
            this.InitializeComponent();

            _states = App.Repository.Get<NewsArticle>(new ModelIdentifier("testmodelid"), SourcePreference.Cache, CancellationToken.None)
                .ObserveOn(SynchronizationContext.Current);

            // Option B
            Progress = _states.Select(state => state.Error?.ToString() ?? state.Result?.Title ?? state.Progress.ToString()).ToReadOnlyReactiveProperty();

            this.Loaded += MainPage_Loaded;
        }

        public ReadOnlyReactiveProperty<string> Progress { get; private set; }


        public string Error { get; set; }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Option A
            for (int i = 0; i < 100; i++) {
                await Task.Delay(TimeSpan.FromMilliseconds(500));

                var tb = new TextBlock();
                TextBlocksPanel.Children.Add(tb);

                int count = 0;
                App.Repository.Get<NewsArticle>(
                    new ModelIdentifier("testmodelid"), 
                    i % 2 == 0 ? SourcePreference.Cache 
                    : SourcePreference.Server, CancellationToken.None)
                        .SelectMany(s => Observable.Return(s).DelaySubscription(TimeSpan.FromMilliseconds(50 * count++)))
                        .ObserveOn(UIDispatcherScheduler.Default)
                        .SubscribeStateChange(
                            onResult: result => tb.Text = result.Title,
                            onProgress: progress => tb.Text = progress.ToString() + "%",
                            onError: error => Error = error.ToString()
                    );
            }

            /*
            DataContext = await states
                .Where(state => state.Result != null)
                .Select(state => state.Result)
                .FirstAsync();
            */

            // Option C
            //DataContext = await states.AwaitResultAsync();
        }
    }

    public class RowViewModel
    {
        public ReadOnlyReactiveProperty<string> Progress { get; private set; }
        
        public void Load()
        {

        }

    }
    
}
