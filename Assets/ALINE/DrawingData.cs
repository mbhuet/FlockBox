using UnityEngine;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Rendering;
using System.Diagnostics;
using Unity.Jobs.LowLevel.Unsafe;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
using System.Linq;

namespace Drawing {
	using Drawing.Text;

	public static class SharedDrawingData {
		/// <summary>
		/// Same as Time.time, but not updated as frequently.
		/// Used since burst jobs cannot access Time.time.
		/// </summary>
		public static readonly Unity.Burst.SharedStatic<float> BurstTime = Unity.Burst.SharedStatic<float>.GetOrCreate<DrawingManager, BurstTimeKey>();

		private class BurstTimeKey {}
	}

	public struct RedrawScope {
		internal DrawingData gizmos;
		/// <summary>
		/// ID of the scope.
		/// Zero means no or invalid scope.
		/// </summary>
		internal int id;

		static int idCounter = 1;

		public RedrawScope (DrawingData gizmos) {
			this.gizmos = gizmos;
			// Should be enough with 4 billion ids before they wrap around.
			id = idCounter++;
		}

		/// <summary>
		/// Everything rendered with this scope and which is not older than one frame is drawn again.
		/// This is useful if you for some reason cannot draw some items during a frame (e.g. some asynchronous process is modifying the contents)
		/// but you still want to draw the same thing as the last frame to at least draw *something*.
		///
		/// Note: The items age will be reset. So the next frame you can call
		/// this method again to draw the items yet again.
		/// </summary>
		public void Draw () {
			if (gizmos != null) gizmos.Draw(this);
		}
	};

	/// <summary>
	/// Helper for drawing Gizmos in a performant way.
	/// This is a replacement for the Unity Gizmos class as that is not very performant
	/// when drawing very large amounts of geometry (for example a large grid graph).
	/// These gizmos can be persistent, so if the data does not change, the gizmos
	/// do not need to be updated.
	///
	/// How to use
	/// - Create a Hasher object and hash whatever data you will be using to draw the gizmos
	///      Could be for example the positions of the vertices or something. Just as long as
	///      if the gizmos should change, then the hash changes as well.
	/// - Check if a cached mesh exists for that hash
	/// - If not, then create a Builder object and call the drawing methods until you are done
	///      and then call Finalize with a reference to a gizmos class and the hash you calculated before.
	/// - Call gizmos.Draw with the hash.
	/// - When you are done with drawing gizmos for this frame, call gizmos.FinalizeDraw
	///
	/// <code>
	/// var a = Vector3.zero;
	/// var b = Vector3.one;
	/// var color = Color.red;
	/// var hasher = DrawingData.Hasher.Create(this);
	///
	/// hasher.Add(a);
	/// hasher.Add(b);
	/// hasher.Add(color);
	/// var gizmos = DrawingManager.instance.gizmos;
	/// if (!gizmos.Draw(hasher)) {
	///     using (var builder = gizmos.GetBuilder(hasher)) {
	///         // Ideally something very complex, not just a single line
	///         builder.Line(a, b, color);
	///     }
	/// }
	/// </code>
	/// </summary>
	public class DrawingData {
		/// <summary>Combines hashes into a single hash value</summary>
		public struct Hasher : IEquatable<Hasher> {
			ulong hash;

			public static Hasher NotSupplied => new Hasher { hash = ulong.MaxValue };

			public static Hasher Create<T>(T init) {
				var h = new Hasher();

				h.Add(init);
				return h;
			}

			public void Add<T>(T hash) {
				// Just a regular hash function. The + 12289 is to make sure that hashing zeros doesn't just produce a zero (and generally that hashing one X doesn't produce a hash of X)
				// (with a struct we can't provide default initialization)
				this.hash = (1572869UL * this.hash) ^ (ulong)hash.GetHashCode() + 12289;
			}

			public ulong Hash {
				get {
					return hash;
				}
			}

			public override int GetHashCode () {
				return (int)hash;
			}

			public bool Equals (Hasher other) {
				return hash == other.hash;
			}
		}

		internal struct ProcessedBuilderData {
			public enum Type {
				Invalid = 0,
				Static,
				Dynamic,
				Persistent,
				CustomMeshes,
			}

			public Type type;
			public BuilderData.Meta meta;
			bool submitted;

