using System;
using UnityEngine;

[CreateAssetMenu( fileName = "ProjectReadme", menuName = "CollieMollie/Readme")]
public class ProjectReadme : ScriptableObject
{
    #region Variable Field
    [SerializeField] private Texture2D _icon = null;
    public Texture2D Icon => _icon;
    
    [SerializeField] private string _title = null;
    public string Title => _title ?? "NULL";
    
    [SerializeField] private Section[] _sections = null;
    public Section[] Sections => _sections;

    [SerializeField] private bool _loadedLayout = false;
    public bool LoadedLayout { get => _loadedLayout; set => _loadedLayout = value; }

    [SerializeField] private Font _font = null;
    public Font Font => _font;
    #endregion

    [Serializable]
    public class Section
    {
        public string Heading = null;
        public string Text = null;
        public string LinkText = null;
        public string Url = null;
    }
}
