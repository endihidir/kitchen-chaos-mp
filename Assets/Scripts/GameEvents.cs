using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class GameEvents
{
    public static Action<IInteractable<BaseCounter>> OnSelectedCounterChanged;

    public static Action<RecipeSO> OnSpawnRecipe, OnDeliverRecipe;

    public static Action<GamePlayState> OnGameStateChanged;

    public static Action<float> OnCountDownUpdate, OnGetGameTimer;

    public static Action OnGamePause;

    public static Action<bool> OnMultiplayerGamePaused, OnLocalGamePaused;

    public static Action OnTryingToJoinGame, OnFailedToJoinGame;

    public static Action OnPlayerReadyChanged, OnPlayerDataNetworkListChanged, OnPlayerReadyToPlay;

    public static Action<bool> OnEnableLobbyFadePanel;

    public static Action<List<Lobby>> OnLobbyListChanged;

    public static Action OnCreateLobbyStarted, OnCreateLobbyFailed, OnJoinStarted, OnQuickJoinFailed, OnJoinFailed;
}