			// A single instance of a MeshBuffers struct.
			// This needs to be stored in a NativeArray because we will use it as a pointer
			// and it needs to be guaranteed to stay in the same position in memory.
			public NativeArray<MeshBuffers> temporaryMeshBuffers;
			JobHandle buildJob, splitterJob;
			public List<MeshWithType> meshes;

			public bool isValid => type != Type.Invalid;

			public struct MeshBuffers {
				public UnsafeAppendBuffer splitterOutput, vertices, triangles, solidVertices, solidTriangles, textVertices, textTriangles;
				public Bounds bounds;

				public MeshBuffers(Allocator allocator) {
					splitterOutput = new UnsafeAppendBuffer(0, 4, allocator);
					vertices = new UnsafeAppendBuffer(0, 4, allocator);
					triangles = new UnsafeAppendBuffer(0, 4, allocator);
					solidVertices = new UnsafeAppendBuffer(0, 4, allocator);
					solidTriangles = new UnsafeAppendBuffer(0, 4, allocator);
					textVertices = new UnsafeAppendBuffer(0, 4, allocator);
					textTriangles = new UnsafeAppendBuffer(0, 4, allocator);
					bounds = new Bounds();
				}

				public void Dispose () {
					splitterOutput.Dispose();
					vertices.Dispose();
					triangles.Dispose();
					solidVertices.Dispose();
					solidTriangles.Dispose();
					textVertices.Dispose();
					textTriangles.Dispose();
				}
			}

