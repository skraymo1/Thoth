// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;

namespace ClientApp.Services;
public class ChatHistoryServiceUI
{
    private const string CHAT_HISTORY_SESSIONS_KEY = "chatHistorySessions";

    // todo: save across browser sessions
    private readonly Dictionary<int, ChatHistorySessionUI> _chatHistorySessions = new();

    public event Action OnChange;
    public List<EventCallback> listeners { get; private set; } = new List<EventCallback>();


    private void NotifyStateChanged() => OnChange?.Invoke();

    public ChatHistorySessionUI AddChatHistorySession(Dictionary<UserQuestion, ChatAppResponseOrError?> questionAnswerMap)
    {
        var id = _chatHistorySessions.Keys.Any() ? _chatHistorySessions.Keys.Max() + 1 : 1;
        // todo: generate sessionName, sessionStartTime, sessionEndTime
        var sessionName = $"Session {id}";
        var chatHistorySessionUI = new ChatHistorySessionUI(id, sessionName, DateTime.Now, DateTime.Now, questionAnswerMap);
        _chatHistorySessions.Add(id, chatHistorySessionUI);
        NotifyStateChanged();
        return chatHistorySessionUI;
    }

    public bool TryGetChatHistorySession(int id, out ChatHistorySessionUI chatHistorySessionUI)
    {
        return _chatHistorySessions.TryGetValue(id, out chatHistorySessionUI);
    }

    public IEnumerable<ChatHistorySessionUI> GetChatHistorySessions()
    {
        return _chatHistorySessions.Values;
    }

    public void DeleteChatHistorySession(int id)
    {
        _chatHistorySessions.Remove(id);
        NotifyStateChanged();
    }
}
