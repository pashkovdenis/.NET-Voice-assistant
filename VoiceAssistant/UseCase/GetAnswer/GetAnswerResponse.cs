namespace VoiceAssistant.UseCase.GetAnswer
{
    public sealed class GetAnswerResponse
    {
        public string Response { get; internal set; }
        public bool IsLocal { get; internal set; }
    }
}
