using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BallData))]
public class BallDataEditor : Editor
{
    private SerializedProperty ballNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty ballSpriteProp;
    private SerializedProperty baseSpeedProp;
    private SerializedProperty maxHealthProp;
    private SerializedProperty scoreTypeProp;
    private SerializedProperty scoreValueProp;
    private SerializedProperty priceProp;
    private SerializedProperty specialAbilityProp;

    private void OnEnable()
    {
        ballNameProp = serializedObject.FindProperty("ballName");
        descriptionProp = serializedObject.FindProperty("description");
        ballSpriteProp = serializedObject.FindProperty("ballSprite");
        baseSpeedProp = serializedObject.FindProperty("baseSpeed");
        maxHealthProp = serializedObject.FindProperty("maxHealth");
        scoreTypeProp = serializedObject.FindProperty("scoreType");
        scoreValueProp = serializedObject.FindProperty("scoreValue");
        priceProp = serializedObject.FindProperty("price");
        specialAbilityProp = serializedObject.FindProperty("specialAbility");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(ballNameProp);
        EditorGUILayout.PropertyField(descriptionProp);
        EditorGUILayout.PropertyField(ballSpriteProp);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(baseSpeedProp);
        EditorGUILayout.PropertyField(maxHealthProp);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Score", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(scoreTypeProp);
        EditorGUILayout.PropertyField(scoreValueProp);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Shop", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(priceProp);

        EditorGUILayout.Space(10f);
        DrawSpecialAbilityPanel();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSpecialAbilityPanel()
    {
        if (specialAbilityProp == null)
        {
            return;
        }

        SerializedProperty useSpecialAbilityProp = specialAbilityProp.FindPropertyRelative("useSpecialAbility");
        SerializedProperty abilityNameProp = specialAbilityProp.FindPropertyRelative("abilityName");
        SerializedProperty abilityDescriptionProp = specialAbilityProp.FindPropertyRelative("abilityDescription");
        SerializedProperty triggerTargetProp = specialAbilityProp.FindPropertyRelative("triggerTarget");
        SerializedProperty maxTriggerCountProp = specialAbilityProp.FindPropertyRelative("maxTriggerCount");
        SerializedProperty abilityAssetProp = specialAbilityProp.FindPropertyRelative("abilityAsset");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Special Ability", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useSpecialAbilityProp, new GUIContent("Enabled"));

        if (!useSpecialAbilityProp.boolValue)
        {
            EditorGUILayout.HelpBox("Enable this to configure when the ball's special ability should trigger.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.PropertyField(abilityNameProp, new GUIContent("Ability Name"));
        EditorGUILayout.PropertyField(abilityDescriptionProp, new GUIContent("Ability Description"));
        EditorGUILayout.PropertyField(abilityAssetProp, new GUIContent("Ability Asset"));
        EditorGUILayout.PropertyField(triggerTargetProp, new GUIContent("Trigger On"));
        EditorGUILayout.PropertyField(maxTriggerCountProp, new GUIContent("Max Trigger Count"));

        if (abilityAssetProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Assign a BallSpecialAbilityBase asset to run an actual effect when triggered.", MessageType.Warning);
        }

        if (maxTriggerCountProp.intValue == 0)
        {
            EditorGUILayout.HelpBox("0 means the ability can trigger on every valid collision.", MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"This ability will trigger up to {maxTriggerCountProp.intValue} times per spawned ball.",
                MessageType.None);
        }

        EditorGUILayout.EndVertical();
    }
}
