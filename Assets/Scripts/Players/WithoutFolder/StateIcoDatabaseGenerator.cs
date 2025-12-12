using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public static class StateIcoDatabaseGenerator
{
    private const string GeneratedPath = "Assets/Resources/StateIcoDatabase_Generated.asset";
    private const string IconsFolderPath = "Assets/Sprites/StateIcons";

    [MenuItem("Tools/Generate StateIcoDatabase")]
    public static void Generate()
    {
        var database = ScriptableObject.CreateInstance<StateIcoDatabase>();
        var allStates = Enum.GetValues(typeof(States)).Cast<States>();

        foreach (var state in allStates)
        {
            var icon = FindSpriteByName(state.ToString());

            database.Entries.Add(new StateIcoData
            {
                State = state,
                Icon = icon,
                BorderColor = Color.white
            });
        }

        AssetDatabase.CreateAsset(database, GeneratedPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = database;
    }

    private static Sprite FindSpriteByName(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Sprite", new[] { IconsFolderPath });
        if (guids.Length == 0) return null;

        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}
