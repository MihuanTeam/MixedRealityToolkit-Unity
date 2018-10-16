﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.SpatialAwarenessSystem;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.EventDatum.SpatialAwarenessSystem;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.SpatialAwarenessSystem;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.SpatialAwarenessSystem.Handlers;
using Microsoft.MixedReality.Toolkit.Core.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.SDK.SpatialAwarenessSystem
{
    /// <summary>
    /// Class poviding the default implementation of the <see cref="IMixedRealitySpatialAwarenessSystem"/> interface.
    /// </summary>
    public class MixedRealitySpatialAwarenessSystem : MixedRealityEventManager, IMixedRealitySpatialAwarenessSystem
    {
        private GameObject spatialAwarenessParent = null;

        /// <summary>
        /// Parent <see cref="GameObject"/> which will encapsulate all of the spatial awareness system created scene objects.
        /// </summary>
        private GameObject SpatialAwarenessParent => spatialAwarenessParent ?? (spatialAwarenessParent = CreateSpatialAwarenessParent());

        /// <summary>
        /// Creates the parent for spatial awareness objects so that the scene heirarchy does not get overly cluttered.
        /// </summary>
        /// <returns>
        /// The <see cref="GameObject"/> to which spatial awareness created objects will be parented.
        /// </returns>
        private GameObject CreateSpatialAwarenessParent()
        {
            return new GameObject("Spatial Awareness System");
        }

        private GameObject meshParent = null;

        /// <summary>
        /// Parent <see cref="GameObject"/> which will encapsulate all of the system created mesh objects.
        /// </summary>
        private GameObject MeshParent => meshParent ?? (meshParent = CreateSecondGenerationParent("Meshes"));

        private GameObject surfaceParent = null;

        /// <summary>
        /// Parent <see cref="GameObject"/> which will encapsulate all of the system created mesh objects.
        /// </summary>
        private GameObject SurfaceParent => surfaceParent ?? (surfaceParent = CreateSecondGenerationParent("Surfaces"));

        /// <summary>
        /// Creates the a parent, that is a child if the Spatial Awareness System parent so that the scene heirarchy does not get overly cluttered.
        /// </summary>
        /// <returns>
        /// The <see cref="GameObject"/> to whichspatial awareness objects will be parented.
        /// </returns>
        private GameObject CreateSecondGenerationParent(string name)
        {
            GameObject secondGeneration = new GameObject(name);

            secondGeneration.transform.parent = SpatialAwarenessParent.transform;

            return secondGeneration;
        }

        private IMixedRealitySpatialAwarenessObserver spatialAwarenessObserver = null;

        /// <summary>
        /// The <see cref="IMixedRealitySpatialAwarenessObserver"/>, if any, that is active on the current platform.
        /// </summary>
        private IMixedRealitySpatialAwarenessObserver SpatialAwarenessObserver => spatialAwarenessObserver ?? (spatialAwarenessObserver = MixedRealityManager.Instance.GetManager<IMixedRealitySpatialAwarenessObserver>());

        #region IMixedRealityManager Implementation

        private MixedRealitySpatialAwarenessEventData meshEventData = null;
        private MixedRealitySpatialAwarenessEventData surfaceFindingEventData = null;

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
            InitializeInternal();
        }

        /// <summary>
        /// Performs initialization tasks for the spatial awareness system.
        /// </summary>
        private void InitializeInternal()
        {
            meshEventData = new MixedRealitySpatialAwarenessEventData(EventSystem.current);
            surfaceFindingEventData = new MixedRealitySpatialAwarenessEventData(EventSystem.current);

            // General settings
            StartupBehavior = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.StartupBehavior;
            ObservationExtents = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.ObservationExtents;
            UpdateInterval = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.UpdateInterval;

            // Mesh settings
            UseMeshSystem = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.UseMeshSystem;
            MeshPhysicsLayer = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.MeshPhysicsLayer;
            MeshLevelOfDetail = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.MeshLevelOfDetail;
            MeshTrianglesPerCubicMeter = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.MeshTrianglesPerCubicMeter;
            MeshRecalculateNormals = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.MeshRecalculateNormals;
            MeshDisplayOption = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.MeshDisplayOption;
            MeshVisibleMaterial = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.MeshVisibleMaterial;
            MeshOcclusionMaterial = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.MeshOcclusionMaterial;

            // Surface finding settings
            UseSurfaceFindingSystem = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.UseSurfaceFindingSystem;
            SurfacePhysicsLayer = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.SurfaceFindingPhysicsLayer;
            SurfaceFindingMinimumArea = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.SurfaceFindingMinimumArea;
            DisplayFloorSurfaces = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.DisplayFloorSurfaces;
            FloorSurfaceMaterial = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.FloorSurfaceMaterial;
            DisplayCeilingSurfaces = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.DisplayCeilingSurface;
            CeilingSurfaceMaterial = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.CeilingSurfaceMaterial;
            DisplayWallSurfaces = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.DisplayWallSurface;
            WallSurfaceMaterial = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.WallSurfaceMaterial;
            DisplayPlatformSurfaces = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.DisplayPlatformSurfaces;
            PlatformSurfaceMaterial = MixedRealityManager.Instance.ActiveProfile.SpatialAwarenessProfile.PlatformSurfaceMaterial;
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            // todo: cleanup some objects but not the root scene items
            InitializeInternal();
        }

        public override void Destroy()
        {
            // Cleanup game objects created during execution.
            if (Application.isPlaying)
            {
                // Detach the child objects and clean up the parent.
                if (spatialAwarenessParent != null)
                {
                    spatialAwarenessParent.transform.DetachChildren();
                    if (Application.isEditor)
                    {
                        Object.DestroyImmediate(spatialAwarenessParent);
                    }
                    else
                    {
                        Object.Destroy(spatialAwarenessParent);
                    }
                    spatialAwarenessParent = null;
                }

                // Detach the mesh objects (they are to be cleaned up by the observer) and cleanup the parent
                if (meshParent != null)
                {
                    meshParent.transform.DetachChildren();
                    if (Application.isEditor)
                    {
                        Object.DestroyImmediate(meshParent);
                    }
                    else
                    {
                        Object.Destroy(meshParent);
                    }
                    meshParent = null;
                }

                // Detach the surface objects (they are to be cleaned up by the observer) and cleanup the parent
                if (surfaceParent != null)
                {
                    surfaceParent.transform.DetachChildren();
                    if (Application.isEditor)
                    {
                        Object.DestroyImmediate(surfaceParent);
                    }
                    else
                    {
                        Object.Destroy(surfaceParent);
                    }
                    surfaceParent = null;
                }

                // Tell the observer to clean up
                SpatialAwarenessObserver?.Destroy();
            }
        }

        #region Mesh Events

        /// <inheritdoc />
        public void RaiseMeshAdded(int meshId, GameObject mesh)
        {
            if (!UseMeshSystem) { return; }

            // Parent the mesh object
            mesh.transform.parent = MeshParent.transform;

            meshEventData.Initialize(this, meshId, mesh);
            HandleEvent(meshEventData, OnMeshAdded);
        }

        /// <summary>
        /// Event sent whenever a mesh is added.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialAwarenessMeshHandler> OnMeshAdded =
            delegate (IMixedRealitySpatialAwarenessMeshHandler handler, BaseEventData eventData)
            {
                MixedRealitySpatialAwarenessEventData spatialEventData = ExecuteEvents.ValidateEventData<MixedRealitySpatialAwarenessEventData>(eventData);
                handler.OnMeshAdded(spatialEventData);
            };

        /// <inheritdoc />
        public void RaiseMeshUpdated(int meshId, GameObject mesh)
        {
            if (!UseMeshSystem) { return; }

            // Parent the mesh object
            mesh.transform.parent = MeshParent.transform;

            meshEventData.Initialize(this, meshId, mesh);
            HandleEvent(meshEventData, OnMeshUpdated);
        }

        /// <summary>
        /// Event sent whenever a mesh is updated.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialAwarenessMeshHandler> OnMeshUpdated =
            delegate (IMixedRealitySpatialAwarenessMeshHandler handler, BaseEventData eventData)
            {
                MixedRealitySpatialAwarenessEventData spatialEventData = ExecuteEvents.ValidateEventData<MixedRealitySpatialAwarenessEventData>(eventData);
                handler.OnMeshUpdated(spatialEventData);
            };


        /// <inheritdoc />
        public void RaiseMeshRemoved(int meshId)
        {
            if (!UseMeshSystem) { return; }

            meshEventData.Initialize(this, meshId, null);
            HandleEvent(meshEventData, OnMeshRemoved);
        }

        /// <summary>
        /// Event sent whenever a mesh is discarded.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialAwarenessMeshHandler> OnMeshRemoved =
            delegate (IMixedRealitySpatialAwarenessMeshHandler handler, BaseEventData eventData)
            {
                MixedRealitySpatialAwarenessEventData spatialEventData = ExecuteEvents.ValidateEventData<MixedRealitySpatialAwarenessEventData>(eventData);
                handler.OnMeshRemoved(spatialEventData);
            };

        #endregion Mesh Events

        #region Surface Finding Events

        /// <inheritdoc />
        public void RaiseSurfaceAdded(int surfaceId, GameObject surfaceObject)
        {
            if (!UseSurfaceFindingSystem) { return; }

            surfaceFindingEventData.Initialize(this, surfaceId, surfaceObject);
            HandleEvent(surfaceFindingEventData, OnSurfaceAdded);
        }

        /// <summary>
        /// Event sent whenever a planar surface is added.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialAwarenessSurfaceFindingHandler> OnSurfaceAdded =
            delegate (IMixedRealitySpatialAwarenessSurfaceFindingHandler handler, BaseEventData eventData)
            {
                MixedRealitySpatialAwarenessEventData spatialEventData = ExecuteEvents.ValidateEventData<MixedRealitySpatialAwarenessEventData>(eventData);
                handler.OnSurfaceAdded(spatialEventData);
            };

        /// <inheritdoc />
        public void RaiseSurfaceUpdated(int surfaceId, GameObject surfaceObject)
        {
            if (!UseSurfaceFindingSystem) { return; }

            surfaceFindingEventData.Initialize(this, surfaceId, surfaceObject);
            HandleEvent(surfaceFindingEventData, OnSurfaceUpdated);
        }

        /// <summary>
        /// Event sent whenever a planar surface is updated.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialAwarenessSurfaceFindingHandler> OnSurfaceUpdated =
            delegate (IMixedRealitySpatialAwarenessSurfaceFindingHandler handler, BaseEventData eventData)
            {
                MixedRealitySpatialAwarenessEventData spatialEventData = ExecuteEvents.ValidateEventData<MixedRealitySpatialAwarenessEventData>(eventData);
                handler.OnSurfaceUpdated(spatialEventData);
            };

        /// <inheritdoc />
        public void RaiseSurfaceRemoved(int surfaceId)
        {
            if (!UseSurfaceFindingSystem) { return; }

            surfaceFindingEventData.Initialize(this, surfaceId, null);
            HandleEvent(surfaceFindingEventData, OnSurfaceRemoved);
        }

        /// <summary>
        /// Event sent whenever a planar surface is discarded.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialAwarenessSurfaceFindingHandler> OnSurfaceRemoved =
            delegate (IMixedRealitySpatialAwarenessSurfaceFindingHandler handler, BaseEventData eventData)
            {
                MixedRealitySpatialAwarenessEventData spatialEventData = ExecuteEvents.ValidateEventData<MixedRealitySpatialAwarenessEventData>(eventData);
                handler.OnSurfaceRemoved(spatialEventData);
            };
        
        #endregion Surface Finding Events

        #endregion IMixedRealityManager Implementation

        #region IMixedRealtyEventSystem Implementation

        /// <inheritdoc />
        public override void HandleEvent<T>(BaseEventData eventData, ExecuteEvents.EventFunction<T> eventHandler)
        {
            base.HandleEvent(eventData, eventHandler);
        }

        /// <summary>
        /// Registers the <see cref="GameObject"/> to listen for boundary events.
        /// </summary>
        /// <param name="listener"></param>
        public override void Register(GameObject listener)
        {
            base.Register(listener);
        }

        /// <summary>
        /// UnRegisters the <see cref="GameObject"/> to listen for boundary events.
        /// /// </summary>
        /// <param name="listener"></param>
        public override void Unregister(GameObject listener)
        {
            base.Unregister(listener);
        }

        #endregion

        #region IMixedRealityEventSource Implementation

        /// <inheritdoc />
        bool IEqualityComparer.Equals(object x, object y)
        {
            // There shouldn't be other Spatial Awareness Managers to compare to.
            return false;
        }

        /// <inheritdoc />
        public int GetHashCode(object obj)
        {
            return Mathf.Abs(SourceName.GetHashCode());
        }

        /// <inheritdoc />
        public uint SourceId { get; } = 0;

        /// <inheritdoc />
        public string SourceName { get; } = "Mixed Reality Spatial Awareness System";

        #endregion IMixedRealityEventSource Implementation

        #region IMixedRealitySpatialAwarenessSystem Implementation

        /// <inheritdoc />
        public AutoStartBehavior StartupBehavior { get; set; } = AutoStartBehavior.AutoStart;

        /// <inheritdoc />
        public Vector3 ObservationExtents { get; set; } = Vector3.one * 10;

        /// <inheritdoc />
        public Vector3 ObserverOrigin { get; set; } = Vector3.zero;

        private float updateInterval = 3.5f;

        /// <inheritdoc />
        public float UpdateInterval
        {
            get
            {
                return updateInterval;
            }

            set
            {
                if (IsObserverRunning)
                {
                    Debug.LogError("UpdateInterval cannot be modified while the observer is running.");
                    return;
                }

                updateInterval = value;
            }
        }

        /// <inheritdoc />
        public bool IsObserverRunning
        {
            get
            {
                if (SpatialAwarenessObserver == null) { return false; }
                return SpatialAwarenessObserver.IsRunning;
            }
        }

        /// <inheritdoc />
        public void ResumeObserver()
        {
            if (SpatialAwarenessObserver == null) { return; }
            SpatialAwarenessObserver.StartObserving();
        }

        /// <inheritdoc />
        public void SuspendObserver()
        {
            if (SpatialAwarenessObserver == null) { return; }
            SpatialAwarenessObserver.StopObserving();
        }

        #region Mesh Handling implementation

        /// <inheritdoc />
        public bool UseMeshSystem { get; set; } = true;

        /// <inheritdoc />
        public int MeshPhysicsLayer { get; set; } = 31;

        /// <inheritdoc />
        public int MeshPhysicsLayerMask => 1 << MeshPhysicsLayer;

        private SpatialAwarenessMeshLevelOfDetail meshLevelOfDetail = SpatialAwarenessMeshLevelOfDetail.Coarse;

        /// <inheritdoc />
        public SpatialAwarenessMeshLevelOfDetail MeshLevelOfDetail
        {
            get
            { 
                return meshLevelOfDetail;
            }

            set
            {
                if (IsObserverRunning)
                {
                    Debug.LogError("MeshLevelOfDetail cannot be modified while the observer is running.");
                    return;
                }

                if (meshLevelOfDetail != value)
                {
                    // Non-custom values automatically modify MeshTrianglesPerCubicMeter
                    if (value != SpatialAwarenessMeshLevelOfDetail.Custom)
                    {
                        meshTrianglesPerCubicMeter = (int)value;
                    }

                    meshLevelOfDetail = value;
                }
            }
        }

        private int meshTrianglesPerCubicMeter = (int)SpatialAwarenessMeshLevelOfDetail.Coarse;

        /// <inheritdoc />
        public int MeshTrianglesPerCubicMeter
        {
            get
            {
                return meshTrianglesPerCubicMeter;
            }

            set
            {
                if (IsObserverRunning)
                {
                    Debug.LogError("MeshTrianglesPerCubicMeter cannot be modified while the observer is running.");
                    return;
                }

                meshTrianglesPerCubicMeter = value;
            }
        }

        /// <inheritdoc />
        public bool MeshRecalculateNormals { get; set; } = true;

        /// <inheritdoc />
        public SpatialMeshDisplayOptions MeshDisplayOption { get; set; } = SpatialMeshDisplayOptions.None;

        /// <inheritdoc />
        public Material MeshVisibleMaterial { get; set; } = null;

        /// <inheritdoc />
        public Material MeshOcclusionMaterial { get; set; } = null;

        /// <inheritdoc />
        public IDictionary<int, GameObject> Meshes
        {
            get
            {
                // The observer manages the mesh collection.
                return SpatialAwarenessObserver.Meshes;
            }
        }

        #endregion Mesh Handling implementation

        #region Surface Finding Handling implementation

        /// <inheritdoc />
        public bool UseSurfaceFindingSystem { get; set; } = false;

        /// <inheritdoc />
        public int SurfacePhysicsLayer { get; set; } = 31;

        /// <inheritdoc />
        public int SurfacePhysicsLayerMask => 1 << SurfacePhysicsLayer;

        /// <inheritdoc />
        public float SurfaceFindingMinimumArea { get; set; } = 0.025f;

        /// <inheritdoc />
        public bool DisplayFloorSurfaces { get; set; } = false;

        /// <inheritdoc />
        public Material FloorSurfaceMaterial { get; set; } = null;

        /// <inheritdoc />
        public bool DisplayCeilingSurfaces { get; set; } = false;

        /// <inheritdoc />
        public Material CeilingSurfaceMaterial { get; set; } = null;

        /// <inheritdoc />
        public bool DisplayWallSurfaces { get; set; } = false;

        /// <inheritdoc />
        public Material WallSurfaceMaterial { get; set; } = null;

        /// <inheritdoc />
        public bool DisplayPlatformSurfaces { get; set; } = false;

        /// <inheritdoc />
        public Material PlatformSurfaceMaterial { get; set; } = null;

        /// <inheritdoc />
        public IDictionary<int, GameObject> PlanarSurfaces
        {
            get
            {
                // This implementation of the spatial awareness system manages game objects.
                // todo
                return new Dictionary<int, GameObject>(0);
            }
        }

        #endregion Surface Finding Handling implementation

        #endregion IMixedRealitySpatialAwarenessSystem Implementation
    }
}
