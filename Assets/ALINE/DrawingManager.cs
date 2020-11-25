#pragma warning disable 649 // Field `Drawing.GizmoContext.activeTransform' is never assigned to, and will always have its default value `null'. Not used outside of the unity editor.
using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;
#if MODULE_RENDER_PIPELINES_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Drawing {
	/// <summary>Info about the current selection in the editor</summary>
	public static class GizmoContext {
#if UNITY_EDITOR
		static Transform activeTransform;
#endif

		static HashSet<Transform> selectedTransforms = new HashSet<Transform>();

		static internal bool drawingGizmos;

		/// <summary>Number of top-level transforms that are selected</summary>
		public static int selectionSize { get; private set; }

		internal static void Refresh () {
#if UNITY_EDITOR
			activeTransform = Selection.activeTransform;
			selectedTransforms.Clear();
			var topLevel = Selection.transforms;
			for (int i = 0; i < topLevel.Length; i++) selectedTransforms.Add(topLevel[i]);
			selectionSize = topLevel.Length;
#endif
		}

		/// <summary>
		/// True if the component is selected.
		/// This is a deep selection: even children of selected transforms are considered to be selected.
		/// </summary>
		public static bool InSelection (Component c) {
			if (!drawingGizmos) throw new System.Exception("Can only be used inside the ALINE library's gizmo drawing functions.");
			return InSelection(c.transform);
		}

		/// <summary>
		/// True if the transform is selected.
		/// This is a deep selection: even children of selected transforms are considered to be selected.
		/// </summary>
		public static bool InSelection (Transform tr) {
			if (!drawingGizmos) throw new System.Exception("Can only be used inside the ALINE library's gizmo drawing functions.");
			var leaf = tr;
			while (tr != null) {
				if (selectedTransforms.Contains(tr)) {
					selectedTransforms.Add(leaf);
					return true;
				}
				tr = tr.parent;
			}
			return false;
		}

		/// <summary>
		/// True if the component is shown in the inspector.
		/// The active selection is the GameObject that is currently visible in the inspector.
		/// </summary>
		public static bool InActiveSelection (Component c) {
			if (!drawingGizmos) throw new System.Exception("Can only be used inside the ALINE library's gizmo drawing functions.");
			return InActiveSelection(c.transform);
		}

		/// <summary>
		/// True if the transform is shown in the inspector.
		/// The active selection is the GameObject that is currently visible in the inspector.
		/// </summary>
		public static bool InActiveSelection (Transform tr) {
#if UNITY_EDITOR
			if (!drawingGizmos) throw new System.Exception("Can only be used inside the ALINE library's gizmo drawing functions.");
			return tr.transform == activeTransform;
#else
			return false;
#endif
		}
	}

	/// <summary>
	/// Every object that wants to draw gizmos should implement this interface.
	/// See: <see cref="Drawing.MonoBehaviourGizmos"/>
	/// </summary>
	public interface IDrawGizmos {
		void DrawGizmos();
	}

	public enum DetectedRenderPipeline {
		BuiltInOrCustom,
		HDRP,
		URP
	}

	/// <summary>
	/// Global script which draws debug items and gizmos.
	/// If a Draw.* method has been used or if any script inheriting from the <see cref="Drawing.MonoBehaviourGizmos"/> class is in the scene then an instance of this script
	/// will be created and put on a hidden GameObject.
	///
	/// It will inject drawing logic into any cameras that are rendered.
	///
	/// Usually you never have to interact with this class.
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("")]
	public class DrawingManager : MonoBehaviour {
		public DrawingData gizmos;
		static List<IDrawGizmos> gizmoDrawers = new List<IDrawGizmos>();
		static DrawingManager _instance;
		bool framePassed;
		int lastFrameCount = int.MinValue;
		bool builtGizmos;
		RedrawScope previousFrameRedrawScope;

		/// <summary>
		/// Allow rendering to cameras that render to RenderTextures.
		/// By default cameras which render to render textures are never rendered to.
		/// You may enable this if you wish.
		///
		/// See: <see cref="Drawing.CommandBuilder.cameraTargets"/>
		/// See: advanced (view in online documentation for working links)
		/// </summary>
		public static bool allowRenderToRenderTextures = false;
		public static bool drawToAllCameras = false;
		CommandBuffer commandBuffer;
		DetectedRenderPipeline detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;

#if MODULE_RENDER_PIPELINES_UNIVERSAL
		HashSet<ScriptableRenderer> scriptableRenderersWithPass = new HashSet<ScriptableRenderer>();
		AlineURPRenderPassFeature renderPassFeature;
#endif

		public static DrawingManager instance {
			get {
				if (_instance == null) Init();
				return _instance;
			}
		}

#if UNITY_EDITOR
		[InitializeOnLoadMethod]
#endif
		public static void Init () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (Unity.Jobs.LowLevel.Unsafe.JobsUtility.IsExecutingJob) throw new System.Exception("Draw.* methods cannot be called from inside a job. See the documentation for info about how to use drawing functions from the Unity Job System.");
#endif
			if (_instance != null) return;

			// Find any existing instance.
			// Includes objects on HideInInspector GameObjects
			var instances = Resources.FindObjectsOfTypeAll<DrawingManager>();
			if (instances.Length > 0) _instance = instances[0];
			if (_instance == null) {
				var go = new GameObject("RetainedGizmos") {
					hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInInspector
				};
				_instance = go.AddComponent<DrawingManager>();
				if (Application.isPlaying) DontDestroyOnLoad(go);
			}

#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION && MODULE_RENDER_PIPELINES_UNIVERSAL
			Debug.LogError("You have both the universal and high definition render pipelines installed. They are known to conflict with each other in some cases. Please keep only one of them installed.");
#endif
		}

		/// <summary>Detects which render pipeline is being used and configures them for rendering</summary>
		void RefreshRenderPipelineMode () {
			var pipelineType = RenderPipelineManager.currentPipeline != null? RenderPipelineManager.currentPipeline.GetType() : null;

#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION
			if (pipelineType == typeof(HDRenderPipeline)) {
				if (detectedRenderPipeline != DetectedRenderPipeline.HDRP) {
					detectedRenderPipeline = DetectedRenderPipeline.HDRP;
					if (!_instance.gameObject.TryGetComponent<CustomPassVolume>(out CustomPassVolume volume)) {
						volume = _instance.gameObject.AddComponent<CustomPassVolume>();
						volume.isGlobal = true;
						volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
						volume.customPasses.Add(new AlineHDRPCustomPass());
					}
				}
				return;
			}
#endif
#if MODULE_RENDER_PIPELINES_UNIVERSAL
			if (pipelineType == typeof(UniversalRenderPipeline)) {
				detectedRenderPipeline = DetectedRenderPipeline.URP;
				return;
			}
#endif
			detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;
		}

		void OnEnable () {
			if (gizmos == null) gizmos = new DrawingData();
			gizmos.frameRedrawScope = new RedrawScope(gizmos);
			Draw.builder = gizmos.GetBuiltInBuilder(false);
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "ALINE Gizmos";

			// Callback when rendering with the built-in render pipeline
			Camera.onPostRender += PostRender;
			// Callback when rendering with a scriptable render pipeline
			UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering += BeginFrameRendering;
			UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += EndCameraRendering;
#if UNITY_EDITOR
			EditorApplication.update += OnUpdate;
#endif
		}

		void BeginFrameRendering (ScriptableRenderContext context, Camera[] cameras) {
			RefreshRenderPipelineMode();

#if MODULE_RENDER_PIPELINES_UNIVERSAL
			if (detectedRenderPipeline == DetectedRenderPipeline.URP) {
				for (int i = 0; i < cameras.Length; i++) {
					var cam = cameras[i];
					if (cam.TryGetComponent<UniversalAdditionalCameraData>(out UniversalAdditionalCameraData data)) {
						var renderer = data.scriptableRenderer;

						// Ensure we don't add passes every frame, we only need to do this once
						if (!scriptableRenderersWithPass.Contains(renderer)) {
							// Use reflection to access the rendererFeatures on the scriptable renderer.
							// That property is unfortunately protected and there is no other good way to add custom render passes from a script like this that I have found.
							var rendererFeatures = renderer.GetType().GetProperty("rendererFeatures", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(renderer) as List<ScriptableRendererFeature>;
							if (renderPassFeature == null) {
								renderPassFeature = ScriptableObject.CreateInstance<AlineURPRenderPassFeature>();
							}

							rendererFeatures.Add(renderPassFeature);
							scriptableRenderersWithPass.Add(renderer);
						}
					}
				}
			}
#endif
		}

		void OnDisable () {
			Camera.onPostRender -= PostRender;
			UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering -= BeginFrameRendering;
			UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= EndCameraRendering;
#if UNITY_EDITOR
			EditorApplication.update -= OnUpdate;
#endif
			Draw.builder.DiscardAndDisposeInternal();
			Draw.ingame_builder.DiscardAndDisposeInternal();
			gizmos.ClearData();
#if MODULE_RENDER_PIPELINES_UNIVERSAL
			if (renderPassFeature != null) {
				ScriptableObject.DestroyImmediate(renderPassFeature);
				renderPassFeature = null;
			}
#endif
		}

		// When enter play mode = reload scene & reload domain
		//	editor => play mode: OnDisable -> OnEnable (same object)
		//  play mode => editor: OnApplicationQuit (note: no OnDisable/OnEnable)
		// When enter play mode = reload scene & !reload domain
		//	editor => play mode: Nothing
		//  play mode => editor: OnApplicationQuit
		// When enter play mode = !reload scene & !reload domain
		//	editor => play mode: Nothing
		//  play mode => editor: OnApplicationQuit
		// OnDestroy is never really called for this object (unless Unity or the game quits I quess)

		// TODO: Should run in OnDestroy. OnApplicationQuit runs BEFORE OnDestroy (which we do not want)
		// private void OnApplicationQuit () {
		// Debug.Log("OnApplicationQuit");
		// Draw.builder.DiscardAndDisposeInternal();
		// Draw.ingame_builder.DiscardAndDisposeInternal();
		// gizmos.ClearData();
		// Draw.builder = gizmos.GetBuiltInBuilder(false);
		// Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
		// }

		void OnUpdate () {
			framePassed = true;
			if (Time.frameCount > lastFrameCount + 1) {
				// More than one frame old
				// It is possible no camera is being rendered at all.
				// Ensure we don't get any memory leaks from drawing items being queued every frame.
				CheckFrameTicking();

				// Note: We do not always want to call the above method here
				// because it is nicer to call it right after the cameras have been rendered.
				// Otherwise drawing items queued before OnUpdate or after OnUpdate may end up
				// in different frames (for the purposes of rendering gizmos)
			}
		}

		internal void ExecuteCustomRenderPass (ScriptableRenderContext context, Camera camera) {
			UnityEngine.Profiling.Profiler.BeginSample("ALINE");
			commandBuffer.Clear();
			SubmitFrame(camera, commandBuffer, true);
			context.ExecuteCommandBuffer(commandBuffer);
			UnityEngine.Profiling.Profiler.EndSample();
		}

		private void EndCameraRendering (ScriptableRenderContext context, Camera camera) {
			if (detectedRenderPipeline == DetectedRenderPipeline.BuiltInOrCustom) {
				// Execute the custom render pass after the camera has finished rendering.
				// For the HDRP and URP the render pass will already have been executed.
				// However for a custom render pipline we execute the rendering code here.
				// This is only best effort. It's impossible to be compatible with all custom render pipelines.
				// However it should work for most simple ones.
				// For Unity's built-in render pipeline the EndCameraRendering method will never be called.
				ExecuteCustomRenderPass(context, camera);
			}
		}

		void PostRender (Camera camera) {
			// This method is only called when using Unity's built-in render pipeline
			commandBuffer.Clear();
			SubmitFrame(camera, commandBuffer, false);
			UnityEngine.Profiling.Profiler.BeginSample("Executing command buffer");
			Graphics.ExecuteCommandBuffer(commandBuffer);
			UnityEngine.Profiling.Profiler.EndSample();
		}

		void CheckFrameTicking () {
			if (Time.frameCount != lastFrameCount) {
				framePassed = true;
				lastFrameCount = Time.frameCount;
				previousFrameRedrawScope = gizmos.frameRedrawScope;
				gizmos.frameRedrawScope = new RedrawScope(gizmos);
				Draw.builder.DisposeInternal();
				Draw.ingame_builder.DisposeInternal();
				Draw.builder = gizmos.GetBuiltInBuilder(false);
				Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			} else if (framePassed && Application.isPlaying) {
				// Rendered frame passed without a game frame passing!
				// This might mean the game is paused.
				// Redraw gizmos while the game is paused.
				// It might also just mean that we are rendering with multiple cameras.
				previousFrameRedrawScope.Draw();
			}

			if (framePassed) {
				gizmos.TickFrame();
				builtGizmos = false;
				framePassed = false;
			}
		}

		internal void SubmitFrame (Camera camera, CommandBuffer cmd, bool usingRenderPipeline) {
#if UNITY_EDITOR
			bool isSceneViewCamera = SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera;
#else
			bool isSceneViewCamera = false;
#endif
			// Do not include when rendering to a texture unless this is a scene view camera
			bool allowCameraDefault = allowRenderToRenderTextures || drawToAllCameras || camera.targetTexture == null || isSceneViewCamera;

			CheckFrameTicking();

			Submit(camera, cmd, usingRenderPipeline, allowCameraDefault);
		}

#if UNITY_EDITOR
		static System.Reflection.MethodInfo IsGizmosAllowedForObject = typeof(UnityEditor.EditorGUIUtility).GetMethod("IsGizmosAllowedForObject", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
		static System.Type AnnotationUtility = typeof(UnityEditor.PlayModeStateChange).Assembly?.GetType("UnityEditor.AnnotationUtility");
		System.Object[] cachedObjectParameterArray = new System.Object[1];
#endif

		bool use3dGizmos {
			get {
#if UNITY_EDITOR
				var use3dGizmosProperty = AnnotationUtility.GetProperty("use3dGizmos", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
				return (bool)use3dGizmosProperty.GetValue(null);
#else
				return true;
#endif
			}
		}

		Dictionary<System.Type, bool> typeToGizmosEnabled = new Dictionary<Type, bool>();

		bool ShouldDrawGizmos (UnityEngine.Object obj) {
#if UNITY_EDITOR
			// Use reflection to call EditorGUIUtility.IsGizmosAllowedForObject which is an internal method.
			// It is exactly the information we want though.
			// In case Unity has changed its API or something so that the method can no longer be found then just return true
			cachedObjectParameterArray[0] = obj;
			return IsGizmosAllowedForObject == null || (bool)IsGizmosAllowedForObject.Invoke(null, cachedObjectParameterArray);
#else
			return true;
#endif
		}

		void RemoveDestroyedGizmoDrawers () {
			for (int i = gizmoDrawers.Count - 1; i >= 0; i--) {
				var mono = gizmoDrawers[i] as MonoBehaviour;
				if (!mono || (mono.hideFlags & HideFlags.HideInHierarchy) == HideFlags.HideInHierarchy) {
					gizmoDrawers.RemoveAt(i);
				}
			}
		}

		void DrawGizmos (bool usingRenderPipeline) {
			UnityEngine.Profiling.Profiler.BeginSample("Refresh Selection Cache");
			GizmoContext.Refresh();
			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("GizmosAllowed");
			typeToGizmosEnabled.Clear();
			if (!usingRenderPipeline) {
				// Fill the typeToGizmosEnabled dict with info about which classes should be drawn
				// We take advantage of the fact that IsGizmosAllowedForObject only depends on the type of the object and if it is active and enabled
				// and not the specific object instance.
				// When using a render pipeline the ShouldDrawGizmos method cannot be used because it seems to occasionally crash Unity :(
				// So we need these two separate cases.
				for (int i = gizmoDrawers.Count - 1; i >= 0; i--) {
					var tp = gizmoDrawers[i].GetType();
					if (!typeToGizmosEnabled.ContainsKey(tp) && (gizmoDrawers[i] as MonoBehaviour).isActiveAndEnabled) {
						typeToGizmosEnabled[tp] = ShouldDrawGizmos((UnityEngine.Object)gizmoDrawers[i]);
					}
				}
			}

			UnityEngine.Profiling.Profiler.EndSample();

			// Set the current frame's redraw scope to an empty scope.
			// This is because gizmos are rendered every frame anyway so we never want to redraw them.
			// The frame redraw scope is otherwise used when the game has been paused.
			var frameRedrawScope = gizmos.frameRedrawScope;
			gizmos.frameRedrawScope = default(RedrawScope);

			// This would look nicer as a 'using' block, but built-in command builders
			// cannot be disposed normally to prevent user error.
			// The try-finally is equivalent to a 'using' block.
			var gizmoBuilder = gizmos.GetBuiltInBuilder();
			try {
				// Replace Draw.builder with a custom one just for gizmos
				var debugBuilder = Draw.builder;
				Draw.builder = gizmoBuilder;

				UnityEngine.Profiling.Profiler.BeginSample("DrawGizmos");
				GizmoContext.drawingGizmos = true;
				if (usingRenderPipeline) {
					for (int i = gizmoDrawers.Count - 1; i >= 0; i--) {
						if ((gizmoDrawers[i] as MonoBehaviour).isActiveAndEnabled) {
							try {
								gizmoDrawers[i].DrawGizmos();
							} catch (System.Exception e) {
								Debug.LogException(e, gizmoDrawers[i] as MonoBehaviour);
							}
						}
					}
				} else {
					for (int i = gizmoDrawers.Count - 1; i >= 0; i--) {
						if ((gizmoDrawers[i] as MonoBehaviour).isActiveAndEnabled && typeToGizmosEnabled[gizmoDrawers[i].GetType()]) {
							try {
								gizmoDrawers[i].DrawGizmos();
							} catch (System.Exception e) {
								Debug.LogException(e, gizmoDrawers[i] as MonoBehaviour);
							}
						}
					}
				}
				GizmoContext.drawingGizmos = false;
				UnityEngine.Profiling.Profiler.EndSample();

				// Revert to the original builder
				Draw.builder = debugBuilder;
			} finally {
				gizmoBuilder.DisposeInternal();
			}

			gizmos.frameRedrawScope = frameRedrawScope;

			// Schedule jobs that may have been scheduled while drawing gizmos
			JobHandle.ScheduleBatchedJobs();
		}

		/// <summary>Submit a camera for rendering.</summary>
		/// <param name="allowCameraDefault">Indicates if built-in command builders and custom ones without a custom CommandBuilder.cameraTargets should render to this camera.</param>
		void Submit (Camera camera, CommandBuffer cmd, bool usingRenderPipeline, bool allowCameraDefault) {
			// This must always be done to avoid a potential memory leak if gizmos are never drawn
			RemoveDestroyedGizmoDrawers();
#if UNITY_EDITOR
			bool drawGizmos = Handles.ShouldRenderGizmos() || drawToAllCameras;
#else
			bool drawGizmos = false;
#endif
			// Only build gizmos if a camera actually needs them.
			// This is only done for the first camera that needs them each frame.
			if (drawGizmos && !builtGizmos && allowCameraDefault) {
				builtGizmos = true;
				DrawGizmos(usingRenderPipeline);
			}

			UnityEngine.Profiling.Profiler.BeginSample("Submit Gizmos");
			Draw.builder.DisposeInternal();
			Draw.ingame_builder.DisposeInternal();
			gizmos.Render(camera, drawGizmos, cmd, allowCameraDefault, detectedRenderPipeline);
			Draw.builder = gizmos.GetBuiltInBuilder(false);
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			UnityEngine.Profiling.Profiler.EndSample();
		}

		/// <summary>
		/// Registers an object for gizmo drawing.
		/// The DrawGizmos method on the object will be called every frame until it is destroyed (assuming there are cameras with gizmos enabled).
		/// </summary>
		public static void Register (IDrawGizmos item) {
			gizmoDrawers.Add(item);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// <code>
		/// // Create a new CommandBuilder
		/// using (var draw = DrawingManager.GetBuilder()) {
		///     // Use the exact same API as the global Draw class
		///     draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.
		/// If false, it will only be rendered in the editor when gizmos are enabled.</param>
		public static CommandBuilder GetBuilder (bool renderInGame = false) {
			return instance.gizmos.GetBuilder(renderInGame);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="redrawScope">Scope for this command builder. See #GetRedrawScope.</param>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.
		/// If false, it will only be rendered in the editor when gizmos are enabled.</param>
		public static CommandBuilder GetBuilder (RedrawScope redrawScope, bool renderInGame = false) {
			return instance.gizmos.GetBuilder(redrawScope, renderInGame);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		/// TODO: Example usage.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="hasher">Hash of whatever inputs you used to generate the drawing data.</param>
		/// <param name="redrawScope">Scope for this command builder. See #GetRedrawScope.</param>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public static CommandBuilder GetBuilder (DrawingData.Hasher hasher, RedrawScope redrawScope = default, bool renderInGame = false) {
			return instance.gizmos.GetBuilder(hasher, redrawScope, renderInGame);
		}

		/// <summary>
		/// A scope which can be used to draw things over multiple frames.
		/// You can use <see cref="GetBuilder(RedrawScope,bool)"/> to get a builder with a given redraw scope.
		/// After you have disposed the builder you may call <see cref="Drawing.RedrawScope.Draw"/> in any number of future frames to render the command builder again.
		///
		/// Note: The data will only be kept if <see cref="Drawing.RedrawScope.Draw"/> is called every frame.
		/// The command builder's data will be cleared if you do not call <see cref="Drawing.RedrawScope.Draw"/> in a future frame.
		/// After that point calling <see cref="Drawing.RedrawScope.Draw"/> will not do anything.
		/// </summary>
		public static RedrawScope GetRedrawScope () {
			return new RedrawScope(instance.gizmos);
		}
	}
}
