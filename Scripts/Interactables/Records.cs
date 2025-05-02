using System;
using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts.Interactables;

public partial class Records : Interactable {
  private int _index = -1;
  private const int MaxIndex = 17;
  private List<int> _order = new ();

  public override void _Ready() {
    base._Ready();

    SetStillFrame("idle", ResourceLoader.Load<Texture2D>("res://Assets/Sprites/Records.png"));
  }

  public override void OnNudge(Player player, World world) {
    base.OnNudge(player, world);

    world.Camera.Shake(10);
    SpawnFloatingLabel(GetText(), height: Global.GlobalTileSize.Y * 3);
  }

  private string GetText() {
    _index++;

    switch (_index) {
      case 0: {
        _order = new List<int>();
        for (var i = 0; i < MaxIndex; i++) _order.Add(i);
        Utils.FisherYatesShuffle(_order);
        break;
      }
      case >= MaxIndex:
        _index = 0;
        break;
    }

    return _order[_index] switch {
      0 => $"Started {Statistics.Runs} runs",
      1 => $"{Statistics.Deaths} deaths",
      2 => $"Killed {Statistics.EnemiesKilled} enemies",
      3 => $"Collected {Statistics.CardsCollected} cards",
      4 => $"Lost {Statistics.CardsLost} cards",
      5 => $"Upgraded {Statistics.CardsUpgraded} cards",
      6 => $"Taken {Statistics.Steps} steps",
      7 => $"Opened {Statistics.DoorsOpened} doors",
      8 => $"Opened {Statistics.ChestsOpened} chests",
      9 => $"Taken {Statistics.ActionsTaken} actions",
      10 => $"Lit {Statistics.BonfiresLit} bonfires",
      11 => $"Played {Statistics.CardsPlayed} cards",
      12 => $"Healed {Statistics.TotalHealAmount} health",
      13 => $"Dealt {Statistics.TotalDamage} damage",
      14 => $"Received {Statistics.TotalDamageTaken} damage",
      15 => $"Nudged {Statistics.Nudges} times",
      16 => $"Playing for {(int)(DateTime.Now - Statistics.StartTime).TotalMinutes} minutes",
      _ => "?",
    };
  }
}