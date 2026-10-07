using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Expedition33.Combat;

namespace Expedition33.EditorTools
{
    public static class CombatSandboxBuilder
    {
        [MenuItem("Expedition33/Build Combat Sandbox Scene")]
        public static void BuildScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Environment & Lighting
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.96f, 0.9f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Arena_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            var floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            floorMat.color = new Color(0.18f, 0.2f, 0.24f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 2. Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 3.2f, -6f);
            camGo.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

            // 3. Characters
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Projects/Models/Idle.fbx");
            var animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Projects/Animations/CombatCharacter.controller");

            // Player
            var playerGo = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            playerGo.name = "Player_Gustave";
            playerGo.transform.position = new Vector3(-2.2f, 0f, 0f);
            playerGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            var pAnimator = playerGo.GetComponent<Animator>();
            if (pAnimator == null) pAnimator = playerGo.AddComponent<Animator>();
            pAnimator.runtimeAnimatorController = animController;
            var pView = playerGo.AddComponent<CombatActorView>();

            // Enemy
            var enemyGo = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            enemyGo.name = "Enemy_Stalker";
            enemyGo.transform.position = new Vector3(2.2f, 0f, 0f);
            enemyGo.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            var eAnimator = enemyGo.GetComponent<Animator>();
            if (eAnimator == null) eAnimator = enemyGo.AddComponent<Animator>();
            eAnimator.runtimeAnimatorController = animController;
            var eView = enemyGo.AddComponent<CombatActorView>();

            // 4. UI Canvas & EventSystem
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var canvasGo = new GameObject("CombatCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();
            var hud = canvasGo.AddComponent<CombatHUD>();

            // Timeline banner (Top Center)
            var timelineObj = new GameObject("TimelineBanner");
            timelineObj.transform.SetParent(canvasGo.transform, false);
            var tlRect = timelineObj.AddComponent<RectTransform>();
            tlRect.anchorMin = new Vector2(0.2f, 0.91f);
            tlRect.anchorMax = new Vector2(0.8f, 0.98f);
            tlRect.offsetMin = Vector2.zero;
            tlRect.offsetMax = Vector2.zero;
            var tlText = timelineObj.AddComponent<TextMeshProUGUI>();
            tlText.fontSize = 24;
            tlText.alignment = TextAlignmentOptions.Center;
            tlText.text = "TIMELINE PREVIEW";

            // Turn Banner (Center Upper)
            var bannerObj = new GameObject("TurnBanner");
            bannerObj.transform.SetParent(canvasGo.transform, false);
            var bRect = bannerObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.2f, 0.82f);
            bRect.anchorMax = new Vector2(0.8f, 0.90f);
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            var bText = bannerObj.AddComponent<TextMeshProUGUI>();
            bText.fontSize = 38;
            bText.fontStyle = FontStyles.Bold;
            bText.alignment = TextAlignmentOptions.Center;
            bText.text = "BATTLE START";

            // Player Status (Bottom Left)
            var playerPanel = new GameObject("PlayerPanel");
            playerPanel.transform.SetParent(canvasGo.transform, false);
            var ppRect = playerPanel.AddComponent<RectTransform>();
            ppRect.anchorMin = new Vector2(0.04f, 0.05f);
            ppRect.anchorMax = new Vector2(0.32f, 0.26f);
            ppRect.offsetMin = Vector2.zero;
            ppRect.offsetMax = Vector2.zero;
            var ppBg = playerPanel.AddComponent<Image>();
            ppBg.color = new Color(0.1f, 0.15f, 0.2f, 0.88f);

            var pNameObj = new GameObject("PlayerName");
            pNameObj.transform.SetParent(playerPanel.transform, false);
            var pnRect = pNameObj.AddComponent<RectTransform>();
            pnRect.anchorMin = new Vector2(0.05f, 0.7f);
            pnRect.anchorMax = new Vector2(0.95f, 0.95f);
            pnRect.offsetMin = Vector2.zero;
            pnRect.offsetMax = Vector2.zero;
            var pnText = pNameObj.AddComponent<TextMeshProUGUI>();
            pnText.fontSize = 28;
            pnText.fontStyle = FontStyles.Bold;
            pnText.color = new Color(0.4f, 0.8f, 1f);
            pnText.text = "GUSTAVE";

