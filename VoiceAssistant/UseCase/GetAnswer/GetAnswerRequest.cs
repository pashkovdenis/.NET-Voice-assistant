using MediatR;

namespace VoiceAssistant.UseCase.GetAnswer
{
    public sealed class GetAnswerRequest : IRequest<GetAnswerResponse>
    {  
        public string Input { get; set; }   

        public Guid Client { get; set; }

    }
}
