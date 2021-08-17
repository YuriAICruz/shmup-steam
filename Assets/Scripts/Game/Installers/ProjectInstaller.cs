using Graphene.Game.Presentation;
using Graphene.Game.Systems;
using Graphene.Game.Systems.Gameplay;
using Graphene.Game.Systems.Input;
using UnityEngine;
using Zenject;

namespace Graphene.Game.Installers
{
    [CreateAssetMenu(fileName = "ProjectInstaller", menuName = "Installers/ProjectInstaller")]
    public class ProjectInstaller : ScriptableObjectInstaller<ProjectInstaller>
    {
        public InputSettings inputSettings;
        public ScoreSystem.ScoreSettings scoreSettings;

        public bool debug;
    
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<InputManager.InputDownEvent>();
            Container.DeclareSignal<InputManager.InputUpEvent>();
            Container.DeclareSignal<InputManager.InputConstantEvent>();
            Container.DeclareSignal<InputManager.InputAxisEvent>();
        
            DeclarePlayerSignals(Container);

            Container.DeclareSignal<ScoreUpdate>();

            inputSettings.SetState(InputSettings.InputState.Game);
            Container.Bind<InputSettings>().FromInstance(inputSettings);
        
            Container.Bind<ApplicationManager>().AsSingle().NonLazy();
        
            Container.Bind<ScoreSystem>().AsSingle().NonLazy();
            Container.Bind<ScoreSystem.ScoreSettings>().FromInstance(scoreSettings);
        
            Container.BindInterfacesAndSelfTo<InputManager>().AsSingle().NonLazy();

            if (debug)
            {
                Container.Bind<InputDebugger>().AsSingle().NonLazy();
            }
        }

        public static void DeclarePlayerSignals(DiContainer container)
        {
            container.DeclareSignal<BulletShoot>();
            container.DeclareSignal<BulletHit>();
            container.DeclareSignal<BulletKill>();
            container.DeclareSignal<DashKill>();
            container.DeclareSignal<DashReflect>();
            container.DeclareSignal<PlayerHit>();
            container.DeclareSignal<PlayerDeath>();
            container.DeclareSignal<BossDeath>();
            container.DeclareSignal<GameOver>();
        }
    }
}