			public unsafe UnsafeAppendBuffer* splitterOutputPtr => & ((MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr())->splitterOutput;

			public void Init (Type type, BuilderData.Meta meta) {
				submitted = false;
				this.type = type;
				this.meta = meta;

				if (meshes == null) meshes = new List<MeshWithType>();
				if (!temporaryMeshBuffers.IsCreated) {
					temporaryMeshBuffers = new NativeArray<MeshBuffers>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
					temporaryMeshBuffers[0] = new MeshBuffers(Allocator.Persistent);
				}
			}

			static int SubmittedJobs = 0;

			public void SetSplitterJob (DrawingData gizmos, JobHandle splitterJob) {
				this.splitterJob = splitterJob;
				if (type == Type.Static) {
					unsafe {
						buildJob = CommandBuilder.Build(gizmos, (MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(temporaryMeshBuffers), null, splitterJob);
					}

					SubmittedJobs++;
					// ScheduleBatchedJobs is expensive, so only do it once in a while
					if (SubmittedJobs % 8 == 0) {
						Profiler.BeginSample("ScheduleJobs");
						JobHandle.ScheduleBatchedJobs();
						Profiler.EndSample();
					}
				}
			}

			public void SchedulePersistFilter (int version, float time, bool isPlaying) {
				if (type != Type.Persistent) throw new System.InvalidOperationException();

				splitterJob.Complete();

				// If data was from a different game mode then it shouldn't live any longer.
				// E.g. editor mode => game mode
				if (meta.isCreatedInGameMode != isPlaying) {
					meta.version = -1;
					return;
				}

				// If the command buffer is empty then this instance should not live longer
				var splitterOutput = temporaryMeshBuffers[0].splitterOutput;
				if (splitterOutput.GetLength() == 0) {
					meta.version = -1;
					return;
				}

				meta.version = version;
				// Guarantee that all drawing commands survive at least one frame
				// Don't filter them until we have drawn them once at least.
				if (submitted) {
					buildJob.Complete();
					unsafe {
						splitterJob = new CommandBuilder.PersistentFilterJob {
							buffer = &((MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafePtr(temporaryMeshBuffers))->splitterOutput,
							time = time,
						}.Schedule(splitterJob);
					}
				}
			}

			public bool IsValidForCamera (Camera camera, bool allowGizmos, bool allowCameraDefault) {
				if (!allowGizmos && meta.isGizmos) return false;

				if (meta.cameraTargets != null) {
					return meta.cameraTargets.Contains(camera);
				} else {
					return allowCameraDefault;
				}
			}

			public void Schedule (DrawingData gizmos, Camera camera) {
				if (type != Type.Static && type != Type.CustomMeshes) {
					unsafe {
						buildJob = CommandBuilder.Build(gizmos, (MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(temporaryMeshBuffers), camera, splitterJob);
					}
				}
			}

			public void BuildMeshes (DrawingData gizmos) {
				if ((type == Type.Static && submitted) || type == Type.CustomMeshes) return;
				buildJob.Complete();
				unsafe {
					PoolMeshes(gizmos);
					CommandBuilder.BuildMesh(gizmos, meshes, meta.drawOrderIndex, (MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr());
				}
				submitted = true;
			}

			void PoolMeshes (DrawingData gizmos) {
				if (!isValid) throw new System.InvalidOperationException();
				if (type != Type.CustomMeshes) {
					for (int i = 0; i < meshes.Count; i++) gizmos.PoolMesh(meshes[i].mesh);
				}
				meshes.Clear();
			}

			public void Release (DrawingData gizmos) {
				if (!isValid) throw new System.InvalidOperationException();
				PoolMeshes(gizmos);
				type = Type.Invalid;
				splitterJob.Complete();
				buildJob.Complete();
			}

			public void Dispose () {
				if (isValid) throw new System.InvalidOperationException();
				splitterJob.Complete();
				buildJob.Complete();
				if (temporaryMeshBuffers.IsCreated) {
					temporaryMeshBuffers[0].Dispose();
					temporaryMeshBuffers.Dispose();
				}
			}
		}

		internal struct BuilderData : IDisposable {
			public enum State {
				Free,
				Reserved,
				Initialized,
				WaitingForSplitter,
			}

			public struct Meta {
				public Hasher hasher;
				public RedrawScope redrawScope1;
				public RedrawScope redrawScope2;
				public int version;
				public bool isGizmos;
				public bool isCreatedInGameMode;
				public int drawOrderIndex;
				public Camera[] cameraTargets;
			}

			public struct BitPackedMeta {
				uint flags;

				const int UniqueIDBitshift = 17;
				const int IsBuiltInFlagIndex = 16;
				const int IndexMask = (1 << IsBuiltInFlagIndex) - 1;


				public BitPackedMeta (int dataIndex, int uniqueID, bool isBuiltInCommandBuilder) {
					// Important to make ensure bitpacking doesn't collide
					if (dataIndex > IndexMask) throw new System.Exception("Too many command builders active. Are some command builders not being disposed?");

					flags = (uint)(dataIndex | uniqueID << UniqueIDBitshift | (isBuiltInCommandBuilder ? 1 << IsBuiltInFlagIndex : 0));
				}

				public int dataIndex {
					get {
						return (int)(flags & IndexMask);
					}
				}

				public int uniqueID {
					get {
						return (int)(flags >> UniqueIDBitshift);
					}
				}

				public bool isBuiltInCommandBuilder {
					get {
						return (flags & (1 << IsBuiltInFlagIndex)) != 0;
					}
				}

				public static bool operator== (BitPackedMeta lhs, BitPackedMeta rhs) {
					return lhs.flags == rhs.flags;
				}

				public static bool operator!= (BitPackedMeta lhs, BitPackedMeta rhs) {
					return lhs.flags != rhs.flags;
				}

				public override bool Equals (object obj) {
					if (obj is BitPackedMeta meta) {
						return flags == meta.flags;
					}
					return false;
				}

				public override int GetHashCode () {
					return (int)flags;
				}
			}

			public BitPackedMeta packedMeta;
			public List<Mesh> meshes;
			public NativeArray<UnsafeAppendBuffer> commandBuffers;
			public State state { get; private set; }
			// TODO?
			public bool preventDispose;
			JobHandle splitterJob;
			public Meta meta;

			public void Reserve (int dataIndex, bool isBuiltInCommandBuilder) {
				if (state != State.Free) throw new System.InvalidOperationException();
				state = BuilderData.State.Reserved;
				packedMeta = new BitPackedMeta(dataIndex, (UniqueIDCounter++), isBuiltInCommandBuilder);
			}

			static int UniqueIDCounter = 0;

			public void Init (Hasher hasher, RedrawScope frameRedrawScope, RedrawScope customRedrawScope, bool isGizmos, int drawOrderIndex) {
				if (state != State.Reserved) throw new System.InvalidOperationException();

				meta = new Meta {
					hasher = hasher,
					redrawScope1 = frameRedrawScope,
					redrawScope2 = customRedrawScope,
					isGizmos = isGizmos,
					version = 0, // Will be filled in later
					drawOrderIndex = drawOrderIndex,
					isCreatedInGameMode = Application.isPlaying,
					cameraTargets = null,
				};

				if (meshes == null) meshes = new List<Mesh>();
				if (!commandBuffers.IsCreated) {
					commandBuffers = new NativeArray<UnsafeAppendBuffer>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
					for (int i = 0; i < commandBuffers.Length; i++) commandBuffers[i] = new UnsafeAppendBuffer(0, 4, Allocator.Persistent);
				}

				state = State.Initialized;
			}

			public unsafe UnsafeAppendBuffer* bufferPtr {
				get {
					return (UnsafeAppendBuffer*)commandBuffers.GetUnsafePtr();
				}
			}

			public void Submit (DrawingData gizmos) {
				if (state != State.Initialized) throw new System.InvalidOperationException();

				meta.version = gizmos.version;

				// Command stream
				// split to static, dynamic and persistent
				// render static
				// render dynamic per camera
				// render persistent per camera
				const int PersistentDrawOrderOffset = 1000000;
				var tmpMeta = meta;
				// Reserve some buffers.
				// We need to set a deterministic order in which things are drawn to avoid flickering.
				// The shaders use the z buffer most of the time, but there are still
				// things which are not order independent.
				// Static stuff is drawn first
				tmpMeta.drawOrderIndex = meta.drawOrderIndex*3 + 0;
				int staticBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Static, tmpMeta);
				// Dynamic stuff is drawn directly after the static stuff
				tmpMeta.drawOrderIndex = meta.drawOrderIndex*3 + 1;
				int dynamicBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Dynamic, tmpMeta);
				// Persistent stuff is always drawn after everything else
				tmpMeta.drawOrderIndex = meta.drawOrderIndex + PersistentDrawOrderOffset;
				int persistentBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Persistent, tmpMeta);

				unsafe {
					splitterJob = new CommandBuilder.StreamSplitter {
						inputBuffers = commandBuffers,
						staticBuffer = gizmos.processedData.Get(staticBuffer).splitterOutputPtr,
						dynamicBuffer = gizmos.processedData.Get(dynamicBuffer).splitterOutputPtr,
						persistentBuffer = gizmos.processedData.Get(persistentBuffer).splitterOutputPtr,
					}.Schedule();
				}

				gizmos.processedData.Get(staticBuffer).SetSplitterJob(gizmos, splitterJob);
				gizmos.processedData.Get(dynamicBuffer).SetSplitterJob(gizmos, splitterJob);
				gizmos.processedData.Get(persistentBuffer).SetSplitterJob(gizmos, splitterJob);

				if (meshes.Count > 0) {
					// Custom meshes stuff are drawn after the dynamic stuff
					tmpMeta.drawOrderIndex = meta.drawOrderIndex*3 + 2;

					var customMeshes = gizmos.processedData.Get(gizmos.processedData.Reserve(ProcessedBuilderData.Type.CustomMeshes, tmpMeta)).meshes;
					// Copy meshes to render
					for (int i = 0; i < meshes.Count; i++) customMeshes.Add(new MeshWithType { mesh = meshes[i], type = MeshType.Solid, drawingOrderIndex = tmpMeta.drawOrderIndex });
					meshes.Clear();
				}

				// TODO: Allocate 3 output objects and pipe splitter to them

				// Only meshes valid for all cameras have been submitted.
				// Meshes that depend on the specific camera will be submitted just before rendering
				// that camera. Line drawing depends on the exact camera.
				// In particular when drawing circles different number of segments
				// are used depending on the distance to the camera.
				state = State.WaitingForSplitter;
			}

			public void Release () {
				if (state == State.Free) throw new System.InvalidOperationException();
				state = BuilderData.State.Free;
				ClearData();
			}

			void ClearData () {
				// Wait for any jobs that might be running
				// This is important to avoid memory corruption bugs
				splitterJob.Complete();
				meta = default;
				preventDispose = false;
				meshes.Clear();
				for (int i = 0; i < commandBuffers.Length; i++) {
					var buffer = commandBuffers[i];
					buffer.Reset();
					commandBuffers[i] = buffer;
				}
			}

			public void Dispose () {
				if (state == State.Reserved || state == State.Initialized) {
					UnityEngine.Debug.LogError("Drawing data is being destroyed, but a drawing instance is still active. Are you sure you have called Dispose on all drawing instances? This will cause a memory leak!");
					return;
				}

				splitterJob.Complete();
				if (commandBuffers.IsCreated) {
					for (int i = 0; i < commandBuffers.Length; i++) {
						commandBuffers[i].Dispose();
					}
					commandBuffers.Dispose();
				}
			}
		}

		internal struct BuilderDataContainer : IDisposable {
			BuilderData[] data;


			public BuilderData.BitPackedMeta Reserve (bool isBuiltInCommandBuilder) {
				if (data == null) data = new BuilderData[1];
				for (int i = 0; i < data.Length; i++) {
					if (data[i].state == BuilderData.State.Free) {
						data[i].Reserve(i, isBuiltInCommandBuilder);
						return data[i].packedMeta;
					}
				}

				var newData = new BuilderData[data.Length * 2];
				data.CopyTo(newData, 0);
				data = newData;
				return Reserve(isBuiltInCommandBuilder);
			}

			public void Release (BuilderData.BitPackedMeta meta) {
				data[meta.dataIndex].Release();
			}

			public bool StillExists (BuilderData.BitPackedMeta meta) {
				int index = meta.dataIndex;

				if (data == null || index >= data.Length) return false;
				return data[index].packedMeta == meta;
			}

			public ref BuilderData Get (BuilderData.BitPackedMeta meta) {
				int index = meta.dataIndex;

				if (data[index].state == BuilderData.State.Free) throw new System.ArgumentException("Data is not reserved");
				if (data[index].packedMeta != meta) throw new System.ArgumentException("This command builder has already been disposed");
				return ref data[index];
			}

			public void ReleaseAllUnused () {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].state == BuilderData.State.WaitingForSplitter) {
						data[i].Release();
					}
				}
			}

