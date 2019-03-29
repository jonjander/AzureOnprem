using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameBuilder
{
    static void build()
    {

        // Place all your scenes here
        string[] scenes = {"Assets/scenes/SampleScene.unity"};

        string pathToDeploy = "bin/AzureOnprem.exe";

        BuildPipeline.BuildPlayer(scenes, pathToDeploy, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

}
