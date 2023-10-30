// Project: WorldCreatorBridge
// Filename: BridgeEditor.cs
// Copyright (c) 2023 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;
using System.Globalization;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR

namespace BtB.WC.Bridge
{
    [Serializable]
    public class BridgeEditor : EditorWindow, IHasCustomMenu
    {
        private class ImportPostprocessor : AssetPostprocessor
        {
            public static bool WorldCreatorModelImportActive = false;
            public static bool WorldCreatorTextureImportActive = false;

            private void OnPreprocessModel()
            {
                if (!WorldCreatorModelImportActive) return;
                
                ModelImporter modelImporter = assetImporter as ModelImporter;
                modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            }
            
            private void OnPreprocessTexture()
            {
                if (!WorldCreatorTextureImportActive) return;

                TextureImporter importer = assetImporter as TextureImporter;
                importer.alphaSource = TextureImporterAlphaSource.None;
            }
            
            private void OnPostprocessTexture(Texture2D texture)
            {
                if (!WorldCreatorTextureImportActive) return;
                TextureImporter importer = assetImporter as TextureImporter;

                // Detect original texture resolution
                Texture2D tmpTexture = new Texture2D(1, 1);
                byte[] tmpBytes = File.ReadAllBytes(importer.assetPath);
                tmpTexture.LoadImage(tmpBytes);

                importer.maxTextureSize = tmpTexture.width;
            }
        }

        #region Fields

        #region Private
        
        private BridgeLogic logic = new BridgeLogic();

        private readonly string[] toolbarItems =
        {
            "General",
            "About"
        };

        private bool locked;

        private Vector2 scrollPosGeneralTab;
        //private Vector2 scrollPosObjects; 

        private int selectedToolbarItemIndex;
        private Vector2 folderScrollPos;
        
        #endregion
        
        #region Public

        public static BridgeEditor Window;

        public ParamsBridge pb;

        private Sprite bannerWorldCreator;
        private Sprite logoYouTube;
        private Sprite logoFacebook;
        private Sprite logoTwitter;
        private Sprite logoDiscord;
        private Sprite logoArtstation;
        private Sprite logoInstagram;
        private Sprite logoVimeo;
        private Sprite logoTwitch;
        
        #endregion Public

        #endregion Fields
        
        #region Methods (Public)

        public void Awake()
        {
            LoadSettings();
        }

        #region Settings

        private string GetSettingsDirectory()
        {
            return Application.dataPath + @"/WorldCreatorBridge/Settings";
        }
        
        private string GetSettingsFilePath()
        {
            return GetSettingsDirectory() + "/BridgeSettings.json";
        }

