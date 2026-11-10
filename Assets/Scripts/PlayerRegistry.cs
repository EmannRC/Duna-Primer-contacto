using System.Collections.Generic;
using UnityEngine;

public static class PlayerRegistry
{
    private static readonly HashSet<PlayerContext> players = new();

    public static IReadOnlyCollection<PlayerContext> Players => players;

    public static void Register(PlayerContext player)
    {
        if (player != null)
            players.Add(player);
    }

    public static void Unregister(PlayerContext player)
    {
        if (player != null)
            players.Remove(player);
    }
}