            // Player HP Slider
            var pHpSliderObj = new GameObject("PlayerHpSlider");
            pHpSliderObj.transform.SetParent(playerPanel.transform, false);
            var phRect = pHpSliderObj.AddComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0.05f, 0.44f);
            phRect.anchorMax = new Vector2(0.95f, 0.62f);
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;
            var pSlider = pHpSliderObj.AddComponent<Slider>();
            var phFill = new GameObject("Fill");
            phFill.transform.SetParent(pHpSliderObj.transform, false);
            var phfRect = phFill.AddComponent<RectTransform>();
            phfRect.anchorMin = Vector2.zero;
            phfRect.anchorMax = Vector2.one;
            var phfImg = phFill.AddComponent<Image>();
            phfImg.color = new Color(0.2f, 0.85f, 0.4f);
            pSlider.targetGraphic = phfImg;
            pSlider.fillRect = phfRect;

            var pHpTextObj = new GameObject("PlayerHpText");
            pHpTextObj.transform.SetParent(playerPanel.transform, false);
            var phtRect = pHpTextObj.AddComponent<RectTransform>();
            phtRect.anchorMin = new Vector2(0.05f, 0.44f);
            phtRect.anchorMax = new Vector2(0.95f, 0.62f);
            phtRect.offsetMin = Vector2.zero;
            phtRect.offsetMax = Vector2.zero;
            var phtText = pHpTextObj.AddComponent<TextMeshProUGUI>();
            phtText.fontSize = 18;
            phtText.alignment = TextAlignmentOptions.Center;
            phtText.text = "HP: 120 / 120";

            // Player AP Slider
            var pApSliderObj = new GameObject("PlayerApSlider");
            pApSliderObj.transform.SetParent(playerPanel.transform, false);
            var paRect = pApSliderObj.AddComponent<RectTransform>();
            paRect.anchorMin = new Vector2(0.05f, 0.12f);
            paRect.anchorMax = new Vector2(0.95f, 0.32f);
            paRect.offsetMin = Vector2.zero;
            paRect.offsetMax = Vector2.zero;
            var pApSlider = pApSliderObj.AddComponent<Slider>();
            var paFill = new GameObject("Fill");
            paFill.transform.SetParent(pApSliderObj.transform, false);
            var pafRect = paFill.AddComponent<RectTransform>();
            pafRect.anchorMin = Vector2.zero;
            pafRect.anchorMax = Vector2.one;
            var pafImg = paFill.AddComponent<Image>();
            pafImg.color = new Color(0.2f, 0.65f, 1f);
            pApSlider.targetGraphic = pafImg;
            pApSlider.fillRect = pafRect;

            var pApTextObj = new GameObject("PlayerApText");
            pApTextObj.transform.SetParent(playerPanel.transform, false);
            var patRect = pApTextObj.AddComponent<RectTransform>();
            patRect.anchorMin = new Vector2(0.05f, 0.12f);
            patRect.anchorMax = new Vector2(0.95f, 0.32f);
            patRect.offsetMin = Vector2.zero;
            patRect.offsetMax = Vector2.zero;
            var patText = pApTextObj.AddComponent<TextMeshProUGUI>();
            patText.fontSize = 18;
            patText.alignment = TextAlignmentOptions.Center;
            patText.text = "AP: 2 / 6";

            // Enemy Status (Top Right)
            var enemyPanel = new GameObject("EnemyPanel");
            enemyPanel.transform.SetParent(canvasGo.transform, false);
            var epRect = enemyPanel.AddComponent<RectTransform>();
            epRect.anchorMin = new Vector2(0.68f, 0.75f);
            epRect.anchorMax = new Vector2(0.96f, 0.92f);
            epRect.offsetMin = Vector2.zero;
            epRect.offsetMax = Vector2.zero;
            var epBg = enemyPanel.AddComponent<Image>();
            epBg.color = new Color(0.25f, 0.1f, 0.1f, 0.85f);

