using UnityEditor;
using UnityEditor.SceneManagement;

namespace Gallery.Editor
{
    public static class SceneMenuEditor
    {
        private const string s_productionScenePath = "Assets/ImmersiveGallery/Scenes/Production/";
        private const string s_developmentScenePath = "Assets/ImmersiveGallery/Scenes/Development/";

        #region Menus
        [MenuItem("Gallery/Scene Shortcuts/Initializer")]
        public static void OpenInitializer()
        {
            EditorSceneManager.OpenScene(s_productionScenePath + "Initializer.unity", OpenSceneMode.Single);
        }

        [MenuItem("Gallery/Scene Shortcuts/Persistent")]
        public static void OpenPersistent()
        {
            EditorSceneManager.OpenScene(s_productionScenePath + "Persistent.unity", OpenSceneMode.Single);
        }

        [MenuItem("Gallery/Scene Shortcuts/Title")]
        public static void OpenTitle()
        {
            OpenSceneByName("Title");
        }

        [MenuItem("Gallery/Scene Shortcuts/Loading")]
        public static void OpenLoading()
        {
            OpenSceneByName("Loading");
        }

        [MenuItem("Gallery/Scene Shortcuts/ImmersiveGallery")]
        public static void OpenImmersiveGallery()
        {
            OpenSceneByName("ImmersiveGallery");
        }

        #endregion

        private static void OpenSceneByName(string name)
        {
            EditorSceneManager.OpenScene(s_productionScenePath + "Persistent.unity", OpenSceneMode.Single);
            EditorSceneManager.OpenScene(s_productionScenePath + name + ".unity", OpenSceneMode.Additive);
        }
    }
}
