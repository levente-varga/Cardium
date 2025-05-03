using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards.Types;

namespace Cardium.Scripts;

public enum Level {
  Lobby,
  One,
}

public enum Difficulty {
  Easy,
  Moderate,
  Hard,
  Brutal,
}

public static class Data {
  public static Level Level;
  public static Difficulty Difficulty = Difficulty.Easy;
  public static bool InitialStart = true;
  public static bool InitialMenuOpen = true;
  public static int InitialCardPlaysLeft = 1;
  public static bool Fog;
  public static bool Hand;
  public static bool ShowHealth;
  public static bool CameraOnPlayer;
  public static int MenusOpen;
  public static bool PauseMenuOpen;
  public static bool MenuOpen => MenusOpen > 0 || PauseMenuOpen;
  public static readonly Pile Stash = new ();
  public static readonly Pile Inventory = new ();
  public static readonly Pile Deck = new () { Capacity = 20 };

  public static int TotalCardCount => Stash.Size + Inventory.Size + Deck.Size;
  public static List<Card> GetAllCards() => Stash.Cards
    .Union(Inventory.Cards)
    .Union(Deck.Cards)
    .ToList();

  public static void EraseUnprotectedCardsOutsideStash() {
    Inventory.RemoveUnprotected();
    Deck.RemoveUnprotected();
  }
  
  public static int UnprotectedCardCount => Inventory.UnprotectedCount + Deck.UnprotectedCount;

  public static void LoadLobbyData() {
    Level = Level.Lobby;
    Fog = false;
    Hand = false;
    ShowHealth = false;
    CameraOnPlayer = false;
    MenusOpen = 0;
  }

  public static void LoadDungeonData() {
    Level = Level.One;
    Fog = true;
    Hand = true;
    ShowHealth = true;
    CameraOnPlayer = true;
    MenusOpen = 0;
  }
}