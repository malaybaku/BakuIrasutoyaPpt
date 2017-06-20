using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace Baku.IrasutoyaPpt
{
    /// <summary>いらすとやからスクレイピングで画像拾ってくるための処理とか</summary>
    public class IrasutoyaSearchResponse
    {
        internal static string BaseUrl { get; } = "http://www.irasutoya.com/search?q=";

        //internal static string ThumbnailXpath = @"//div[@class=""boxim""]/a";

        /// <summary>検索結果ページにおける、サムネ要素(0個以上)へのXPath</summary>
        internal static string ThumbnailXpath = @"//div[@class=""date-outer""]//div[@class=""boxim""]/a";

        /// <summary>検索結果ページにおける「次のページ」(0個または1個)へのXPath</summary>
        internal static string NextPageXPath = @"//a[@class=""blog-pager-older-link""]";

        /// <summary>検索結果ページにおける「前のページ」(0個または1個)へのXPath</summary>
        internal static string PrevPageXPath = @"//a[@class=""blog-pager-newer-link""]";

        /// <summary>検索結果ページで、サムネ要素の中にあるサムネイルURLを取得するための正規表現文字列</summary>
        internal static string ThumbnailPathRegex = @"\(bp_thumbnail_resize\(\""([^\""]+)\""";

        /// <summary>検索結果ページで、サムネ要素の下にあるサムネイルテキストを取得するための正規表現文字列</summary>
        internal static string ThumbnailTextRegex = @"\"",\""([^\""]+)\""\)\);";

        /// <summary>個別のイラストページにおける、メイン画像要素へのXPath</summary>
        internal static string MainIllustXPath = @"//div[@class=""separator""]/a";


        public IrasutoyaSearchResponse(HtmlDocument doc, bool hasValidResult)
        {
            _doc = doc;
            HasValidResult = hasValidResult;
        }

        private readonly HtmlDocument _doc;
        public bool HasValidResult { get; }

        private bool? _hasNextPage = null;
        public bool HasNextPage
        {
            get
            {
                if (_hasNextPage == null)
                {
                    _hasNextPage = !string.IsNullOrEmpty(NextPageUrl);
                }
                return _hasNextPage.Value;
            }
        }

        private bool? _hasPrevPage = null;
        public bool HasPrevPage
        {
            get
            {
                if (_hasPrevPage == null)
                {
                    _hasPrevPage = !string.IsNullOrEmpty(PrevPageUrl);
                }
                return _hasPrevPage.Value;
            }
        }

        private string _nextPageUrl = null;
        public string NextPageUrl
        {
            get
            {
                if (_nextPageUrl == null)
                {
                    _nextPageUrl = HasValidResult ?
                        (_doc?.DocumentNode
                            ?.SelectSingleNode(NextPageXPath)
                            ?.GetAttributeValue("href", "") 
                            ?? "") :
                        "";
                }
                return _nextPageUrl;
            }
        }

        private string _prevPageUrl = null;
        public string PrevPageUrl
        {
            get
            {
                if (_prevPageUrl == null)
                {
                    _prevPageUrl = HasValidResult ?
                        (_doc?.DocumentNode
                            ?.SelectSingleNode(PrevPageXPath)
                            ?.GetAttributeValue("href", "") 
                            ?? "") :
                        "";
                }
                return _prevPageUrl;
            }
        }


        //適当なサニタイズ
        private static string GetRestricted(string rawKeyword)
        {
            //クエリに変なこと書かないで欲しいだけなので適当で
            return rawKeyword.Replace("&", "");
        }

        private static string GetCharReplaced(string thumbnailText)
        {
            //カッコとかがまともに見えてればいいので、ここも適当に
            return thumbnailText
                .Replace("&#65288;", "(")
                .Replace("&#65289;", ")")
                .Replace("&#12300;", "「")
                .Replace("&#12301;", "」")
                .Replace("&#12539;", "・");
        }

        //とっかかり: キーワード検索によってレスポンスを得る
        public static async Task<IrasutoyaSearchResponse> GetResponse(string keyword)
        {
            string sanitizedKeyword = GetRestricted(keyword);
            string url = BaseUrl + HttpUtility.UrlEncode(sanitizedKeyword);

            return await GetFrom(url);
        }

        /// <summary>ページ遷移っぽい: 「次のページ」があるとき、それを使って移動</summary>
        /// <returns></returns>
        public async Task<IrasutoyaSearchResponse> LoadNextPageAsync()
        {
            if (!HasValidResult || !HasNextPage)
            {
                return this;
            }

            return await GetFrom(NextPageUrl);
        }

        /// <summary>ページ遷移っぽい: 「前のページ」があるとき、それを使って移動</summary>
        /// <returns></returns>
        public async Task<IrasutoyaSearchResponse> LoadPrevPageAsync()
        {
            if (!HasValidResult || !HasPrevPage)
            {
                return this;
            }

            return await GetFrom(PrevPageUrl);            
        }

        private static async Task<IrasutoyaSearchResponse> GetFrom(string url)
        {
            var res = new HtmlDocument();
            try
            {
                string rawHtml = await new WebClient()
                {
                    Encoding = Encoding.UTF8
                }
                    .DownloadStringTaskAsync(url);

                res.LoadHtml(rawHtml);
                return new IrasutoyaSearchResponse(res, true);
            }
            catch (Exception ex)
            {
                ThisAddIn.Instance.ShowErrorMessage(ex.Message);
                return new IrasutoyaSearchResponse(res, false);
            }
        }

        /// <summary>検索結果ページにあるサムネを列挙</summary>
        /// <returns></returns>
        public IEnumerable<IrasutoyaItemViewModel> EnumResults()
        {
            if (!HasValidResult)
            {
                return Enumerable.Empty<IrasutoyaItemViewModel>();
            }

            try
            {
                return _doc
                    .DocumentNode
                    .SelectNodes(ThumbnailXpath)
                    ?.Select(node =>
                    {
                        string targetUrl = node.GetAttributeValue("href", "");
                        //ダメな方法: JSを実行しないとimg要素が作られないのでDownloadStringと相性悪い
                        //string thumbnailUrl = node.ChildNodes["img"].GetAttributeValue("src", "");
                        //string thumbnailText = node.ChildNodes["img"].GetAttributeValue("alt", "");
                        var thumbnailUrlMatch = Regex.Match(node.ChildNodes[0].InnerText, ThumbnailPathRegex);
                        string thumbnailUrl = thumbnailUrlMatch.Groups[1].Value;

                        var thumbnailTextMatch = Regex.Match(node.ChildNodes[0].InnerText, ThumbnailTextRegex);
                        string thumbnailText = GetCharReplaced(
                            thumbnailTextMatch.Groups[1].Value
                            );

                        return new IrasutoyaItemViewModel(
                            thumbnailUrl,
                            thumbnailText,
                            targetUrl
                            );
                    }) ??
                    Enumerable.Empty<IrasutoyaItemViewModel>();
            }
            catch (Exception ex)
            {
                ThisAddIn.Instance.ShowErrorMessage(ex.Message);
                return Enumerable.Empty<IrasutoyaItemViewModel>();
            }
        }

        /// <summary>
        /// 個別のイラスト紹介ページで、そのページ内部にある画像へのURLを列挙
        /// NOTE: いらすとによっては1つの紹介ページに複数枚あるのでIEnumerableになっている
        /// </summary>
        /// <param name="targetUrl"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<string>> GetImageUrlFromTargetUrl(string targetUrl)
        {
            try
            {
                string res = await new WebClient().DownloadStringTaskAsync(targetUrl);

                var doc = new HtmlDocument();
                doc.LoadHtml(res);

                return doc
                    .DocumentNode
                    .SelectNodes(MainIllustXPath)
                    ?.Select(node => node.GetAttributeValue("href", ""))
                    ?? Enumerable.Empty<string>();
            }
            catch(Exception ex)
            {
                ThisAddIn.Instance.ShowErrorMessage(ex.Message);
                return Enumerable.Empty<string>();
            }
        }

    }
}
