using System;
using Dacne.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Threading;
using System.Reactive.Linq;

namespace SampleApplication
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IObservable<IOperationState<NewsArticle>> _states;

        public MainPage()
        {
            InitializeComponent();

            /*
            _states = App.Repository.Get<NewsArticle>(new ModelIdentifier("testmodelid"), SourcePreference.Cache, CancellationToken.None)
                .ObserveOn(SynchronizationContext.Current);

            // Option B
            Progress = _states.Select(state => state.Error?.ToString() ?? state.Result?.Title ?? state.Progress.ToString()).ToReadOnlyReactiveProperty();
            */

            Loaded += MainPage_Loaded;
        }

        public string Error { get; set; }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var cts = new CancellationTokenSource();
            //cts.Cancel();

            var titleUpdateToken = new object();
            var theArticle = ModelLoader.GetBbcArticleIdentifier(44289404, "world", "europe");

            App.Repository.Commit(
                theArticle.ChangeTitle(
                    title => title += " <- My fav!",
                    titleUpdateToken
                )
            );

            var i = 0;
            // Option A:

            //for (int i = 0; i < 100; i++) {
            //await Task.Delay(TimeSpan.FromMilliseconds(10 * i));

            var tb = new TextBlock();
            tb.Tapped += Tb_Tapped;

            TextBlocksPanel.Children.Add(tb);

            //int count = 0;
            int j = i;
            App.Repository.Get(
                theArticle,
                //ModelLoader.GetBbcArticlesIdentifier(),
                SourcePreference.CacheWithServerFallback,
                    i % 2 == 0 ? cts.Token : CancellationToken.None)
                    //.SelectMany(s => Observable.Return(s).DelaySubscription(TimeSpan.FromMilliseconds(50 * count++)))
                    .ObserveOn(SynchronizationContext.Current)
                    .SubscribeStateChange(
                        onProgress: progress => tb.Text = progress.ToString() + "%",
                        //onResult: result => tb.Text = result.Count().ToString(),
                        onError: error => tb.Text = error.ToString(),
                        onCompleted: state => tb.Text = state?.IsCancelled ?? true
                                                            ? ":("
                                                            : state?.Result?.Title + " / " + state?.ResultSource.ToString()
                    );
            //}

            // Option B
            /*
            DataContext = await states
                .Where(state => state.Result != null)
                .Select(state => state.Result)
                .FirstAsync();
            */

            // Option C
            //DataContext = await states.AwaitResultAsync();
        }

        private void Tb_Tapped(object sender, TappedRoutedEventArgs e)
        {
            App.Repository.PushAll(ModelSource.Server);
        }
    }
    
}
