//创建者:Icarus
//手动滑稽,滑稽脸
//ヾ(•ω•`)o
//https://www.ykls.app
//2019年09月04日-23:50
//Node_Editor_Framework

using NodeEditorFramework;
using UnityEngine;

namespace CabinIcarus.NodeEditorFramework
{
    public static class SelectBox
    {

        #region GUI

        /// <summary>
		/// 绘制选择框
		/// </summary>
		/// <param name="editorState"></param>
		public static void DrawSelectBox(NodeEditorState editorState)
		{
			if (editorState.IsDrawSelectSelectBox && !editorState.focusedConnectionKnob)
			{
				ClearSelectBoxElement(editorState);
				
				Rect rect = new Rect(editorState.MouseDownPos.x, editorState.MouseDownPos.y, 
					Event.current.mousePosition.x - editorState.MouseDownPos.x, Event.current.mousePosition.y - editorState.MouseDownPos.y);
				
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, new Color(0, 0, 0, 0.1f), new Color(1, 1, 1, 0.6f));			
				}
				else
				{
					var defaultColor = GUI.color;
					GUI.color = new Color(0,0,0,0.5f);
					{
						GUI.Box(rect, "");
					}
					GUI.color = defaultColor;
				}
#else
				GUI.Box(rect,"");
#endif

				#region Node Contains

				// 如果是从右到左的选择框则重新设置一下坐标起点和大小
				{
					if (rect.width < 0)
					{
						rect.x += rect.width;
						rect.width = -rect.width;
					}

					if (rect.height < 0)
					{
						rect.y += rect.height;
						rect.height = -rect.height;
					}
				}

				_selectGroup(editorState, rect);
				
				_selectNode(editorState, rect);
				
				#endregion
			}
		}

        private static void _selectGroup(NodeEditorState editorState, Rect rect)
        {
	        foreach (var nodeGroup in editorState.canvas.groups)
	        {
		        if (_isClipped(editorState, nodeGroup.fullAABBRect))
		        {
			        //跳过
			        continue;
		        }

		        var groupPos = _getSceneRect(editorState, nodeGroup.rect);

		        if (rect.Overlaps(groupPos))
		        {
			        if (!editorState.BoxContainGroup.Contains(nodeGroup))
			        {
				        editorState.BoxContainGroup.Add(nodeGroup);
			        }
		        }
	        }
        }
        
        private static void _selectNode(NodeEditorState editorState, Rect rect)
        {
	        foreach (var node in editorState.canvas.nodes)
	        {
		        if (_isClipped(editorState, node.fullAABBRect))
		        {
			        //跳过
			        continue;
		        }

		        var nodePos = _getSceneRect(editorState, node.rect);

		        if (rect.Overlaps(nodePos))
		        {
			        if (!editorState.BoxContainNodes.Contains(node))
			        {
				        editorState.BoxContainNodes.Add(node);
			        }
		        }
	        }
        }

        /// <summary>
		/// 是否被裁减
		/// </summary>
		/// <param name="editorState"></param>
		/// <param name="elementRect"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		private static bool _isClipped(NodeEditorState editorState,Rect elementRect)
		{
			return !editorState.canvasViewport.Overlaps(elementRect);
		}

		/// <summary>
		/// 将画布坐标转为窗口坐标
		/// </summary>
		/// <param name="editorState"></param>
		/// <param name="elementRect"></param>
		/// <returns></returns>
		static Rect _getSceneRect(NodeEditorState editorState, Rect elementRect)
		{
			var pos = (editorState.zoomPanAdjust + editorState.panOffset + elementRect.position) / editorState.zoom;
			
			return new Rect(pos,elementRect.size);
		}

        #endregion
        
        #region 选择框

        [EventHandlerAttribute (EventType.MouseDown, 106)] // Priority over hundred to make it call after the GUI
        private static void HandleSelectBoxStart (NodeEditorInputInfo inputInfo) 
        {
            if (GUIUtility.hotControl > 0)
                return; // GUI has control
			
            NodeEditorState state = inputInfo.editorState;
			
            state.MouseDownPos = inputInfo.inputPos;

            if (inputInfo.inputEvent.button == 0)
            {
                //没有选择节点也没有选择节点组,清空
                if (!state.focusedNode && state.activeGroup == null)
                {
                    state.IsDrawSelectSelectBox = true;
                    ClearSelectBoxElement(state);
				
                    return;
                }

                //选择了节点
                if (state.focusedNode)
                {
                    //不是选择框选中的节点,清空
                    if (!state.BoxContainNodes.Contains(state.focusedNode))
                    {
                        ClearSelectBoxElement(state);
                    }
                }
                else
                {
                    //不是选择框选中的节点组,清空
                    if (!state.BoxContainGroup.Contains(state.activeGroup))
                    {
                        ClearSelectBoxElement(state);
                    }
                }
            }
        }

        [EventHandlerAttribute (EventType.MouseUp)]
        private static void HandleSelectBoxEndMouseUp (NodeEditorInputInfo inputInfo) 
        {
            inputInfo.editorState.IsDrawSelectSelectBox = false;
            Event.current.Use();
        }
		
        /// <summary>
        /// 鼠标离开窗口
        /// </summary>
        /// <param name="inputInfo"></param>
        [EventHandlerAttribute (EventType.Ignore)]
        private static void HandleSelectBoxEndIgnore (NodeEditorInputInfo inputInfo) 
        {
            if (inputInfo.inputEvent.rawType == EventType.MouseUp)
            {
                inputInfo.editorState.IsDrawSelectSelectBox = false;
                NodeEditor.RepaintClients();
            }
        }

        #endregion
        
        /// <summary>
        /// 更新选择框的元素坐标
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pos">移动量</param>
        public static void UpdateSelectBoxElementPos(NodeEditorState state,Vector2 pos)
        {
            foreach (var node in state.BoxContainNodes)
            {
                if (node != state.selectedNode)
                {
	                //选择的是节点组,需要排除节点组中的节点,否则会被移动2次
	                if (state.activeGroup != null)
	                {
		                bool isContains = false;

		                foreach (var @group in state.BoxContainGroup)
		                {
			                if (@group.IsContainsNode(node))
			                {
				                isContains = true;

				                break;
			                }
		                }

		                //已经在某个被选中的组中
		                if (isContains)
		                {
			                continue;
		                }
	                }
	                
                    node.position += pos;
                }
            }
            
            foreach (var nodeGroup in state.BoxContainGroup)
            {
                if (nodeGroup != state.activeGroup)
                {
	                //选择的是节点组,需要排除节点组中的节点组,否则会被移动2次
	                if (state.activeGroup != null)
	                {
		                bool isContains = false;

		                foreach (var @group in state.BoxContainGroup)
		                {
			                if (@group.IsContainsNodeGroup(nodeGroup))
			                {
				                isContains = true;

				                break;
			                }
		                }

		                //已经在某个被选中的组中
		                if (isContains)
		                {
			                continue;
		                }
	                }
	                
                    nodeGroup.rect.position += pos;
                }
            }
        }
        
        /// <summary>
        /// 清空选择框内容
        /// </summary>
        /// <param name="state"></param>
        public static void ClearSelectBoxElement(NodeEditorState state)
        {
            state.BoxContainGroup.Clear();
            state.BoxContainNodes.Clear();
        }
    }
}