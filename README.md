# .NET-Voice-assistant powered by chat GPT
.NET Voice assistant implementation using ChatGPT and Cognetive services.

# Config 

appsettings.json 
 
```  

  "ConnectionStrings": {
    "MongoDb": "mongodb://127.0.0.1:27017"
  }
  "Catalog": "data",  --> MongoDb 
  "Settings": { 
                "ChatGptToken": "YOUR chatGpt  key",
                "VoiceApiKey": "FTXVxY7SYdhg5H4G89jFhwzueWAjpV2sfs14zme1KYq6XNXhj13tqA==", --> can be used this one 
                "CognetiveKey": " ",  -- Azure Cognetive key
                "CognetiveRegion": "northeurope",
                "DefaultUserName": "c48f1ca8-324e 4f26-9bb4-23630eac5071", --> for internal session id 
                "SearchThresshold": "0.38" --> Aura lib search thresshold
              }
							
```
           
# Start up  
 
 The default hot key is "ALEXA" after hot key detected beep sound will be played after it you can speak sentence to bot.  
 After sentence will be recognized at first it will try find local answer using aura search lib,  if no answer found requuest will be redirected to the chatGPT 
 

# Optional 

You can download and use BERT ONNX model to extract short answer.   
put model in Data directory


 
