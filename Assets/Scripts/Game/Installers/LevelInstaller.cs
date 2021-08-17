using Graphene.Game.Presentation;
using Graphene.Game.Systems;
using Graphene.Game.Systems.Factories;
using Graphene.Game.Systems.Gameplay;
using Graphene.Game.Systems.Gameplay.Enemies;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Graphene.Game.Installers
{
    public class LevelInstaller : MonoInstaller
    {
        public Image health;
        public Image life;

        public Level Level;

        public override void InstallBindings()
        {
            //UI
            Container.BindFactory<Image, ImageFactory>().FromComponentInNewPrefab(health)
                .WhenInjectedInto<PlayerHealth>();
            Container.BindFactory<Image, ImageFactory>().FromComponentInNewPrefab(life).WhenInjectedInto<PlayerLives>();
            
            Container.Bind<Player>().FromInstance(FindObjectOfType<Player>());
            Container.Bind<CameraManager>().FromInstance(FindObjectOfType<CameraManager>());
            
            Container.Bind<LevelConstructor>().AsSingle();
            
            Container.BindFactory<CameraManager, CameraManager.Factory>().FromComponentInNewPrefabResource("Cameras/ScrollUp");
            Container.BindFactory<Player, Player.Factory>().FromComponentInNewPrefabResource("Player/Base");
            
            LevelBindings(Container, Level);
            SignalBindings(Container);
        }

        public static void SignalBindings(DiContainer container)
        {
            container.DeclareSignal<LevelStarted>();
            container.DeclareSignal<LevelEnded>();
            container.DeclareSignal<LevelCompleted>();
            container.DeclareSignal<SectorChange>();

            container.DeclareSignal<PowerUp.Score>();
            container.DeclareSignal<PowerUp.WeaponUpgrade>();
            container.DeclareSignal<PowerUp.Bomb>();
        }
        
        public static void LevelBindings(DiContainer container, Level level)
        {
            container.Bind<SectorSettings>().FromInstance(level.initialSectorSettings);
            container.Bind<LevelSettings>().FromInstance(level.levelSettings);

            container.BindFactory<Gun, Gun.Factory>().AsTransient();
            container.BindFactory<Bullet, Bullet.Factory>().FromComponentInNewPrefabResource("Bullet/Generic");
            container.BindFactory<Explosion, Explosion.Factory>().FromComponentInNewPrefabResource("FX/GenericExplosion");
            container.BindFactory<MovingEnemy, EnemyPool.Factory>().FromComponentInNewPrefabResource("Enemies/Generic_00");
            container.BindFactory<PowerUp, PowerUp.Factory>().FromComponentInNewPrefabResource("PowerUps/Base");
            
            container.Bind<EnemyPool>().AsSingle().NonLazy();
            container.Bind<BulletPool>().AsSingle().NonLazy();
            container.Bind<PowerUpPool>().AsSingle().NonLazy();
            container.Bind<LevelManager>().AsSingle().NonLazy();

            container.Bind<Delayer>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}