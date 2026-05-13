using System;
using System.Collections.Generic;

namespace MFPS.Runtime.AI
{
    [Serializable]
    public enum BotGameState : byte
    {
        Playing = 0,
        Death,
        Replaced,
        WaitingNextRound,
    }

    [Serializable]
    public class MFPSBotProperties
    {
        public string Name;
        public BotGameState GameState;
        public int Kills;
        public int Deaths;
        public int Assists;
        public int Score;
        public Team Team;
        public int ViewID;
    }

    public class AIStateExecutionRecord
    {
        private readonly Dictionary<string, (int lastFrame, int followedCount)> calls = new();

        public int AddCall(string methodName, int frame, int nextFrameOffset, int targetTrigger = -1)
        {
            //if (methodName == "18") UnityEngine.Debug.Log($"{frame}");
            if (calls.ContainsKey(methodName))
            {
                var data = calls[methodName];
                // if the method was called in the last frame
                if ((frame + nextFrameOffset) - data.lastFrame <= nextFrameOffset)
                {
                    data.followedCount += 1;
                    calls[methodName] = (frame + nextFrameOffset, data.followedCount);
                    return targetTrigger == -1 ? 1 : data.followedCount >= targetTrigger ? 2 : 1;
                }
                else
                {
                    // the method was not called in the last frame
                    calls[methodName] = (frame + nextFrameOffset, 0);
                    return -1;
                }
            }
            else
            {
                calls.Add(methodName, (frame, 0));
                return 0;
            }
        }
    }
}