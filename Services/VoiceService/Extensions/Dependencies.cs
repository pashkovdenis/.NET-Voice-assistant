using AuraSearch.Abstractions;
using AuraSearch.Comparers;
using AuraSearch.UseCases.Index.Request;
using AuraSearch.UseCases.Index;
using AuraSearch.UseCases.Reinforce.Request;
using AuraSearch.UseCases.Reinforce;
using AuraSearch.UseCases.Search;
using AuraSearch.UseCases.Search.Request;
using AuraSearch.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using VoiceAssistant.Infrastructure.Output;
using VoiceAssistant.Infrastructure.Repositories;
using VoiceAssistant.Models;
using AuraSearch.Domain.Events;
using AuraSearch.EventHandlers.Handlers;
using Microsoft.ML.Models.BERT;
using System.Reflection;
using MediatR;
using VoiceAssistant.Behaviour;
using System;
using VoiceAssistant.Infrastructure.Events;
using AuraSearch.UseCases.Index.Filter;
using InputTokenizerFilter;

namespace VoiceService.Extensions
{
    public static class Dependencies
    {
        public static void RegisterVoiceServices(this IServiceCollection services, IConfiguration configuration)
        {
            ConventionRegistry.Register("Camel Case", new ConventionPack { new IgnoreExtraElementsConvention(true) }, _ => true);
            
            services.AddScoped(_ => new MongoClient(configuration.GetConnectionString("MongoDb")).GetDatabase(configuration.GetValue<string>("Catalog")));

            var settings = AddConfigurationServices(configuration);
            
            services.AddSingleton(settings);
                         
            services.AddTransient<IWordRepository, WordRepository>();
            services.AddTransient<IThoughtRepository, ThoughtRepository>();
            services.AddTransient<ISymbolComparer, DefaultSymbolComparer>();
            services.AddTransient<IStringCompareAlgorithm, StringMatcher>();

            services.AddSingleton<IContextAccessor, ContextAccessor>();

            services.AddTransient<IContextRepository, ClientRepository>(); 

            services.AddScoped<ISearchOutput, SearchOutput>();

            services.AddTransient<IUseCase<StoreIndexRequest>, CreateIndexUseCase>();
            services.AddTransient<IUseCase<ReinforceRequest>, ReinforceUseCase>();
            services.AddTransient<IUseCase<SearchRequest>, SearchUseCase>();
             
            services.AddTransient<IInputFilter<ReinforceRequest>, ReinforceTokenizerFilter>();
            services.AddTransient<IInputFilter<StoreIndexRequest>, TokenizerInputFilter>();
            services.AddTransient<IInputFilter<SearchRequest>, TokenizerSearchRequestFilter>();
            services.AddTransient<IInputFilter<StoreIndexRequest>, ProduceSymbolsFilter>();

            services.AddTransient(typeof(IEventHandler<RefreshEvent>), typeof(RefreshEventHandler));
            services.AddTransient(typeof(IEventHandler<AddToContextEvent>), typeof(AddToContextHandler));
            services.AddTransient(typeof(IEventHandler<BoostOldThoughtEvent>), typeof(BoostOldThoughtHandler));
             
            services.AddSingleton((_) =>
            {
                var modelConfig = new BertModelConfiguration()
                {
                    VocabularyFile = "Data/vocab.txt",
                    ModelPath = "Data/bertsquad-10.onnx"
                };

                var model = new BertModel(modelConfig);
                
                model.Initialize(); 

                return model;
            });

            // register mediator. 
            services.AddMediatR(cfg => {

                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>))
                    .AddBehavior(typeof(IPipelineBehavior<,>), typeof(BertBehaviour<,>));
                 
            });
             

            new DefaultEventDispatcher(services.BuildServiceProvider());

        }

        internal static Settings AddConfigurationServices(IConfiguration configuration)
        {
            var settings = new Settings();
            configuration.GetSection("Settings").Bind(settings);
            return settings;
        }
    }
}