        private void SaveSettings()
        {
            try
            {
                DirectoryInfo target = new DirectoryInfo(GetSettingsDirectory());

                if (!target.Exists)
                {
                    target.Create();
                    target.Refresh();
                }

                string settingsFilePath = GetSettingsFilePath();

                string dataAsJson = JsonUtility.ToJson(pb);
                File.WriteAllText(settingsFilePath, dataAsJson);

                // Debug.Log("Saving Bridge Settings: " + settingsFilePath);
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't save settings: " + e);
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsFilePath = GetSettingsFilePath();
                
                // Debug.Log("Loading Bridge Settings: " + settingsFilePath);

                if (File.Exists(settingsFilePath))
                {
                    string dataAsJson = File.ReadAllText(settingsFilePath);
                    pb = JsonUtility.FromJson<ParamsBridge>(dataAsJson);
                }
                else
                {
                    pb = new ParamsBridge();
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load settings: " + e);
            }
        }

        #endregion Settings

        public void Update()
        {
            logic.Update();
        }

        public void OnGUI()
        {
            if(pb == null)
                LoadSettings();

            EditorGUILayout.BeginVertical("box");
            {
                selectedToolbarItemIndex = GUILayout.Toolbar(selectedToolbarItemIndex, toolbarItems, GUILayout.Height(32));
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            {
                scrollPosGeneralTab = GUILayout.BeginScrollView(scrollPosGeneralTab);
                {
                    switch (selectedToolbarItemIndex)
                    {
                        case 0:
                            DrawTabGeneral();
                            break;
                        case 1:
                            DrawTabAbout();
                            break;
                    }
                }
                
                GUILayout.EndScrollView();
                
                GUILayout.FlexibleSpace();
            }
            
            EditorGUILayout.EndVertical();
            
            // Only show the synchronize button when a project folder has been selected
            if (pb.IsBridgeFileValid())
            {
                if (GUILayout.Button("SYNCHRONIZE", GUILayout.Height(50)))
                {
                    if (!File.Exists(pb.bridgeFilePath))
                    {
                        Debug.LogError("Selected file does not exist");
                        return;
                    }
                    
                    // Copy the sync folder...
                    string terrainFolder = Application.dataPath + "/" + pb.terrainsFolderName + "/" + pb.assetName + "/";
                    DirectoryInfo target = new DirectoryInfo(terrainFolder + "Assets");
                    DirectoryInfo source = new DirectoryInfo(pb.bridgeFilePath).Parent;

                    if (source != null && source.Parent != null)
                        source = new DirectoryInfo(source.FullName + "/Assets/");

                    if (pb.deleteUnusedAssets && Directory.Exists(@"Assets/" + pb.terrainsFolderName + "/" + pb.assetName))
                    {
                        foreach (string filePath in Directory.GetFiles(@"Assets/" + pb.terrainsFolderName + "/" + pb.assetName))
                            if (filePath.Contains(pb.assetName + "_") || filePath.EndsWith(".mat") || filePath.EndsWith(".terrainlayer") || filePath.EndsWith(".png"))
                                AssetDatabase.DeleteAsset(filePath);

                        foreach (string file in Directory.GetFiles(@"Assets/" + pb.terrainsFolderName + "/" + pb.assetName + "/Assets/"))
                            AssetDatabase.DeleteAsset(file);

                        foreach (string subdir in Directory.GetDirectories(@"Assets/" + pb.terrainsFolderName + "/" + pb.assetName + "/Assets/"))
                        {
                            foreach (string file in Directory.GetFiles(subdir))
                                AssetDatabase.DeleteAsset(file);
                            AssetDatabase.DeleteAsset(subdir);
                        }
                    }
                    
                    ImportPostprocessor.WorldCreatorModelImportActive = true;
                    ImportPostprocessor.WorldCreatorTextureImportActive = true;
                    AssetDatabase.StartAssetEditing();
                    CopyAll(source, target);
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.Refresh();
                    ImportPostprocessor.WorldCreatorModelImportActive = false;

                    // Copy color map
                    try
                    {
                        foreach (string fileName in Directory.GetFiles(source.Parent.FullName))
                            if(Path.GetFileName(fileName).Contains("colormap"))
                                File.Copy(fileName, terrainFolder + Path.GetFileName(fileName), true);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }

                    AssetDatabase.Refresh();
                    ImportPostprocessor.WorldCreatorTextureImportActive = false;

                    // ... perform synchronization ...
                    logic.Synchronize(pb);
                    
                    // save the settings for the next time the window is used
                    SaveSettings();
                }
            }
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fileInfo in source.GetFiles())
            {
                if (fileInfo.Name.Contains(".xml") || fileInfo.Name.Contains("_thumb")) continue;
                fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), true);
            }

            foreach(DirectoryInfo dirSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(dirSourceSubDir.Name);
                CopyAll(dirSourceSubDir, nextTargetSubDir);
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Lock"), locked, () => { locked = !locked; });
        }
        
        #endregion Methods (Public)
        
        #region Methods (Private)

        private void DrawTabGeneral()
        {
            float spacePixels = 8;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.MiddleLeft;
            boxStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.81f, 0.77f, 0.67f) : Color.black;
            boxStyle.stretchWidth = true;
            
            GUIStyle warningStyle = new GUIStyle(GUI.skin.box);
            warningStyle.alignment = TextAnchor.MiddleLeft;
            warningStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.988f, 0.746f, 0.02f) : Color.black;
            warningStyle.stretchWidth = true;
            

            // Reset Button
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Reset Settings", GUILayout.Width(160)))
                    pb = new ParamsBridge
                    {
                        bridgeFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/World Creator/Sync/bridge.xml"
                    };
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Box("If you moved the sync .xml please select it here. Its default location is in: \n[USER]/Documents/WorldCreator/Sync/bridge.xml", boxStyle);