			public void Dispose () {
				if (data != null) {
					for (int i = 0; i < data.Length; i++) data[i].Dispose();
				}
				// Ensures calling Dispose multiple times is a NOOP
				data = null;
			}
		}

		internal struct ProcessedBuilderDataContainer {
			ProcessedBuilderData[] data;
			Stack<int> freeSlots;

			public int Reserve (ProcessedBuilderData.Type type, BuilderData.Meta meta) {
				if (data == null) {
					data = new ProcessedBuilderData[0];
					freeSlots = new Stack<int>();
				}
				if (freeSlots.Count == 0) {
					var newData = new ProcessedBuilderData[math.max(4, data.Length*2)];
					data.CopyTo(newData, 0);
					for (int i = data.Length; i < newData.Length; i++) freeSlots.Push(i);
					data = newData;
				}
				int index = freeSlots.Pop();
				data[index].Init(type, meta);
				return index;
			}

			public ref ProcessedBuilderData Get (int index) {
				if (!data[index].isValid) throw new System.ArgumentException();
				return ref data[index];
			}

			void Release (DrawingData gizmos, int i) {
				data[i].Release(gizmos);
				freeSlots.Push(i);
			}

			public void SubmitMeshes (DrawingData gizmos, Camera camera, int versionThreshold, bool allowGizmos, bool allowCameraDefault) {
				if (data == null) return;
				Profiler.BeginSample("Schedule");
				int c = 0;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						c++;
						data[i].Schedule(gizmos, camera);
					}
				}

