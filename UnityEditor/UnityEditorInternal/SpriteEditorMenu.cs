using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class SpriteEditorMenu : EditorWindow
	{
		private class Styles
		{
			public GUIStyle background = "grey_border";

			public GUIStyle notice;

			public readonly GUIContent[] spriteAlignmentOptions = new GUIContent[]
			{
				EditorGUIUtility.TextContent("Center"),
				EditorGUIUtility.TextContent("Top Left"),
				EditorGUIUtility.TextContent("Top"),
				EditorGUIUtility.TextContent("Top Right"),
				EditorGUIUtility.TextContent("Left"),
				EditorGUIUtility.TextContent("Right"),
				EditorGUIUtility.TextContent("sBottom Left"),
				EditorGUIUtility.TextContent("Bottom"),
				EditorGUIUtility.TextContent("Bottom Right"),
				EditorGUIUtility.TextContent("Custom")
			};

			public readonly GUIContent[] slicingMethodOptions = new GUIContent[]
			{
				EditorGUIUtility.TextContent("Delete Existing|Delete all existing sprite assets before the slicing operation"),
				EditorGUIUtility.TextContent("Smart|Try to match existing sprite rects to sliced rects from the slicing operation"),
				EditorGUIUtility.TextContent("Safe|Keep existing sprite rects intact")
			};

			public readonly GUIContent methodLabel = EditorGUIUtility.TextContent("Method");

			public readonly GUIContent pivotLabel = EditorGUIUtility.TextContent("Pivot");

			public readonly GUIContent typeLabel = EditorGUIUtility.TextContent("Type");

			public readonly GUIContent sliceButtonLabel = EditorGUIUtility.TextContent("Slice");

			public readonly GUIContent columnAndRowLabel = EditorGUIUtility.TextContent("Column & Row");

			public readonly GUIContent columnLabel = EditorGUIUtility.TextContent("C");

			public readonly GUIContent rowLabel = EditorGUIUtility.TextContent("R");

			public readonly GUIContent pixelSizeLabel = EditorGUIUtility.TextContent("Pixel Size");

			public readonly GUIContent xLabel = EditorGUIUtility.TextContent("X");

			public readonly GUIContent yLabel = EditorGUIUtility.TextContent("Y");

			public readonly GUIContent offsetLabel = EditorGUIUtility.TextContent("Offset");

			public readonly GUIContent paddingLabel = EditorGUIUtility.TextContent("Padding");

			public readonly GUIContent automaticSlicingHintLabel = EditorGUIUtility.TextContent("To obtain more accurate slicing results, manual slicing is recommended!");

			public readonly GUIContent customPivotLabel = EditorGUIUtility.TextContent("Custom Pivot");

			public Styles()
			{
				this.notice = new GUIStyle(GUI.skin.label);
				this.notice.alignment = TextAnchor.MiddleCenter;
				this.notice.wordWrap = true;
			}
		}

		public static SpriteEditorMenu s_SpriteEditorMenu;

		private static SpriteEditorMenu.Styles s_Styles;

		private static long s_LastClosedTime;

		private static int s_Selected;

		private static SpriteEditorMenuSetting s_Setting;

		public static SpriteEditorWindow s_SpriteEditor;

		private void Init(Rect buttonRect)
		{
			if (SpriteEditorMenu.s_Setting == null)
			{
				SpriteEditorMenu.s_Setting = ScriptableObject.CreateInstance<SpriteEditorMenuSetting>();
			}
			buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
			float y = 145f;
			Vector2 windowSize = new Vector2(300f, y);
			base.ShowAsDropDown(buttonRect, windowSize, null, ShowMode.PopupMenuWithKeyboardFocus);
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
		}

		private void UndoRedoPerformed()
		{
			base.Repaint();
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
			SpriteEditorMenu.s_LastClosedTime = DateTime.Now.Ticks / 10000L;
			SpriteEditorMenu.s_SpriteEditorMenu = null;
		}

		internal static bool ShowAtPosition(Rect buttonRect)
		{
			long num = DateTime.Now.Ticks / 10000L;
			bool result;
			if (num >= SpriteEditorMenu.s_LastClosedTime + 50L)
			{
				if (Event.current != null)
				{
					Event.current.Use();
				}
				if (SpriteEditorMenu.s_SpriteEditorMenu == null)
				{
					SpriteEditorMenu.s_SpriteEditorMenu = ScriptableObject.CreateInstance<SpriteEditorMenu>();
				}
				SpriteEditorMenu.s_SpriteEditorMenu.Init(buttonRect);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		private void OnGUI()
		{
			if (SpriteEditorMenu.s_Styles == null)
			{
				SpriteEditorMenu.s_Styles = new SpriteEditorMenu.Styles();
			}
			GUILayout.Space(4f);
			EditorGUIUtility.labelWidth = 124f;
			EditorGUIUtility.wideMode = true;
			GUI.Label(new Rect(0f, 0f, base.position.width, base.position.height), GUIContent.none, SpriteEditorMenu.s_Styles.background);
			EditorGUI.BeginChangeCheck();
			SpriteEditorMenuSetting.SlicingType slicingType = SpriteEditorMenu.s_Setting.slicingType;
			slicingType = (SpriteEditorMenuSetting.SlicingType)EditorGUILayout.EnumPopup(SpriteEditorMenu.s_Styles.typeLabel, slicingType, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(SpriteEditorMenu.s_Setting, "Change slicing type");
				SpriteEditorMenu.s_Setting.slicingType = slicingType;
			}
			if (slicingType != SpriteEditorMenuSetting.SlicingType.GridByCellSize && slicingType != SpriteEditorMenuSetting.SlicingType.GridByCellCount)
			{
				if (slicingType == SpriteEditorMenuSetting.SlicingType.Automatic)
				{
					this.OnAutomaticGUI();
				}
			}
			else
			{
				this.OnGridGUI();
			}
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(EditorGUIUtility.labelWidth + 4f);
			if (GUILayout.Button(SpriteEditorMenu.s_Styles.sliceButtonLabel, new GUILayoutOption[0]))
			{
				this.DoSlicing();
			}
			GUILayout.EndHorizontal();
		}

		private void DoSlicing()
		{
			this.DoAnalytics();
			SpriteEditorMenuSetting.SlicingType slicingType = SpriteEditorMenu.s_Setting.slicingType;
			if (slicingType != SpriteEditorMenuSetting.SlicingType.GridByCellCount && slicingType != SpriteEditorMenuSetting.SlicingType.GridByCellSize)
			{
				if (slicingType == SpriteEditorMenuSetting.SlicingType.Automatic)
				{
					this.DoAutomaticSlicing();
				}
			}
			else
			{
				this.DoGridSlicing();
			}
		}

		private void DoAnalytics()
		{
			UsabilityAnalytics.Event("Sprite Editor", "Slice", "Type", (int)SpriteEditorMenu.s_Setting.slicingType);
			if (SpriteEditorMenu.s_SpriteEditor.originalTexture != null)
			{
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Texture Width", SpriteEditorMenu.s_SpriteEditor.originalTexture.width);
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Texture Height", SpriteEditorMenu.s_SpriteEditor.originalTexture.height);
			}
			if (SpriteEditorMenu.s_Setting.slicingType == SpriteEditorMenuSetting.SlicingType.Automatic)
			{
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Auto Slicing Method", SpriteEditorMenu.s_Setting.autoSlicingMethod);
			}
			else
			{
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Grid Slicing Size X", (int)SpriteEditorMenu.s_Setting.gridSpriteSize.x);
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Grid Slicing Size Y", (int)SpriteEditorMenu.s_Setting.gridSpriteSize.y);
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Grid Slicing Offset X", (int)SpriteEditorMenu.s_Setting.gridSpriteOffset.x);
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Grid Slicing Offset Y", (int)SpriteEditorMenu.s_Setting.gridSpriteOffset.y);
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Grid Slicing Padding X", (int)SpriteEditorMenu.s_Setting.gridSpritePadding.x);
				UsabilityAnalytics.Event("Sprite Editor", "Slice", "Grid Slicing Padding Y", (int)SpriteEditorMenu.s_Setting.gridSpritePadding.y);
			}
		}

		private void TwoIntFields(GUIContent label, GUIContent labelX, GUIContent labelY, ref int x, ref int y)
		{
			float num = 16f;
			Rect rect = GUILayoutUtility.GetRect(EditorGUILayout.kLabelFloatMinW, EditorGUILayout.kLabelFloatMaxW, num, num, EditorStyles.numberField);
			Rect position = rect;
			position.width = EditorGUIUtility.labelWidth;
			position.height = 16f;
			GUI.Label(position, label);
			Rect position2 = rect;
			position2.width -= EditorGUIUtility.labelWidth;
			position2.height = 16f;
			position2.x += EditorGUIUtility.labelWidth;
			position2.width /= 2f;
			position2.width -= 2f;
			EditorGUIUtility.labelWidth = 12f;
			x = EditorGUI.IntField(position2, labelX, x);
			position2.x += position2.width + 3f;
			y = EditorGUI.IntField(position2, labelY, y);
			EditorGUIUtility.labelWidth = position.width;
		}

		private void OnGridGUI()
		{
			int num = (!(SpriteEditorMenu.s_SpriteEditor.previewTexture != null)) ? 4096 : SpriteEditorMenu.s_SpriteEditor.previewTexture.width;
			int num2 = (!(SpriteEditorMenu.s_SpriteEditor.previewTexture != null)) ? 4096 : SpriteEditorMenu.s_SpriteEditor.previewTexture.height;
			if (SpriteEditorMenu.s_Setting.slicingType == SpriteEditorMenuSetting.SlicingType.GridByCellCount)
			{
				int value = (int)SpriteEditorMenu.s_Setting.gridCellCount.x;
				int value2 = (int)SpriteEditorMenu.s_Setting.gridCellCount.y;
				EditorGUI.BeginChangeCheck();
				this.TwoIntFields(SpriteEditorMenu.s_Styles.columnAndRowLabel, SpriteEditorMenu.s_Styles.columnLabel, SpriteEditorMenu.s_Styles.rowLabel, ref value, ref value2);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo(SpriteEditorMenu.s_Setting, "Change column & row");
					SpriteEditorMenu.s_Setting.gridCellCount.x = (float)Mathf.Clamp(value, 1, num);
					SpriteEditorMenu.s_Setting.gridCellCount.y = (float)Mathf.Clamp(value2, 1, num2);
				}
			}
			else
			{
				int value3 = (int)SpriteEditorMenu.s_Setting.gridSpriteSize.x;
				int value4 = (int)SpriteEditorMenu.s_Setting.gridSpriteSize.y;
				EditorGUI.BeginChangeCheck();
				this.TwoIntFields(SpriteEditorMenu.s_Styles.pixelSizeLabel, SpriteEditorMenu.s_Styles.xLabel, SpriteEditorMenu.s_Styles.yLabel, ref value3, ref value4);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo(SpriteEditorMenu.s_Setting, "Change grid size");
					SpriteEditorMenu.s_Setting.gridSpriteSize.x = (float)Mathf.Clamp(value3, 1, num);
					SpriteEditorMenu.s_Setting.gridSpriteSize.y = (float)Mathf.Clamp(value4, 1, num2);
				}
			}
			int num3 = (int)SpriteEditorMenu.s_Setting.gridSpriteOffset.x;
			int num4 = (int)SpriteEditorMenu.s_Setting.gridSpriteOffset.y;
			EditorGUI.BeginChangeCheck();
			this.TwoIntFields(SpriteEditorMenu.s_Styles.offsetLabel, SpriteEditorMenu.s_Styles.xLabel, SpriteEditorMenu.s_Styles.yLabel, ref num3, ref num4);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(SpriteEditorMenu.s_Setting, "Change grid offset");
				SpriteEditorMenu.s_Setting.gridSpriteOffset.x = Mathf.Clamp((float)num3, 0f, (float)num - SpriteEditorMenu.s_Setting.gridSpriteSize.x);
				SpriteEditorMenu.s_Setting.gridSpriteOffset.y = Mathf.Clamp((float)num4, 0f, (float)num2 - SpriteEditorMenu.s_Setting.gridSpriteSize.y);
			}
			int value5 = (int)SpriteEditorMenu.s_Setting.gridSpritePadding.x;
			int value6 = (int)SpriteEditorMenu.s_Setting.gridSpritePadding.y;
			EditorGUI.BeginChangeCheck();
			this.TwoIntFields(SpriteEditorMenu.s_Styles.paddingLabel, SpriteEditorMenu.s_Styles.xLabel, SpriteEditorMenu.s_Styles.yLabel, ref value5, ref value6);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(SpriteEditorMenu.s_Setting, "Change grid padding");
				SpriteEditorMenu.s_Setting.gridSpritePadding.x = (float)Mathf.Clamp(value5, 0, num);
				SpriteEditorMenu.s_Setting.gridSpritePadding.y = (float)Mathf.Clamp(value6, 0, num2);
			}
			this.DoPivotGUI();
			GUILayout.Space(2f);
		}

		private void OnAutomaticGUI()
		{
			float num = 38f;
			if (SpriteEditorMenu.s_SpriteEditor.originalTexture != null && TextureUtil.IsCompressedTextureFormat(SpriteEditorMenu.s_SpriteEditor.originalTexture.format))
			{
				EditorGUILayout.LabelField(SpriteEditorMenu.s_Styles.automaticSlicingHintLabel, SpriteEditorMenu.s_Styles.notice, new GUILayoutOption[0]);
				num -= 31f;
			}
			this.DoPivotGUI();
			EditorGUI.BeginChangeCheck();
			int num2 = SpriteEditorMenu.s_Setting.autoSlicingMethod;
			num2 = EditorGUILayout.Popup(SpriteEditorMenu.s_Styles.methodLabel, num2, SpriteEditorMenu.s_Styles.slicingMethodOptions, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(SpriteEditorMenu.s_Setting, "Change Slicing Method");
				SpriteEditorMenu.s_Setting.autoSlicingMethod = num2;
			}
			GUILayout.Space(num);
		}

		private void DoPivotGUI()
		{
			EditorGUI.BeginChangeCheck();
			int num = SpriteEditorMenu.s_Setting.spriteAlignment;
			num = EditorGUILayout.Popup(SpriteEditorMenu.s_Styles.pivotLabel, num, SpriteEditorMenu.s_Styles.spriteAlignmentOptions, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(SpriteEditorMenu.s_Setting, "Change Alignment");
				SpriteEditorMenu.s_Setting.spriteAlignment = num;
				SpriteEditorMenu.s_Setting.pivot = SpriteEditorUtility.GetPivotValue((SpriteAlignment)num, SpriteEditorMenu.s_Setting.pivot);
			}
			Vector2 vector = SpriteEditorMenu.s_Setting.pivot;
			EditorGUI.BeginChangeCheck();
			using (new EditorGUI.DisabledScope(num != 9))
			{
				vector = EditorGUILayout.Vector2Field(SpriteEditorMenu.s_Styles.customPivotLabel, vector, new GUILayoutOption[0]);
			}
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo(SpriteEditorMenu.s_Setting, "Change custom pivot");
				SpriteEditorMenu.s_Setting.pivot = vector;
			}
		}

		private void DoAutomaticSlicing()
		{
			SpriteEditorMenu.s_SpriteEditor.DoAutomaticSlicing(4, SpriteEditorMenu.s_Setting.spriteAlignment, SpriteEditorMenu.s_Setting.pivot, (SpriteEditorWindow.AutoSlicingMethod)SpriteEditorMenu.s_Setting.autoSlicingMethod);
		}

		private void DoGridSlicing()
		{
			if (SpriteEditorMenu.s_Setting.slicingType == SpriteEditorMenuSetting.SlicingType.GridByCellCount)
			{
				this.DetemineGridCellSizeWithCellCount();
			}
			SpriteEditorMenu.s_SpriteEditor.DoGridSlicing(SpriteEditorMenu.s_Setting.gridSpriteSize, SpriteEditorMenu.s_Setting.gridSpriteOffset, SpriteEditorMenu.s_Setting.gridSpritePadding, SpriteEditorMenu.s_Setting.spriteAlignment, SpriteEditorMenu.s_Setting.pivot);
		}

		private void DetemineGridCellSizeWithCellCount()
		{
			int num = (!(SpriteEditorMenu.s_SpriteEditor.previewTexture != null)) ? 4096 : SpriteEditorMenu.s_SpriteEditor.previewTexture.width;
			int num2 = (!(SpriteEditorMenu.s_SpriteEditor.previewTexture != null)) ? 4096 : SpriteEditorMenu.s_SpriteEditor.previewTexture.height;
			SpriteEditorMenu.s_Setting.gridSpriteSize.x = ((float)num - SpriteEditorMenu.s_Setting.gridSpriteOffset.x - SpriteEditorMenu.s_Setting.gridSpritePadding.x * SpriteEditorMenu.s_Setting.gridCellCount.x) / SpriteEditorMenu.s_Setting.gridCellCount.x;
			SpriteEditorMenu.s_Setting.gridSpriteSize.y = ((float)num2 - SpriteEditorMenu.s_Setting.gridSpriteOffset.y - SpriteEditorMenu.s_Setting.gridSpritePadding.y * SpriteEditorMenu.s_Setting.gridCellCount.y) / SpriteEditorMenu.s_Setting.gridCellCount.y;
			SpriteEditorMenu.s_Setting.gridSpriteSize.x = Mathf.Clamp(SpriteEditorMenu.s_Setting.gridSpriteSize.x, 1f, (float)num);
			SpriteEditorMenu.s_Setting.gridSpriteSize.y = Mathf.Clamp(SpriteEditorMenu.s_Setting.gridSpriteSize.y, 1f, (float)num2);
		}
	}
}
