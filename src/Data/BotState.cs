using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.State;


public static class StateManager
{
    private static Dictionary<long, UserState> State = new();
    public static Dictionary<long, PanelInfo> Data = new();

    public static UserState GetState(long userId)
    {
        if (State.TryGetValue(userId, out var state))
        {
            return state;
        }
        return UserState.Default;
    }

    public static void SetState(long userId, UserState state)
    {
        State[userId] = state;

    }

    public static void Initialise(long userId)
    {
        Data[userId] = new PanelInfo();
    }

    
}


public enum UserState
{
    Default, Url, AdminSecret, SharedSecret, UploadReady, UploadUserId
}

#pragma warning disable CS8618
public class PanelInfo
{
    public string Url { get; set; }
    public string SharedSecret { get; set; }
    public string AdminSecret { get; set; }

}

#pragma warning restore CS8618