            var eNameObj = new GameObject("EnemyName");
            eNameObj.transform.SetParent(enemyPanel.transform, false);
            var enRect = eNameObj.AddComponent<RectTransform>();
            enRect.anchorMin = new Vector2(0.05f, 0.6f);
            enRect.anchorMax = new Vector2(0.95f, 0.95f);
            enRect.offsetMin = Vector2.zero;
            enRect.offsetMax = Vector2.zero;
            var enText = eNameObj.AddComponent<TextMeshProUGUI>();
            enText.fontSize = 26;
            enText.fontStyle = FontStyles.Bold;
            enText.color = new Color(1f, 0.4f, 0.4f);
            enText.text = "EXPEDITION STALKER";

            var eHpSliderObj = new GameObject("EnemyHpSlider");
            eHpSliderObj.transform.SetParent(enemyPanel.transform, false);
            var ehRect = eHpSliderObj.AddComponent<RectTransform>();
            ehRect.anchorMin = new Vector2(0.05f, 0.35f);
            ehRect.anchorMax = new Vector2(0.95f, 0.58f);
            ehRect.offsetMin = Vector2.zero;
            ehRect.offsetMax = Vector2.zero;
            var eSlider = eHpSliderObj.AddComponent<Slider>();
            var ehFill = new GameObject("Fill");
            ehFill.transform.SetParent(eHpSliderObj.transform, false);
            var ehfRect = ehFill.AddComponent<RectTransform>();
            ehfRect.anchorMin = Vector2.zero;
            ehfRect.anchorMax = Vector2.one;
            var ehfImg = ehFill.AddComponent<Image>();
            ehfImg.color = new Color(0.9f, 0.25f, 0.25f);
            eSlider.targetGraphic = ehfImg;
            eSlider.fillRect = ehfRect;

            var eHpTextObj = new GameObject("EnemyHpText");
            eHpTextObj.transform.SetParent(enemyPanel.transform, false);
            var ehtRect = eHpTextObj.AddComponent<RectTransform>();
            ehtRect.anchorMin = new Vector2(0.05f, 0.05f);
            ehtRect.anchorMax = new Vector2(0.95f, 0.32f);
            ehtRect.offsetMin = Vector2.zero;
            ehtRect.offsetMax = Vector2.zero;
            var ehtText = eHpTextObj.AddComponent<TextMeshProUGUI>();
            ehtText.fontSize = 22;
            ehtText.text = "HP: 90 / 90";

            // Command Menu (Bottom Right)
            var cmdMenu = new GameObject("CommandMenu");
            cmdMenu.transform.SetParent(canvasGo.transform, false);
            var cmRect = cmdMenu.AddComponent<RectTransform>();
            cmRect.anchorMin = new Vector2(0.70f, 0.05f);
            cmRect.anchorMax = new Vector2(0.96f, 0.26f);
            cmRect.offsetMin = Vector2.zero;
            cmRect.offsetMax = Vector2.zero;

            // Attack Button
            var atkBtnObj = new GameObject("Button_Attack");
            atkBtnObj.transform.SetParent(cmdMenu.transform, false);
            var atkRect = atkBtnObj.AddComponent<RectTransform>();
            atkRect.anchorMin = new Vector2(0.05f, 0.68f);
            atkRect.anchorMax = new Vector2(0.95f, 0.96f);
            atkRect.offsetMin = Vector2.zero;
            atkRect.offsetMax = Vector2.zero;
            var atkImg = atkBtnObj.AddComponent<Image>();
            atkImg.color = new Color(0.2f, 0.6f, 0.95f, 1f);
            var atkBtn = atkBtnObj.AddComponent<Button>();
            var atkTm = new GameObject("Text");
            atkTm.transform.SetParent(atkBtnObj.transform, false);
            var atktRect = atkTm.AddComponent<RectTransform>();
            atktRect.anchorMin = Vector2.zero;
            atktRect.anchorMax = Vector2.one;
            var atktComp = atkTm.AddComponent<TextMeshProUGUI>();
            atktComp.text = "ATTACK (+1 AP)";
            atktComp.alignment = TextAlignmentOptions.Center;
            atktComp.fontSize = 20;
            atktComp.fontStyle = FontStyles.Bold;

