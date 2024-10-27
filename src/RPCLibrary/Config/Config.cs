

namespace RPCLibrary.Config
{
    public record ServerParms
    {
        public string DownloadFolder { get; set; }
        public string SharedFolder { get; set; }
        public bool ShowScreenContentOnServer { get; set; }
        public string ApiKey { get; set; }
        public int MaxTokens { get; set; }
    }
}
