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
            ppRect.anchorMax = new Vector2(0.32f, 0.22f);
            ppRect.offsetMin = Vector2.zero;
            ppRect.offsetMax = Vector2.zero;
            var ppBg = playerPanel.AddComponent<Image>();
            ppBg.color = new Color(0.1f, 0.15f, 0.2f, 0.85f);

            var pNameObj = new GameObject("PlayerName");
            pNameObj.transform.SetParent(playerPanel.transform, false);
            var pnRect = pNameObj.AddComponent<RectTransform>();
            pnRect.anchorMin = new Vector2(0.05f, 0.6f);
            pnRect.anchorMax = new Vector2(0.95f, 0.95f);
            pnRect.offsetMin = Vector2.zero;
            pnRect.offsetMax = Vector2.zero;
            var pnText = pNameObj.AddComponent<TextMeshProUGUI>();
            pnText.fontSize = 28;
            pnText.fontStyle = FontStyles.Bold;
            pnText.color = new Color(0.4f, 0.8f, 1f);
            pnText.text = "GUSTAVE";

            var pHpSliderObj = new GameObject("PlayerHpSlider");
            pHpSliderObj.transform.SetParent(playerPanel.transform, false);
            var phRect = pHpSliderObj.AddComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0.05f, 0.35f);
            phRect.anchorMax = new Vector2(0.95f, 0.58f);
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;
            var pSlider = pHpSliderObj.AddComponent<Slider>();
            var phFill = new GameObject("Fill");
            phFill.transform.SetParent(pHpSliderObj.transform, false);
            var phfRect = phFill.AddComponent<RectTransform>();
            phfRect.anchorMin = Vector2.zero;
            phfRect.anchorMax = Vector2.one;
            phfRect.offsetMin = Vector2.zero;
            phfRect.offsetMax = Vector2.zero;
            var phfImg = phFill.AddComponent<Image>();
            phfImg.color = new Color(0.2f, 0.85f, 0.4f);
            pSlider.targetGraphic = phfImg;
            pSlider.fillRect = phfRect;

            var pHpTextObj = new GameObject("PlayerHpText");
            pHpTextObj.transform.SetParent(playerPanel.transform, false);
            var phtRect = pHpTextObj.AddComponent<RectTransform>();
            phtRect.anchorMin = new Vector2(0.05f, 0.05f);
            phtRect.anchorMax = new Vector2(0.95f, 0.32f);
            phtRect.offsetMin = Vector2.zero;
            phtRect.offsetMax = Vector2.zero;
            var phtText = pHpTextObj.AddComponent<TextMeshProUGUI>();
            phtText.fontSize = 22;
            phtText.text = "HP: 120 / 120";

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
            ehfRect.offsetMin = Vector2.zero;
            ehfRect.offsetMax = Vector2.zero;
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
            cmRect.anchorMin = new Vector2(0.68f, 0.05f);
            cmRect.anchorMax = new Vector2(0.96f, 0.22f);
            cmRect.offsetMin = Vector2.zero;
            cmRect.offsetMax = Vector2.zero;

            // Attack Button
            var atkBtnObj = new GameObject("Button_Attack");
            atkBtnObj.transform.SetParent(cmdMenu.transform, false);
            var atkRect = atkBtnObj.AddComponent<RectTransform>();
            atkRect.anchorMin = new Vector2(0.05f, 0.52f);
            atkRect.anchorMax = new Vector2(0.95f, 0.95f);
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
            atktComp.text = "ATTACK";
            atktComp.alignment = TextAlignmentOptions.Center;
            atktComp.fontSize = 24;
            atktComp.fontStyle = FontStyles.Bold;

            // Pass Button
            var passBtnObj = new GameObject("Button_Pass");
            passBtnObj.transform.SetParent(cmdMenu.transform, false);
            var passRect = passBtnObj.AddComponent<RectTransform>();
            passRect.anchorMin = new Vector2(0.05f, 0.05f);
            passRect.anchorMax = new Vector2(0.95f, 0.48f);
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
            passtComp.fontSize = 20;

            // Battle Log (Bottom Center)
            var logObj = new GameObject("BattleLog");
            logObj.transform.SetParent(canvasGo.transform, false);
            var logRect = logObj.AddComponent<RectTransform>();
            logRect.anchorMin = new Vector2(0.34f, 0.05f);
            logRect.anchorMax = new Vector2(0.66f, 0.15f);
            logRect.offsetMin = Vector2.zero;
            logRect.offsetMax = Vector2.zero;
            var logText = logObj.AddComponent<TextMeshProUGUI>();
            logText.fontSize = 20;
            logText.alignment = TextAlignmentOptions.Center;
            logText.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            logText.text = "Battle initialized.";

            // 5. Timing Visualizer UI (Center Screen)
            var timingObj = new GameObject("TimingVisualizer");
            timingObj.transform.SetParent(canvasGo.transform, false);
            var timingRect = timingObj.AddComponent<RectTransform>();
            timingRect.anchorMin = new Vector2(0.25f, 0.36f);
            timingRect.anchorMax = new Vector2(0.75f, 0.54f);
            timingRect.offsetMin = Vector2.zero;
            timingRect.offsetMax = Vector2.zero;
            var timingComp = timingObj.AddComponent<TimingVisualizerUI>();

            // Visualizer Prompt
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

            // Bar Track
            var trackObj = new GameObject("BarTrack");
            trackObj.transform.SetParent(timingObj.transform, false);
            var trRect = trackObj.AddComponent<RectTransform>();
            trRect.anchorMin = new Vector2(0.05f, 0.35f);
            trRect.anchorMax = new Vector2(0.95f, 0.60f);
            trRect.offsetMin = Vector2.zero;
            trRect.offsetMax = Vector2.zero;
            var trImg = trackObj.AddComponent<Image>();
            trImg.color = new Color(0.08f, 0.1f, 0.14f, 0.85f);

            // Dodge Zone (yellow)
            var dzObj = new GameObject("DodgeZone");
            dzObj.transform.SetParent(trackObj.transform, false);
            var dzRect = dzObj.AddComponent<RectTransform>();
            dzRect.anchorMin = new Vector2(0.5f, 0f);
            dzRect.anchorMax = new Vector2(0.5f, 1f);
            dzRect.sizeDelta = new Vector2(120f, 0f);
            var dzImg = dzObj.AddComponent<Image>();
            dzImg.color = new Color(0.9f, 0.8f, 0.2f, 0.45f);

            // Parry Zone (green)
            var pzObj = new GameObject("ParryZone");
            pzObj.transform.SetParent(trackObj.transform, false);
            var pzRect = pzObj.AddComponent<RectTransform>();
            pzRect.anchorMin = new Vector2(0.5f, 0f);
            pzRect.anchorMax = new Vector2(0.5f, 1f);
            pzRect.sizeDelta = new Vector2(40f, 0f);
            var pzImg = pzObj.AddComponent<Image>();
            pzImg.color = new Color(0f, 1f, 0.5f, 0.75f);

            // Strike Target Line
            var slObj = new GameObject("StrikeLine");
            slObj.transform.SetParent(trackObj.transform, false);
            var slRect = slObj.AddComponent<RectTransform>();
            slRect.anchorMin = new Vector2(0.5f, -0.2f);
            slRect.anchorMax = new Vector2(0.5f, 1.2f);
            slRect.sizeDelta = new Vector2(4f, 0f);
            var slImg = slObj.AddComponent<Image>();
            slImg.color = Color.white;

            // Cursor (approach indicator)
            var curObj = new GameObject("Cursor");
            curObj.transform.SetParent(trackObj.transform, false);
            var curRect = curObj.AddComponent<RectTransform>();
            curRect.anchorMin = new Vector2(0.5f, -0.3f);
            curRect.anchorMax = new Vector2(0.5f, 1.3f);
            curRect.sizeDelta = new Vector2(8f, 0f);
            var curImg = curObj.AddComponent<Image>();
            curImg.color = new Color(1f, 0.95f, 0.4f, 1f);

            // Result Text
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

            // Wire TimingVisualizer SerializedObject
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

            // Wire up CombatHUD SerializedObject
            var soHud = new SerializedObject(hud);
            soHud.FindProperty("_playerHpSlider").objectReferenceValue = pSlider;
            soHud.FindProperty("_playerHpText").objectReferenceValue = phtText;
            soHud.FindProperty("_playerNameText").objectReferenceValue = pnText;
            soHud.FindProperty("_enemyHpSlider").objectReferenceValue = eSlider;
            soHud.FindProperty("_enemyHpText").objectReferenceValue = ehtText;
            soHud.FindProperty("_enemyNameText").objectReferenceValue = enText;
            soHud.FindProperty("_timelineFallbackText").objectReferenceValue = tlText;
            soHud.FindProperty("_commandMenuRoot").objectReferenceValue = cmdMenu;
            soHud.FindProperty("_attackButton").objectReferenceValue = atkBtn;
            soHud.FindProperty("_passButton").objectReferenceValue = passBtn;
            soHud.FindProperty("_turnBannerText").objectReferenceValue = bText;
            soHud.FindProperty("_battleLogText").objectReferenceValue = logText;
            soHud.ApplyModifiedPropertiesWithoutUndo();

            // Create Attack Pattern Assets
            string dataDir = "Assets/_Projects/Data/Combat";
            var patternStandard = EnemyAttackPatternSO.CreateDefaultPattern("Stalker Claw Combo", AttackTelegraphType.Standard, 1.0f, 22);
            AssetDatabase.CreateAsset(patternStandard, dataDir + "/Attack_StalkerClaw.asset");

            var patternSweep = EnemyAttackPatternSO.CreateDefaultPattern("Ground Shockwave", AttackTelegraphType.GroundSweep, 1.1f, 26);
            AssetDatabase.CreateAsset(patternSweep, dataDir + "/Attack_GroundSweep.asset");

            // 6. Managers Object
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

            var patternsProp = soBattle.FindProperty("_enemyAttackPatterns");
            patternsProp.arraySize = 2;
            patternsProp.GetArrayElementAtIndex(0).objectReferenceValue = patternStandard;
            patternsProp.GetArrayElementAtIndex(1).objectReferenceValue = patternSweep;

            soBattle.ApplyModifiedPropertiesWithoutUndo();

            // Save Scene
            EditorSceneManager.SaveScene(newScene, "Assets/_Projects/Scenes/CombatSandbox.unity");
            Debug.Log("[CombatSandboxBuilder] Scene updated with Milestone 2 Active Defense and saved to Assets/_Projects/Scenes/CombatSandbox.unity");
        }
    }
}
