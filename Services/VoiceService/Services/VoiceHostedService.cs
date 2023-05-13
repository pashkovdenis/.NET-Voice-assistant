using MediatR;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Hosting;
using Pv;
using VoiceAssistant.Models;
using VoiceAssistant.UseCase.GetAnswer;

namespace VoiceService.Services
{
    public sealed class VoiceHostedService : IHostedService
    {     
        private readonly Settings _settings;
        private readonly IMediator _mediator;

        public VoiceHostedService(Settings settings, IMediator mediator)
        {
            _settings = settings;
            _mediator = mediator;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () => { await KeySpottingStartAsync(); }, TaskCreationOptions.LongRunning);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        { 
            return Task.CompletedTask;
        }
         
        private async Task KeySpottingStartAsync()
        {
            var apiKey = _settings.VoiceApiKey;

            using (Porcupine porcupine = Porcupine.FromBuiltInKeywords(apiKey, new List<BuiltInKeyword> { BuiltInKeyword.ALEXA }))
            {
                using PvRecorder recorder = PvRecorder.Create(deviceIndex: -1, frameLength: porcupine.FrameLength);
                
                Console.WriteLine($"Using device: {recorder.SelectedDevice}"); 

                recorder.Start();

                while (true)
                {
                    short[] pcm = recorder.Read();
                    int result = porcupine.Process(pcm);

                    if (result >= 0)
                    {
                        Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Detected  ");

                        Console.Beep();

                        recorder.Stop();

                        await StartListeningSentenceAsync();

                        break;
                    }

                    Thread.Yield();
                }
            }
        }

        private async Task StartListeningSentenceAsync()
        {
            var speechConfig = SpeechConfig.FromSubscription(_settings.CognetiveKey, _settings.CognetiveRegion);
            
            using var cognitiveRecognizer = new SpeechRecognizer(speechConfig, "en-us");

            var result = await cognitiveRecognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                var inputText = result.Text;

                var request = new GetAnswerRequest
                {
                     Input = inputText,
                     Client = Guid.Parse(_settings.DefaultUserName)
                }; 

                var response = await _mediator.Send(request);

                speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";

                using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
                {
                    await speechSynthesizer.SpeakTextAsync(response.Response);
                }
            }

            await KeySpottingStartAsync();
        } 
    }
}
