using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectReadme))]
[InitializeOnLoad]
public class ProjectReadmeEditor : Editor
{
    #region Variable Field
    private static string s_showedreadmeSessionStateName = "ProjectReadmeEditor.showedReadme"; 
    private static float s_space = 16f;

    #endregion
    
    static ProjectReadmeEditor()
    {
        /* delayCall is called once after all inspectors update. */
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    #region Setup
    private static void SelectReadmeAutomatically()
    {
        /* SessionState is Key-Value Store intended for storing and retrieving Editor session state. */
        if (SessionState.GetBool(s_showedreadmeSessionStateName, false)) return;

        ProjectReadme readme = SelectReadme();
        SessionState.SetBool(s_showedreadmeSessionStateName, true);

        if (!readme || readme.LoadedLayout) return;
        LoadLayout();
        readme.LoadedLayout = true;
    }

    [MenuItem("CollieMollie/Show Readme")]
    private static ProjectReadme SelectReadme()
    {
        /* Search the asset database using the search filter string. keyword t is type */
        string[] ids = AssetDatabase.FindAssets("ProjectReadme t:ProjectReadme");
        if (ids.Length > 0)
        {
            Object readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
            /* Access to the selection in the editor. */
            Selection.objects = new []{readmeObject};
            return (ProjectReadme)readmeObject;
        }
        else
        {
            Debug.Log("[Readme] Couldn't find readme for this project.");
            return null;
        }
    }

    private static void LoadLayout()
    {
        Assembly assembly = typeof(EditorApplication).Assembly;
        System.Type windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
        MethodInfo method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Static);
        method?.Invoke(null, new object[]{Path.Combine(Application.dataPath, "ProjectInfo/Layout.wlt"), false});
    }
    #endregion
    
    #region GUIStyle Field
    [SerializeField] private GUIStyle _linkStyle = null;
    [SerializeField] private GUIStyle _titleStyle = null;
    [SerializeField] private GUIStyle _headingStyle = null;
    [SerializeField] private GUIStyle _bodyStyle = null;

    private bool _isInitialized = false;
    #endregion

    /* OnHeaderGUI controls how the header of the inspector is rendered. */
    protected override void OnHeaderGUI()
    {
        ProjectReadme readme = (ProjectReadme)target;
        InitGUIStyle(readme);

        float iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);
        GUILayout.BeginHorizontal("In BigTitle");
        {
            GUILayout.Label(readme.Icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
            GUILayout.Label(readme.Title, _titleStyle);
        }
        GUILayout.EndHorizontal();
    }

    /* OnInspectorGUI is called everytime the inspector is drawn. */
    public override void OnInspectorGUI()
    {
        ProjectReadme readme = (ProjectReadme)target;
        InitGUIStyle(readme);

        foreach (ProjectReadme.Section section in readme.Sections)
        {
            if (!string.IsNullOrEmpty(section.Heading))
            {
                GUILayout.Label(section.Heading, _headingStyle);
            }
            if (!string.IsNullOrEmpty(section.Text))
            {
                GUILayout.Label(section.Text, _bodyStyle);
            }
            if (!string.IsNullOrEmpty(section.LinkText))
            {
                if (LinkLabel(new GUIContent(section.LinkText)))
                {
                    Application.OpenURL(section.Url);
                }
            }
            GUILayout.Space(s_space);
        }
    }
    
    #region Initialize GUIStyle
    private void InitGUIStyle(ProjectReadme readme)
    {
        if (_isInitialized) return;
        /* GUIStyle is a styling information of a GUI element. */
        _bodyStyle = new GUIStyle(EditorStyles.label);
        _bodyStyle.fontSize = 23;
        _bodyStyle.font = readme.Font;
        _bodyStyle.wordWrap = true;
        
        _titleStyle = new GUIStyle(_bodyStyle);
        _titleStyle.fontSize = 60;
        _titleStyle.font = readme.Font;
        
        _headingStyle = new GUIStyle(_bodyStyle);
        _headingStyle.fontSize = 36;
        _headingStyle.font = readme.Font;

        _linkStyle = new GUIStyle(_bodyStyle);
        _bodyStyle.fontSize = 23;
        _bodyStyle.font = readme.Font;
        _linkStyle.wordWrap = false;
        _linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        _linkStyle.stretchWidth = false;

        _isInitialized = true;
    }

    private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        Rect position = GUILayoutUtility.GetRect(label, _linkStyle, options);

        Handles.BeginGUI();
        Handles.color = _linkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();
        
        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
        return GUI.Button(position, label, _linkStyle);
    }
    #endregion
}
