namespace VoiceAssistant.Models
{
    public sealed class Settings
    {
        public string VoiceApiKey { get; set; }

        public string CognetiveKey { get; set; }

        public string CognetiveRegion { get; set; }

        public string ChatGptToken { get; set; }

        public string DefaultUserName { get; set; }

        public double SearchThresshold { get; set; }

        public int TimerInterval { get; set; } = 3;
    }
}
