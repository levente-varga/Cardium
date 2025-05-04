using System;
using System.Collections.Generic;
using System.Linq;
using Cardium.Scripts.Cards;
using Godot;
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
  public static bool LoadedSaveData = false;
  public static bool FirstStart = true;
  public static bool InitialCardPlay = true;
  public static bool LastRunFinished = true;
  public static bool FoundSaveData = false;
  public static bool SetupRan = false;
  public static DateTime LastSaveTime = DateTime.Now;
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

  
  
  
  private static readonly string SavePath = "user://save.json";

  public static void Save() {
    var stashArray = new Godot.Collections.Array();
    var inventoryArray = new Godot.Collections.Array();
    var deckArray = new Godot.Collections.Array();
    foreach (var card in Stash.Cards) stashArray.Add(card.Save());
    foreach (var card in Inventory.Cards) inventoryArray.Add(card.Save());
    foreach (var card in Deck.Cards) deckArray.Add(card.Save());
    
    var save = new Godot.Collections.Dictionary {
      {"statistics", new Godot.Collections.Dictionary {
        {"runs", Statistics.Runs},
        {"deaths", Statistics.Deaths},
        {"enemies_killed", Statistics.EnemiesKilled},
        {"steps", Statistics.Steps},
        {"nudges", Statistics.Nudges},
        {"actions_taken", Statistics.ActionsTaken},
        {"bonfires_lit", Statistics.BonfiresLit},
        {"chests_opened", Statistics.ChestsOpened},
        {"doors_opened", Statistics.DoorsOpened},
        {"cards_collected", Statistics.CardsCollected},
        {"cards_lost", Statistics.CardsLost},
        {"cards_upgraded", Statistics.CardsUpgraded},
        {"cards_played", Statistics.CardsPlayed},
        {"total_damage", Statistics.TotalDamage},
        {"total_damage_taken", Statistics.TotalDamageTaken},
        {"total_heal_amount", Statistics.TotalHealAmount},
        {"minutes_played", Utils.GetMinutesBetween(Statistics.StartTime, DateTime.Now)}
      }},
      {"difficulty", (int)Difficulty},
      {"last_run_finished", LastRunFinished},
      {"first_start", FirstStart},
      {"stash", stashArray},
      {"inventory", inventoryArray},
      {"deck", deckArray},
    };
    
    var jsonString = Json.Stringify(save);

    var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
    file.StoreLine(jsonString);
    file.Close();
    
    GD.Print($"Game data has been saved to {file.GetPathAbsolute()}");
    LastSaveTime = DateTime.Now;
  }

  public static void Load() {
    LoadedSaveData = true;
    
    if (!FileAccess.FileExists(SavePath)) return;

    var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
    var jsonString = file.GetAsText();
    file.Close();

    var json = new Json();
    var result = json.Parse(jsonString);

    if (result != Error.Ok) {
      GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
      return;
    }
    
    var save = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
    
    var stats = (Godot.Collections.Dictionary)save["statistics"];
    Statistics.Runs = (int)stats.GetValueOrDefault("runs", 0);
    Statistics.Deaths = (int)stats.GetValueOrDefault("deaths", 0);
    Statistics.EnemiesKilled = (int)stats.GetValueOrDefault("enemies_killed", 0);
    Statistics.Steps = (int)stats.GetValueOrDefault("steps", 0);
    Statistics.Nudges = (int)stats.GetValueOrDefault("nudges", 0);
    Statistics.ActionsTaken = (int)stats.GetValueOrDefault("actions_taken", 0);
    Statistics.BonfiresLit = (int)stats.GetValueOrDefault("bonfires_lit", 0);
    Statistics.ChestsOpened = (int)stats.GetValueOrDefault("chests_opened", 0);
    Statistics.DoorsOpened = (int)stats.GetValueOrDefault("doors_opened", 0);
    Statistics.CardsCollected = (int)stats.GetValueOrDefault("cards_collected", 0);
    Statistics.CardsLost = (int)stats.GetValueOrDefault("cards_lost", 0);
    Statistics.CardsUpgraded = (int)stats.GetValueOrDefault("cards_upgraded", 0);
    Statistics.CardsPlayed = (int)stats.GetValueOrDefault("cards_played", 0);
    Statistics.TotalDamage = (int)stats.GetValueOrDefault("total_damage", 0);
    Statistics.TotalDamageTaken = (int)stats.GetValueOrDefault("total_damage_taken", 0);
    Statistics.TotalHealAmount = (int)stats.GetValueOrDefault("total_heal_amount", 0);
    Statistics.StartTime = DateTime.Now.AddMinutes(-(int)stats.GetValueOrDefault("minutes_played", 0));

    Difficulty = (Difficulty)(int)save.GetValueOrDefault("difficulty", 0);
    LastRunFinished = (bool)save.GetValueOrDefault("last_run_finished", true);
    FirstStart = (bool)save.GetValueOrDefault("first_start", true);
    
    Stash.Clear();
    Inventory.Clear();
    Deck.Clear();

    if (save.TryGetValue("stash", out var stashArray)) {
      foreach (var cardData in (Godot.Collections.Array)stashArray)
        Stash.Cards.Add(CardFromDict((Godot.Collections.Dictionary)cardData, Card.Origins.Stash));
    }

    if (save.TryGetValue("inventory", out var inventoryArray)) {
      foreach (var cardData in (Godot.Collections.Array)inventoryArray)
        Inventory.Cards.Add(CardFromDict((Godot.Collections.Dictionary)cardData, Card.Origins.Inventory));
    }

    if (save.TryGetValue("deck", out var deckArray)) {
      foreach (var cardData in (Godot.Collections.Array)deckArray)
        Deck.Cards.Add(CardFromDict((Godot.Collections.Dictionary)cardData, Card.Origins.Deck));
    }

    FoundSaveData = true;
  }
  
  private static Card CardFromDict(Godot.Collections.Dictionary dict, Card.Origins origin) {
    var card = CardFactory.Create(dict["name"].ToString());
    var level = (int)dict["level"];
    for (var i = 0; i < level; i++) card.Upgrade();
    card.Protected = (bool)dict["protected"];
    card.Origin = origin;
    return card;
  }
}