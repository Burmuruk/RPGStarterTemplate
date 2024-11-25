using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Burmuruk.AI;
using UnityEditor.UIElements;
using NUnit.Framework.Interfaces;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Burmuruk.Tesis.Saving;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;

public class TabSystemEditor : BaseLevelEditor
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
        window.titleContent = new GUIContent("Main Window");
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

        Show_NavMesh();
    }

    protected override void GetInfoContainers()
    {
        infoContainers.Add(infoNavName, container.Q<VisualElement>(infoNavName));
        infoContainers.Add(infoInteractionName, container.Q<VisualElement>(infoInteractionName));
        infoContainers.Add(infoMissionsName, container.Q<VisualElement>(infoMissionsName));
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

    private void Show_NavMesh()
    {
        if (CheckNavFileStatus())
            Notify("Navigation map found.", Color.green);
        else
            Notify("The Navigation map wasn't found.", Color.red);

        ChangeTab(infoContainers[infoNavName]);
        VisualElement navInfo = container.Q<VisualElement>("navInfoContainer");

        if (navInfo.childCount == 0)
        {
            var nodesInsta = ScriptableObject.CreateInstance<NodesList>();
            var nodesEditor = NodeListEditor.CreateEditor(nodesInsta, typeof(NodeListEditor));
            //NodeListEditor.CreateEditor();
            //var visual = nodeListEditor.CreateInspectorGUI();
            //var inst = new InspectorElement(nodeListEditor);
            
            navInfo.Add(new InspectorElement(nodesEditor)); 
        }
    }

    private void Show_Missions()
    {
        DisableNotification();
        ChangeTab(btnMissionName);
    }

    private void Show_Saving()
    {
        DisableNotification();
        ChangeTab(infoSavingName);

        var infoContainer = container.Q<VisualElement>("savingInfoCont");
        infoContainer.Clear();

        for (int i = 0; i < Enum.GetValues(typeof(SavingExecution)).Length; i++)
        {
            var textField = new Label(((SavingExecution)i).ToString());
            
            infoContainer.Add(textField);
        }

        infoContainer.Add(new TextField());
    }

    private void Show_Interactions()
    {
        DisableNotification();
        ChangeTab(btnInteractionName);
    }

    #region Navigation
    bool CheckNavFileStatus()
    {
        int sceneIdx = SceneManager.GetActiveScene().buildIndex;

        JsonWriter jsonWriter = new();
        var path = jsonWriter.GetPathFromSaveFile("Saving" + sceneIdx);

        return File.Exists(path);
    }
    #endregion
}