            // Skills Button
            var skillBtnObj = new GameObject("Button_Skills");
            skillBtnObj.transform.SetParent(cmdMenu.transform, false);
            var skRect = skillBtnObj.AddComponent<RectTransform>();
            skRect.anchorMin = new Vector2(0.05f, 0.36f);
            skRect.anchorMax = new Vector2(0.95f, 0.64f);
            skRect.offsetMin = Vector2.zero;
            skRect.offsetMax = Vector2.zero;
            var skImg = skillBtnObj.AddComponent<Image>();
            skImg.color = new Color(0.55f, 0.3f, 0.85f, 1f);
            var skBtn = skillBtnObj.AddComponent<Button>();
            var skTm = new GameObject("Text");
            skTm.transform.SetParent(skillBtnObj.transform, false);
            var sktRect = skTm.AddComponent<RectTransform>();
            sktRect.anchorMin = Vector2.zero;
            sktRect.anchorMax = Vector2.one;
            var sktComp = skTm.AddComponent<TextMeshProUGUI>();
            sktComp.text = "SKILLS";
            sktComp.alignment = TextAlignmentOptions.Center;
            sktComp.fontSize = 20;
            sktComp.fontStyle = FontStyles.Bold;

            // Pass Button
            var passBtnObj = new GameObject("Button_Pass");
            passBtnObj.transform.SetParent(cmdMenu.transform, false);
            var passRect = passBtnObj.AddComponent<RectTransform>();
            passRect.anchorMin = new Vector2(0.05f, 0.04f);
            passRect.anchorMax = new Vector2(0.95f, 0.32f);
            passRect.offsetMin = Vector2.zero;
            passRect.offsetMax = Vector2.zero;
            var passImg = passBtnObj.AddComponent<Image>();
            passImg.color = new Color(0.35f, 0.35f, 0.4f, 1f);
            var passBtn = passBtnObj.AddComponent<Button>();
            var passTm = new GameObject("Text");
            passTm.transform.SetParent(passBtnObj.transform, false);
            var passtRect = passTm.AddComponent<RectTransform>();
            passtRect.anchorMin = Vector2.zero;
            passtRect.anchorMax = Vector2.one;
            var passtComp = passTm.AddComponent<TextMeshProUGUI>();
            passtComp.text = "PASS TURN";
            passtComp.alignment = TextAlignmentOptions.Center;
            passtComp.fontSize = 18;

            // Skill Submenu Panel (pops up above command menu)
            var skillMenuPanel = new GameObject("SkillMenuPanel");
            skillMenuPanel.transform.SetParent(canvasGo.transform, false);
            var smRect = skillMenuPanel.AddComponent<RectTransform>();
            smRect.anchorMin = new Vector2(0.68f, 0.28f);
            smRect.anchorMax = new Vector2(0.96f, 0.58f);
            smRect.offsetMin = Vector2.zero;
            smRect.offsetMax = Vector2.zero;
            var smBg = skillMenuPanel.AddComponent<Image>();
            smBg.color = new Color(0.12f, 0.14f, 0.22f, 0.95f);

            var skillContainer = new GameObject("SkillButtonsContainer");
            skillContainer.transform.SetParent(skillMenuPanel.transform, false);
            var scRect = skillContainer.AddComponent<RectTransform>();
            scRect.anchorMin = new Vector2(0.05f, 0.22f);
            scRect.anchorMax = new Vector2(0.95f, 0.95f);
            scRect.offsetMin = Vector2.zero;
            scRect.offsetMax = Vector2.zero;
            var vlg = skillContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;

