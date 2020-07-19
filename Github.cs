using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ElectricShimmer
{
    class Github
    {
        private string _repo;

        public Github(string repo)
        {
            _repo = repo;
        }

        public List<string> ReleaseNames
        {
            get
            {
                if (_releases == null)
                    _releases = GetReleasesList();

                return _releases.Select(x => x.name).ToList();
            }
        }

        public GithubObjects.Releases GetReleasebyVersion(string version = "latest")
        {
            try
            {
                if (_releases == null)
                    _releases = GetReleasesList();

                return version == "latest" ? _releases[0] : _releases.Where(x => x.tag_name == version).ToList()[0];
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
                return null;
            }
        }

        private List<GithubObjects.Releases> _releases;
        public string Get(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = "ElectricShimmer";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
                return null;
            }
        }

        private List<GithubObjects.Releases> GetReleasesList()
        {
            return JsonConvert.DeserializeObject<List<GithubObjects.Releases>>(Get(_repo));
        }
    }

    class GithubObjects
    {
        public class Releases
        {
            public string url { get; set; }
            public string html_url { get; set; }
            public string assets_url { get; set; }
            public string upload_url { get; set; }
            public string tarball_url { get; set; }
            public string zipball_url { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string tag_name { get; set; }
            public string target_commitish { get; set; }
            public string name { get; set; }
            public string body { get; set; }
            public bool draft { get; set; }
            public bool prerelease { get; set; }
            public DateTime created_at { get; set; }
            public DateTime published_at { get; set; }
            public Author author { get; set; }
            public List<Assets> assets { get; set; }

            public class Author
            {
                public string login { get; set; }
                public int id { get; set; }
                public string node_id { get; set; }
                public string avatar_url { get; set; }
                public string gravatar_id { get; set; }
                public string url { get; set; }
                public string html_url { get; set; }
                public string followers_url { get; set; }
                public string following_url { get; set; }
                public string gists_url { get; set; }
                public string starred_url { get; set; }
                public string subscriptions_url { get; set; }
                public string organizations_url { get; set; }
                public string repos_url { get; set; }
                public string events_url { get; set; }
                public string received_events_url { get; set; }
                public string type { get; set; }
                public bool site_admin { get; set; }
            }

            public class Assets
            {
                public string url { get; set; }
                public string browser_download_url { get; set; }
                public int id { get; set; }
                public string node_id { get; set; }
                public string name { get; set; }
                public string label { get; set; }
                public string state { get; set; }
                public string content_type { get; set; }
                public int size { get; set; }
                public int download_count { get; set; }
                public DateTime created_at { get; set; }
                public DateTime updated_at { get; set; }
                public Uploader uploader { get; set; }

                public class Uploader
                {
                    public string login { get; set; }
                    public int id { get; set; }
                    public string node_id { get; set; }
                    public string avatar_url { get; set; }
                    public string gravatar_id { get; set; }
                    public string url { get; set; }
                    public string html_url { get; set; }
                    public string followers_url { get; set; }
                    public string following_url { get; set; }
                    public string gists_url { get; set; }
                    public string starred_url { get; set; }
                    public string subscriptions_url { get; set; }
                    public string organizations_url { get; set; }
                    public string repos_url { get; set; }
                    public string events_url { get; set; }
                    public string received_events_url { get; set; }
                    public string type { get; set; }
                    public bool site_admin { get; set; }

                }
            }
        }
    }
}
