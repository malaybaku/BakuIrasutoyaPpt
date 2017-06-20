using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Baku.IrasutoyaPpt
{
    public class IrasutoyaSearchViewModel : ViewModelBase
    {
        public IrasutoyaSearchViewModel()
        {
            RefreshSearchCommand = new ActionCommand(async () =>
            {
                IsUpdating = true;

                var response = await IrasutoyaSearchResponse.GetResponse(SearchKeyword);
                UpdateResponse(response);

                IsUpdating = false;
            });

            GotoNextPageCommand = new ActionCommand(async () =>
            {
                if (_latestResponse == null) return;

                IsUpdating = true;

                var response = await _latestResponse.LoadNextPageAsync();
                UpdateResponse(response);

                IsUpdating = false;
            });

            GotoPrevPageCommand = new ActionCommand(async () =>
            {
                if (_latestResponse == null) return;

                IsUpdating = true;

                var response = await _latestResponse.LoadPrevPageAsync();
                UpdateResponse(response);

                IsUpdating = false;
            });

            ShowTermsCommand = new ActionCommand(() =>
            {
                new TermInformation().ShowDialog();
            });
        }

        private IrasutoyaSearchResponse _latestResponse = null;
        private void UpdateResponse(IrasutoyaSearchResponse response)
        {
            DispatcherHolder.UIDispatcher.Invoke(() =>
            {
                _latestResponse = response;
                Results.Clear();
                foreach (var result in _latestResponse.EnumResults())
                {
                    Results.Add(result);
                }

                HasNextPage = _latestResponse.HasNextPage;
                HasPrevPage = _latestResponse.HasPrevPage;
            });
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetValue(ref _searchKeyword, value);
        }

        private bool _isUpdating = false;
        public bool IsUpdating
        {
            get => _isUpdating;
            set => SetValue(ref _isUpdating, value);
        }

        private bool _hasNextPage = false;
        public bool HasNextPage
        {
            get => _hasNextPage;
            set => SetValue(ref _hasNextPage, value);
        }

        private bool _hasPrevPage = false;
        public bool HasPrevPage
        {
            get => _hasPrevPage;
            set => SetValue(ref _hasPrevPage, value);
        }

        public ObservableCollection<IrasutoyaItemViewModel> Results { get; }
            = new ObservableCollection<IrasutoyaItemViewModel>();

        public ICommand RefreshSearchCommand { get; }

        public ICommand GotoNextPageCommand { get; }
        public ICommand GotoPrevPageCommand { get; }

        public ICommand ShowTermsCommand { get; }
    }

}