            var backBtnObj = new GameObject("Button_Back");
            backBtnObj.transform.SetParent(skillMenuPanel.transform, false);
            var bkRect = backBtnObj.AddComponent<RectTransform>();
            bkRect.anchorMin = new Vector2(0.05f, 0.04f);
            bkRect.anchorMax = new Vector2(0.95f, 0.20f);
            bkRect.offsetMin = Vector2.zero;
            bkRect.offsetMax = Vector2.zero;
            var bkImg = backBtnObj.AddComponent<Image>();
            bkImg.color = new Color(0.4f, 0.4f, 0.45f, 1f);
            var bkBtn = backBtnObj.AddComponent<Button>();
            var bkTm = new GameObject("Text");
            bkTm.transform.SetParent(backBtnObj.transform, false);
            var bktRect = bkTm.AddComponent<RectTransform>();
            bktRect.anchorMin = Vector2.zero;
            bktRect.anchorMax = Vector2.one;
            var bktComp = bkTm.AddComponent<TextMeshProUGUI>();
            bktComp.text = "BACK";
            bktComp.alignment = TextAlignmentOptions.Center;
            bktComp.fontSize = 18;

            // Battle Log (Bottom Center)
            var logObj = new GameObject("BattleLog");
            logObj.transform.SetParent(canvasGo.transform, false);
            var logRect = logObj.AddComponent<RectTransform>();
            logRect.anchorMin = new Vector2(0.34f, 0.05f);
            logRect.anchorMax = new Vector2(0.68f, 0.15f);
            logRect.offsetMin = Vector2.zero;
            logRect.offsetMax = Vector2.zero;
            var logText = logObj.AddComponent<TextMeshProUGUI>();
            logText.fontSize = 20;
            logText.alignment = TextAlignmentOptions.Center;
            logText.color = new Color(0.9f, 0.9f, 0.9f, 0.85f);
            logText.text = "Battle initialized.";

            // 5. Timing Visualizer UI (Center Screen for Defense)
            var timingObj = new GameObject("TimingVisualizer");
            timingObj.transform.SetParent(canvasGo.transform, false);
            var timingRect = timingObj.AddComponent<RectTransform>();
            timingRect.anchorMin = new Vector2(0.25f, 0.36f);
            timingRect.anchorMax = new Vector2(0.75f, 0.54f);
            timingRect.offsetMin = Vector2.zero;
            timingRect.offsetMax = Vector2.zero;
            var timingComp = timingObj.AddComponent<TimingVisualizerUI>();

            var promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(timingObj.transform, false);
            var prRect = promptObj.AddComponent<RectTransform>();
            prRect.anchorMin = new Vector2(0f, 0.65f);
            prRect.anchorMax = new Vector2(1f, 1f);
            prRect.offsetMin = Vector2.zero;
            prRect.offsetMax = Vector2.zero;
            var prText = promptObj.AddComponent<TextMeshProUGUI>();
            prText.fontSize = 24;
            prText.alignment = TextAlignmentOptions.Center;
            prText.text = "[F] PARRY  |  [SPACE] DODGE";

            var trackObj = new GameObject("BarTrack");
            trackObj.transform.SetParent(timingObj.transform, false);
            var trRect = trackObj.AddComponent<RectTransform>();
            trRect.anchorMin = new Vector2(0.05f, 0.35f);
            trRect.anchorMax = new Vector2(0.95f, 0.60f);
            trRect.offsetMin = Vector2.zero;
            trRect.offsetMax = Vector2.zero;
            var trImg = trackObj.AddComponent<Image>();
            trImg.color = new Color(0.08f, 0.1f, 0.14f, 0.85f);

