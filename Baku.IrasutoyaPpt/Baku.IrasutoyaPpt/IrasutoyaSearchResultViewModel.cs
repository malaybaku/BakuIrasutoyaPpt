using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;

namespace Baku.IrasutoyaPpt
{
    public class IrasutoyaItemViewModel : ViewModelBase
    {
        public IrasutoyaItemViewModel(string thumbnailUrl, string thumbnailText, string targetUrl)
        {
            ThumbnailUrl = thumbnailUrl;
            ThumbnailText = thumbnailText;
            TargetUrl = targetUrl;

            AddToSlideCommand = new ActionCommand(async () =>
            {
                try
                {
                    IEnumerable<string> urls = await IrasutoyaSearchResponse.GetImageUrlFromTargetUrl(TargetUrl);
                    foreach(var url in urls)
                    {
                        var imgBin = await new WebClient().DownloadDataTaskAsync(url);
                        ThisAddIn.Instance.AddImageToCurrentSlide(imgBin);
                    }
                }
                catch(Exception ex)
                {
                    ThisAddIn.Instance.ShowErrorMessage(ex.Message);
                }
            });
        }

        public string ThumbnailUrl { get; }
        public string ThumbnailText { get; }
        public string TargetUrl { get; }

        public ICommand AddToSlideCommand { get; }
    }
}
