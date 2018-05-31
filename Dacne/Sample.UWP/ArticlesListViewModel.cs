using Dacne.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.UWP
{
    class ArticlesListViewModel : ReactiveObject
    {
        public ReactiveCommand Refresh { get; private set; }
        public ReactiveCommand SaveChanges { get; private set; }

        private double _progress;

        public double Progress
        {
            get { return _progress; }
            set { this.RaiseAndSetIfChanged(ref _progress, value); }
        }

        public ReactiveList<ArticleListItemViewModel> Articles { get; } = new ReactiveList<ArticleListItemViewModel>();

        public ArticlesListViewModel()
        {
            Refresh = ReactiveCommand.CreateFromTask(Load);
            SaveChanges = ReactiveCommand.CreateFromTask(Save);
        }

        public async Task Save()
        {
            await App.Repository.PushAll(ModelSource.Server);
        }

        public async Task<bool> Load()
        {
            var tcs = new TaskCompletionSource<bool>();

            App.Repository.Get(
                ModelLoader.GetBbcArticlesIdentifier(),
                SourcePreference.Server)
                    .ObserveOn(SynchronizationContext.Current)
                    .SubscribeStateChange(
                        onProgress: progress => Progress = progress,
                        onResult: result => Articles.AddRange(result.Select(article => new ArticleListItemViewModel(article, this))),
                        onCompleted: s => tcs.SetResult(!s.IsCancelled)
                    );

            return await tcs.Task;
        }
    }

    class ArticleListItemViewModel : ReactiveObject
    {
        public string Title
        {
            get { return _model.Title; }
            set
            {
                var proxy = ModelLoader.GetIdentifier(_model) as BbcArticleIdentifier;
                
                App.Repository.Commit(
                    proxy.SetTitle(
                        value,
                        _updateToken
                    )
                );
            }
        }

        private readonly NewsArticle _model;
        private readonly object _updateToken;

        public ArticleListItemViewModel(NewsArticle model, object updateToken)
        {
            _model = model;
            _updateToken = updateToken;
        }

    }
}
