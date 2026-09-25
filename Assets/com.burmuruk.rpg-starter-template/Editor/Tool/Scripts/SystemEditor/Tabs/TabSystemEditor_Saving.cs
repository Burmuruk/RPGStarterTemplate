using Burmuruk.RPGStarterTemplate.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.RPGStarterTemplate.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.RPGStarterTemplate.Editor
{
    public partial class TabSystemEditor : BaseLevelEditor
    {
        private const string ECRYPT_PREF_KEY = "RPGTemplate_EncryptSaving";
        private readonly List<SavingStageDraft> savingStages = new();
        private readonly List<string> savingBaseline = new();
        private readonly Dictionary<SavingStageDraft, VisualElement> savingRows = new();
        private bool savingDraftLoaded;
        private bool savingAwaitingCompilation;
        private VisualElement savingRowsContainer;
        private VisualElement savingRemovalPanel;
        private Label savingStatus;
        private SavingStageDraft savingDragged;
        private SavingStageDraft savingDropTarget;
        private bool savingDropAfter;

        public Toggle TglEncrypt { get; private set; }

        private void InitializeSaving()
        {
            var buttons = container.Q<VisualElement>("SavingButtons");
            TglEncrypt = container.Q<Toggle>("TglEncryptSaving");
            TglEncrypt.SetValueWithoutNotify(PlayerPrefs.GetInt(ECRYPT_PREF_KEY, 0) != 0);
            TglEncrypt.RegisterValueChangedCallback(OnTglEcryptClicked);
            GetAceptButton(buttons).clicked += OnAccept_SavingBtn;
            GetCancelButton(buttons).clicked += OnCanceled_SavingBtn;
        }

        private void OnTglEcryptClicked(ChangeEvent<bool> evt) =>
            PlayerPrefs.SetInt(ECRYPT_PREF_KEY, evt.newValue ? 1 : 0);

        private void Show_Saving()
        {
            DisableNotification(NotificationType.System);
            ChangeTab(infoSavingName);
            SelectTabBtn(btnSavingName);
            if (!savingDraftLoaded)
                LoadSavingDraft();
            CreateSavingTextFields();
        }

        private void LoadSavingDraft()
        {
            savingStages.Clear();
            savingBaseline.Clear();
            foreach (SavingExecution stage in Enum.GetValues(typeof(SavingExecution)))
            {
                string name = stage.ToString();
                savingBaseline.Add(name);
                savingStages.Add(new SavingStageDraft { OriginalName = name, Name = name });
            }
            savingDraftLoaded = true;
        }

        private void CreateSavingTextFields()
        {
            var info = container.Q<VisualElement>("savingInfoCont");
            info.Clear();
            savingRows.Clear();
            savingRowsContainer = new VisualElement();
            info.Add(new Label("Stages run from top to bottom. Drag the handle to change their order.")
            { style = { whiteSpace = WhiteSpace.Normal, marginBottom = 6 } });
            info.Add(savingRowsContainer);

            foreach (var stage in savingStages.Where(s => !s.Removed))
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;
                var handle = new Label("≡")
                { tooltip = stage.IsSystem ? "This system stage has a fixed position." : "Drag to change execution order." };
                handle.style.width = 24;
                handle.style.unityTextAlign = TextAnchor.MiddleCenter;
                var field = new TextField { value = stage.Name, isReadOnly = stage.IsSystem };
                field.style.flexGrow = 1;
                field.tooltip = stage.IsSystem ? "Required system stage." : "C# stage name. References update when you apply.";
                field.RegisterValueChangedCallback(evt =>
                {
                    stage.Name = evt.newValue;
                    // A displayed replacement menu becomes stale after editing a name.
                    savingRemovalPanel?.Clear();
                    RefreshSavingValidation();
                });
                var remove = new Button(() => ShowSavingRemoval(stage)) { text = "−", tooltip = "Remove stage" };
                remove.style.width = 26;
                remove.style.marginLeft = 4;
                var icon = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
                if (icon != null)
                {
                    remove.text = "";
                    remove.Add(new Image
                    {
                        image = icon,
                        scaleMode = ScaleMode.ScaleToFit,
                        style = { width = 14, height = 14 }
                    });
                }
                remove.SetEnabled(!stage.IsSystem && !savingAwaitingCompilation);
                handle.SetEnabled(!stage.IsSystem && !savingAwaitingCompilation);
                field.SetEnabled(!stage.IsSystem && !savingAwaitingCompilation);
                AttachSavingDrag(handle, stage);
                row.Add(handle);
                row.Add(field);
                row.Add(remove);
                savingRows[stage] = row;
                savingRowsContainer.Add(row);
            }
            var add = new Button(AddSavingStage) { text = "+ Add stage" };
            add.style.alignSelf = Align.FlexStart;
            add.style.marginTop = 6;
            add.SetEnabled(!savingAwaitingCompilation);
            info.Add(add);
            savingRemovalPanel = new VisualElement();
            savingRemovalPanel.style.marginTop = 8;
            info.Add(savingRemovalPanel);
            savingStatus = new Label();
            savingStatus.style.whiteSpace = WhiteSpace.Normal;
            savingStatus.style.marginTop = 6;
            info.Add(savingStatus);
            RefreshSavingValidation();
        }

        private void AddSavingStage()
        {
            var reserved = new HashSet<string>(savingStages.Select(s => s.Name), StringComparer.OrdinalIgnoreCase);
            reserved.UnionWith(savingBaseline);
            reserved.UnionWith(SavingExecutionAliases.GetMappings().Keys);
            string name = "NewStage";
            int suffix = 2;
            while (reserved.Contains(name))
                name = "NewStage" + suffix++;
            var stage = new SavingStageDraft { Name = name };
            savingStages.Add(stage);
            CreateSavingTextFields();
            var field = savingRows[stage].Q<TextField>();
            field.schedule.Execute(() => { field.Focus(); field.SelectAll(); });
        }

        private void ShowSavingRemoval(SavingStageDraft stage)
        {
            if (stage.IsSystem)
                return;
            savingRemovalPanel.Clear();
            if (stage.OriginalName == null && !savingStages.Any(s => s.Replacement == stage))
            {
                savingStages.Remove(stage);
                CreateSavingTextFields();
                return;
            }
            var candidates = savingStages.Where(s => !s.Removed && s != stage).ToList();
            var choices = new List<string> { "No replacement (only if unused in code)" };
            choices.AddRange(candidates.Select(s => s.Name));
            savingRemovalPanel.Add(new Label($"Remove '{stage.Name}'. Redirect references and old saved data to:")
            { style = { whiteSpace = WhiteSpace.Normal } });
            var replacement = new DropdownField(choices, 0);
            savingRemovalPanel.Add(replacement);
            var actions = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            actions.Add(new Button(() =>
            {
                stage.Removed = true;
                stage.Replacement = replacement.index > 0 ? candidates[replacement.index - 1] : null;
                CreateSavingTextFields();
            })
            { text = "Remove stage" });
            actions.Add(new Button(() => savingRemovalPanel.Clear()) { text = "Keep stage" });
            savingRemovalPanel.Add(actions);
            savingRemovalPanel.Add(new Label("Without a replacement, data saved in that stage will be skipped.")
            { style = { whiteSpace = WhiteSpace.Normal } });
        }

        private void AttachSavingDrag(Label handle, SavingStageDraft stage)
        {
            handle.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0 || stage.IsSystem || savingAwaitingCompilation)
                    return;
                savingDragged = stage;
                savingDropTarget = null;
                handle.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });
            handle.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (savingDragged != stage || !handle.HasPointerCapture(evt.pointerId))
                    return;
                foreach (var row in savingRows.Values)
                {
                    row.style.borderTopWidth = 0;
                    row.style.borderBottomWidth = 0;
                }
                savingDropTarget = null;
                // Use the Y coordinate so dragging slightly outside the handle remains easy.
                foreach (var pair in savingRows)
                {
                    if (pair.Key.IsSystem || pair.Key == stage)
                        continue;
                    var bounds = pair.Value.worldBound;
                    if (evt.position.y < bounds.yMin || evt.position.y > bounds.yMax)
                        continue;
                    savingDropTarget = pair.Key;
                    savingDropAfter = evt.position.y > bounds.center.y;
                    if (savingDropAfter)
                    {
                        pair.Value.style.borderBottomWidth = 2;
                        pair.Value.style.borderBottomColor = new Color(0.25f, 0.6f, 1f);
                    }
                    else
                    {
                        pair.Value.style.borderTopWidth = 2;
                        pair.Value.style.borderTopColor = new Color(0.25f, 0.6f, 1f);
                    }
                    break;
                }
                evt.StopPropagation();
            });
            handle.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (savingDragged != stage)
                    return;
                var target = savingDropTarget;
                bool after = savingDropAfter;
                savingDragged = null;
                savingDropTarget = null;
                if (handle.HasPointerCapture(evt.pointerId))
                    handle.ReleasePointer(evt.pointerId);
                if (target != null)
                {
                    savingStages.Remove(stage);
                    savingStages.Insert(savingStages.IndexOf(target) + (after ? 1 : 0), stage);
                }
                CreateSavingTextFields();
                evt.StopPropagation();
            });
            handle.RegisterCallback<PointerCaptureOutEvent>(_ =>
            {
                savingDragged = null;
                savingDropTarget = null;
                foreach (var row in savingRows.Values)
                {
                    row.style.borderTopWidth = 0;
                    row.style.borderBottomWidth = 0;
                }
            });
        }

        private bool SavingHasChanges()
        {
            var active = savingStages.Where(s => !s.Removed).ToList();
            return !active.Select(s => s.OriginalName).SequenceEqual(savingBaseline) ||
                active.Any(s => s.Name != s.OriginalName);
        }

        private void RefreshSavingValidation()
        {
            bool changed = SavingHasChanges();
            string error = null;
            try
            { SavingStageEditor.BuildChanges(savingBaseline, savingStages); }
            catch (Exception ex) { error = ex.Message; }
            changesInTab = changed;
            var buttons = container.Q<VisualElement>("SavingButtons");
            EnableSavingButtons(changed);
            GetAceptButton(buttons).SetEnabled(changed && error == null && !savingAwaitingCompilation);
            GetCancelButton(buttons).SetEnabled(changed && !savingAwaitingCompilation);
            if (savingStatus != null)
                savingStatus.text = savingAwaitingCompilation ? "Changes written. Waiting for Unity to compile." :
                    error ?? (changed ? "Pending changes. Apply updates the enum and its script references." : "No pending changes.");
        }

        private void EnableSavingButtons(bool shouldEnable)
        {
            var buttons = container.Q<VisualElement>("SavingButtons");
            GetAceptButton(buttons).EnableInClassList("Invisible", !shouldEnable);
            GetCancelButton(buttons).EnableInClassList("Invisible", !shouldEnable);
        }

        private void OnAccept_SavingBtn()
        {
            if (!SavingHasChanges() || savingAwaitingCompilation)
                return;
            try
            {
                int count = SavingStageEditor.Apply(savingBaseline, savingStages);
                savingAwaitingCompilation = count > 0;
                CreateSavingTextFields();
                Notify($"Updated {count} scripts.", BorderColour.Success, NotificationType.System);
            }
            catch (Exception ex)
            {
                Notify(ex.Message, BorderColour.Error, NotificationType.System);
            }
        }

        private void OnCanceled_SavingBtn()
        {
            if (savingAwaitingCompilation)
                return;
            LoadSavingDraft();
            CreateSavingTextFields();
            DisableNotification(NotificationType.System);
        }
    }
}
