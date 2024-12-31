using Burmuruk.AI;
using Burmuruk.Tesis.Saving;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabSystemEditor : BaseLevelEditor
    {
        const string btnNavName = "btnNavigation";
        const string btnInteractionName = "btnInteractions";
        const string btnMissionName = "btnMissions";
        const string btnSavingName = "btnSaving";

        const string infoNavName = "navContainer";
        const string infoInteractionName = "interactionContainer";
        const string infoMissionsName = "missionsContainer";
        const string infoSavingName = "savingContainer";

        const string defaultSaveFile = "miGuardado-";

        [MenuItem("LevelEditor/System")]
        public static void ShowWindow()
        {
            TabSystemEditor window = GetWindow<TabSystemEditor>();
            window.titleContent = new GUIContent("System settings");
            window.minSize = new Vector2(400, 300);
        }

        public void CreateGUI()
        {
            container = rootVisualElement;
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/SystemTab.uxml");
            container.Add(visualTree.Instantiate());

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/BasicSS.uss");
            container.styleSheets.Add(styleSheet);

            GetTabButtons();
            GetInfoContainers();
            GetNotificationSection();

            InitializeSaving();

            Show_NavMesh();
        }

        protected override void GetInfoContainers()
        {
            infoContainers.Add(infoNavName, container.Q<VisualElement>(infoNavName));
            //infoContainers.Add(infoInteractionName, Parent.Q<VisualElement>(infoInteractionName));
            //infoContainers.Add(infoMissionsName, Parent.Q<VisualElement>(infoMissionsName));
            infoContainers.Add(infoSavingName, container.Q<VisualElement>(infoSavingName));

            foreach (var container in infoContainers.Values)
            {
                container.AddToClassList("Disable");
            }
        }

        protected override void GetTabButtons()
        {
            tabButtons.Add(btnNavName, container.Q<Button>(btnNavName));
            tabButtons[btnNavName].clicked += Show_NavMesh;

            tabButtons.Add(btnInteractionName, container.Q<Button>(btnInteractionName));
            tabButtons[btnInteractionName].clicked += Show_Interactions;

            tabButtons.Add(btnMissionName, container.Q<Button>(btnMissionName));
            tabButtons[btnMissionName].clicked += Show_Missions;

            tabButtons.Add(btnSavingName, container.Q<Button>(btnSavingName));
            tabButtons[btnSavingName].clicked += Show_Saving;
        }

        private void Show_Missions()
        {
            if (changesInTab) ;
            //Display warning

            DisableNotification();
            ChangeTab(btnMissionName);
        }

        private void Show_Interactions()
        {
            if (changesInTab) ;
            //Display warning

            DisableNotification();
            ChangeTab(btnInteractionName);
        }

        #region Navigation
        private void Show_NavMesh()
        {
            if (CheckNavFileStatus())
                Notify("Navigation map found.", BorderColour.Approved);
            else
                Notify("The Navigation map wasn't found.", BorderColour.Error);

            ChangeTab(infoContainers[infoNavName]);
            SelectTabBtn(btnNavName);
            VisualElement navInfo = container.Q<VisualElement>("navInfoContainer");

            if (navInfo.childCount == 0)
            {
                var nodesInsta = ScriptableObject.CreateInstance<NodesList>();
                var nodesEditor = NodeListEditor.CreateEditor(nodesInsta, typeof(NodeListEditor));

                navInfo.Add(new InspectorElement(nodesEditor));
            }
        }

        bool CheckNavFileStatus()
        {
            int sceneIdx = SceneManager.GetActiveScene().buildIndex;

            JsonWriter jsonWriter = new();
            var path = jsonWriter.GetPathFromSaveFile("Saving" + sceneIdx);

            return File.Exists(path);
        }
        #endregion
    } 
}
