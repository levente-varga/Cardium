using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public class CombatManager
{
    private readonly Player _player;
    private readonly List<Enemy> _enemiesInCombat = new();
    
    private readonly List<Entity> _turnOrder = new();
    
    public CombatManager(Player player) => _player = player;
    private Entity _currentTurnEntity;
    
    public void EnterCombat(Enemy enemy)
    {
        if (_enemiesInCombat.Count == 0)
        {
            _turnOrder.Add(_player);
        }
        
        if (!_enemiesInCombat.Contains(enemy))
        {
            _enemiesInCombat.Add(enemy);
            _turnOrder.Add(enemy);
            enemy.OnTurnFinishedEvent += OnTurnFinished;
            enemy.OnLeaveCombatEvent += OnLeaveCombat;
        }
    }

    private void OnTurnFinished(Entity entity)
    {
        if (entity != _currentTurnEntity)
        {
            GD.Print("[ANOMALY] " + entity.Name + " finished their turn out of order.");
            return;
        }
        
        _turnOrder.Remove(entity);
        _turnOrder.Add(entity);
        _currentTurnEntity = _turnOrder[0];
        _currentTurnEntity.OnTurn(_player);
    }
    
    private void OnLeaveCombat(Entity entity)
    {
        _turnOrder.Remove(entity);
        _enemiesInCombat.Remove(entity as Enemy);

        if (_enemiesInCombat.Count == 0)
        {
            _turnOrder.Remove(_player);
            _player.OnFled();
            return;
        }
        
        if (entity != _currentTurnEntity) return;
        
        _currentTurnEntity = _turnOrder[0];
        _currentTurnEntity.OnTurn(_player);
    }
}