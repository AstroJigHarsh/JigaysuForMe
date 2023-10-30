// Project: WorldCreatorBridge
// Filename: BridgeLogic.cs
// Copyright (c) 2023 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
namespace BtB.WC.Bridge
{
    public class BridgeLogic
    {
        #region Fields (Static / Public)

        /// <summary>
        ///   The path to the project folder.
        /// </summary>
        public string projectFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/World Creator/Sync/bridge.xml";
        
        #endregion

        /// <summary>
        ///   Updates the logic, e.g. auto synchronization.
        /// </summary>
        public void Update()
        {
            
        }

        /// <summary>
        ///   Opens a window with which you can select a project.
        ///   Returns null if the user pressed cancel.
        /// </summary>
        public void SelectProjectFolder(ParamsBridge bp)
        {
            #if UNITY_EDITOR
            
            string path = bp.bridgeFilePath ?? projectFolderPath;

            path = EditorUtility.OpenFilePanel("Select World Creator Bridge XML File", path, "xml");

            if (!string.IsNullOrEmpty(path))
                bp.bridgeFilePath = path;
            
            #endif
        }

        /// <summary>
        ///   Synchronize/Update the terrain.
        /// </summary>
        public void Synchronize(ParamsBridge @params)
        {
            if (!@params.IsBridgeFileValid()) return;

            UnityTerrainUtility.CreateTerrainFromFile(@params);
            // UnityTerrainUtilityBackup.CreateTerrainFromFile(settings);
        }
    }
}

#endif