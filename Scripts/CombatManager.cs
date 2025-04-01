#nullable enable
using System.Collections.Generic;
using Godot;

namespace Cardium.Scripts;

public class CombatManager
{
    private readonly World _world;
    private readonly Player _player;
    public readonly List<Enemy> Enemies = new();
    private readonly Label _debugLabel;
    
    public int Turn = 0;
    
    public CombatManager(Player player, World world, Label debugLabel)
    {
        _player = player;
        _world = world;
        _debugLabel = debugLabel;
        _player.OnActionEvent += OnPlayerAction;

        UpdateDebugLabel();
    }

    private void OnPlayerAction()
    {
        foreach (var enemy in Enemies)
        {
            enemy.OnTakeTurn(_player, _world);
        }
        
        UpdateDebugLabel();
    }
    
    public void UpdateDebugLabel()
    {
        _debugLabel.Text = "Turn order: \n"
                           + "Current turn: " + Turn + "\n"
                           + "Enemy count: " + Enemies.Count;
    }

    private void StartCombat()
    {
        GD.Print("\n>>>>>>>>>>>>>>>>>>> Combat started! >>>>>>>>>>>>>>>>>>>\n");
        
        Turn = 0;
        
        UpdateDebugLabel();
    }

    private void EndCombat()
    {
        GD.Print("\n<<<<<<<<<<<<<<<<<<<< Combat ended! <<<<<<<<<<<<<<<<<<<<\n");
        
        UpdateDebugLabel();
    }
    
    public void AddEnemy(Enemy enemy)
    {
        GD.Print("[CM] Added enemy: " + enemy.Name);
        Enemies.Add(enemy);
        enemy.OnDeathEvent += OnEnemyDeath;
        enemy.OnNudgeEvent += OnNudge;
        enemy.OnMoveEvent += _world.OnEntityMove;
    }
    
    private void OnEnemyDeath(Enemy enemy)
    {
        GD.Print(enemy.Name + " died!");
        Enemies.Remove(enemy);
        enemy.OnDeathEvent -= OnEnemyDeath;
        enemy.OnNudgeEvent -= OnNudge;
        enemy.OnMoveEvent -= _world.OnEntityMove;
        
        _world.KillEnemy(enemy);
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
}