            var dzObj = new GameObject("DodgeZone");
            dzObj.transform.SetParent(trackObj.transform, false);
            var dzRect = dzObj.AddComponent<RectTransform>();
            dzRect.anchorMin = new Vector2(0.5f, 0f);
            dzRect.anchorMax = new Vector2(0.5f, 1f);
            dzRect.sizeDelta = new Vector2(120f, 0f);
            var dzImg = dzObj.AddComponent<Image>();
            dzImg.color = new Color(0.9f, 0.8f, 0.2f, 0.45f);

            var pzObj = new GameObject("ParryZone");
            pzObj.transform.SetParent(trackObj.transform, false);
            var pzRect = pzObj.AddComponent<RectTransform>();
            pzRect.anchorMin = new Vector2(0.5f, 0f);
            pzRect.anchorMax = new Vector2(0.5f, 1f);
            pzRect.sizeDelta = new Vector2(40f, 0f);
            var pzImg = pzObj.AddComponent<Image>();
            pzImg.color = new Color(0f, 1f, 0.5f, 0.75f);

            var slObj = new GameObject("StrikeLine");
            slObj.transform.SetParent(trackObj.transform, false);
            var slRect = slObj.AddComponent<RectTransform>();
            slRect.anchorMin = new Vector2(0.5f, -0.2f);
            slRect.anchorMax = new Vector2(0.5f, 1.2f);
            slRect.sizeDelta = new Vector2(4f, 0f);
            var slImg = slObj.AddComponent<Image>();
            slImg.color = Color.white;

            var curObj = new GameObject("Cursor");
            curObj.transform.SetParent(trackObj.transform, false);
            var curRect = curObj.AddComponent<RectTransform>();
            curRect.anchorMin = new Vector2(0.5f, -0.3f);
            curRect.anchorMax = new Vector2(0.5f, 1.3f);
            curRect.sizeDelta = new Vector2(8f, 0f);
            var curImg = curObj.AddComponent<Image>();
            curImg.color = new Color(1f, 0.95f, 0.4f, 1f);

            var resObj = new GameObject("ResultText");
            resObj.transform.SetParent(timingObj.transform, false);
            var resRect = resObj.AddComponent<RectTransform>();
            resRect.anchorMin = new Vector2(0f, 0f);
            resRect.anchorMax = new Vector2(1f, 0.32f);
            resRect.offsetMin = Vector2.zero;
            resRect.offsetMax = Vector2.zero;
            var resText = resObj.AddComponent<TextMeshProUGUI>();
            resText.fontSize = 28;
            resText.fontStyle = FontStyles.Bold;
            resText.alignment = TextAlignmentOptions.Center;

            var soTiming = new SerializedObject(timingComp);
            soTiming.FindProperty("_container").objectReferenceValue = timingObj;
            soTiming.FindProperty("_barTrackRect").objectReferenceValue = trRect;
            soTiming.FindProperty("_cursorRect").objectReferenceValue = curRect;
            soTiming.FindProperty("_dodgeZoneRect").objectReferenceValue = dzRect;
            soTiming.FindProperty("_parryZoneRect").objectReferenceValue = pzRect;
            soTiming.FindProperty("_strikeLineRect").objectReferenceValue = slRect;
            soTiming.FindProperty("_parryZoneImage").objectReferenceValue = pzImg;
            soTiming.FindProperty("_dodgeZoneImage").objectReferenceValue = dzImg;
            soTiming.FindProperty("_cursorImage").objectReferenceValue = curImg;
            soTiming.FindProperty("_promptText").objectReferenceValue = prText;
            soTiming.FindProperty("_resultText").objectReferenceValue = resText;
            soTiming.ApplyModifiedPropertiesWithoutUndo();

            // 6. Offensive QTE Widget (Over target)
            var qteObj = new GameObject("OffensiveQTEWidget");
            qteObj.transform.SetParent(canvasGo.transform, false);
            var qteRect = qteObj.AddComponent<RectTransform>();
            qteRect.sizeDelta = new Vector2(300f, 300f);
            var qteComp = qteObj.AddComponent<OffensiveQTEWidget>();

