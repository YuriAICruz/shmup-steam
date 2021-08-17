using Graphene.Game.Installers;
using Graphene.Game.Systems;
using Graphene.Game.Systems.Gameplay;
using Graphene.Game.Systems.Gameplay.LevelDesign;
using Graphene.LevelEditor.Presentation;
using Graphene.LevelEditor.Presentation.AssetMenus;
using Graphene.LevelEditor.Presentation.Button;
using Graphene.LevelEditor.Presentation.LevelAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Graphene.LevelEditor.Installers
{
    [CreateAssetMenu(fileName = "LevelEditorInstaller", menuName = "Installers/LevelEditorInstaller")]
    public class LevelEditorInstaller : ScriptableObjectInstaller<LevelEditorInstaller>
    {
        public EditorSettings editorSettings;
        
        public float panDelay = 0.4f;
    
        public override void InstallBindings()
        {
            Container.Bind<EditorSettings>().FromInstance(editorSettings);
            Container.Bind<EventSystem>().FromInstance(FindObjectOfType<EventSystem>());
            Container.Bind<GraphicRaycaster>().FromInstance(FindObjectOfType<GraphicRaycaster>());
            
            Container.Bind<Grid>().FromInstance(FindObjectOfType<Grid>());
            Container.Bind<EditorCamera>().FromInstance(FindObjectOfType<EditorCamera>());
            
            Container.Bind<ToolsManager>().AsSingle();
            Container.Bind<Viewport>().AsSingle().WithArguments(panDelay).NonLazy();
            
            Container.Bind<LevelConstructor>().AsSingle();
            
            Container.BindFactory<CameraManager, CameraManager.Factory>().FromComponentInNewPrefabResource("Cameras/ScrollUp");
            Container.BindFactory<Player, Player.Factory>().FromComponentInNewPrefabResource("Player/Base");
            
            Container.BindInterfacesAndSelfTo<InputManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PopupManager>().AsSingle().NonLazy();

            Container.DeclareSignal<ToolSelection>();
            Container.DeclareSignal<MenuWindowOpen>();
            Container.DeclareSignal<BrushSelected>();
            Container.DeclareSignal<BrushReleased>();
            
            Container.DeclareSignal<OpenAssetMenu>();
            Container.DeclareSignal<CloseAssetMenu>();
            
            Container.DeclareSignal<InputEventUp>();
            Container.DeclareSignal<InputEventDown>();
            Container.DeclareSignal<InputEvent>();
            
            Container.BindFactory<GameObject, Level.GenericFactory>().FromResource("Editor/Buttons/AssetButton");
            Container.BindFactory<AssetButton, AssetButton.Factory>().FromResource("Editor/Buttons/AssetButton");
            Container.BindFactory<LevelAsset, LevelAsset.Factory>().FromResource("Editor/LevelAssets/AssetButton");
            Container.BindFactory<AssetMenu, AssetMenu.Factory>().FromResource("Editor/AssetMenus/Base");
            
            LevelInstaller.SignalBindings(Container);
        }
    }
}