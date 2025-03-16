#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Cardium.Scripts;

public class CombatManager
{
    private readonly World _world;
    private readonly Player _player;
    public readonly List<Enemy> Enemies = new();
    public List<Enemy> EnemiesInCombat => Enemies.Where(e => e.InCombat).ToList();
    private readonly Label _debugLabel;
    
    private readonly List<Entity> _turnOrder = new();
    private Entity? _currentTurnEntity;
    
    public bool CombatOngoing => EnemiesInCombat.Count > 0;
    
    public CombatManager(Player player, World world, Label debugLabel)
    {
        _player = player;
        _world = world;
        _debugLabel = debugLabel;
        _player.OnMoveEvent += OnPlayerMove;

        UpdateDebugLabel();
    }

    private void OnPlayerMove(Vector2I oldPos, Vector2I newPos)
    {
        foreach (var enemy in Enemies)
        {
            enemy.OnPlayerMove(_player, oldPos, newPos, this);
        }
        
        UpdateDebugLabel();
    }
    
    public void UpdateDebugLabel()
    {
        if (!Global.Debug)
        {
            _debugLabel.Text = "";
            return;
        }
        _debugLabel.Text = "Turn order: \n"
            + "Current turn: " + _currentTurnEntity?.Name
            + "\n\nTurn order:";
        foreach (var entity in _turnOrder)
        {
            _debugLabel.Text += "\n" + entity.Name;
        }
        _debugLabel.Text += "\n\nEnemies in combat: " + EnemiesInCombat.Count;
        foreach (var entity in EnemiesInCombat)
        {
            _debugLabel.Text += "\n" + entity.Name;
        }
    }

    private void StartCombat()
    {
        if (_turnOrder.Contains(_player))
        {
            GD.Print("[ANOMALY] Player is already in the turn order on combat start.");
        }
        
        _world.Camera.Focus = true;
        
        _turnOrder.Add(_player);
        _player.InCombat = true;
        AdvanceTurn();
        
        UpdateDebugLabel();
    }

    private async void AdvanceTurn()
    {
        _currentTurnEntity = _turnOrder.FirstOrDefault();
     
        GD.Print("Starting turn for " + _currentTurnEntity?.Name);
        GD.Print("Turn order: " + string.Join(", ", _turnOrder.Select(e => e.Name)));
        
        UpdateDebugLabel();
        
        if (_currentTurnEntity is null || EnemiesInCombat.Count == 0)
        {
            EndCombat();
            return;
        }
        
        await _currentTurnEntity.OnTurn(_player, _world);

        if (_currentTurnEntity is not null)
        {
            _turnOrder.Remove(_currentTurnEntity);
            _turnOrder.Add(_currentTurnEntity);
            GD.Print(_currentTurnEntity.Name + " finished their turn.");
        }
        
        AdvanceTurn();
    }
    
    private void EndCombat()
    {
        _turnOrder.Remove(_player);
        _player.InCombat = false;
        _currentTurnEntity = null;
        _world.Camera.Focus = false;
        UpdateDebugLabel();
    }
    
    public void AddEnemy(Enemy enemy)
    {
        GD.Print("[CM] Added enemy: " + enemy.Name);
        Enemies.Add(enemy);
        enemy.OnPlayerSpottedEvent += OnPlayerSpottedByEnemy;
        enemy.OnPlayerLostEvent += OnPlayerLostByEnemy;
        enemy.OnDamagedEvent += OnEnemyDamaged;
        enemy.OnDeathEvent += OnEnemyDeath;
        enemy.OnNudgeEvent += OnNudge;
        enemy.OnMoveEvent += _world.OnEntityMove;
    }
    
    private void OnEnemyDeath(Enemy enemy)
    {
        GD.Print(enemy.Name + " died!");
        Enemies.Remove(enemy);
        enemy.OnPlayerSpottedEvent -= OnPlayerSpottedByEnemy;
        enemy.OnPlayerLostEvent -= OnPlayerLostByEnemy;
        enemy.OnDamagedEvent -= OnEnemyDamaged;
        enemy.OnDeathEvent -= OnEnemyDeath;
        enemy.OnNudgeEvent -= OnNudge;
        enemy.OnMoveEvent -= _world.OnEntityMove;
        
        if (enemy.InCombat) RemoveEnemyFromCombat(enemy);
        
        _world.KillEnemy(enemy);
    }
    
    private void OnPlayerSpottedByEnemy(Enemy enemy)
    {
        if (!EnemiesInCombat.Contains(enemy))
        {
            AddEnemyToCombat(enemy);
        }
        
        if (EnemiesInCombat.Count == 0) StartCombat();

    }
    
    private void OnPlayerLostByEnemy(Enemy enemy)
    {
        if (CanLeaveCombat(enemy)) RemoveEnemyFromCombat(enemy);
    }
    
    private void OnEnemyDamaged(Enemy enemy)
    {
        if (!EnemiesInCombat.Contains(enemy))
        {
            AddEnemyToCombat(enemy);
        }
    }
    
    private void OnNudge(Vector2I position)
    {
        //Interact(position);
        //Attack(position, _player);
    }

    private void Attack(Entity target, Entity source)
    {
        GD.Print(source.Name + " attacked " + target.Name + " for " + source.Damage + " damage!");
        target.ReceiveDamage(source, source.Damage);
	    
        _world.Camera.Shake(6 * source.Damage);
    }
    
    public void Attack(Vector2I position, Entity source)
    {
        var enemy = _world.GetEnemyAt(position);
        if (enemy == null) return;
	    
        Attack(enemy, source);
    }

    private bool CanLeaveCombat(Enemy enemy)
    {
        if (!enemy.InCombat) return false;
        if (enemy.GroupId == null) return true;
        return !Enemies.Any(e => e.GroupId == enemy.GroupId && e.SeesPlayer);
    }

    private void RemoveEnemyFromCombat(Enemy enemy)
    {
        if (!enemy.InCombat) return;
        
        _turnOrder.Remove(enemy);
        enemy.InCombat = false;

        if (enemy.GroupId != null)
        {
            foreach (var e in Enemies.Where(e => e.GroupId == enemy.GroupId))
            {
                if (CanLeaveCombat(e)) RemoveEnemyFromCombat(e);
            }
        }

        if (EnemiesInCombat.Count == 0)
        {
            EndCombat();
            return;
        }

        UpdateDebugLabel();
    }

    private void AddEnemyToCombat(Enemy enemy)
    {
        if (enemy.InCombat) return;
        
        var shouldStartCombat = EnemiesInCombat.Count == 0;
         
        _turnOrder.Add(enemy);
        enemy.InCombat = true;
        
        if (enemy.GroupId is not null)
        {
            foreach (var e in Enemies.Where(e => e.GroupId == enemy.GroupId))
            {
                AddEnemyToCombat(e);
            }
        }
        
        GD.Print(enemy.Name + " entered combat.");
        UpdateDebugLabel();
        
        if (shouldStartCombat) StartCombat();
    }
}