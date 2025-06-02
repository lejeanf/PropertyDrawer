namespace jeanf.propertyDrawer
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	public class ScriptableObjectDrawerAttribute : Attribute
	{
		public ScriptableObjectDrawerAttribute()
		{
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ScriptableObject), true)]
	public class ScriptableObjectDrawer : PropertyDrawer
	{
		// Cache to prevent multiple SerializedObjects for same asset
		private static Dictionary<int, SerializedObject> serializedObjectCache = new Dictionary<int, SerializedObject>();
		
		private static bool CheckAttribute(System.Type t)
		{
			System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t);

			foreach (System.Attribute attr in attrs)
				if (attr is ScriptableObjectDrawerAttribute)
				{
					return true;
				}

			return false;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float totalHeight = EditorGUIUtility.singleLineHeight;
			
			if (property.objectReferenceValue == null)
			{
				return totalHeight;
			}

			if (!AreAnySubPropertiesVisible(property))
				return totalHeight;
				
			if (property.isExpanded)
			{
				var data = property.objectReferenceValue as ScriptableObject;
				if (data == null) return EditorGUIUtility.singleLineHeight;
				
				// Use cached or create new SerializedObject
				SerializedObject serializedObject = GetOrCreateSerializedObject(data);
				SerializedProperty prop = serializedObject.GetIterator();
				
				if (prop.NextVisible(true))
				{
					do
					{
						if (prop.name == "m_Script") continue;
						var subProp = serializedObject.FindProperty(prop.name);
						if (subProp != null)
						{
							float height = EditorGUI.GetPropertyHeight(subProp, null, true) +
							               EditorGUIUtility.standardVerticalSpacing;
							totalHeight += height;
						}
					} while (prop.NextVisible(false));
				}

				totalHeight += EditorGUIUtility.standardVerticalSpacing;
			}

			return totalHeight;
		}

		const int buttonWidth = 66;

		static readonly List<string> ignoreClassFullNames = new List<string> { "TMPro.TMP_FontAsset" };

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			
			if (property.objectReferenceValue != null && AreAnySubPropertiesVisible(property))
			{
				if (AreAnySubPropertiesVisible(property) && CheckAttribute(property.objectReferenceValue.GetType()))
				{
					property.isExpanded =
						EditorGUI.Foldout(
							new Rect(position.x, position.y, EditorGUIUtility.labelWidth,
								EditorGUIUtility.singleLineHeight), property.isExpanded, property.displayName, true);
				}
				else
				{
					EditorGUI.LabelField(
						new Rect(position.x, position.y, EditorGUIUtility.labelWidth,
							EditorGUIUtility.singleLineHeight), property.displayName);
					property.isExpanded = false;
				}

				const int offset = 2;
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(
					new Rect(EditorGUIUtility.labelWidth + offset, position.y,
						position.width - EditorGUIUtility.labelWidth - offset, EditorGUIUtility.singleLineHeight),
					property, GUIContent.none, true);
				
				if (EditorGUI.EndChangeCheck())
				{
					property.serializedObject.ApplyModifiedProperties();
					ClearCacheForObject(property.objectReferenceValue);
				}
				
				if (property.objectReferenceValue == null) 
				{
					EditorGUI.EndProperty();
					return;
				}

				if (property.isExpanded)
				{
					DrawExpandedProperties(position, property);
				}
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.ObjectField(
					new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight), property);
				
				if (EditorGUI.EndChangeCheck())
				{
					property.serializedObject.ApplyModifiedProperties();
				}
				
				if (GUI.Button(
					new Rect(position.x + position.width - 58, position.y, 58, EditorGUIUtility.singleLineHeight),
					"Create"))
				{
					CreateNewScriptableObject(property);
				}
			}

			EditorGUI.EndProperty();
		}

		private void DrawExpandedProperties(Rect position, SerializedProperty property)
		{
			GUI.Box(
				new Rect(0,
					position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 1, 
					Screen.width,
					position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing), 
				"");

			EditorGUI.indentLevel++;
			var data = (ScriptableObject)property.objectReferenceValue;
			SerializedObject serializedObject = GetOrCreateSerializedObject(data);

			bool hasChanges = false;

			SerializedProperty prop = serializedObject.GetIterator();
			float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			
			if (prop.NextVisible(true))
			{
				do
				{
					if (prop.name == "m_Script") continue;
					
					float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
					
					EditorGUI.BeginChangeCheck();
					EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prop, true);
					
					if (EditorGUI.EndChangeCheck())
					{
						hasChanges = true;
					}
					
					y += height + EditorGUIUtility.standardVerticalSpacing;
				} while (prop.NextVisible(false));
			}

			if (hasChanges)
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(data);
				AssetDatabase.SaveAssets();
			}

			EditorGUI.indentLevel--;
		}

		private void CreateNewScriptableObject(SerializedProperty property)
		{
			string selectedAssetPath = "Assets";
			if (property.serializedObject.targetObject is MonoBehaviour)
			{
				MonoScript ms = MonoScript.FromMonoBehaviour((MonoBehaviour)property.serializedObject.targetObject);
				selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
			}

			Type type = GetFieldType();
			var newAsset = CreateAssetWithSavePrompt(type, selectedAssetPath);
			if (newAsset != null)
			{
				property.objectReferenceValue = newAsset;
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		private static SerializedObject GetOrCreateSerializedObject(ScriptableObject target)
		{
			if (target == null) return null;
			
			int instanceId = target.GetInstanceID();
			
			if (serializedObjectCache.TryGetValue(instanceId, out SerializedObject cached))
			{
				if (cached != null && cached.targetObject != null)
				{
					cached.Update(); // Ensure we have latest data
					return cached;
				}
				else
				{
					serializedObjectCache.Remove(instanceId);
				}
			}
			
			var newSerializedObject = new SerializedObject(target);
			serializedObjectCache[instanceId] = newSerializedObject;
			return newSerializedObject;
		}

		private static void ClearCacheForObject(UnityEngine.Object target)
		{
			if (target == null) return;
			
			int instanceId = target.GetInstanceID();
			if (serializedObjectCache.TryGetValue(instanceId, out SerializedObject cached))
			{
				cached?.Dispose();
				serializedObjectCache.Remove(instanceId);
			}
		}

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void OnScriptsReloaded()
		{
			ClearAllCache();
		}

		private static void ClearAllCache()
		{
			foreach (var kvp in serializedObjectCache)
			{
				kvp.Value?.Dispose();
			}
			serializedObjectCache.Clear();
		}

		public static T _GUILayout<T>(string label, T objectReferenceValue, ref bool isExpanded)
			where T : ScriptableObject
		{
			return _GUILayout<T>(new GUIContent(label), objectReferenceValue, ref isExpanded);
		}

		public static T _GUILayout<T>(GUIContent label, T objectReferenceValue, ref bool isExpanded)
			where T : ScriptableObject
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			
			if (objectReferenceValue != null)
			{
				isExpanded = EditorGUILayout.Foldout(isExpanded, label, true);
			}
			else
			{
				EditorGUILayout.LabelField(label);
			}

			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUILayout.ObjectField(objectReferenceValue, typeof(T), false) as T;
			
			if (EditorGUI.EndChangeCheck())
			{
				if (objectReferenceValue != newValue)
				{
					ClearCacheForObject(objectReferenceValue);
					objectReferenceValue = newValue;
				}
			}

			if (objectReferenceValue == null)
			{
				if (GUILayout.Button("Create", GUILayout.Width(buttonWidth)))
				{
					string selectedAssetPath = "Assets";
					var newAsset = CreateAssetWithSavePrompt(typeof(T), selectedAssetPath);
					if (newAsset != null)
					{
						objectReferenceValue = (T)newAsset;
					}
				}
			}

			EditorGUILayout.EndHorizontal();

			if (objectReferenceValue != null && isExpanded)
			{
				DrawScriptableObjectChildFields(objectReferenceValue);
			}

			EditorGUILayout.EndVertical();
			return objectReferenceValue;
		}

		static void DrawScriptableObjectChildFields<T>(T objectReferenceValue) where T : ScriptableObject
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical(GUI.skin.box);

			var serializedObject = GetOrCreateSerializedObject(objectReferenceValue);
			bool hasChanges = false;
			
			SerializedProperty prop = serializedObject.GetIterator();
			if (prop.NextVisible(true))
			{
				do
				{
					if (prop.name == "m_Script") continue;
					
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(prop, true);
					
					if (EditorGUI.EndChangeCheck())
					{
						hasChanges = true;
					}
				} while (prop.NextVisible(false));
			}

			if (hasChanges)
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(objectReferenceValue);
				AssetDatabase.SaveAssets();
			}

			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}

		public static T DrawScriptableObjectField<T>(GUIContent label, T objectReferenceValue, ref bool isExpanded)
			where T : ScriptableObject
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			if (objectReferenceValue != null)
			{
				isExpanded = EditorGUILayout.Foldout(isExpanded, label, true);
			}
			else
			{
				EditorGUILayout.LabelField(label);
			}

			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUILayout.ObjectField(objectReferenceValue, typeof(T), false) as T;
			
			if (EditorGUI.EndChangeCheck())
			{
				if (objectReferenceValue != newValue)
				{
					ClearCacheForObject(objectReferenceValue);
					objectReferenceValue = newValue;
				}
			}

			if (objectReferenceValue == null)
			{
				if (GUILayout.Button("Create", GUILayout.Width(buttonWidth)))
				{
					string selectedAssetPath = "Assets";
					var newAsset = CreateAssetWithSavePrompt(typeof(T), selectedAssetPath);
					if (newAsset != null)
					{
						objectReferenceValue = (T)newAsset;
					}
				}
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			return objectReferenceValue;
		}

		static ScriptableObject CreateAssetWithSavePrompt(Type type, string path)
		{
			path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", type.Name + ".asset", "asset",
				"Enter a file name for the ScriptableObject.", path);
			if (path == "") return null;
			
			ScriptableObject asset = ScriptableObject.CreateInstance(type);
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			EditorGUIUtility.PingObject(asset);
			return asset;
		}

		Type GetFieldType()
		{
			Type type = fieldInfo.FieldType;
			if (type.IsArray) type = type.GetElementType();
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
				type = type.GetGenericArguments()[0];
			return type;
		}

		static bool AreAnySubPropertiesVisible(SerializedProperty property)
		{
			var data = (ScriptableObject)property.objectReferenceValue;
			if (data == null) return false;
			
			SerializedObject serializedObject = GetOrCreateSerializedObject(data);
			SerializedProperty prop = serializedObject.GetIterator();

			while (prop.NextVisible(true))
			{
				if (prop.name == "m_Script") continue;
				return true;
			}

			return false;
		}
	}
#endif
}