            // Target Ring (base center circle)
            var trRingObj = new GameObject("TargetRing");
            trRingObj.transform.SetParent(qteObj.transform, false);
            var trrRect = trRingObj.AddComponent<RectTransform>();
            trrRect.sizeDelta = new Vector2(90f, 90f);
            var trrImg = trRingObj.AddComponent<Image>();
            trrImg.color = new Color(1f, 1f, 1f, 0.85f);

            // Shrinking Ring
            var shRingObj = new GameObject("ShrinkingRing");
            shRingObj.transform.SetParent(qteObj.transform, false);
            var shrRect = shRingObj.AddComponent<RectTransform>();
            shrRect.sizeDelta = new Vector2(280f, 280f);
            var shrImg = shRingObj.AddComponent<Image>();
            shrImg.color = new Color(1f, 0.85f, 0.2f, 0.9f);

            var qtePromptObj = new GameObject("Prompt");
            qtePromptObj.transform.SetParent(qteObj.transform, false);
            var qprRect = qtePromptObj.AddComponent<RectTransform>();
            qprRect.anchoredPosition = new Vector2(0f, 85f);
            qprRect.sizeDelta = new Vector2(300f, 50f);
            var qprText = qtePromptObj.AddComponent<TextMeshProUGUI>();
            qprText.fontSize = 20;
            qprText.fontStyle = FontStyles.Bold;
            qprText.alignment = TextAlignmentOptions.Center;
            qprText.text = "TIMED STRIKE!\n<b>[SPACE]</b>";

            var qteResObj = new GameObject("Result");
            qteResObj.transform.SetParent(qteObj.transform, false);
            var qreRect = qteResObj.AddComponent<RectTransform>();
            qreRect.anchoredPosition = new Vector2(0f, -85f);
            qreRect.sizeDelta = new Vector2(300f, 50f);
            var qreText = qteResObj.AddComponent<TextMeshProUGUI>();
            qreText.fontSize = 28;
            qreText.fontStyle = FontStyles.Bold;
            qreText.alignment = TextAlignmentOptions.Center;

            var soQte = new SerializedObject(qteComp);
            soQte.FindProperty("_container").objectReferenceValue = qteObj;
            soQte.FindProperty("_targetRingRect").objectReferenceValue = trrRect;
            soQte.FindProperty("_shrinkingRingRect").objectReferenceValue = shrRect;
            soQte.FindProperty("_targetRingImage").objectReferenceValue = trrImg;
            soQte.FindProperty("_shrinkingRingImage").objectReferenceValue = shrImg;
            soQte.FindProperty("_promptText").objectReferenceValue = qprText;
            soQte.FindProperty("_resultText").objectReferenceValue = qreText;
            soQte.ApplyModifiedPropertiesWithoutUndo();

            // Wire up CombatHUD SerializedObject
            var soHud = new SerializedObject(hud);
            soHud.FindProperty("_playerHpSlider").objectReferenceValue = pSlider;
            soHud.FindProperty("_playerHpText").objectReferenceValue = phtText;
            soHud.FindProperty("_playerNameText").objectReferenceValue = pnText;
            soHud.FindProperty("_playerApSlider").objectReferenceValue = pApSlider;
            soHud.FindProperty("_playerApText").objectReferenceValue = patText;
            soHud.FindProperty("_enemyHpSlider").objectReferenceValue = eSlider;
            soHud.FindProperty("_enemyHpText").objectReferenceValue = ehtText;
            soHud.FindProperty("_enemyNameText").objectReferenceValue = enText;
            soHud.FindProperty("_timelineFallbackText").objectReferenceValue = tlText;
            soHud.FindProperty("_commandMenuRoot").objectReferenceValue = cmdMenu;
            soHud.FindProperty("_attackButton").objectReferenceValue = atkBtn;
            soHud.FindProperty("_skillsButton").objectReferenceValue = skBtn;
            soHud.FindProperty("_passButton").objectReferenceValue = passBtn;
            soHud.FindProperty("_skillMenuRoot").objectReferenceValue = skillMenuPanel;
            soHud.FindProperty("_skillButtonsContainer").objectReferenceValue = skillContainer.transform;
            soHud.FindProperty("_skillBackBtn").objectReferenceValue = bkBtn;
            soHud.FindProperty("_turnBannerText").objectReferenceValue = bText;
            soHud.FindProperty("_battleLogText").objectReferenceValue = logText;
            soHud.ApplyModifiedPropertiesWithoutUndo();

