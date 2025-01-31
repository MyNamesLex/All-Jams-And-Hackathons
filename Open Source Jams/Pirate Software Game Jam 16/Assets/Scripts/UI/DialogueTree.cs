using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DialogueTree))]
public class DialogueTreeEditor : Editor
{
    private SerializedProperty nodesProp;

    private void OnEnable()
    {
        nodesProp = serializedObject.FindProperty("nodes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Node Management", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Node"))
        {
            nodesProp.arraySize++;
        }
        if (GUILayout.Button("Remove Last Node") && nodesProp.arraySize > 0)
        {
            nodesProp.arraySize--;
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < nodesProp.arraySize; i++)
        {
            DrawNode(i);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawNode(int index)
    {
        SerializedProperty node = nodesProp.GetArrayElementAtIndex(index);
        SerializedProperty npcLine = node.FindPropertyRelative("npcLine");
        SerializedProperty responses = node.FindPropertyRelative("responses");

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Node {index}", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Response", GUILayout.Width(100)))
        {
            responses.arraySize++;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(npcLine);

        for (int i = 0; i < responses.arraySize; i++)
        {
            DrawResponse(responses.GetArrayElementAtIndex(i), index);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawResponse(SerializedProperty response, int currentNodeIndex)
    {
        SerializedProperty text = response.FindPropertyRelative("text");
        SerializedProperty isGood = response.FindPropertyRelative("isGoodResponse");
        SerializedProperty nextNode = response.FindPropertyRelative("nextNodeIndex");

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Response {currentNodeIndex}-{nextNode.intValue}", EditorStyles.miniBoldLabel);
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            response.DeleteCommand();
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(text);
        EditorGUILayout.PropertyField(isGood, new GUIContent("Is Good Response"));

        List<int> validIndices = new List<int>() { -1 };
        for (int i = 0; i < nodesProp.arraySize; i++) validIndices.Add(i);

        int currentValue = nextNode.intValue;
        int selected = validIndices.IndexOf(currentValue);
        if (selected == -1) selected = 0;

        string[] options = new string[validIndices.Count];
        for (int i = 0; i < validIndices.Count; i++)
        {
            options[i] = validIndices[i] == -1 ? "End Conversation" : $"Node {validIndices[i]}";
        }

        selected = EditorGUILayout.Popup("Next Node", selected, options);
        nextNode.intValue = validIndices[selected];

        EditorGUILayout.EndVertical();
    }
}
#endif

[CreateAssetMenu(fileName = "NewDialogueTree", menuName = "Dialogue/Dialogue Tree")]
public class DialogueTree : ScriptableObject
{
    public List<DialogueNode> nodes;
}

[System.Serializable]
public class DialogueResponse
{
    [Tooltip("Button text for this response")]
    public string text;
    [Tooltip("Does this response reduce awkwardness?")]
    public bool isGoodResponse;
    [Tooltip("Index of next dialogue node (-1 ends conversation)")]
    public int nextNodeIndex;
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(3, 5)]
    public string npcLine;
    public List<DialogueResponse> responses;
}

