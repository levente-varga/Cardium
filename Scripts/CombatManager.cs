using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public class CombatManager
{
    private readonly World _world;
    private readonly Player _player;
    private readonly List<Enemy> _enemiesInCombat = new();
    private readonly Label _debugLabel;
    
    private readonly List<Entity> _turnOrder = new();
    private Entity _currentTurnEntity;

    public CombatManager(Player player, World world, Label debugLabel)
    {
        _player = player;
        _world = world;
        _debugLabel = debugLabel;
        _player.OnTurnFinishedEvent += OnTurnFinished;

        UpdateDebugLabel();
    }
    
    public void EnterCombat(Enemy enemy)
    {
        if (_enemiesInCombat.Count == 0)
        {
            StartCombat();
        }
        
        if (!_enemiesInCombat.Contains(enemy))
        {
            _enemiesInCombat.Add(enemy);
            _turnOrder.Add(enemy);
            enemy.OnTurnFinishedEvent += OnTurnFinished;
            enemy.OnLeaveCombatEvent += OnLeaveCombat;
        }

        UpdateDebugLabel();
    }

    private void OnTurnFinished(Entity entity)
    {
        if (entity != _currentTurnEntity)
        {
            GD.Print("[ANOMALY] " + entity.Name + " finished their turn out of order.");
            return;
        }
        GD.Print(entity.Name + " finished their turn.");
        
        _turnOrder.Remove(entity);
        _turnOrder.Add(entity);
        _currentTurnEntity = _turnOrder[0];
        _currentTurnEntity.OnTurn(_player, _world);

        UpdateDebugLabel();
    }
    
    private void OnLeaveCombat(Entity entity)
    {
        _turnOrder.Remove(entity);
        _enemiesInCombat.Remove(entity as Enemy);
        entity.OnTurnFinishedEvent -= OnTurnFinished;

        if (_enemiesInCombat.Count == 0)
        {
            EndCombat();
            return;
        }
        
        if (entity != _currentTurnEntity) return;
        
        _currentTurnEntity = _turnOrder[0];
        _currentTurnEntity.OnTurn(_player, _world);

        UpdateDebugLabel();
    }
    
    private void UpdateDebugLabel()
    {
        _debugLabel.Text = "Turn order: \n"
            + "Current turn: " + _currentTurnEntity?.Name
            + "\n\nTurn order:";
        foreach (var entity in _turnOrder)
        {
            _debugLabel.Text += "\n" + entity.Name;
        }
        _debugLabel.Text += "\n\nEnemies in combat: " + _enemiesInCombat.Count;
        foreach (var entity in _enemiesInCombat)
        {
            _debugLabel.Text += "\n" + entity.Name;
        }
    }

    private void StartCombat()
    {
        _turnOrder.Add(_player);
        _currentTurnEntity = _player;
        _player.OnTurn(_player, _world);
    }

    private void EndCombat()
    {
        _turnOrder.Remove(_player);
        _player.OnCombatEnd();
    }
}