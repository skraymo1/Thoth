// Copyright (c) Microsoft. All rights reserved.
#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0011 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using Azure.Core;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Newtonsoft.Json;

public class ChatHistoryService : IChatHistoryService
{
    private static string s_collection = "chathistory";
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly SemanticTextMemory _textMemory;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly Kernel _kernel;

    public ChatHistoryService(
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        IMemoryStore memoryStore,
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        IConfiguration configuration,
        TokenCredential? tokenCredential = null)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        var embeddingModelName = configuration["AZURE_OPENAI_EMBEDDING_DEPLOYMENT"];

        if (!string.IsNullOrEmpty(embeddingModelName))
        {
            var endpoint = configuration["AZURE_OPENAI_ENDPOINT"];
            ArgumentNullException.ThrowIfNullOrWhiteSpace(endpoint);
            kernelBuilder = kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(embeddingModelName, endpoint, tokenCredential ?? new DefaultAzureCredential());
        }

        _kernel = kernelBuilder.Build();
        // The combination of the text embedding generator and the memory store makes up the 'SemanticTextMemory' object used to
        // store and retrieve memories.
        _textMemory = new(memoryStore, _kernel.GetRequiredService<AzureOpenAITextEmbeddingGenerationService>());
    }

    public async Task AddChatHistorySession(ChatHistorySession chatHistory)
    {
        //await EnsureCollectionAsync();
        var jsonHistory = JsonConvert.SerializeObject(chatHistory.ChatHistory);
        await _textMemory.SaveInformationAsync(s_collection, chatHistory.SessionId.ToString(), jsonHistory);
    }

    public async Task DeleteChatHistorySession(string sessionId)
    {
        await _textMemory.RemoveAsync(s_collection, sessionId.ToString());
    }

    public async Task<ChatHistorySession?> GetChatHistorySession(string sessionId)
    {
        //await EnsureCollectionAsync();
        var lookup = await _textMemory.GetAsync(s_collection, sessionId.ToString());

        if (lookup == null)
            return null;

        var chatHistory = JsonConvert.DeserializeObject<ChatHistory>(lookup!.Metadata.Text ?? "[]");

        return new() { SessionId = sessionId, ChatHistory = chatHistory ?? [] };
    }

    public Task<IEnumerable<ChatHistorySession>> GetChatHistorySessions()
    {
        throw new NotImplementedException();
    }

    // necessary?
    //private async Task EnsureCollectionAsync()
    //{
    //    if (!await _memoryStore.DoesCollectionExistAsync(s_collection))
    //    {
    //        await _memoryStore.CreateCollectionAsync(s_collection);
    //    }
    //}
}