            GUILayout.Space(spacePixels);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("SELECT BRIDGE .xml FILE", GUILayout.Height(30)))
                    logic.SelectProjectFolder(pb);
            }
            GUILayout.EndHorizontal();

            GUI.enabled = false;
            
            folderScrollPos = EditorGUILayout.BeginScrollView(folderScrollPos);
            { 
                string path = pb.IsBridgeFileValid() ? pb.bridgeFilePath : logic.projectFolderPath;

                EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndScrollView();
            
            GUI.enabled = true;

            GUILayout.Space(spacePixels);
            
            GUILayout.BeginHorizontal();
            {
                pb.assetName = EditorGUILayout.TextField(new GUIContent("Terrain Asset Name", "Name of the GameObject container that holds your terrain GameObject(s)."),pb.assetName, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(spacePixels);
            
            GUILayout.BeginHorizontal();
            {
                pb.deleteUnusedAssets = EditorGUILayout.Toggle(new GUIContent("Delete unused Assets", "If enabled automatically cleans up unused terrain assets."), pb.deleteUnusedAssets);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(spacePixels);
            
            GUILayout.BeginHorizontal();
            {
                pb.isImportTextures = EditorGUILayout.Toggle(new GUIContent("Import Layers", "Choose whether terrain layers are automatically imported. \n If 'false' the terrain uses only a simple colormap for texturing."), pb.isImportTextures);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(spacePixels);

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(new GUIContent("Split Threshold", "Specifies when to split the created Unity terrain. This might be important if you want to split your Unity terrain into smaller chunks (e.g. for streaming)."), GUILayout.Width(160));
                pb.userSplit = Mathf.RoundToInt(GUILayout.HorizontalSlider(pb.userSplit, 0, 5));
                GUILayout.Label((pb.UserSplit).ToString(), GUILayout.Width(36));            
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(spacePixels);

            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField) {alignment = TextAnchor.MiddleRight};

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(new GUIContent("World Scale", "Allows you to scale the terrain to a different value."), GUILayout.Width(160));

                float oldScale = pb.worldScale;
                float newScale = GUILayout.HorizontalSlider(pb.worldScale, 0, 8, GUILayout.ExpandWidth(true));
                if (oldScale != newScale)
                {
                    pb.worldScaleString = newScale.ToString("#0.00");
                    pb.worldScale = newScale;
                }

                string oldString = pb.worldScaleString;
                pb.worldScaleString = GUILayout.TextField(pb.worldScaleString, textFieldStyle, GUILayout.Width(50));
                
                if (oldString != pb.worldScaleString)
                    if (float.TryParse(pb.worldScaleString, NumberStyles.Any, CultureInfo.InvariantCulture, out float newVal))
                        pb.worldScale = newVal;
                
                GUILayout.Label("m", GUILayout.Width(14));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(spacePixels);
            
            GUILayout.BeginHorizontal();
            {
                pb.materialType = (MaterialType)EditorGUILayout.EnumPopup(new GUIContent("Material Type", "The rendering pipeline for which the terrain material will be set up for. Chose custom to set you own material."),pb.materialType, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
            
            if (pb.layerWarning)
            {
                if (pb.materialType == MaterialType.URP || pb.materialType == MaterialType.Standard)
                {
                    GUILayout.Space(spacePixels);

                    GUILayout.Box("Warning - " + (pb.materialType == MaterialType.URP ? "URP" : "The Built-in Render Pipeline") + " uses additional shader passes with more than 4 terrain layers. This can cause performance issues. To avoid this reduce your material layers in World Creator." , warningStyle);

                    GUILayout.Space(spacePixels);
                }
                else if (pb.materialType == MaterialType.HDRP)
                {
                    GUILayout.Space(spacePixels);

                    GUILayout.Box("Warning - HDRP only supports up to 8 terrain layers. Every additional layer will not be rendered. To avoid this reduce your material layers in World Creator." , warningStyle);

                    GUILayout.Space(spacePixels);
                }
            }

            if (pb.materialType == MaterialType.Custom)
            {
                GUILayout.BeginHorizontal();
                {
                    pb.customMaterial = EditorGUILayout.ObjectField("", pb.customMaterial, typeof(Material), false, GUILayout.ExpandWidth(true)) as Material;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawTabAbout()
        {
            string spritesFolder = @"Assets/WorldCreatorBridge/Content/Sprites/";
            
            bannerWorldCreator = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "banner_wc.png" : "banner_wc_inv.png"));
            logoYouTube = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_youtube.png" : "icon_youtube_inv.png"));
            logoFacebook = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_facebook.png" : "icon_facebook_inv.png"));
            logoTwitter = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_twitter.png" : "icon_twitter_inv.png"));
            logoInstagram = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_instagram.png" : "icon_instagram_inv.png"));
            logoVimeo = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_vimeo.png" : "icon_vimeo_inv.png"));
            logoTwitch = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_twitch.png" : "icon_twitch_inv.png"));
            logoDiscord = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_discord.png" : "icon_discord_inv.png"));
            logoArtstation = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_artstation.png" : "icon_artstation_inv.png"));
            
            if(bannerWorldCreator != null)
                if(GUILayout.Button(bannerWorldCreator.texture))
                    Application.OpenURL("https://www.world-creator.com");

            GUIStyle guiStyleButton = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            GUIStyle styleLegal = new GUIStyle(GUI.skin.box) { richText = true };
            GUILayoutOption[] guiLayoutOptionsHelpLarge = {GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)};

            string col = EditorGUIUtility.isProSkin ? "#D0C6AB" : "#000000";

            GUILayout.Box
            ("<color=" + col + ">\nJoin our community on DISCORD and follow us on our social sites to get the " +
             "latest information of the World Creator product series.\n\n" +
             "Get in touch with the devs and share your ideas and suggestions.\n</color>", styleLegal, guiLayoutOptionsHelpLarge);

            GUILayout.BeginHorizontal();
            {
                if (logoDiscord != null)
                    if (GUILayout.Button(logoDiscord.texture))
                        Application.OpenURL("https://discordapp.com/invite/bjMteus");
                
                if(logoFacebook != null)
                    if(GUILayout.Button(logoFacebook.texture))
                        Application.OpenURL("https://www.facebook.com/worldcreator3d");
                
                if(logoTwitter != null)
                    if(GUILayout.Button(logoTwitter.texture))
                        Application.OpenURL("https://twitter.com/worldcreator3d");
                
                if(logoYouTube != null)
                    if(GUILayout.Button(logoYouTube.texture))
                        Application.OpenURL("https://www.youtube.com/channel/UClabqa6PHVjXzR2Y7s1MP0Q");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                if (logoInstagram != null)
                    if (GUILayout.Button(logoInstagram.texture))
                        Application.OpenURL("https://www.instagram.com/worldcreator3d/");

                if (logoVimeo != null)
                    if (GUILayout.Button(logoVimeo.texture))
                        Application.OpenURL("https://vimeo.com/user82114310");

                if (logoTwitch != null)
                    if (GUILayout.Button(logoTwitch.texture))
                        Application.OpenURL("https://www.twitch.tv/worldcreator3d");

                if (logoArtstation != null)
                    if (GUILayout.Button(logoArtstation.texture))
                        Application.OpenURL("https://www.artstation.com/worldcreator");
            }
            GUILayout.EndHorizontal();

            GUILayout.Box("<color=" + col + ">\nWorld Creator Bridge for Unity \nVersion 1.0.0 \n</color>", styleLegal, guiLayoutOptionsHelpLarge);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("COMPANY", guiStyleButton))
                EditorUtility.DisplayDialog(
                    "About - Company",
                    "BiteTheBytes GmbH\n" + "Mainzer Str. 9\n" + "36039 Fulda\n\n" +
                    "Responsible: BiteTheBytes GmbH\n" + "Commercial Register Fulda: HRB 5804\n" +
                    "VAT / Ust-IdNr: DE 272746606", "OK");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("WEBSITE", guiStyleButton))
                {
                    Application.OpenURL("https://www.world-creator.com");
                }

                if (GUILayout.Button("DISCORD", guiStyleButton))
                    Application.OpenURL("https://discordapp.com/invite/bjMteus");
            }
            GUILayout.EndHorizontal();
        }

        #endregion Methods (Private)

        #region Methods (Static / Public)

        [MenuItem("Window/World Creator Bridge")]
        public static void Init()
        {
            Window = (BridgeEditor) GetWindow(typeof(BridgeEditor));
            Window.autoRepaintOnSceneChange = true;
            Window.minSize = new Vector2(425, 500);
            Window.titleContent = new GUIContent("World Creator Bridge", AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/WorldCreatorBridge/Content/Sprites/" + (EditorGUIUtility.isProSkin ? "icon_wc.png" : "icon_wc_inv.png")));
            Window.Show();
        }
        
        #endregion Methods (Static / Public)
    }
}

#endif