				Profiler.EndSample();

				// Ensure all jobs start to be executed on the worker threads now
				JobHandle.ScheduleBatchedJobs();

				Profiler.BeginSample("Build");
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						data[i].BuildMeshes(gizmos);
					}
				}
				Profiler.EndSample();
			}

			public void CollectMeshes (int versionThreshold, List<MeshWithType> meshes, Camera camera, bool allowGizmos, bool allowCameraDefault) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						var itemMeshes = data[i].meshes;
						for (int j = 0; j < itemMeshes.Count; j++) {
							meshes.Add(itemMeshes[j]);
						}
					}
				}
			}

			public void FilterOldPersistentCommands (int version, float time, bool isPlaying) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].type == ProcessedBuilderData.Type.Persistent) {
						data[i].SchedulePersistFilter(version, time, isPlaying);
					}
				}
			}

			public bool SetVersion (Hasher hasher, int version) {
				if (data == null) return false;
				bool found = false;

				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.hasher.Hash == hasher.Hash) {
						data[i].meta.version = version;
						found = true;
					}
				}
				return found;
			}

			public bool SetVersion (RedrawScope scope, int version) {
				if (data == null) return false;
				bool found = false;

				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && (data[i].meta.redrawScope1.id == scope.id || data[i].meta.redrawScope2.id == scope.id)) {
						data[i].meta.version = version;
						found = true;
					}
				}
				return found;
			}

			public bool SetCustomScope (Hasher hasher, RedrawScope scope) {
				if (data == null) return false;
				bool found = false;

				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.hasher.Hash == hasher.Hash) {
						data[i].meta.redrawScope2 = scope;
						found = true;
					}
				}
				return found;
			}

			public void ReleaseDataOlderThan (DrawingData gizmos, int version) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version < version) {
						Release(gizmos, i);
					}
				}
			}

			public void ReleaseAllWithHash (DrawingData gizmos, Hasher hasher) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.hasher.Hash == hasher.Hash) {
						Release(gizmos, i);
					}
				}
			}

			public void Dispose (DrawingData gizmos) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid) Release(gizmos, i);
					data[i].Dispose();
				}
				// Ensures calling Dispose multiple times is a NOOP
				data = null;
			}
		}

		internal enum MeshType {
			Lines,
			Solid,
			Text
		}

		internal struct MeshWithType {
			public Mesh mesh;
			public MeshType type;
			public int drawingOrderIndex;
		}

		internal BuilderDataContainer data;
		internal ProcessedBuilderDataContainer processedData;
		List<MeshWithType> meshes = new List<MeshWithType>();
		Stack<Mesh> cachedMeshes = new Stack<Mesh>();
		internal SDFLookupData fontData;
		int currentDrawOrderIndex = 0;

		internal int GetNextDrawOrderIndex () {
			currentDrawOrderIndex++;
			return currentDrawOrderIndex;
		}

		void PoolMesh (Mesh mesh) {
			// Note: clearing the mesh here will deallocate the vertex/index buffers
			// This is not good for performance as it will have to be allocated again (likely with the same size) in the next frame
			//mesh.Clear();
			cachedMeshes.Push(mesh);
		}

		internal Mesh GetMesh () {
			if (cachedMeshes.Count > 0) {
				return cachedMeshes.Pop();
			} else {
				var mesh = new Mesh {
					hideFlags = HideFlags.DontSave
				};
				mesh.MarkDynamic();
				return mesh;
			}
		}

		internal void LoadFontDataIfNecessary () {
			if (fontData.material == null) {
				var font = DefaultFonts.LoadDefaultFont();
				fontData = new SDFLookupData(font);
			}
		}

		static float CurrentTime {
			get {
				return Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
			}
		}

		static void UpdateTime () {
			// Time.time cannot be accessed in the job system, so create a global variable which *can* be accessed.
			// It's not updated as frequently, but it's only used for the WithDuration method, so it should be ok
			SharedDrawingData.BurstTime.Data = CurrentTime;
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
		public CommandBuilder GetBuilder (bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, default, !renderInGame, false);
		}

		public CommandBuilder GetBuiltInBuilder (bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, default, !renderInGame, true);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public CommandBuilder GetBuilder (RedrawScope redrawScope, bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, redrawScope, !renderInGame, false);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public CommandBuilder GetBuilder (Hasher hasher, RedrawScope redrawScope = default, bool renderInGame = false) {
			// The user is going to rebuild the data with the given hash
			// Let's clear the previous data with that hash since we know it is not needed any longer.
			// Do not do this if a hash is not given.
			if (!hasher.Equals(Hasher.NotSupplied)) DiscardData(hasher);
			UpdateTime();
			return new CommandBuilder(this, hasher, frameRedrawScope, redrawScope, !renderInGame, false);
		}

		/// <summary>Material to use for surfaces</summary>
		public Material surfaceMaterial;

		/// <summary>Material to use for lines</summary>
		public Material lineMaterial;

		/// <summary>Material to use for text</summary>
		public Material textMaterial;

		public int version { get; private set; } = 1;
		int lastTickVersion;
		int lastTickVersion2;

		public RedrawScope frameRedrawScope;

		struct Range {
			public int start;
			public int end;
		}

		Dictionary<Camera, Range> cameraVersions = new Dictionary<Camera, Range>();

		void DiscardData (Hasher hasher) {
			processedData.ReleaseAllWithHash(this, hasher);
		}

		/// <summary>
		/// Schedules the meshes for the specified hash to be drawn.
		/// Returns: False if there is no cached mesh for this hash, you may want to
		///  submit one in that case. The draw command will be issued regardless of the return value.
		/// </summary>
		public bool Draw (Hasher hasher) {
			if (hasher.Equals(Hasher.NotSupplied)) throw new System.ArgumentException("Invalid hash value");
			return processedData.SetVersion(hasher, version);
		}

		/// <summary>
		/// Schedules the meshes for the specified hash to be drawn.
		/// Returns: False if there is no cached mesh for this hash, you may want to
		///  submit one in that case. The draw command will be issued regardless of the return value.
		///
		/// This overload will draw all meshes within the specified redraw scope.
		/// Note that if they had been drawn with another redraw scope earlier they will be removed from that scope.
		/// </summary>
		public bool Draw (Hasher hasher, RedrawScope scope) {
			if (hasher.Equals(Hasher.NotSupplied)) throw new System.ArgumentException("Invalid hash value");
			processedData.SetCustomScope(hasher, scope);
			return processedData.SetVersion(hasher, version);
		}

		/// <summary>Schedules all meshes that were drawn the last frame with this redraw scope to be drawn again</summary>
		public void Draw (RedrawScope scope) {
			if (scope.id != 0) processedData.SetVersion(scope, version);
		}

		public void TickFrame () {
			// All cameras rendered between the last tick and this one will have
			// a version that is at least lastTickVersion + 1.
			// However the user may want to reuse meshes from the previous frame (see Draw(Hasher)).
			// This requires us to keep data from one more frame and thus we use lastTickVersion2 + 1
			// TODO: One frame should be enough, right?
			data.ReleaseAllUnused();
			// Remove persistent commands that have timed out.
			// When not playing then persistent commands are never drawn twice
			processedData.FilterOldPersistentCommands(version, CurrentTime, Application.isPlaying);
			processedData.ReleaseDataOlderThan(this, lastTickVersion2 + 1);
			lastTickVersion2 = lastTickVersion;
			lastTickVersion = version;
			currentDrawOrderIndex = 0;
			// TODO: Filter cameraVersions to avoid memory leak
		}

		class MeshCompareByDrawingOrder : IComparer<MeshWithType> {
			public int Compare (MeshWithType a, MeshWithType b) {
				return a.drawingOrderIndex - b.drawingOrderIndex;
			}
		}

		static readonly MeshCompareByDrawingOrder meshSorter = new MeshCompareByDrawingOrder();
		Plane[] frustrumPlanes = new Plane[6];

		void ConfigureMaterialFeature (Material material, DetectedRenderPipeline detectedRenderPipeline) {
			if (material) {
				if (detectedRenderPipeline == DetectedRenderPipeline.HDRP) {
					material.EnableKeyword("UNITY_HDRP");
				} else {
					material.DisableKeyword("UNITY_HDRP");
				}
			}
		}

		void LoadMaterials (DetectedRenderPipeline detectedRenderPipeline) {
			// Make sure the material references are correct
#if UNITY_EDITOR
			// When importing the package for the first time the asset database may not be up to date.
			// If this is not done it may not find the assets and it will lead to exceptions until scripts are recompiled.
			// This is a bit hard to test, so I *think* this fix works, but I am not 100% sure.
			if (surfaceMaterial == null || lineMaterial == null) UnityEditor.AssetDatabase.Refresh();
#endif
			if (surfaceMaterial == null) {
				surfaceMaterial = Resources.Load<Material>("aline_surface");
				ConfigureMaterialFeature(surfaceMaterial, detectedRenderPipeline);
			}
			if (lineMaterial == null) {
				lineMaterial = Resources.Load<Material>("aline_outline");
				ConfigureMaterialFeature(lineMaterial, detectedRenderPipeline);
			}
			if (fontData.material == null) {
				var font = DefaultFonts.LoadDefaultFont();
				fontData = new SDFLookupData(font);
				ConfigureMaterialFeature(fontData.material, detectedRenderPipeline);
			}
		}

		public DrawingData() {
			LoadMaterials(DetectedRenderPipeline.BuiltInOrCustom);
		}

		/// <summary>Call after all <see cref="Draw"/> commands for the frame have been done to draw everything.</summary>
		/// <param name="allowCameraDefault">Indicates if built-in command builders and custom ones without a custom CommandBuilder.cameraTargets should render to this camera.</param>
		public void Render (Camera cam, bool allowGizmos, CommandBuffer commandBuffer, bool allowCameraDefault, DetectedRenderPipeline detectedRenderPipeline) {
			Profiler.BeginSample("Draw Retained Gizmos");

			LoadMaterials(detectedRenderPipeline);

			// Warn if the materials could not be found
			if (surfaceMaterial == null || lineMaterial == null) {
				// Note that when the package is installed Unity may start rendering things and call this method before it has initialized the Resources folder with the materials.
				// We don't want to throw exceptions in that case because once the import finishes everything will be good.
				// UnityEngine.Debug.LogWarning("Looks like you just installed ALINE. The ALINE package will start working after the next script recompilation.");
				return;
			}

			var planes = frustrumPlanes;
			GeometryUtility.CalculateFrustumPlanes(cam, planes);

			if (!cameraVersions.TryGetValue(cam, out Range cameraRenderingRange)) {
				cameraRenderingRange = new Range { start = int.MinValue, end = int.MinValue };
			}

			// Check if the last time the camera was rendered
			// was during the current frame.
			if (cameraRenderingRange.end > lastTickVersion) {
				// In some cases a camera is rendered multiple times per frame.
				// In this case we just extend the end of the drawing range up to the current version.
				// The reasoning is that all times the camera is rendered in a frame
				// all things should be drawn.
				// If we did update the start of the range then things would only be drawn
				// the first time the camera was rendered in the frame.

				// Sometimes the scene view will be rendered twice in a single frame
				// due to some internal Unity tooltip code.
				// Without this fix the scene view camera may end up showing no gizmos
				// for a single frame.
				cameraRenderingRange.end = version + 1;
			} else {
				// This is the common case: the previous time the camera was rendered
				// it rendered all versions lower than cameraRenderingRange.end.
				// So now we start by rendering from that version.
				cameraRenderingRange = new Range  { start = cameraRenderingRange.end, end = version + 1 };
			}

			// Don't show anything rendered before the last frame.
			// If the camera has been turned off for a while and then suddenly starts rendering again
			// we want to make sure that we don't render meshes from multiple frames.
			// This happens often in the unity editor as the scene view and game view often skip
			// rendering many frames when outside of play mode.
			cameraRenderingRange.start = Mathf.Max(cameraRenderingRange.start, lastTickVersion2 + 1);

			// If GL.wireframe is enabled (the Wireframe mode in the scene view settings)
			// then I have found no way to draw gizmos in a good way.
			// It's best to disable gizmos altogether to avoid drawing wireframe versions of gizmo meshes.
			if (!GL.wireframe) {
				Profiler.BeginSample("Build Meshes");
				processedData.SubmitMeshes(this, cam, cameraRenderingRange.start, allowGizmos, allowCameraDefault);
				Profiler.EndSample();
				Profiler.BeginSample("Collect Meshes");
				meshes.Clear();
				processedData.CollectMeshes(cameraRenderingRange.start, meshes, cam, allowGizmos, allowCameraDefault);
				Profiler.EndSample();
				Profiler.BeginSample("Sorting Meshes");
				// Note that a stable sort is required as some meshes may have the same sorting index
				// but those meshes will have a consistent ordering between them in the list
				meshes.Sort(meshSorter);
				Profiler.EndSample();

				// First surfaces, then lines
				for (int matIndex = 0; matIndex <= 2; matIndex++) {
					var mat = matIndex == 0 ? surfaceMaterial : (matIndex == 1 ? lineMaterial : fontData.material);
					var meshType = matIndex == 0 ? MeshType.Solid : (matIndex == 1 ? MeshType.Lines : MeshType.Text);

					for (int pass = 0; pass < mat.passCount; pass++) {
						for (int i = 0; i < meshes.Count; i++) {
							if (meshes[i].type == meshType && GeometryUtility.TestPlanesAABB(planes, meshes[i].mesh.bounds)) {
								commandBuffer.DrawMesh(meshes[i].mesh, Matrix4x4.identity, mat, 0, pass, null);
							}
						}
					}
				}

				meshes.Clear();
			}

			cameraVersions[cam] = cameraRenderingRange;
			version++;
			Profiler.EndSample();
		}

		/// <summary>
		/// Destroys all cached meshes.
		/// Used to make sure that no memory leaks happen in the Unity Editor.
		/// </summary>
		public void ClearData () {
			data.Dispose();
			processedData.Dispose(this);

			while (cachedMeshes.Count > 0) {
				Mesh.DestroyImmediate(cachedMeshes.Pop());
			}

			UnityEngine.Assertions.Assert.IsTrue(meshes.Count == 0);
			fontData.Dispose();
		}
	}
}
