using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

[CustomEditor(typeof(BodyAnimationManager))]
public class BodyAnimationManager_Inspector : Editor
{
    public VisualTreeAsset InspectorXML;
    private static List<AnimationSection> m_SectionDatabase = new List<AnimationSection>();

    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement myInspector = new VisualElement();

        // Add a simple label
        myInspector.Add(new Label("This is a custom inspector"));

        // Load and clone a visual tree from UXML
        VisualTreeAsset visualTree = InspectorXML;
        InspectorXML.CloneTree(myInspector);

        // Return the finished inspector UI
        return myInspector;
    }
}
