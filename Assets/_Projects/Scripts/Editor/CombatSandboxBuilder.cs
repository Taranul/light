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
            light.intensity = 1.25f;
            light.color = new Color(1f, 0.97f, 0.92f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Arena_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            var floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            floorMat.color = new Color(0.14f, 0.16f, 0.20f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 2. Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 3.2f, -6f);
            camGo.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
            var camController = camGo.AddComponent<CombatCameraController>();
            var soCam = new SerializedObject(camController);
            soCam.FindProperty("_targetCamera").objectReferenceValue = cam;
            soCam.ApplyModifiedPropertiesWithoutUndo();

            // 3. Characters
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Projects/Models/Idle.fbx");
            var animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Projects/Animations/CombatCharacter.controller");

            // Player Gustave
            var playerGo = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            playerGo.name = "Player_Gustave";
            playerGo.transform.position = new Vector3(-2.2f, 0f, 0f);
            playerGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            var pAnimator = playerGo.GetComponent<Animator>();
            if (pAnimator == null) pAnimator = playerGo.AddComponent<Animator>();
            pAnimator.runtimeAnimatorController = animController;
            var pView = playerGo.AddComponent<CombatActorView>();

            // Enemy Stalker
            var enemyGo = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            enemyGo.name = "Enemy_Stalker";
            enemyGo.transform.position = new Vector3(2.2f, 0f, 0f);
            enemyGo.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            var eAnimator = enemyGo.GetComponent<Animator>();
            if (eAnimator == null) eAnimator = enemyGo.AddComponent<Animator>();
            eAnimator.runtimeAnimatorController = animController;
            var eView = enemyGo.AddComponent<CombatActorView>();

            // Enemy Hitboxes (Milestone 4: Free Aim targets)
            var enemyCapsule = enemyGo.AddComponent<CapsuleCollider>();
            enemyCapsule.center = new Vector3(0f, 0.95f, 0f);
            enemyCapsule.radius = 0.35f;
            enemyCapsule.height = 1.9f;
            var bodyHitbox = enemyGo.AddComponent<CombatHitbox>();
            bodyHitbox.Initialize(HitboxType.Body, eView);

            Transform headBone = FindChildRecursive(enemyGo.transform, "Head");
            if (headBone != null)
            {
                var headSphere = headBone.gameObject.AddComponent<SphereCollider>();
                headSphere.radius = 0.22f;
                headSphere.center = new Vector3(0f, 0.08f, 0f);
                var headHitbox = headBone.gameObject.AddComponent<CombatHitbox>();
                headHitbox.Initialize(HitboxType.WeakPoint, eView);
            }

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

            // 4.1 Timeline Banner (Top Center)
            var timelinePanel = new GameObject("TimelinePanel");
            timelinePanel.transform.SetParent(canvasGo.transform, false);
            var tlRect = timelinePanel.AddComponent<RectTransform>();
            tlRect.anchorMin = new Vector2(0.20f, 0.93f);
            tlRect.anchorMax = new Vector2(0.80f, 0.985f);
            tlRect.offsetMin = Vector2.zero;
            tlRect.offsetMax = Vector2.zero;
            var tlBg = timelinePanel.AddComponent<Image>();
            tlBg.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

            var tlTextObj = new GameObject("TimelineText");
            tlTextObj.transform.SetParent(timelinePanel.transform, false);
            var tltRect = tlTextObj.AddComponent<RectTransform>();
            tltRect.anchorMin = Vector2.zero;
            tltRect.anchorMax = Vector2.one;
            tltRect.offsetMin = new Vector2(10f, 0f);
            tltRect.offsetMax = new Vector2(-10f, 0f);
            var tlText = tlTextObj.AddComponent<TextMeshProUGUI>();
            tlText.fontSize = 20;
            tlText.alignment = TextAlignmentOptions.Center;
            tlText.text = "TIMELINE";

            // 4.2 Turn Banner (Center Upper)
            var bannerObj = new GameObject("TurnBanner");
            bannerObj.transform.SetParent(canvasGo.transform, false);
            var bRect = bannerObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.25f, 0.84f);
            bRect.anchorMax = new Vector2(0.75f, 0.91f);
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            var bText = bannerObj.AddComponent<TextMeshProUGUI>();
            bText.fontSize = 34;
            bText.fontStyle = FontStyles.Bold;
            bText.alignment = TextAlignmentOptions.Center;
            bText.text = "BATTLE START";

            // 4.3 Player Status Panel (Bottom Left)
            var playerPanel = new GameObject("PlayerPanel");
            playerPanel.transform.SetParent(canvasGo.transform, false);
            var ppRect = playerPanel.AddComponent<RectTransform>();
            ppRect.anchorMin = new Vector2(0.03f, 0.04f);
            ppRect.anchorMax = new Vector2(0.28f, 0.22f);
            ppRect.offsetMin = Vector2.zero;
            ppRect.offsetMax = Vector2.zero;
            var ppBg = playerPanel.AddComponent<Image>();
            ppBg.color = new Color(0.06f, 0.09f, 0.14f, 0.92f);

            // Player Name
            var pNameObj = new GameObject("PlayerName");
            pNameObj.transform.SetParent(playerPanel.transform, false);
            var pnRect = pNameObj.AddComponent<RectTransform>();
            pnRect.anchorMin = new Vector2(0.06f, 0.68f);
            pnRect.anchorMax = new Vector2(0.94f, 0.95f);
            pnRect.offsetMin = Vector2.zero;
            pnRect.offsetMax = Vector2.zero;
            var pnText = pNameObj.AddComponent<TextMeshProUGUI>();
            pnText.fontSize = 24;
            pnText.fontStyle = FontStyles.Bold;
            pnText.color = new Color(0.35f, 0.82f, 1f);
            pnText.text = "GUSTAVE";

            // Player HP Bar Background
            var phpBgObj = new GameObject("HpBarBackground");
            phpBgObj.transform.SetParent(playerPanel.transform, false);
            var phpBgRect = phpBgObj.AddComponent<RectTransform>();
            phpBgRect.anchorMin = new Vector2(0.06f, 0.44f);
            phpBgRect.anchorMax = new Vector2(0.94f, 0.58f);
            phpBgRect.offsetMin = Vector2.zero;
            phpBgRect.offsetMax = Vector2.zero;
            var phpBgImg = phpBgObj.AddComponent<Image>();
            phpBgImg.color = new Color(0.12f, 0.15f, 0.22f, 1f);

            // Player HP Fill
            var phpFillObj = new GameObject("HpBarFill");
            phpFillObj.transform.SetParent(phpBgObj.transform, false);
            var phpFillRect = phpFillObj.AddComponent<RectTransform>();
            phpFillRect.anchorMin = Vector2.zero;
            phpFillRect.anchorMax = Vector2.one;
            phpFillRect.offsetMin = Vector2.zero;
            phpFillRect.offsetMax = Vector2.zero;
            var phpFillImg = phpFillObj.AddComponent<Image>();
            phpFillImg.type = Image.Type.Filled;
            phpFillImg.fillMethod = Image.FillMethod.Horizontal;
            phpFillImg.fillAmount = 1f;
            phpFillImg.color = new Color(0f, 0.9f, 0.6f);

            // Player HP Text
            var phtObj = new GameObject("HpText");
            phtObj.transform.SetParent(playerPanel.transform, false);
            var phtRect = phtObj.AddComponent<RectTransform>();
            phtRect.anchorMin = new Vector2(0.50f, 0.64f);
            phtRect.anchorMax = new Vector2(0.94f, 0.95f);
            phtRect.offsetMin = Vector2.zero;
            phtRect.offsetMax = Vector2.zero;
            var phtText = phtObj.AddComponent<TextMeshProUGUI>();
            phtText.fontSize = 18;
            phtText.alignment = TextAlignmentOptions.Right;
            phtText.color = new Color(0.85f, 0.95f, 0.9f);
            phtText.text = "HP  <b>120</b> / 120";

            // Player AP Row
            var patObj = new GameObject("ApText");
            patObj.transform.SetParent(playerPanel.transform, false);
            var patRect = patObj.AddComponent<RectTransform>();
            patRect.anchorMin = new Vector2(0.06f, 0.10f);
            patRect.anchorMax = new Vector2(0.40f, 0.34f);
            patRect.offsetMin = Vector2.zero;
            patRect.offsetMax = Vector2.zero;
            var patText = patObj.AddComponent<TextMeshProUGUI>();
            patText.fontSize = 18;
            patText.alignment = TextAlignmentOptions.Left;
            patText.color = new Color(0.25f, 0.75f, 1f);
            patText.text = "AP  <b>2</b> / 6";

            // Player AP Pips Container (6 discrete pips)
            var pipsObj = new GameObject("ApPipsContainer");
            pipsObj.transform.SetParent(playerPanel.transform, false);
            var pipsRect = pipsObj.AddComponent<RectTransform>();
            pipsRect.anchorMin = new Vector2(0.38f, 0.10f);
            pipsRect.anchorMax = new Vector2(0.94f, 0.34f);
            pipsRect.offsetMin = Vector2.zero;
            pipsRect.offsetMax = Vector2.zero;
            var pipsHlg = pipsObj.AddComponent<HorizontalLayoutGroup>();
            pipsHlg.spacing = 8f;
            pipsHlg.childAlignment = TextAnchor.MiddleRight;
            pipsHlg.childControlWidth = false;
            pipsHlg.childControlHeight = false;
            pipsHlg.childForceExpandWidth = false;
            pipsHlg.childForceExpandHeight = false;

            for (int i = 0; i < 6; i++)
            {
                var pip = new GameObject($"Pip_{i}");
                pip.transform.SetParent(pipsObj.transform, false);
                var pipRect = pip.AddComponent<RectTransform>();
                pipRect.sizeDelta = new Vector2(22f, 18f);
                var pipImg = pip.AddComponent<Image>();
                pipImg.color = (i < 2) ? new Color(0.1f, 0.8f, 1f, 1f) : new Color(0.15f, 0.22f, 0.3f, 0.5f);
            }

            // 4.4 Enemy Status Panel (Top Right)
            var enemyPanel = new GameObject("EnemyPanel");
            enemyPanel.transform.SetParent(canvasGo.transform, false);
            var epRect = enemyPanel.AddComponent<RectTransform>();
            epRect.anchorMin = new Vector2(0.72f, 0.78f);
            epRect.anchorMax = new Vector2(0.97f, 0.93f);
            epRect.offsetMin = Vector2.zero;
            epRect.offsetMax = Vector2.zero;
            var epBg = enemyPanel.AddComponent<Image>();
            epBg.color = new Color(0.14f, 0.06f, 0.08f, 0.92f);

            var eNameObj = new GameObject("EnemyName");
            eNameObj.transform.SetParent(enemyPanel.transform, false);
            var enRect = eNameObj.AddComponent<RectTransform>();
            enRect.anchorMin = new Vector2(0.06f, 0.55f);
            enRect.anchorMax = new Vector2(0.60f, 0.95f);
            enRect.offsetMin = Vector2.zero;
            enRect.offsetMax = Vector2.zero;
            var enText = eNameObj.AddComponent<TextMeshProUGUI>();
            enText.fontSize = 22;
            enText.fontStyle = FontStyles.Bold;
            enText.color = new Color(1f, 0.35f, 0.4f);
            enText.text = "EXPEDITION STALKER";

            var ehpBgObj = new GameObject("HpBarBackground");
            ehpBgObj.transform.SetParent(enemyPanel.transform, false);
            var ehpBgRect = ehpBgObj.AddComponent<RectTransform>();
            ehpBgRect.anchorMin = new Vector2(0.06f, 0.18f);
            ehpBgRect.anchorMax = new Vector2(0.94f, 0.42f);
            ehpBgRect.offsetMin = Vector2.zero;
            ehpBgRect.offsetMax = Vector2.zero;
            var ehpBgImg = ehpBgObj.AddComponent<Image>();
            ehpBgImg.color = new Color(0.24f, 0.12f, 0.14f, 1f);

            var ehpFillObj = new GameObject("HpBarFill");
            ehpFillObj.transform.SetParent(ehpBgObj.transform, false);
            var ehpFillRect = ehpFillObj.AddComponent<RectTransform>();
            ehpFillRect.anchorMin = Vector2.zero;
            ehpFillRect.anchorMax = Vector2.one;
            ehpFillRect.offsetMin = Vector2.zero;
            ehpFillRect.offsetMax = Vector2.zero;
            var ehpFillImg = ehpFillObj.AddComponent<Image>();
            ehpFillImg.type = Image.Type.Filled;
            ehpFillImg.fillMethod = Image.FillMethod.Horizontal;
            ehpFillImg.fillAmount = 1f;
            ehpFillImg.color = new Color(1f, 0.22f, 0.28f);

            var ehtObj = new GameObject("HpText");
            ehtObj.transform.SetParent(enemyPanel.transform, false);
            var ehtRect = ehtObj.AddComponent<RectTransform>();
            ehtRect.anchorMin = new Vector2(0.55f, 0.55f);
            ehtRect.anchorMax = new Vector2(0.94f, 0.95f);
            ehtRect.offsetMin = Vector2.zero;
            ehtRect.offsetMax = Vector2.zero;
            var ehtText = ehtObj.AddComponent<TextMeshProUGUI>();
            ehtText.fontSize = 18;
            ehtText.alignment = TextAlignmentOptions.Right;
            ehtText.color = new Color(1f, 0.85f, 0.85f);
            ehtText.text = "HP  <b>90</b> / 90";

            // 4.5 Command Menu (Bottom Right)
            var cmdMenu = new GameObject("CommandMenu");
            cmdMenu.transform.SetParent(canvasGo.transform, false);
            var cmRect = cmdMenu.AddComponent<RectTransform>();
            cmRect.anchorMin = new Vector2(0.76f, 0.04f);
            cmRect.anchorMax = new Vector2(0.97f, 0.27f);
            cmRect.offsetMin = Vector2.zero;
            cmRect.offsetMax = Vector2.zero;
            var cmVlg = cmdMenu.AddComponent<VerticalLayoutGroup>();
            cmVlg.spacing = 8f;
            cmVlg.childControlHeight = true;
            cmVlg.childControlWidth = true;
            cmVlg.childForceExpandHeight = true;
            cmVlg.childForceExpandWidth = true;

            // Attack Button
            var atkBtnObj = new GameObject("Button_Attack");
            atkBtnObj.transform.SetParent(cmdMenu.transform, false);
            var atkImg = atkBtnObj.AddComponent<Image>();
            atkImg.color = new Color(0.12f, 0.24f, 0.38f, 0.95f);
            var atkBtn = atkBtnObj.AddComponent<Button>();
            var atkTm = new GameObject("Text");
            atkTm.transform.SetParent(atkBtnObj.transform, false);
            var atktRect = atkTm.AddComponent<RectTransform>();
            atktRect.anchorMin = Vector2.zero;
            atktRect.anchorMax = Vector2.one;
            var atktComp = atkTm.AddComponent<TextMeshProUGUI>();
            atktComp.text = "ATTACK  <color=#00D0FF>[+1 AP]</color>";
            atktComp.alignment = TextAlignmentOptions.Center;
            atktComp.fontSize = 19;
            atktComp.fontStyle = FontStyles.Bold;

            // Skills Button
            var skillBtnObj = new GameObject("Button_Skills");
            skillBtnObj.transform.SetParent(cmdMenu.transform, false);
            var skImg = skillBtnObj.AddComponent<Image>();
            skImg.color = new Color(0.26f, 0.16f, 0.38f, 0.95f);
            var skBtn = skillBtnObj.AddComponent<Button>();
            var skTm = new GameObject("Text");
            skTm.transform.SetParent(skillBtnObj.transform, false);
            var sktRect = skTm.AddComponent<RectTransform>();
            sktRect.anchorMin = Vector2.zero;
            sktRect.anchorMax = Vector2.one;
            var sktComp = skTm.AddComponent<TextMeshProUGUI>();
            sktComp.text = "SKILLS";
            sktComp.alignment = TextAlignmentOptions.Center;
            sktComp.fontSize = 19;
            sktComp.fontStyle = FontStyles.Bold;

            // Free Aim Button
            var freeAimBtnObj = new GameObject("Button_FreeAim");
            freeAimBtnObj.transform.SetParent(cmdMenu.transform, false);
            var faImg = freeAimBtnObj.AddComponent<Image>();
            faImg.color = new Color(0.16f, 0.28f, 0.40f, 0.95f);
            var faBtn = freeAimBtnObj.AddComponent<Button>();
            var faTm = new GameObject("Text");
            faTm.transform.SetParent(freeAimBtnObj.transform, false);
            var fatRect = faTm.AddComponent<RectTransform>();
            fatRect.anchorMin = Vector2.zero;
            fatRect.anchorMax = Vector2.one;
            var fatComp = faTm.AddComponent<TextMeshProUGUI>();
            fatComp.text = "FREE AIM  <color=#00D0FF>[1 AP]</color>";
            fatComp.alignment = TextAlignmentOptions.Center;
            fatComp.fontSize = 19;
            fatComp.fontStyle = FontStyles.Bold;

            // Pass Button
            var passBtnObj = new GameObject("Button_Pass");
            passBtnObj.transform.SetParent(cmdMenu.transform, false);
            var passImg = passBtnObj.AddComponent<Image>();
            passImg.color = new Color(0.18f, 0.20f, 0.24f, 0.95f);
            var passBtn = passBtnObj.AddComponent<Button>();
            var passTm = new GameObject("Text");
            passTm.transform.SetParent(passBtnObj.transform, false);
            var passtRect = passTm.AddComponent<RectTransform>();
            passtRect.anchorMin = Vector2.zero;
            passtRect.anchorMax = Vector2.one;
            var passtComp = passTm.AddComponent<TextMeshProUGUI>();
            passtComp.text = "PASS TURN";
            passtComp.alignment = TextAlignmentOptions.Center;
            passtComp.fontSize = 17;

            // 4.6 Skill Submenu Panel
            var skillMenuPanel = new GameObject("SkillMenuPanel");
            skillMenuPanel.transform.SetParent(canvasGo.transform, false);
            var smRect = skillMenuPanel.AddComponent<RectTransform>();
            smRect.anchorMin = new Vector2(0.74f, 0.24f);
            smRect.anchorMax = new Vector2(0.97f, 0.52f);
            smRect.offsetMin = Vector2.zero;
            smRect.offsetMax = Vector2.zero;
            var smBg = skillMenuPanel.AddComponent<Image>();
            smBg.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);

            var skillContainer = new GameObject("SkillButtonsContainer");
            skillContainer.transform.SetParent(skillMenuPanel.transform, false);
            var scRect = skillContainer.AddComponent<RectTransform>();
            scRect.anchorMin = new Vector2(0.06f, 0.24f);
            scRect.anchorMax = new Vector2(0.94f, 0.94f);
            scRect.offsetMin = Vector2.zero;
            scRect.offsetMax = Vector2.zero;
            var vlg = skillContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;

            var backBtnObj = new GameObject("Button_Back");
            backBtnObj.transform.SetParent(skillMenuPanel.transform, false);
            var bkRect = backBtnObj.AddComponent<RectTransform>();
            bkRect.anchorMin = new Vector2(0.06f, 0.05f);
            bkRect.anchorMax = new Vector2(0.94f, 0.20f);
            bkRect.offsetMin = Vector2.zero;
            bkRect.offsetMax = Vector2.zero;
            var bkImg = backBtnObj.AddComponent<Image>();
            bkImg.color = new Color(0.24f, 0.26f, 0.32f, 1f);
            var bkBtn = backBtnObj.AddComponent<Button>();
            var bkTm = new GameObject("Text");
            bkTm.transform.SetParent(backBtnObj.transform, false);
            var bktRect = bkTm.AddComponent<RectTransform>();
            bktRect.anchorMin = Vector2.zero;
            bktRect.anchorMax = Vector2.one;
            var bktComp = bkTm.AddComponent<TextMeshProUGUI>();
            bktComp.text = "BACK";
            bktComp.alignment = TextAlignmentOptions.Center;
            bktComp.fontSize = 17;

            // 4.7 Battle Log (Bottom Center)
            var logObj = new GameObject("BattleLog");
            logObj.transform.SetParent(canvasGo.transform, false);
            var logRect = logObj.AddComponent<RectTransform>();
            logRect.anchorMin = new Vector2(0.30f, 0.04f);
            logRect.anchorMax = new Vector2(0.70f, 0.12f);
            logRect.offsetMin = Vector2.zero;
            logRect.offsetMax = Vector2.zero;
            var logText = logObj.AddComponent<TextMeshProUGUI>();
            logText.fontSize = 19;
            logText.alignment = TextAlignmentOptions.Center;
            logText.color = new Color(0.9f, 0.9f, 0.9f, 0.85f);
            logText.text = "Battle initialized.";

            // 5. Timing Visualizer UI (Center Screen for Defense)
            var timingObj = new GameObject("TimingVisualizer");
            timingObj.transform.SetParent(canvasGo.transform, false);
            var timingRect = timingObj.AddComponent<RectTransform>();
            timingRect.anchorMin = new Vector2(0.28f, 0.38f);
            timingRect.anchorMax = new Vector2(0.72f, 0.52f);
            timingRect.offsetMin = Vector2.zero;
            timingRect.offsetMax = Vector2.zero;
            var timingComp = timingObj.AddComponent<TimingVisualizerUI>();

            var promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(timingObj.transform, false);
            var prRect = promptObj.AddComponent<RectTransform>();
            prRect.anchorMin = new Vector2(0f, 0.62f);
            prRect.anchorMax = new Vector2(1f, 1f);
            prRect.offsetMin = Vector2.zero;
            prRect.offsetMax = Vector2.zero;
            var prText = promptObj.AddComponent<TextMeshProUGUI>();
            prText.fontSize = 22;
            prText.alignment = TextAlignmentOptions.Center;
            prText.text = "[F] PARRY  |  [SPACE] DODGE";

            var trackObj = new GameObject("BarTrack");
            trackObj.transform.SetParent(timingObj.transform, false);
            var trRect = trackObj.AddComponent<RectTransform>();
            trRect.anchorMin = new Vector2(0.05f, 0.35f);
            trRect.anchorMax = new Vector2(0.95f, 0.58f);
            trRect.offsetMin = Vector2.zero;
            trRect.offsetMax = Vector2.zero;
            var trImg = trackObj.AddComponent<Image>();
            trImg.color = new Color(0.06f, 0.08f, 0.12f, 0.90f);

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
            slRect.sizeDelta = new Vector2(3f, 0f);
            var slImg = slObj.AddComponent<Image>();
            slImg.color = Color.white;

            var curObj = new GameObject("Cursor");
            curObj.transform.SetParent(trackObj.transform, false);
            var curRect = curObj.AddComponent<RectTransform>();
            curRect.anchorMin = new Vector2(0.5f, -0.3f);
            curRect.anchorMax = new Vector2(0.5f, 1.3f);
            curRect.sizeDelta = new Vector2(6f, 0f);
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
            resText.fontSize = 26;
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

            // 6. Offensive QTE Widget
            var qteObj = new GameObject("OffensiveQTEWidget");
            qteObj.transform.SetParent(canvasGo.transform, false);
            var qteRect = qteObj.AddComponent<RectTransform>();
            qteRect.sizeDelta = new Vector2(300f, 300f);
            var qteComp = qteObj.AddComponent<OffensiveQTEWidget>();

            var trRingObj = new GameObject("TargetRing");
            trRingObj.transform.SetParent(qteObj.transform, false);
            var trrRect = trRingObj.AddComponent<RectTransform>();
            trrRect.sizeDelta = new Vector2(90f, 90f);
            var trrImg = trRingObj.AddComponent<Image>();
            trrImg.color = new Color(1f, 1f, 1f, 0.85f);

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

            // 7. Free Aim HUD Overlay
            var freeAimHudObj = new GameObject("FreeAimHUD");
            freeAimHudObj.transform.SetParent(canvasGo.transform, false);
            var faHudRect = freeAimHudObj.AddComponent<RectTransform>();
            faHudRect.anchorMin = Vector2.zero;
            faHudRect.anchorMax = Vector2.one;
            faHudRect.offsetMin = Vector2.zero;
            faHudRect.offsetMax = Vector2.zero;
            var freeAimComp = freeAimHudObj.AddComponent<FreeAimHUD>();

            var faTopPanel = new GameObject("TopPanel");
            faTopPanel.transform.SetParent(freeAimHudObj.transform, false);
            var fatpRect = faTopPanel.AddComponent<RectTransform>();
            fatpRect.anchorMin = new Vector2(0.35f, 0.86f);
            fatpRect.anchorMax = new Vector2(0.65f, 0.94f);
            fatpRect.offsetMin = Vector2.zero;
            fatpRect.offsetMax = Vector2.zero;
            var fatpBg = faTopPanel.AddComponent<Image>();
            fatpBg.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

            var faTimerTrack = new GameObject("TimerTrack");
            faTimerTrack.transform.SetParent(faTopPanel.transform, false);
            var fattRect = faTimerTrack.AddComponent<RectTransform>();
            fattRect.anchorMin = new Vector2(0.05f, 0.55f);
            fattRect.anchorMax = new Vector2(0.95f, 0.85f);
            fattRect.offsetMin = Vector2.zero;
            fattRect.offsetMax = Vector2.zero;
            var fattBg = faTimerTrack.AddComponent<Image>();
            fattBg.color = new Color(0.12f, 0.16f, 0.22f, 1f);

            var faTimerFill = new GameObject("TimerFill");
            faTimerFill.transform.SetParent(faTimerTrack.transform, false);
            var fatfRect = faTimerFill.AddComponent<RectTransform>();
            fatfRect.anchorMin = Vector2.zero;
            fatfRect.anchorMax = Vector2.one;
            fatfRect.offsetMin = Vector2.zero;
            fatfRect.offsetMax = Vector2.zero;
            var fatfImg = faTimerFill.AddComponent<Image>();
            fatfImg.type = Image.Type.Filled;
            fatfImg.fillMethod = Image.FillMethod.Horizontal;
            fatfImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fatfImg.fillAmount = 1f;
            fatfImg.color = new Color(0.2f, 0.8f, 1f, 1f);

            var faTimerTextObj = new GameObject("TimerText");
            faTimerTextObj.transform.SetParent(faTopPanel.transform, false);
            var fttRect = faTimerTextObj.AddComponent<RectTransform>();
            fttRect.anchorMin = new Vector2(0.05f, 0.08f);
            fttRect.anchorMax = new Vector2(0.48f, 0.50f);
            fttRect.offsetMin = Vector2.zero;
            fttRect.offsetMax = Vector2.zero;
            var fttComp = faTimerTextObj.AddComponent<TextMeshProUGUI>();
            fttComp.fontSize = 20;
            fttComp.fontStyle = FontStyles.Bold;
            fttComp.alignment = TextAlignmentOptions.MidlineLeft;
            fttComp.text = "4.0s";

            var faAmmoTextObj = new GameObject("AmmoText");
            faAmmoTextObj.transform.SetParent(faTopPanel.transform, false);
            var fatTextRect = faAmmoTextObj.AddComponent<RectTransform>();
            fatTextRect.anchorMin = new Vector2(0.50f, 0.08f);
            fatTextRect.anchorMax = new Vector2(0.95f, 0.50f);
            fatTextRect.offsetMin = Vector2.zero;
            fatTextRect.offsetMax = Vector2.zero;
            var fatTextComp = faAmmoTextObj.AddComponent<TextMeshProUGUI>();
            fatTextComp.fontSize = 19;
            fatTextComp.alignment = TextAlignmentOptions.MidlineRight;
            fatTextComp.text = "SHOTS: <b>2</b> [1 AP / SHOT]";

            var faPromptObj = new GameObject("WeakPointPrompt");
            faPromptObj.transform.SetParent(freeAimHudObj.transform, false);
            var fapRect = faPromptObj.AddComponent<RectTransform>();
            fapRect.anchorMin = new Vector2(0.3f, 0.77f);
            fapRect.anchorMax = new Vector2(0.7f, 0.84f);
            fapRect.offsetMin = Vector2.zero;
            fapRect.offsetMax = Vector2.zero;
            var fapText = faPromptObj.AddComponent<TextMeshProUGUI>();
            fapText.fontSize = 24;
            fapText.fontStyle = FontStyles.Bold;
            fapText.alignment = TextAlignmentOptions.Center;
            fapText.text = "";

            var faCtrlObj = new GameObject("ControlsPrompt");
            faCtrlObj.transform.SetParent(freeAimHudObj.transform, false);
            var facRect = faCtrlObj.AddComponent<RectTransform>();
            facRect.anchorMin = new Vector2(0.2f, 0.04f);
            facRect.anchorMax = new Vector2(0.8f, 0.10f);
            facRect.offsetMin = Vector2.zero;
            facRect.offsetMax = Vector2.zero;
            var facText = faCtrlObj.AddComponent<TextMeshProUGUI>();
            facText.fontSize = 20;
            facText.alignment = TextAlignmentOptions.Center;
            facText.text = "<b>[LEFT CLICK]</b> FIRE (1 AP)   |   <b>[RIGHT CLICK / ESC]</b> FINISH";

            // Procedural Targeting Reticle
            var reticleTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            reticleTex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                    bool isOuterRing = dist >= 22f && dist <= 25f;
                    bool isCrosshairTick = (dist >= 12f && dist <= 29f) && (Mathf.Abs(x - 31.5f) <= 1f || Mathf.Abs(y - 31.5f) <= 1f);
                    bool isCenterDot = dist <= 3f;
                    if (isOuterRing || isCrosshairTick || isCenterDot)
                        reticleTex.SetPixel(x, y, Color.white);
                    else
                        reticleTex.SetPixel(x, y, Color.clear);
                }
            }
            reticleTex.Apply();
            var reticleSprite = Sprite.Create(reticleTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));

            var crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(freeAimHudObj.transform, false);
            var crossRect = crosshairObj.AddComponent<RectTransform>();
            crossRect.sizeDelta = new Vector2(64f, 64f);
            var crossImg = crosshairObj.AddComponent<Image>();
            crossImg.sprite = reticleSprite;
            crossImg.color = new Color(1f, 1f, 1f, 0.9f);

            var dotObj = new GameObject("CenterDot");
            dotObj.transform.SetParent(crosshairObj.transform, false);
            var dotRect = dotObj.AddComponent<RectTransform>();
            dotRect.sizeDelta = new Vector2(4f, 4f);
            var dotImg = dotObj.AddComponent<Image>();
            dotImg.color = new Color(1f, 0.2f, 0.2f, 0.95f);

            var soFa = new SerializedObject(freeAimComp);
            soFa.FindProperty("_container").objectReferenceValue = freeAimHudObj;
            soFa.FindProperty("_crosshairRect").objectReferenceValue = crossRect;
            soFa.FindProperty("_crosshairImage").objectReferenceValue = crossImg;
            soFa.FindProperty("_crosshairCenterDot").objectReferenceValue = dotImg;
            soFa.FindProperty("_timerFillImage").objectReferenceValue = fatfImg;
            soFa.FindProperty("_timerText").objectReferenceValue = fttComp;
            soFa.FindProperty("_ammoText").objectReferenceValue = fatTextComp;
            soFa.FindProperty("_weakPointPromptText").objectReferenceValue = fapText;
            soFa.FindProperty("_controlsPromptText").objectReferenceValue = facText;
            soFa.ApplyModifiedPropertiesWithoutUndo();

            freeAimHudObj.SetActive(false);

            // Wire CombatHUD SerializedObject
            var soHud = new SerializedObject(hud);
            soHud.FindProperty("_playerHpFill").objectReferenceValue = phpFillImg;
            soHud.FindProperty("_playerHpText").objectReferenceValue = phtText;
            soHud.FindProperty("_playerNameText").objectReferenceValue = pnText;
            soHud.FindProperty("_playerApPipsContainer").objectReferenceValue = pipsObj.transform;
            soHud.FindProperty("_playerApText").objectReferenceValue = patText;
            soHud.FindProperty("_enemyHpFill").objectReferenceValue = ehpFillImg;
            soHud.FindProperty("_enemyHpText").objectReferenceValue = ehtText;
            soHud.FindProperty("_enemyNameText").objectReferenceValue = enText;
            soHud.FindProperty("_timelineText").objectReferenceValue = tlText;
            soHud.FindProperty("_commandMenuRoot").objectReferenceValue = cmdMenu;
            soHud.FindProperty("_attackButton").objectReferenceValue = atkBtn;
            soHud.FindProperty("_skillsButton").objectReferenceValue = skBtn;
            soHud.FindProperty("_freeAimButton").objectReferenceValue = faBtn;
            soHud.FindProperty("_passButton").objectReferenceValue = passBtn;
            soHud.FindProperty("_skillMenuRoot").objectReferenceValue = skillMenuPanel;
            soHud.FindProperty("_skillButtonsContainer").objectReferenceValue = skillContainer.transform;
            soHud.FindProperty("_skillBackBtn").objectReferenceValue = bkBtn;
            soHud.FindProperty("_turnBannerText").objectReferenceValue = bText;
            soHud.FindProperty("_battleLogText").objectReferenceValue = logText;
            soHud.ApplyModifiedPropertiesWithoutUndo();

            // Create Skills and Attack Pattern Assets
            string dataDir = "Assets/_Projects/Data/Combat";
            var skillCleave = AssetDatabase.LoadAssetAtPath<SkillDefinitionSO>(dataDir + "/Skill_OverchargeCleave.asset");
            if (skillCleave == null)
            {
                skillCleave = SkillDefinitionSO.CreateSkill("Overcharge Cleave", 3, 2.2f, "Heavy devastating blow with bonus break force.");
                AssetDatabase.CreateAsset(skillCleave, dataDir + "/Skill_OverchargeCleave.asset");
            }

            var skillFlurry = AssetDatabase.LoadAssetAtPath<SkillDefinitionSO>(dataDir + "/Skill_SwiftFlurry.asset");
            if (skillFlurry == null)
            {
                skillFlurry = SkillDefinitionSO.CreateSkill("Swift Flurry", 2, 1.5f, "Quick dual-hit flurry.");
                AssetDatabase.CreateAsset(skillFlurry, dataDir + "/Skill_SwiftFlurry.asset");
            }

            var patternStandard = AssetDatabase.LoadAssetAtPath<EnemyAttackPatternSO>(dataDir + "/Attack_StalkerClaw.asset");
            if (patternStandard == null)
            {
                patternStandard = EnemyAttackPatternSO.CreateDefaultPattern("Stalker Claw Combo", AttackTelegraphType.Standard, 1.0f, 22);
                AssetDatabase.CreateAsset(patternStandard, dataDir + "/Attack_StalkerClaw.asset");
            }

            var patternSweep = AssetDatabase.LoadAssetAtPath<EnemyAttackPatternSO>(dataDir + "/Attack_GroundSweep.asset");
            if (patternSweep == null)
            {
                patternSweep = EnemyAttackPatternSO.CreateDefaultPattern("Ground Shockwave", AttackTelegraphType.GroundSweep, 1.1f, 26);
                AssetDatabase.CreateAsset(patternSweep, dataDir + "/Attack_GroundSweep.asset");
            }

            // 8. Managers Object
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
            soBattle.FindProperty("_cameraController").objectReferenceValue = camController;
            soBattle.FindProperty("_freeAimHUD").objectReferenceValue = freeAimComp;

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
            Debug.Log("[CombatSandboxBuilder] Scene rebuilt with polished UI hierarchy and Free Aim support!");
        }

        private static Transform FindChildRecursive(Transform parent, string partialName)
        {
            foreach (Transform child in parent)
            {
                if (child.name.IndexOf(partialName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return child;

                var found = FindChildRecursive(child, partialName);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