            // Create Skills and Attack Pattern Assets
            string dataDir = "Assets/_Projects/Data/Combat";
            var skillCleave = SkillDefinitionSO.CreateSkill("Overcharge Cleave", 3, 2.2f, "Heavy devastating blow with bonus break force.");
            AssetDatabase.CreateAsset(skillCleave, dataDir + "/Skill_OverchargeCleave.asset");

            var skillFlurry = SkillDefinitionSO.CreateSkill("Swift Flurry", 2, 1.5f, "Quick dual-hit flurry.");
            AssetDatabase.CreateAsset(skillFlurry, dataDir + "/Skill_SwiftFlurry.asset");

            var patternStandard = EnemyAttackPatternSO.CreateDefaultPattern("Stalker Claw Combo", AttackTelegraphType.Standard, 1.0f, 22);
            AssetDatabase.CreateAsset(patternStandard, dataDir + "/Attack_StalkerClaw.asset");

            var patternSweep = EnemyAttackPatternSO.CreateDefaultPattern("Ground Shockwave", AttackTelegraphType.GroundSweep, 1.1f, 26);
            AssetDatabase.CreateAsset(patternSweep, dataDir + "/Attack_GroundSweep.asset");

            // 7. Managers Object
            var mgrObj = new GameObject("BattleManagers");
            var audioPlayer = mgrObj.AddComponent<CombatAudioPlayer>();
            var gameFeel = mgrObj.AddComponent<GameFeelManager>();
            var inputBuf = mgrObj.AddComponent<InputBuffer>();
            var battleMgr = mgrObj.AddComponent<BattleManager>();

            var soBattle = new SerializedObject(battleMgr);
            soBattle.FindProperty("_playerData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CombatActorDataSO>("Assets/_Projects/Data/Combat/Player_Gustave.asset");
            soBattle.FindProperty("_enemyData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CombatActorDataSO>("Assets/_Projects/Data/Combat/Enemy_Stalker.asset");
            soBattle.FindProperty("_playerView").objectReferenceValue = pView;
            soBattle.FindProperty("_enemyView").objectReferenceValue = eView;
            soBattle.FindProperty("_hud").objectReferenceValue = hud;
            soBattle.FindProperty("_audioPlayer").objectReferenceValue = audioPlayer;
            soBattle.FindProperty("_gameFeel").objectReferenceValue = gameFeel;
            soBattle.FindProperty("_inputBuffer").objectReferenceValue = inputBuf;
            soBattle.FindProperty("_timingVisualizer").objectReferenceValue = timingComp;
            soBattle.FindProperty("_qteWidget").objectReferenceValue = qteComp;

            var skillsProp = soBattle.FindProperty("_playerSkills");
            skillsProp.arraySize = 2;
            skillsProp.GetArrayElementAtIndex(0).objectReferenceValue = skillCleave;
            skillsProp.GetArrayElementAtIndex(1).objectReferenceValue = skillFlurry;

            var patternsProp = soBattle.FindProperty("_enemyAttackPatterns");
            patternsProp.arraySize = 2;
            patternsProp.GetArrayElementAtIndex(0).objectReferenceValue = patternStandard;
            patternsProp.GetArrayElementAtIndex(1).objectReferenceValue = patternSweep;

            soBattle.ApplyModifiedPropertiesWithoutUndo();

            // Save Scene
            EditorSceneManager.SaveScene(newScene, "Assets/_Projects/Scenes/CombatSandbox.unity");
            Debug.Log("[CombatSandboxBuilder] Scene updated with Milestone 3 (AP Economy, Skills, QTE Rings) and saved!");
        }
    }
}
